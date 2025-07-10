using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(GlbMauExternalPasswordValidation))]
sealed class GlbMauExternalPasswordValidationTest : GlbExternalPasswordValidationTest<GlbMauExternalPassword, GlbMauExternalPasswordValidation>
{
	public void TestCheckGP_UserID_MandatoryValidation()
	{
		var externalPassword = GetExternalPassword();

		CombineAssertions(() =>
		{
			externalPassword.GP_UserID = "";
			AssertHasErrorContaining(externalPassword.GP_UserIDInfo, MandatoryValidation.MustBeEntered);

			externalPassword.GP_UserID = "INTCODE0";
			AssertNoErrorContaining(externalPassword.GP_UserIDInfo, MandatoryValidation.MustBeEntered);
		});
	}

	public void TestCheckGP_UserID_MustBeUniqueValidation()
	{
		const string expectedError = "Internal Code must be unique for this Company.";

		var externalPassword1 = GetExternalPassword();
		var companyWrapper = GlbCompanyWrapper.Get(externalPassword1.Company);
		var externalPassword2 = companyWrapper.PasswordCollection.AddNew();

		CombineAssertions("When there are duplicate internal codes", () =>
		{
			externalPassword1.GP_UserID = "INTCODE1";
			externalPassword2.GP_UserID = "INTCODE1";
			externalPassword1.Validation.ValidateGP_UserID();
			AssertHasErrorContaining(externalPassword1.GP_UserIDInfo, expectedError);
			AssertHasErrorContaining(externalPassword2.GP_UserIDInfo, expectedError);
		});

		CombineAssertions("When there are no duplicate internal codes", () =>
		{
			externalPassword1.GP_UserID = "INTCODE1";
			externalPassword2.GP_UserID = "INTCODE2";
			externalPassword1.Validation.ValidateGP_UserID();
			AssertNoErrorContaining(externalPassword1.GP_UserIDInfo, expectedError);
			AssertNoErrorContaining(externalPassword2.GP_UserIDInfo, expectedError);
		});
	}

	public void TestCheckGP_UserID_AllowedCharsValidation()
	{
		const string expectedFormatErrorMessage = "Allow only chars [A-Z][0-9][-_]";

		var externalPassword = GetExternalPassword();

		CombineAssertions(() =>
		{
			externalPassword.GP_UserID = "aaA";
			AssertHasErrorContaining(externalPassword.GP_UserIDInfo, expectedFormatErrorMessage);

			externalPassword.GP_UserID = "?<>";
			AssertHasErrorContaining(externalPassword.GP_UserIDInfo, expectedFormatErrorMessage);

			externalPassword.GP_UserID = "A G";
			AssertHasErrorContaining(externalPassword.GP_UserIDInfo, expectedFormatErrorMessage);

			externalPassword.GP_UserID = "AAAA-01";
			AssertNoErrorContaining(externalPassword.GP_UserIDInfo, expectedFormatErrorMessage);

			externalPassword.GP_UserID = "_AAA-90";
			AssertHasErrorContaining(externalPassword.GP_UserIDInfo, expectedFormatErrorMessage);

			externalPassword.GP_UserID = "1LEOPARDO";
			AssertNoErrorContaining(externalPassword.GP_UserIDInfo, expectedFormatErrorMessage);

			externalPassword.GP_UserID = "A";
			AssertNoErrorContaining(externalPassword.GP_UserIDInfo, expectedFormatErrorMessage);
		});
	}

	public void TestCheckGP_UserID_MinLenghtValidation()
	{
		const string expectedError = "Internal Code must be at least 5 chars length.";

		var externalPassword = GetExternalPassword();

		CombineAssertions(() =>
		{
			externalPassword.GP_UserID = "ABCD";
			AssertHasErrorContaining(externalPassword.GP_UserIDInfo, expectedError);

			externalPassword.GP_UserID = "ABCDE";
			AssertNoErrorContaining(externalPassword.GP_UserIDInfo, expectedError);
		});
	}

	public void TestCheckGP_Name_MandatoryValidation()
	{
		var externalPassword = GetExternalPassword();

		CombineAssertions(() =>
		{
			externalPassword.GP_Name = "";
			AssertHasErrorContaining(externalPassword.GP_NameInfo, MandatoryValidation.MustBeEntered);

			externalPassword.GP_Name = "1234567890";
			AssertNoErrorContaining(externalPassword.GP_NameInfo, MandatoryValidation.MustBeEntered);
		});
	}

	public void TestCheckGP_Name_ListValidation()
	{
		var declarant = Factory.NewWithValidTestData<OrgHeader>();

		var externalPassword = GetExternalPassword();

		CombineAssertions(() =>
		{
			externalPassword.GP_Name = "123456";
			AssertHasErrorContaining(externalPassword.GP_NameInfo, ListValidation.InvalidCodeError);

			externalPassword.GP_Name = declarant.OH_Code;
			AssertNoErrorContaining(externalPassword.GP_NameInfo, ListValidation.InvalidCodeError);
		});
	}

	public void TestCheckGP_MailBoxID_MandatoryValidation()
	{
		var externalPassword = GetExternalPassword();

		CombineAssertions(() =>
		{
			externalPassword.GP_MailBoxID = "";
			AssertHasErrorContaining(externalPassword.GP_MailBoxIDInfo, MandatoryValidation.MustBeEntered);

			externalPassword.GP_MailBoxID = "1234567890";
			AssertNoErrors(externalPassword.GP_MailBoxIDInfo);
		});
	}

	public void TestCheckGP_MailBoxID_FormatValidation_VAT()
	{
		const string expectedMessageError = "Authorized User should be a valid VAT or fiscal code.";

		var externalPassword = GetExternalPassword();
		CombineAssertions(() =>
		{
			externalPassword.GP_MailBoxID = "008912301536";
			AssertHasMessageErrorContaining(externalPassword.GP_MailBoxIDInfo, expectedMessageError);

			externalPassword.GP_MailBoxID = "00891230153";
			AssertNoMessageErrorContaining(externalPassword.GP_MailBoxIDInfo, expectedMessageError);

			externalPassword.GP_MailBoxID = "00891230155";
			AssertNoMessageErrorContaining(externalPassword.GP_MailBoxIDInfo, expectedMessageError);
		});
	}

	public void TestCheckGP_MailBoxID_FormatValidation_FiscalCode()
	{
		const string expectedMessageError = "Authorized User should be a valid VAT or fiscal code.";

		var externalPassword = GetExternalPassword();
		CombineAssertions(() =>
		{
			externalPassword.GP_MailBoxID = "AAAAAA12A45A678AA";
			AssertHasMessageErrorContaining(externalPassword.GP_MailBoxIDInfo, expectedMessageError);

			externalPassword.GP_MailBoxID = "AAAAAA12A45A678A";
			AssertNoMessageErrorContaining(externalPassword.GP_MailBoxIDInfo, expectedMessageError);
		});
	}

	protected override GlbMauExternalPassword GetExternalPassword()
	{
		var company = Factory.NewWithValidTestData<GlbCompany>();
		var companyWrapper = GlbCompanyWrapper.Get(company);
		return companyWrapper.PasswordCollection.AddNew();
	}
}
