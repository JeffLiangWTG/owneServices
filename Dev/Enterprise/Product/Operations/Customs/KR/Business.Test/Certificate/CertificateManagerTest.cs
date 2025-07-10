using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class CertificateManagerTest : TestCaseWithFactory
	{
		[TestDate(2022, 10, 10)]
		public void TestGetRandomValue()
		{
			var helper = new CertificateTestHelper();
			var password = helper.GetGlbExternalPassword("12840.pfx", "rk34879800!");
			var randomValueInfo = CertificateManager.GetRawRandomValueAndLocalKeyID(password);
			AssertEquals("RandomValue", randomValue12840, randomValueInfo.Item1);
			AssertEquals("LocalKeyID", "#01000000", randomValueInfo.Item2);

			password = helper.GetGlbExternalPassword("12410.pfx", "readykorea1!");
			randomValueInfo = CertificateManager.GetRawRandomValueAndLocalKeyID(password);
			AssertEquals(randomValue12410, randomValueInfo.Item1);
			AssertEquals("LocalKeyID", "#01000000", randomValueInfo.Item2);
		}

		public void TestEncodeRandomValue()
		{
			CertificateTestHelper.SetCustomsPublicKey();
			var encodedRValue = string.Empty;
			AssertNoExceptionThrown("RValue should be RSA encrypted and Base64 encoded properly.", () => { encodedRValue = CertificateManager.EncodeRandomValue(randomValue12840); });
			AssertNotNullOrEmpty(encodedRValue);

			encodedRValue = string.Empty;
			AssertNoExceptionThrown("RValue should be RSA encrypted and Base64 encoded properly.", () => { encodedRValue = CertificateManager.EncodeRandomValue(randomValue12410); });
			AssertNotNullOrEmpty(encodedRValue);
		}

		readonly byte[] randomValue12840 = { 127, 65, 246, 101, 132, 153, 207, 63, 218, 254, 204, 91, 159, 51, 110, 239, 109, 11, 57, 26 };
		readonly byte[] randomValue12410 = { 178, 207, 32, 81, 5, 236, 13, 214, 114, 121, 146, 31, 165, 93, 155, 255, 52, 211, 52, 117 };
	}
}
