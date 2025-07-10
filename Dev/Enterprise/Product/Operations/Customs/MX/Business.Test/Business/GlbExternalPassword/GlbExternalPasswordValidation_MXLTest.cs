using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Business.Testing
{
	[TestedType(typeof(GlbExternalPasswordValidation_MXL))]
	class GlbExternalPasswordValidation_MXLTest : MasterFiles.Business.Testing.GlbExternalPasswordValidationTest<GlbExternalPassword_MXL, GlbExternalPasswordValidation_MXL>
	{
		public void TestCheckGP_CertificateAuthority()
		{
			Factory.New<OrgHeader>().OH_Code = "DEC1";
			Factory.Save();
			var staff = Factory.New<GlbStaff>();
			var staffWrapper = MXGlbStaffWrapper.Get(staff);
			var externalPassword1 = staffWrapper.StaffLicenses.AddNew();

			externalPassword1.GP_CertificateAuthority = "";
			AssertNoNotifications(externalPassword1.GP_CertificateAuthorityInfo);

			externalPassword1.GP_CertificateAuthority = "A12";
			AssertHasError(externalPassword1.GP_CertificateAuthorityInfo, "Customs Area must only contain numeric characters.");

			externalPassword1.GP_CertificateAuthority = "123";
			AssertNoNotifications(externalPassword1.GP_CertificateAuthorityInfo);

			var externalPassword2 = staffWrapper.StaffLicenses.AddNew();
			externalPassword2.GP_CertificateAuthority = "123";
			externalPassword1.Validation.ValidateGP_CertificateAuthority();
			AssertHasError(externalPassword1.GP_CertificateAuthorityInfo, "The same Customs Area cannot be entered twice.");
			AssertHasError(externalPassword2.GP_CertificateAuthorityInfo, "The same Customs Area cannot be entered twice.");

			externalPassword2.GP_CertificateAuthority = "456";
			externalPassword1.Validation.ValidateGP_CertificateAuthority();
			AssertNoNotifications(externalPassword1.GP_CertificateAuthorityInfo);
			AssertNoNotifications(externalPassword2.GP_CertificateAuthorityInfo);
		}

		public void TestCheckGP_UserID()
		{
			Factory.New<OrgHeader>().OH_Code = "DEC1";
			Factory.Save();
			var staff = Factory.New<GlbStaff>();
			var staffWrapper = MXGlbStaffWrapper.Get(staff);
			var externalPassword1 = staffWrapper.StaffLicenses.AddNew();

			externalPassword1.GP_CertificateAuthority = "123";
			externalPassword1.GP_UserID = "";
			AssertNoNotifications(externalPassword1.GP_UserIDInfo);

			externalPassword1.GP_UserID = "A123";
			AssertHasError(externalPassword1.GP_UserIDInfo, "Number must only contain numeric characters.");

			externalPassword1.GP_UserID = "123";
			AssertHasError(externalPassword1.GP_UserIDInfo, "Number has less than 4 characters.");

			externalPassword1.GP_UserID = "1234";
			AssertNoNotifications(externalPassword1.GP_UserIDInfo);

			var externalPassword2 = staffWrapper.StaffLicenses.AddNew();
			externalPassword2.GP_UserID = "1234";
			externalPassword1.Validation.ValidateGP_UserID();
			AssertHasError(externalPassword1.GP_UserIDInfo, "The same Customs Area cannot be entered twice for the same Number.");
			AssertHasError(externalPassword2.GP_UserIDInfo, "The same Customs Area cannot be entered twice for the same Number.");

			externalPassword2.GP_UserID = "5678";
			externalPassword1.Validation.ValidateGP_UserID();
			AssertNoNotifications(externalPassword1.GP_UserIDInfo);
			AssertNoNotifications(externalPassword2.GP_UserIDInfo);
		}
	}
}
