using CargoWise.Types;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(GlbCompanyCredentialValidation))]
	public class GlbCompanyCredentialValidationTest : GlbExternalPasswordWithCertificateValidationTest<GlbCompanyCredential, GlbCompanyCredentialValidation>
	{
		public void TestCheckGP_MailBoxID()
		{
			GlbExternalPassword.CurrentDecryptedCertificatePassphrase = ZString.Empty;
			GlbExternalPassword.GP_MailBoxID = ZString.Empty;
			var info = GlbExternalPassword.GP_MailBoxIDInfo;
			AssertNoErrors(info);
			GlbExternalPassword.GP_Certificate = ValidCertificate;
			GlbExternalPassword.CurrentDecryptedCertificatePassphrase = ValidPassword;
			GlbExternalPassword.Validation.ValidateGP_MailBoxID();
			var mustEntereMessage = "Please enter a Message Sender EORI.";
			var invaliEORIMessage = "Please enter a valid EORI format: A member state or third country code [a2] plus a unique identifier [an..15]";
			AssertHasError(info, mustEntereMessage);
			AssertNoMessageError(info, invaliEORIMessage);
			GlbExternalPassword.GP_MailBoxID = "!@SDFD";
			AssertNoError(info, mustEntereMessage);
			AssertHasMessageError(info, invaliEORIMessage);
			GlbExternalPassword.GP_MailBoxID = "IE123456789123456";
			AssertNoError(info, mustEntereMessage);
			AssertNoMessageError(info, invaliEORIMessage);
		}

		protected override byte[] ValidCertificate => ROSCertificateTestHelper.ValidCertificate;
		protected override string ValidPassword => ROSCertificateTestHelper.ValidPassword;
		protected override bool IsCertificateMandatory => false;
	}
}
