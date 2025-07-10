using System.Security.Cryptography.X509Certificates;
using CargoWise.IO;
using WTG.TrustedMessaging;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	public class CertificatesProviderForTest : ICertificatesProvider
	{
		public bool IsServer { get; set; } = true;
		public X509Certificate2 LocalCertificate => IsServer ? LoadCert("Server.pfx") : LoadCert("Client.pfx");
		public X509Certificate2 RemoteCertificate => IsServer ? LoadCert("Client.pfx") : LoadCert("Server.pfx");
		X509Certificate2 LoadCert(string certFileName)
		{
			var resourceRetriever = new EmbeddedResourceRetriever(System.Reflection.Assembly.GetExecutingAssembly());
			var certificateBytes = resourceRetriever.GetBytes(@"ZClientWebEDI.Test.TestFiles.Certificates." + certFileName);
			var cert = new X509Certificate2(certificateBytes, "", X509KeyStorageFlags.Exportable);
			return cert;
		}
	}
}
