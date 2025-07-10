using Enterprise.ZClientWebCargoWiseEDI.OIDC;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	public class OIDCAuthUserCookieDataTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestLoadDataFromCookieValue()
		{
			var fullValue = new OIDCAuthUserCookieData("sss", "vvv", "nnn", "eee").ToString();
			var data = OIDCAuthUserCookieData.Deserialize(fullValue);
			AssertEquals("sss", data.State);
			AssertEquals("vvv", data.Verifier);
			AssertEquals("nnn", data.Nonce);
			AssertEquals("eee", data.EmailAddress);

			var nullData = OIDCAuthUserCookieData.Deserialize(null);
			AssertNullOrEmpty(nullData.State);
			AssertNullOrEmpty(nullData.Verifier);
			AssertNullOrEmpty(nullData.Nonce);
			Assert(!nullData.IsCookieDecodingSuccessful);
		}

		public void TestCreateObject()
		{
			var userData = new UserCredentialData("123@oidc.com", "oidcOrg", "/myaccount/default.aspx");
			var cookieData = new OIDCAuthUserCookieData("verifier", "nonce", userData);
			AssertEquals("/myaccount/default.aspx", cookieData.State);
			AssertEquals("nonce", cookieData.Nonce);
			AssertEquals("verifier", cookieData.Verifier);
			AssertEquals("oidcOrg", cookieData.OrganisationCode);
			AssertEquals("123@oidc.com", cookieData.EmailAddress);
		}

		public void TestDeserializeInvalidValue()
		{
			var randomValue = "xx3431234ccqaasmnb__-123sdsss123___1111";
			var result1 = OIDCAuthUserCookieData.Deserialize(randomValue);
			Assert(!result1.IsCookieDecodingSuccessful);
			AssertNotNullOrEmpty(result1.DeserializingMessage);
		}
	}
}
