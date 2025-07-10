using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class AuthorisationNumberValidationHelperTest : TestCase
	{
		public void TestGetAuthorisationNumberInvalidFormatMessage_CountryCode()
		{
			var errorMessage = "Digit 01+02: Country Code 'DE' required.";
			CombineAssertions(() =>
			{
				AssertEquals("ACE Invalid", errorMessage, AuthorisationNumberValidationHelper.GetAuthorisationNumberInvalidFormatMessage("FRACE1234ZE000123", CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit));
				AssertEquals("ACT Invalid", errorMessage, AuthorisationNumberValidationHelper.GetAuthorisationNumberInvalidFormatMessage("FRACT1234ZT000123", CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir));
			});
		}

		public void TestGetAuthorisationNumberInvalidFormatMessage_AuthorisationType()
		{
			var errorMessageACE = "Digit 03-05: EU Authorization Type 'ACE' is required.";
			var errorMessageACT = "Digit 03-05: EU Authorization Type 'ACT' is required.";
			CombineAssertions(() =>
			{
				AssertEquals("ACE Invalid", errorMessageACE, AuthorisationNumberValidationHelper.GetAuthorisationNumberInvalidFormatMessage("DEACT1234ZE000123", CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit));
				AssertEquals("ACT Invalid", errorMessageACT, AuthorisationNumberValidationHelper.GetAuthorisationNumberInvalidFormatMessage("DEACE1234ZT000123", CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir));
			});
		}

		public void TestGetAuthorisationNumberInvalidFormatMessage_Office()
		{
			var errorMessage = "Digit 06-09: Office Code of the issuing Main Customs Office required.";
			CombineAssertions(() =>
			{
				AssertEquals("ACE Invalid", errorMessage, AuthorisationNumberValidationHelper.GetAuthorisationNumberInvalidFormatMessage("DEACE1A34ZE000123", CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit));
				AssertEquals("ACT Invalid", errorMessage, AuthorisationNumberValidationHelper.GetAuthorisationNumberInvalidFormatMessage("DEACT1A34ZT000123", CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir));
			});
		}

		public void TestGetAuthorisationNumberInvalidFormatMessage_Code()
		{
			var errorMessageACE = "Digit 10+11: Authorization Number of Type 'ACE' requires 'ZE'.";
			var errorMessageACT = "Digit 10+11: Authorization Number of Type 'ACT' requires 'ZT'.";
			CombineAssertions(() =>
			{
				AssertEquals("ACE Invalid", errorMessageACE, AuthorisationNumberValidationHelper.GetAuthorisationNumberInvalidFormatMessage("DEACE1234ZT000123", CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit));
				AssertEquals("ACT Invalid", errorMessageACT, AuthorisationNumberValidationHelper.GetAuthorisationNumberInvalidFormatMessage("DEACT1234ZE000123", CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir));
			});
		}

		public void TestGetAuthorisationNumberInvalidFormatMessage_SequenceNumber()
		{
			var errorMessage = "Digit 12-17: Sequence Number of Customs Office for national Authorization Type required.";
			CombineAssertions(() =>
			{
				AssertEquals("ACE Invalid", errorMessage, AuthorisationNumberValidationHelper.GetAuthorisationNumberInvalidFormatMessage("DEACE1234ZE000X23", CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit));
				AssertEquals("ACT Invalid", errorMessage, AuthorisationNumberValidationHelper.GetAuthorisationNumberInvalidFormatMessage("DEACT1234ZT000X23", CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir));
			});
		}

		public void TestGetAuthorisationNumberInvalidFormatMessage_Length()
		{
			var errorMessage = "Authorization Number must have 17 digits.";
			CombineAssertions(() =>
			{
				AssertEquals("ACE", errorMessage, AuthorisationNumberValidationHelper.GetAuthorisationNumberInvalidFormatMessage("ABC123", CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit));
				AssertEquals("ACT", errorMessage, AuthorisationNumberValidationHelper.GetAuthorisationNumberInvalidFormatMessage("ABC123", CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir));
			});
		}

		public void TestIsAuthorisationNumberValid()
		{
			CombineAssertions(() =>
			{
				Assert("Invalid Length", !AuthorisationNumberValidationHelper.IsAuthorisationNumberValid("DEACE1234ZE00012", CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit));
				Assert("Invalid Country", !AuthorisationNumberValidationHelper.IsAuthorisationNumberValid("FRACE1234ZE000123", CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit));
				Assert("ACE Invalid Type", !AuthorisationNumberValidationHelper.IsAuthorisationNumberValid("DEACT1234ZE000123", CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit));
				Assert("ACT Invalid Type", !AuthorisationNumberValidationHelper.IsAuthorisationNumberValid("DEACE1234ZT000123", CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir));
				Assert("Invalid Office", !AuthorisationNumberValidationHelper.IsAuthorisationNumberValid("DEACE12X4ZE000123", CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit));
				Assert("ACE Invalid Code", !AuthorisationNumberValidationHelper.IsAuthorisationNumberValid("DEACE1234ZT000123", CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit));
				Assert("ACT Invalid Code", !AuthorisationNumberValidationHelper.IsAuthorisationNumberValid("DEACT1234ZE000123", CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir));
				Assert("Invalid Sequence", !AuthorisationNumberValidationHelper.IsAuthorisationNumberValid("DEACE1234ZE000Z23", CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit));
				Assert("Valid ACE", AuthorisationNumberValidationHelper.IsAuthorisationNumberValid("DEACE1234ZE000123", CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit));
				Assert("Valid ACT", AuthorisationNumberValidationHelper.IsAuthorisationNumberValid("DEACT1234ZT000123", CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir));
				Assert("Valid Other", AuthorisationNumberValidationHelper.IsAuthorisationNumberValid("ABC123", "XXX"));
				Assert("Valid Empty", AuthorisationNumberValidationHelper.IsAuthorisationNumberValid(ZString.Empty, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit));
			});
		}
	}
}
