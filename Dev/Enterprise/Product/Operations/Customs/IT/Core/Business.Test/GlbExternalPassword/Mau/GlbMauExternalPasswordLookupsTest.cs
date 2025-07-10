using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class GlbMauExternalPasswordLookupsTest : GlbExternalPasswordLookupsTest<GlbMauExternalPassword>
{
	public void TestDeclarants()
	{
		var externalPassword = GetExternalPassword();
		AssertType<OrgHeaderCollection>("Declarants Type", externalPassword.Lookups.Declarants);
	}

	protected override GlbMauExternalPassword GetExternalPassword()
	{
		var company = Factory.NewWithValidTestData<GlbCompany>();
		var companyWrapper = GlbCompanyWrapper.Get(company);
		return companyWrapper.PasswordCollection.AddNew();
	}
}
