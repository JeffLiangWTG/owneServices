using System.Security.Cryptography;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	class CertificateHelperTest : TestCase
	{
		public void TestTryGetHexValue()
		{
			var result = CertificateHelper.TryGetHexValue("", out var hexValue);
			AssertEquals("When empty string passed, Result", false, result);
			AssertNull("When empty string passed, Hex value", hexValue);

			result = CertificateHelper.TryGetHexValue("GHDGDJD", out hexValue);
			AssertEquals("When invalid string passed, Result", false, result);
			AssertNull("When invalid string passed, Hex value", hexValue);

			result = CertificateHelper.TryGetHexValue("123DCA12", out hexValue);
			AssertEquals("When valid string passed, Result", true, result);
			AssertNotNull("When valid string passed, Hex value", hexValue);
		}

		public void TestGetNewCryptokiCertificateProvider()
		{
			var provider = CertificateHelper.GetNewCryptokiCertificateProvider("Windows");
			AssertType<CryptokiWindowsCertificateProvider>("For Windows, Type", provider);

			provider = CertificateHelper.GetNewCryptokiCertificateProvider("Token");
			AssertType<CryptokiTokenCertificateProvider>("For Token, Type", provider);

			provider = CertificateHelper.GetNewCryptokiCertificateProvider("XYZ");
			AssertNull("For Unknown, Type", provider);
		}

		public void TestParseChipset()
		{
			AssertEquals("Bit4Id chipset", CargoWise.Cryptoki.Common.ClientServerApi.Chipset.BIT4ID, CertificateHelper.ParseChipset("BIT4ID"));

			AssertExceptionThrown<CryptographicException>(
				"GetChipset with unknown chipset",
				"Unknown chipset: 'Not-A-Chipset'.",
				() => CertificateHelper.ParseChipset("Not-A-Chipset")
			);
		}
	}
}
