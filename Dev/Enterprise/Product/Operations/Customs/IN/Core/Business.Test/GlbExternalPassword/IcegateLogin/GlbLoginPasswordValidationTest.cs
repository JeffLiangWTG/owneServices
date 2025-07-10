using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(GlbLoginPasswordValidation))]
sealed class GlbLoginPasswordValidationTest : GlbExternalPasswordValidationTest<GlbLoginPassword, GlbLoginPasswordValidation>
{
	public void TestCheckGP_MailBoxID()
	{
		var password = GlbExternalPassword;

		const string invalidEmail = "Invalid Email ID";
		const string invalidEmail2 = "<a@a.com>";

		CombineAssertions(() =>
		{
			password.GP_MailBoxID = invalidEmail;
			AssertHasMessageError("Invalid Email ID", password.GP_MailBoxIDInfo, $"{invalidEmail} is not a valid email address.");

			password.GP_MailBoxID = "test@test.com";
			AssertNoMessageErrors("Valid Email ID", password.GP_MailBoxIDInfo);

			password.GP_MailBoxID = invalidEmail2;
			AssertHasMessageError("Invalid Email ID", password.GP_MailBoxIDInfo, $"{invalidEmail2} is not a valid email address.");
		});
	}

	public void TestCheckCopyToMailBox()
	{
		var expectedMessage = "Email Address not found. Please update the email address under Staff Details.";
		var password = GlbExternalPassword;
		password.Validation.ValidateCopyToMailBox();
		AssertNoMessageError("Email missing in Staff Details and Copy not needed", password.CopyToMailBoxInfo, expectedMessage);

		password.NeedCopyOfEmails = true;
		AssertHasMessageError("Email missing in Staff Details and Copy needed", password.CopyToMailBoxInfo, expectedMessage);

		var email = Staff.EmailAddresses.AddNew();
		email.GSE_Type = Core.Constants.EmailFromAddressTypes.Codes.Main;
		email.GSE_EmailAddress = "user@wtg.in";
		password.Validation.ValidateCopyToMailBox();
		AssertNoMessageError("Email present in Staff Details and Copy needed", password.CopyToMailBoxInfo, expectedMessage);

		password.NeedCopyOfEmails = false;
		AssertNoMessageError("Email present in Staff Details and Copy not needed", password.CopyToMailBoxInfo, expectedMessage);
	}
}
