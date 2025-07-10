using System.Security.Cryptography;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Certificates.Testing
{
	sealed class WindowsCertificateSelectorTest : TestCase
	{
		public void TestChooseCertificate()
		{
			var selector = WindowsCertificateSelector.New("WINDOWS");
			var chosenCertificate = selector.ChooseCertificate();
			AssertNull("[DialogResult=None] Chosen Certificate", chosenCertificate);
		}

		public void TestShowCertificateInfoThrowsExceptionIfInvalidSerialNumber()
		{
			AssertExceptionThrown<CryptographicException>(
				"ShowCertificateInfo invalid certificate serial number",
				"Serial number must have even number of hexadecimal digits.",
				() => WindowsCertificateSelector.New("WINDOWS").ShowCertificateInfo("InvalidSerialNumber"));
		}

		public void TestShowCertificateInfoThrowsExceptionIfIfCertificateNotFound()
		{
			AssertExceptionThrown<CryptographicException>(
				"Calling ChooseCertificate with unkown chipset",
				"Cannot find certificate with serial number '6062382AAEB24F3586B258395BB27509' using WINDOWS library.",
				() => WindowsCertificateSelector.New("WINDOWS").ShowCertificateInfo("6062382AAEB24F3586B258395BB27509"));
		}

		public void TestCertificateSource()
		{
			var windowsCertificateSelector = new WindowsCertificateSelectorForTest("XYZ");
			AssertEquals("CertificateSource", "Windows", windowsCertificateSelector.CertificateSourceExposedForTest);
		}

		class WindowsCertificateSelectorForTest : WindowsCertificateSelector
		{
			public WindowsCertificateSelectorForTest(string chipset) : base(chipset)
			{
			}

			public string CertificateSourceExposedForTest => CertificateSource;
		}
	}
}
