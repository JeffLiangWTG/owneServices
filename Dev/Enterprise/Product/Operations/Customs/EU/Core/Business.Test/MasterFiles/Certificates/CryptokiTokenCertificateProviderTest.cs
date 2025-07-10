using System.Security.Cryptography;

namespace Enterprise.Customs.EU.Business.Testing
{
	class CryptokiTokenCertificateProviderTest : CryptokiCertificateProviderTest
	{
		public override void TestGetCertificateList()
		{
			AssertExceptionThrown<CryptographicException>(
				"GetTokenCertificateList with unkown chipset",
				"Unknown chipset: 'Not-A-Chipset'.",
				() => cryptokiCertificateProvider.GetCertificateList("Not-A-Chipset"));
		}

		public override void TestReadCertificate()
		{
			AssertExceptionThrown<CryptographicException>(
				"ReadTokenCertificate with unkown chipset",
				"Unknown chipset: 'Not-A-Chipset'.",
				() => cryptokiCertificateProvider.ReadCertificate("Not-A-Chipset", System.Array.Empty<byte>()));
		}

		protected override string CerificateSourceToTest => "Token";
	}
}
