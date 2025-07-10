using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(GlbStaffWrapper))]
sealed class GlbStaffWrapperTest : MasterFiles.Business.Testing.GlbStaffWrapperTest<GlbStaffWrapper>
{
	public void TestGetWrapperForCurrentUser()
	{
		var expectedWrapper = GlbStaff.CurrentUser.GetINWrapper();
		AssertEquals(expectedWrapper, GlbStaffWrapper.GetWrapperForCurrentUser());
	}

	public void TestLoginPassword()
	{
		var password = Wrapper.LoginPassword;
		CombineAssertions(() =>
		{
			AssertEquals("Password Type", "INC", password.GP_PasswordType);
			AssertEquals("Staff", Staff.PK, password.GP_GS);
			AssertEquals("Company", GlbCompany.CurrentCompany.PK, password.GP_GC);
		});
	}

	public void TestGetLoginPassword()
	{
		CombineAssertions(() =>
		{
			AssertNull("Login Password not set", Wrapper.GetLoginPassword());

			var password = Wrapper.LoginPassword;
			AssertEquals("Login password is set", password, Wrapper.GetLoginPassword());

			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			otherCompany.GC_Code = "AAA";
			otherCompany.GC_Name = "AAA Company";
			otherCompany.GC_RN_NKCountryCode = "AU";
			otherCompany.GC_RX_NKLocalCurrency = "AUD";
			var otherBranch = otherCompany.Branches.AddNew();
			otherBranch.GB_Code = "ABR";
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, otherBranch.PK.ToGuid(), department.PK.ToGuid()))
			{
				AssertNull("Login password is not set in another company", Wrapper.GetLoginPassword());
			}
		});
	}

	public void TestCertificatePassword()
	{
		var password = Wrapper.CertificatePassword;
		CombineAssertions(() =>
		{
			AssertEquals("Password Type", "INX", password.GP_PasswordType);
			AssertEquals("Staff", Staff.PK, password.GP_GS);
			AssertEquals("Company", GlbCompany.CurrentCompany.PK, password.GP_GC);
		});
	}

	public void TestGetCertificatePassword()
	{
		CombineAssertions(() =>
		{
			AssertNull("Certificate Password not set", Wrapper.GetCertificatePassword());

			var password = Wrapper.CertificatePassword;
			AssertEquals("Certificate password is set", password, Wrapper.GetCertificatePassword());

			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			otherCompany.GC_Code = "AAA";
			otherCompany.GC_Name = "AAA Company";
			otherCompany.GC_RN_NKCountryCode = "AU";
			otherCompany.GC_RX_NKLocalCurrency = "AUD";
			var otherBranch = otherCompany.Branches.AddNew();
			otherBranch.GB_Code = "ABR";
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, otherBranch.PK.ToGuid(), department.PK.ToGuid()))
			{
				AssertNull("Certificate Password not set in other country", Wrapper.GetCertificatePassword());
			}
		});
	}

	protected override GlbStaffWrapper CreateNewWrapper(GlbStaff staff)
	{
		return GlbStaffWrapper.Get(staff);
	}
}
