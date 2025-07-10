using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.MasterFiles.Integration.CustomsIntegration.IT;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(GlbCompanyWrapper))]
sealed class GlbCompanyWrapperTest : MasterFiles.Business.Testing.GlbCompanyWrapperTest<GlbCompanyWrapper>
{
	public void TestIGlbCompanyWrapperMembers()
	{
		Wrapper.ToString();
		var company = Factory.NewWithValidTestData<GlbCompany>();
		var wrapper = GlbCompanyWrapper.Get(company);
		IGlbCompanyWrapper iWrapper = wrapper;
		AssertEquals(wrapper.PasswordCollection, iWrapper.PasswordCollection);
	}

	public void TestITMPasswordCollection()
	{
		var company1 = Factory.NewWithValidTestData<GlbCompany>();
		company1.GC_Code = "DK@";
		var password1 = CreateExternalPassword("1", company1);
		var password2 = CreateExternalPassword("2", company1);
		CreateExternalPassword("3", GlbCompany.CurrentCompany);
		CreateExternalPassword("4", GlbCompany.CurrentCompany);

		Factory.Save();

		var factory = new BusinessObjectFactory();
		company1 = factory.Load<GlbCompany>(company1.PK);
		password1 = factory.Load<GlbMauExternalPassword>(password1.PK);
		password2 = factory.Load<GlbMauExternalPassword>(password2.PK);
		var wrapper1 = GlbCompanyWrapper.Get(company1);

		AssertEquals(2, wrapper1.PasswordCollection.Count);
		AssertCollectionContains(password1, wrapper1.PasswordCollection);
		AssertCollectionContains(password2, wrapper1.PasswordCollection);
	}

	public void TestIsValidWrapper()
	{
		var company = Factory.NewWithValidTestData<GlbCompany>();
		company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;

		var wrapper = GetWrapper(company);
		Assert("Should be true as the country code is IT.", wrapper.IsValidWrapper);
	}

	public void TestGetCachedValue()
	{
		var company = Factory.NewWithValidTestData<GlbCompany>();
		var wrapper1 = MasterFiles.Business.GlbCompanyWrapper.GetWrapper<MasterFiles.Business.GlbCompanyWrapper>(company);
		var wrapper2 = MasterFiles.Business.GlbCompanyWrapper.GetWrapper<MasterFiles.Business.GlbCompanyWrapper>(company);
		AssertSame(wrapper1, wrapper2);
	}

	GlbExternalPassword CreateExternalPassword(string userId, GlbCompany company)
	{
		var password = Factory.NewWithValidTestData<GlbMauExternalPassword>();
		password.GP_UserID = userId;
		password.GP_GC = company.PK;
		return password;
	}
}
