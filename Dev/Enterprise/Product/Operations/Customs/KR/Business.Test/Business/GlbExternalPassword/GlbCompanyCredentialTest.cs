using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(GlbCompanyCredential))]
	sealed class GlbCompanyCredentialTest : GlbExternalPasswordWithCertificateTest<GlbCompanyCredential>
	{
		public void TestValidationType()
		{
			AssertType<GlbCompanyCredentialValidation>(password.Validation);
		}

		public void TestCaptions()
		{
			AssertEquals("Mailbox", DataBoundResourceStrings.GetDataForProperty(password.GP_MailBoxIDInfo).Caption);
			AssertEquals("Sender ID", DataBoundResourceStrings.GetDataForProperty(password.GP_UserIDInfo).Caption);
			AssertEquals("Certificate Password", DataBoundResourceStrings.GetDataForProperty(password.CurrentDecryptedCertificatePassphraseInfo).Caption);
			AssertEquals("Status", DataBoundResourceStrings.GetDataForProperty(password.GP_PasswordStatusInfo).Caption);
			AssertEquals("Status Reason", DataBoundResourceStrings.GetDataForProperty(password.GP_StatusReasonInfo).Caption);
		}

		public void TestClearData()
		{
			password.GP_StatusReason = "Identification Error(신원확인오류)";

			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;

			AssertEquals("Status Reason has been cleared", ZString.Empty, password.GP_StatusReason);
		}

		public void TestChangeStatusWhenChangingUserIDAndMailBoxID()
		{
			password.GP_UserID = "AAAAAA";
			password.GP_MailBoxID = "BBBBBB";
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;

			password.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			password.GP_StatusReason = "Identification Error(신원확인오류)";
			password.GP_Name = "AAAAAP";
			AssertEquals(PasswordStatusList.Codes.Valid, password.GP_PasswordStatus);

			password.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			password.GP_StatusReason = "Identification Error(신원확인오류)";
			password.GP_MailBoxID = "BBBBBP";
			AssertEquals(PasswordStatusList.Codes.Valid, password.GP_PasswordStatus);

			password.CurrentDecryptedCertificatePassphrase = "TEST INVALID PASSWORD";
			password.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			password.GP_StatusReason = "Identification Error(신원확인오류)";
			password.GP_MailBoxID = "AAAAAA";
			AssertEquals(PasswordStatusList.Codes.Invalid, password.GP_PasswordStatus);
		}

		protected override void SetUp()
		{
			base.SetUp();
			password = Factory.New<GlbCompanyCredential>();
		}
		GlbCompanyCredential password;
	}
}
