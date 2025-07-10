using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(GlbCompanyCredentialValidation))]
	sealed class GlbCompanyCredentialValidationTest : GlbExternalPasswordWithCertificateValidationTest<GlbCompanyCredential, GlbCompanyCredentialValidation>
	{
		public void TestCheckGP_Name()
		{
			var credential = Factory.New<GlbCompanyCredential>();
			credential.GP_Name = "";
			AssertNoErrorContaining(credential.GP_NameInfo, MandatoryValidation.MustBeEntered);

			credential.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			credential.Validation.ValidateGP_Name();
			AssertHasErrorContaining(credential.GP_NameInfo, MandatoryValidation.MustBeEntered);

			credential.GP_Name = "AAAAAA";
			AssertNoErrorContaining(credential.GP_NameInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckGP_MailBoxID()
		{
			var credential = Factory.New<GlbCompanyCredential>();
			credential.GP_MailBoxID = "";
			AssertNoErrorContaining(credential.GP_MailBoxIDInfo, MandatoryValidation.MustBeEntered);

			credential.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			credential.Validation.ValidateGP_MailBoxID();
			AssertHasErrorContaining(credential.GP_MailBoxIDInfo, MandatoryValidation.MustBeEntered);

			credential.GP_MailBoxID = "ABC123";
			AssertNoErrorContaining(credential.GP_MailBoxIDInfo, MandatoryValidation.MustBeEntered);
		}

		protected override bool IsCertificateMandatory => false;
	}
}
