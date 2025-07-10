using CargoWise.Types;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(EMCSGlbCompanyCredentialValidation))]
	public class EMCSGlbCompanyCredentialValidationTest : GlbExternalPasswordWithCertificateValidationTest<EMCSGlbCompanyCredential, EMCSGlbCompanyCredentialValidation>
	{
		public void TestCheckGP_MailBoxID()
		{
			var expectedError = "Certificate Identifier must be unique.";
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var companyWrapper = GlbCompanyWrapper.Get(company);

			var externalPassword1 = companyWrapper.EMCSGlbExternalPasswordCollection.AddNew();
			externalPassword1.GP_MailBoxID = "Certificate Identifier1";

			var externalPassword2 = companyWrapper.EMCSGlbExternalPasswordCollection.AddNew();
			externalPassword2.GP_MailBoxID = "Certificate Identifier1";

			externalPassword1.Validation.ValidateGP_MailBoxID();
			externalPassword2.Validation.ValidateGP_MailBoxID();
			CombineAssertions(() =>
			{
				AssertHasErrorContaining(externalPassword1.GP_MailBoxIDInfo, expectedError);
				AssertHasErrorContaining(externalPassword2.GP_MailBoxIDInfo, expectedError);
			});

			externalPassword2.GP_MailBoxID = "Certificate Identifier2";
			externalPassword1.Validation.ValidateGP_MailBoxID();
			externalPassword2.Validation.ValidateGP_MailBoxID();
			CombineAssertions(() =>
			{
				AssertNoErrorContaining(externalPassword1.GP_MailBoxIDInfo, expectedError);
				AssertNoErrorContaining(externalPassword2.GP_MailBoxIDInfo, expectedError);
			});

			var externalPassword3 = companyWrapper.EMCSGlbExternalPasswordCollection.AddNew();
			externalPassword3.GP_MailBoxID = ZString.Empty;
			externalPassword3.Validation.ValidateGP_MailBoxID();
			AssertMandatoryValidationError(externalPassword3.GP_MailBoxIDInfo, true);
		}

		public void TestValidateDuplicateConstraint()
		{
			var expectedError = "Duplicate credential found; there is already another GlbExternalPassword with the same data.";
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var companyWrapper = GlbCompanyWrapper.Get(company);

			var externalPassword1 = companyWrapper.EMCSGlbExternalPasswordCollection.AddNew();
			externalPassword1.GP_PasswordType = "IEM";
			externalPassword1.GP_MailBoxID = "Certificate Identifier1";

			var externalPassword2 = companyWrapper.EMCSGlbExternalPasswordCollection.AddNew();
			externalPassword2.GP_PasswordType = "IEM";
			externalPassword2.GP_MailBoxID = "Certificate Identifier1";

			externalPassword1.Validation.ValidateDuplicateConstraint();
			externalPassword2.Validation.ValidateDuplicateConstraint();
			CombineAssertions(() =>
			{
				AssertHasRowErrorContaining(externalPassword1, expectedError);
				AssertHasRowErrorContaining(externalPassword2, expectedError);
			});

			externalPassword1.ClearRowNotifications();
			externalPassword2.ClearRowNotifications();
			externalPassword2.GP_MailBoxID = "Certificate Identifier2";
			externalPassword1.Validation.ValidateDuplicateConstraint();
			externalPassword2.Validation.ValidateDuplicateConstraint();
			CombineAssertions(() =>
			{
				AssertNoRowErrorContaining(externalPassword1, expectedError);
				AssertNoRowErrorContaining(externalPassword2, expectedError);
			});

			externalPassword2.GP_MailBoxID = "Certificate Identifier1";
			externalPassword2.GP_PasswordType = "IER";
			externalPassword1.Validation.ValidateDuplicateConstraint();
			externalPassword2.Validation.ValidateDuplicateConstraint();
			CombineAssertions(() =>
			{
				AssertNoRowErrorContaining(externalPassword1, expectedError);
				AssertNoRowErrorContaining(externalPassword2, expectedError);
			});
		}

		protected override byte[] ValidCertificate => ROSCertificateTestHelper.ValidCertificate;
		protected override string ValidPassword => ROSCertificateTestHelper.ValidPassword;
		protected override bool IsCertificateMandatory => false;
	}
}
