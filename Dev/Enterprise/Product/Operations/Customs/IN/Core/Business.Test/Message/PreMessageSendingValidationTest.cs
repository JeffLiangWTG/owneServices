using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(PreMessageSendingValidation))]
sealed class PreMessageSendingValidationTest : TestCaseWithFactory
{
	public void TestValidateMessageSending_StaffCredentials()
	{
		const string chipset = "WatchData";
		const string serialNumber = "XYZ-123";
		const string userID = "icguser123";
		const string emailID = "icguser@ic.wtg.in";
		const string loginIdEmailIdTokenMissingMessage = "ICEGATE Login ID and Email ID and DSC Token Profile is not defined for current user under Staff & Resources Credentials.";
		const string loginIdEmailIdMissingMessage = "ICEGATE Login ID and Email ID are not defined for current user under Staff & Resources Credentials.";
		const string loginIdMissingMessage = "ICEGATE Login ID is not defined for current user.";
		const string emailIdMissingMessage = "ICEGATE Email ID is not defined for current user.";
		const string tokenMissingMessage = "DSC Token Profile is not setup for current user.";

		RefDataSetupTestHelper.SetupCertificateTokenData(Factory);

		var staffWrapper = GlbStaffWrapper.GetWrapperForCurrentUser();
		AssertNull("Pre Condition: Certificate not set", staffWrapper.GetCertificatePassword());
		AssertEquals("No Error when Certificate not set for CWSupport", ZString.Empty, Validation.ValidateMessageSending());

		CombineAssertions(() =>
		{
			GlbStaff.CurrentUser.GS_LoginName = "Enterprise User";

			AssertEquals("Icegate details not setup", loginIdEmailIdTokenMissingMessage, Validation.ValidateMessageSending());

			var certificatePassword = staffWrapper.CertificatePassword;
			certificatePassword.GP_Name = chipset;
			certificatePassword.GP_CertificateSerialNumber = serialNumber;
			AssertEquals("Icegate login id and email id missing", loginIdEmailIdMissingMessage, Validation.ValidateMessageSending());

			var loginPassword = staffWrapper.LoginPassword;
			loginPassword.GP_UserID = userID;
			AssertEquals("Icegate email id missing", emailIdMissingMessage, Validation.ValidateMessageSending());

			loginPassword.GP_UserID = ZString.Empty;
			loginPassword.GP_MailBoxID = emailID;
			AssertEquals("Icegate login id missing", loginIdMissingMessage, Validation.ValidateMessageSending());

			loginPassword.GP_UserID = userID;
			certificatePassword.GP_Name = chipset;
			certificatePassword.GP_CertificateSerialNumber = ZString.Empty;
			AssertEquals("Certificate Serial Number is empty", tokenMissingMessage, Validation.ValidateMessageSending());

			certificatePassword.GP_Name = "XYZ";
			certificatePassword.GP_CertificateSerialNumber = serialNumber;
			AssertEquals("Chipset Name is empty", tokenMissingMessage, Validation.ValidateMessageSending());

			certificatePassword.GP_Name = chipset;
			certificatePassword.GP_CertificateSerialNumber = serialNumber;
			AssertEquals("Valid Icegate details", ZString.Empty, Validation.ValidateMessageSending());
		});
	}

	PreMessageSendingValidation Validation => validation ??= new PreMessageSendingValidation();
	PreMessageSendingValidation validation;
}
