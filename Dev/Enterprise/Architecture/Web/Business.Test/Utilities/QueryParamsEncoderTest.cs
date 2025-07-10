using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Business.Utilities.Testing
{
	sealed class QueryParamsEncoderTest : TestCase
	{
		public void TestProhibitedCharacters()
		{
			QueryParamsEncoder encoder = new QueryParamsEncoder();

			const string originalString = "Let sleeping dogs lie, =, /, +";
			string encryptedString = encoder.Encrypt(originalString);

			char[] charsProhibitedInUrl = new[] { '=', '/', '+' };
			foreach (char prohibitedChar in charsProhibitedInUrl)
			{
				string msg = string.Format("Character '{0}' is prohibited to be used in url", prohibitedChar);
				AssertCollectionNotContains(msg, prohibitedChar, encryptedString);
			}

			AssertEquals("Strings should be the same after encryption and decryption", originalString, encoder.Decrypt(encryptedString));
		}

		public void TestDecryptNullString()
		{
			var encoder = new QueryParamsEncoder();

			AssertNoExceptionThrown(() => encoder.Decrypt(null));
		}
	}
}
