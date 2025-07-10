using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Testing
{
	class OrgCusAccountProviderTest : TestCaseWithFactory
	{
		public void TestOrgCusAccountProvider()
		{
			var orgCusAccount = Factory.New<OrgCusAccount>();
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			var provider = orgCusAccount.Provider;
			AssertType<OrgCusAccountProvider>(provider);
			AssertType<OrgCusAccountLookups>(provider.GetNewLookups(orgCusAccount));
			AssertType<OrgCusAccountValidation>(provider.GetNewValidation(orgCusAccount));
		}
	}
}
