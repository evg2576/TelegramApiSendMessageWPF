using QRCoder;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using TL;
using WTelegram;

namespace TelegramApiSendMessageWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int _apiId = 23690756;
        string _apiHash = "3c162901b8bf0bb5e53124d27f8cfdce";
        string _to = "jesterq";
        Client _client;

        public MainWindow()
        {
            InitializeComponent();
        }

        async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (await Send())
                infoLabel.Content = "Send successful";
        }

        async void loginButton_Click(object sender, RoutedEventArgs e)
        {
            bool isSuccess = false;
            if (_client.User is null)
            {
                if ((bool)phoneRadioButton.IsChecked)
                    isSuccess = await LoginByCode(_client, secureCodeTextbox.Text);
                else image.Source = await LoginByQR(_client, _apiId, _apiHash);
            }
            if (_client.User is not null || isSuccess)
                infoLabel.Content = "Login is success";
        }

        async Task<bool> Send()
        {            
            if (_client.User is not null)
            {
                Contacts_ResolvedPeer res;
                if (_to.Any(c => char.IsLetter(c)))
                    res = await _client.Contacts_ResolveUsername(_to);
                else res = await _client.Contacts_ResolvePhone(_to);
                return await _client.SendMessageAsync(res.User, "123321") is not null;
            }
            return false;
        }

        async Task<bool> LoginByCode(Client client, string loginInfo)
        {
            if (client.User is null && await client.Login(loginInfo) == "verification_code")
                return await client.Login(loginInfo) == null;
            return false;
        }

        async Task<BitmapImage> LoginByQR(Client client, int apiId, string apiHash)
        {
            if (client.User is null)
            {
                client.CollectAccessHash = true;
                var loginQr = (Auth_LoginToken)await client.Auth_ExportLoginToken(apiId, apiHash);
                string qr = Convert.ToBase64String(loginQr.token);
                return BmpConvert(GenerateQRCode($"tg://login?token={qr}"));
            }
            return null;
        }

        Bitmap GenerateQRCode(string data)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            return qrCodeImage;
        }

        BitmapImage BmpConvert(Bitmap src)
        {
            MemoryStream ms = new MemoryStream();
            src.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }

        async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            _client = new Client(_apiId, _apiHash);
            await _client.ConnectAsync();
            await _client.Login("+79286072587");
        }
    }
}
