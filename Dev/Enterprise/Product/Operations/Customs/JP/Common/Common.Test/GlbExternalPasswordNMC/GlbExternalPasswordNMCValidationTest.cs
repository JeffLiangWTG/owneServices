using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing;

[TestedType(typeof(GlbExternalPasswordNMCValidation))]
sealed class GlbExternalPasswordNMCValidationTest : GlbExternalPasswordValidationTest<GlbExternalPasswordNMC, GlbExternalPasswordNMCValidation>
{
	public void TestCheckGP_MailBoxID_NACCS()
	{
		var expectMessageError = "Mailbox must contain only capitalized characters or numbers.";

		var validEmails = new[]
		{
			"DJP0001",
			"UPPERCASE",
			"UPPERCASE123",
			"123",
		};

		foreach (var email in validEmails)
		{
			GlbExternalPassword.GP_MailBoxID = email;
			AssertNoError(GlbExternalPassword.GP_MailBoxIDInfo, expectMessageError);
		}

		var invalidEmails = new[]
		{
			"randomstring",
			"@domain.com",
			"user@",
			"abc",
			"neo@domain123.com",
		};

		foreach (var email in invalidEmails)
		{
			GlbExternalPassword.GP_MailBoxID = email;
			AssertHasError(GlbExternalPassword.GP_MailBoxIDInfo, expectMessageError);
		}

		GlbExternalPassword.GP_MailBoxID = "";
		AssertNoError(GlbExternalPassword.GP_MailBoxIDInfo, expectMessageError);

		GlbExternalPassword.CurrentDecryptedPassword = "123";
		GlbExternalPassword.Validation.ValidateGP_MailBoxID();
		AssertHasError(GlbExternalPassword.GP_MailBoxIDInfo, expectMessageError);
	}

	public void TestCheckCurrentDecryptedPassword()
	{
		var expectMessageError = "Please enter a valid password when the Mailbox is not empty.";
		GlbExternalPassword.GP_MailBoxID = "UPPERCASE";
		GlbExternalPassword.CurrentDecryptedPassword = "12345678";
		AssertNoError(GlbExternalPassword.CurrentDecryptedPasswordInfo, expectMessageError);

		GlbExternalPassword.CurrentDecryptedPassword = "";
		AssertHasError(GlbExternalPassword.CurrentDecryptedPasswordInfo, expectMessageError);
	}

	protected override GlbExternalPasswordNMC CreateNewGlbExternalPassword() => Factory.New<GlbExternalPasswordNMC>();
}
