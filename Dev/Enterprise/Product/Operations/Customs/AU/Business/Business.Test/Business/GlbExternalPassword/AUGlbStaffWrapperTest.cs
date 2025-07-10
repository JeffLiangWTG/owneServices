using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Customs.AU;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AUGlbStaffWrapper))]
	sealed class AUGlbStaffWrapperTest : MasterFiles.Business.Testing.GlbStaffWrapperTest<AUGlbStaffWrapper>
	{
		public void TestIGlbStaffWrapperMembers()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var wrapper = AUGlbStaffWrapper.Get(staff);
			IAUGlbStaffWrapper iWrapper = wrapper;
			AssertEquals(wrapper.NUTPassword, iWrapper.NUTPassword);
		}

		public void TestNUTPassword()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var password = Factory.NewWithValidTestData<GlbExternalPassword_NUT>();
			password.GP_GC = GlbCompany.CurrentCompany.PK;
			password.GP_GS = staff1.PK;
			password.GP_PasswordStatus = Core.Constants.PasswordOK;

			Factory.Save();

			var wrapper1 = AUGlbStaffWrapper.Get(staff1);
			AssertEquals(password.PK, wrapper1.NUTPassword.PK);
			AssertEquals("HasValidCredential = true", true, wrapper1.NUTPassword.HasValidCredential);
			staff1.GS_IsActive = false;
			Factory.Save();
			AssertEquals("HasValidCredential = false when staff is deactivated", false, wrapper1.NUTPassword.HasValidCredential);

			var wrapper2 = AUGlbStaffWrapper.Get(staff2);
			var password2 = wrapper2.NUTPassword;
			Assert(!password2.IsInDatabase);
			Assert(!password2.HasChanges);
			AssertEquals(staff2.PK, password2.GP_GS);
			AssertEquals(GlbCompany.CurrentCompany.PK, password2.GP_GC);
			AssertEquals(PasswordTypesList.Codes.NUT, password2.GP_PasswordType);
		}

		protected override AUGlbStaffWrapper CreateNewWrapper(GlbStaff staff) => AUGlbStaffWrapper.Get(staff);
	}
}
