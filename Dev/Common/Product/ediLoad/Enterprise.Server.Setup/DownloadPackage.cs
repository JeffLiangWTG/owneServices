using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using CargoWise.Common;
using CargoWise.IO;
using CargoWise.Loader.Common;
using Enterprise.Upgrades;
using Enterprise.Upgrades.UpgradePackageServices;

namespace Enterprise.Server.Setup
{
	public class DownloadPackage : InstallationItem
	{
		public DownloadPackage(Installation installation, InstallationSettings installationSettings)
			: base(installation)
		{
			this.installationSettings = installationSettings;
		}

		protected override bool NeedsToInstallCore()
		{
			return true;
		}

		public string GetPackageFromEdiProd(string licenseCode)
		{
			installationSettings.LicenseCode = licenseCode;
			var installationResult = InstallExcludingDependencies();
			return installationSettings.UpgradePackageFile;
		}

		protected override InstallationResult InstallExcludingDependencies()
		{
			ChangeCurrentTaskDescription("Getting latest client installation package");
			var upgradePackageService = UpgradePackageServiceClient;
			var currentVersion = new UpgradeInfo(Guid.Empty, 0, 0, 0, 0);
			var licenceCode = installationSettings.LicenseNineCode;
			var encryptedMessage = Encrypt(UpgradePackageServiceFactory.GetRawMessage(licenceCode, currentVersion));
			var urlRequest = new UpgradePackageUrlRequest() { EncryptedMessage = encryptedMessage, LicenceCode = licenceCode, CurrentVersionNumber = currentVersion.ToString() };
			var urlResponse = upgradePackageService.GetPackageUrl(urlRequest);
			if (urlResponse.ResponseClass == UpgradePackageUrlResponse.ResponseClassType.Success)
			{
				if (string.IsNullOrEmpty(urlResponse.URL))
				{
					return InstallationResult.Error("Skipped downloading of the package: " + urlResponse.ErrorMessage);
				}

				ChangeCurrentTaskDescription("Downloading latest client installation package");
				WebRequest.DefaultWebProxy.Credentials = CredentialCache.DefaultCredentials;
				try
				{
					var uri = new Uri(urlResponse.URL);
					var qs = new QueryString(uri.Query.Substring(1));
					string secureValue = qs[SecureQueryString.QueryStringKey];
					if (!string.IsNullOrEmpty(secureValue))
					{
						qs = new SecureQueryString(secureValue);
					}
					uri = new Uri(uri.GetLeftPart(UriPartial.Path));
#pragma warning disable SYSLIB0014 // 'WebRequest.Create(Uri)' is obsolete: 'WebRequest, HttpWebRequest, ServicePoint, and WebClient are obsolete. Use HttpClient instead.'
					var downloadRequest = WebRequest.Create(uri);
#pragma warning restore SYSLIB0014
					if (qs != null)
					{
						string user = qs["user"];
						string password = qs["pw"];
						if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(password))
						{
							downloadRequest.Credentials = new NetworkCredential(user, password);
						}
					}
					var downloadResponse = downloadRequest.GetResponse();
					var targetFile = Path.Combine(Temp.TempPath, Guid.NewGuid().ToString());
					using (var downloadStream = downloadResponse.GetResponseStream())
					using (var fileStream = File.Create(targetFile))
					{
						downloadStream.CopyTo(fileStream);
					}
					installationSettings.UpgradePackageFile = targetFile;
					return InstallationResult.OK();
				}
				catch (WebException ex)
				{
					return InstallationResult.Error("Failed to download the latest installation package: " + ex.ToString());
				}
				catch (System.ServiceModel.ProtocolException ex)
				{
					return InstallationResult.Error("Failed to get latest installation package: " + ex.ToString());
				}
			}
			else
			{
				return InstallationResult.Error("Failed to get latest installation package: " + urlResponse.ErrorMessage);
			}
		}

		string Encrypt(string textToEncrypt)
		{
			var aes = Aes.Create();
			aes.Key = Encoding.ASCII.GetBytes("6052D90C81D64D5B8F5677AA055CAE23");
			aes.IV = Encoding.ASCII.GetBytes(("878d7aca-ffc3-49fc-9710-969ca0c0f2ac").Replace("-", "").Substring(5, 16));

			var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
			var dataToEncrypt = Encoding.Unicode.GetBytes(textToEncrypt);

			using var transformationStream = new MemoryStream();
			using var encryptStream = new CryptoStream(transformationStream, encryptor, CryptoStreamMode.Write);
			encryptStream.Write(dataToEncrypt, 0, dataToEncrypt.Length);
			encryptStream.FlushFinalBlock();

			return Convert.ToBase64String(transformationStream.ToArray());
		}

		readonly InstallationSettings installationSettings;

		protected virtual IUpgradePackageService UpgradePackageServiceClient
		{
			get { return UpgradePackageServiceFactory.GetUpgradePackageServiceClient(); }
		}
	}
}
