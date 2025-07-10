using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Business.Testing
{
	[TestedType(typeof(GlbExternalPassword_MXL))]
	class GlbExternalPassword_MXLTest : GlbExternalPasswordTest<GlbExternalPassword_MXL>
	{
		public void TestHumanReadableNameCore()
		{
			var staff = Factory.New<GlbStaff>();
			var wrapper = MXGlbStaffWrapper.Get(staff);
			var externalPasswordBr = wrapper.StaffLicenses.AddNew();
			AssertEquals("HumanReadableNameCore for MXL", "Staff License", externalPasswordBr.HumanReadableName);
		}

		public void TestSetDefaultValues()
		{
			var password = Factory.New<GlbExternalPassword_MXL>();
			AssertEquals("MXL", password.GP_PasswordType);
		}

		public void TestGP_UserID()
		{
			Assert(GlbExternalPassword.GP_UserID.IsEmpty);

			GlbExternalPassword.GP_CertificateAuthority = "123";
			GlbExternalPassword.GP_UserID = "4567";
			AssertEquals("4567", GlbExternalPassword.GP_UserID);

			GlbExternalPassword.GP_CertificateAuthority = "789";
			AssertEquals("4567", GlbExternalPassword.GP_UserID);

			GlbExternalPassword.GP_CertificateAuthority = ZString.Empty;
			Assert(GlbExternalPassword.GP_UserID.IsEmpty);
		}

		public void TestGP_UserIDReadOnly()
		{
			GlbExternalPassword.GP_CertificateAuthority = string.Empty;
			Assert("GP_UserIDInfo.ReadOnly should be TRUE", GlbExternalPassword.GP_UserIDInfo.ReadOnly);
			GlbExternalPassword.GP_CertificateAuthority = "123";
			Assert("GP_UserIDInfo.ReadOnly should be FALSE", !GlbExternalPassword.GP_UserIDInfo.ReadOnly);
		}
	}
}
