using System.Security.Cryptography;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Certificates.Testing
{
	sealed class TokenCertificateSelectorTest : TestCase
	{
		public void TestChooseCertificateThrowsExceptionIfUnknownChipset()
		{
			AssertExceptionThrown<CryptographicException>(
				"ChooseCertificate with unkown chipset",
				"Unknown chipset: 'Not-A-Chipset'.",
				() => TokenCertificateSelector.New("Not-A-Chipset").ChooseCertificate());
		}

		public void TestShowCertificateInfoThrowsExceptionIfUnknownChipset()
		{
			AssertExceptionThrown<CryptographicException>(
				"ShowCertificateInfo with unkown chipset",
				"Unknown chipset: 'Not-A-Chipset'.",
				() => TokenCertificateSelector.New("Not-A-Chipset").ShowCertificateInfo("6062382AAEB24F3586B258395BB27509"));
		}

		public void TestShowCertificateInfoThrowsExceptionIfInvalidSerialNumber()
		{
			AssertExceptionThrown<CryptographicException>(
				"ShowCertificateInfo invalid certificate serial number",
				"Serial number must have even number of hexadecimal digits.",
				() => TokenCertificateSelector.New("anything").ShowCertificateInfo("InvalidSerialNumber"));
		}

		public void TestCanLocateCertificate_WithUnknownChipset()
		{
			AssertEquals(false, TokenCertificateSelector.New("Not-A-Chipset").CanLocateCertificate("6062382AAEB24F3586B258395BB27509"));
		}

		public void TestCanLocateCertificate_WithInvalidSerialNumber()
		{
			AssertEquals(false, TokenCertificateSelector.New("anything").CanLocateCertificate("InvalidSerialNumber"));
		}

		public void TestCertificateSource()
		{
			var tokenCertificateSelector = new TokenCertificateSelectorForTest("XYZ");
			AssertEquals("CertificateSource", "Token", tokenCertificateSelector.CertificateSourceExposedForTest);
		}

		class TokenCertificateSelectorForTest : TokenCertificateSelector
		{
			public TokenCertificateSelectorForTest(string chipset) : base(chipset)
			{
			}

			public string CertificateSourceExposedForTest => CertificateSource;
		}
	}
}
