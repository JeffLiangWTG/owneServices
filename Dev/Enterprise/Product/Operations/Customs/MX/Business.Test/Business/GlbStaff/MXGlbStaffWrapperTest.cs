using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Business.Testing
{
	[TestedType(typeof(MXGlbStaffWrapper))]
	class MXGlbStaffWrapperTest : GlbStaffWrapperTest<MXGlbStaffWrapper>
	{
		protected override MXGlbStaffWrapper CreateNewWrapper(GlbStaff staff)
		{
			return MXGlbStaffWrapper.Get(staff);
		}

		public void TestMXGlbStaffWrapper()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var wrapper = MXGlbStaffWrapper.Get(staff);
			AssertSame(wrapper, MXGlbStaffWrapper.Get(staff));
		}

		public void TestStaffLicenses()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "DK@";
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "DK1";
			var wrapper = MXGlbStaffWrapper.Get(staff1);
			var password1 = wrapper.StaffLicenses.AddNew();
			password1.GP_GS = staff1.PK;
			password1.GP_CertificateAuthority = "111";
			password1.GP_UserID = "1234";
			password1.GP_GC = company1.PK;
			var password2 = wrapper.StaffLicenses.AddNew();
			password2.GP_GS = staff1.PK;
			password2.GP_CertificateAuthority = "222";
			password2.GP_UserID = "5678";
			password2.GP_GC = company1.PK;
			var password3 = wrapper.StaffLicenses.AddNew();
			password3.GP_GS = staff1.PK;
			password3.GP_CertificateAuthority = "333";
			password3.GP_UserID = "9012";
			password3.GP_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();
			AssertEquals(3, wrapper.StaffLicenses.Count);
			AssertContainsExactElementsInAnyOrder(new[] { password1, password2, password3 }, wrapper.StaffLicenses);
		}
	}
}
