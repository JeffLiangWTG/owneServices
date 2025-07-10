using System.Reflection;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(GlbExternalPasswordWithCertificateValidation))]
	sealed class GlbExternalPasswordWithCertificateValidationTest : MasterFiles.Business.Testing.GlbExternalPasswordWithCertificateValidationTest<GlbILExternalPassword, GlbExternalPasswordWithCertificateValidation>
	{
		public void TestIsCurrentDecryptedCertificatePassphraseMandatory()
		{
			var parent = Factory.New<GlbILExternalPassword>();
			var glbExternalPasswordWithCertificateValidation = new GlbExternalPasswordWithCertificateValidation(parent);

			var propertyInfo = typeof(GlbExternalPasswordWithCertificateValidation)
				.GetProperty("IsCurrentDecryptedCertificatePassphraseMandatory", BindingFlags.NonPublic | BindingFlags.Instance);
			var isCurrentDecryptedCertificatePassphraseMandatory = (bool)propertyInfo.GetValue(glbExternalPasswordWithCertificateValidation);

			AssertEquals("IsCurrentDecryptedCertificatePassphraseMandatory", false, isCurrentDecryptedCertificatePassphraseMandatory);
		}

		public void TestCheckGP_UserID()
		{
			var staffWithoutPassword = Factory.NewWithValidTestData<GlbStaff>();
			staffWithoutPassword.GS_Code = "AAA";
			staffWithoutPassword.CompanyName = GlbCompany.CurrentCompany.CompanyName;

			var staffWithPassword = Factory.NewWithValidTestData<GlbStaff>();
			staffWithPassword.GS_Code = "BBB";
			staffWithPassword.CompanyName = GlbCompany.CurrentCompany.CompanyName;
			var pwd = Factory.New<GlbILStaffExternalPassword>();
			pwd.GP_CertificateAuthority = "COM";
			pwd.CurrentDecryptedPassword = "1234";
			pwd.GP_GS = staffWithPassword.PK;

			Factory.Save();

			var parent = Factory.New<GlbILExternalPassword>();
			var glbExternalPasswordWithCertificateValidation = new GlbExternalPasswordWithCertificateValidation(parent);
			parent.GP_UserID = "CCC";
			AssertHasMessageError("There is error message when user does not exist", parent.GP_UserIDInfo, "The selected user either does not exist or does not have certificate details in the staff record.");
			parent.GP_UserID = "AAA";
			AssertHasMessageError("There is error message when user does not have password", parent.GP_UserIDInfo, "The selected user either does not exist or does not have certificate details in the staff record.");
			parent.GP_UserID = "BBB";
			AssertNoMessageError("There is no error message when user satisfies all conditions", parent.GP_UserIDInfo, "The selected user either does not exist or does not have certificate details in the staff record.");
		}

		protected override bool IsCertificateMandatory => false;
	}
}
