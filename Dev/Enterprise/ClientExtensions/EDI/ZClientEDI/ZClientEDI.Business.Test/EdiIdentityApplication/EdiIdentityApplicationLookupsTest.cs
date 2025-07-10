using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.IdentityApplication.Business.Testing
{
	class EdiIdentityApplicationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRedirectUrlStatus()
		{
			AssertContainsExactElementsInAnyOrder(new EdiIdentityApplicationRedirectUrlStatus(), new EdiIdentityApplicationLookups(null).EdiIdentityApplicationRedirectUrlStatusList);
		}
	}
}
