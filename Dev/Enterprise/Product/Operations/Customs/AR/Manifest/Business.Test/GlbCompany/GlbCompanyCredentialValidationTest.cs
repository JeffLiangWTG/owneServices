using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	[TestedType(typeof(GlbCompanyCredentialValidation))]
	class GlbCompanyCredentialValidationTest : GlbExternalPasswordWithCertificateValidationTest<GlbCompanyCredential, GlbCompanyCredentialValidation>
	{
		public void TestCheckCurrentDecryptedCertificatePassphrase()
		{
			var credential = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(Factory.New<GlbCompany>()).GlbExternalPassword;

			credential.CurrentDecryptedCertificatePassphrase = ZString.Empty;
			AssertHasMessageError(credential.CurrentDecryptedCertificatePassphraseInfo, "You have not entered a value.");

			credential.CurrentDecryptedCertificatePassphrase = "1234567890";
			AssertNoNotifications(credential.CurrentDecryptedCertificatePassphraseInfo);
			Assert(!credential.CurrentDecryptedCertificatePassphraseInfo.HasMessageError("Password must be less than 11 characters."));
		}

		protected override bool IsCertificateMandatory => false;
	}
}
