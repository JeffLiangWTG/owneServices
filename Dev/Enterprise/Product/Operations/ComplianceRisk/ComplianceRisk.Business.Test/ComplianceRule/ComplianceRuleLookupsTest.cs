using CargoWise.EntityFramework.Testing;

namespace Enterprise.ComplianceRisk.Business.Test
{
	class ComplianceRuleLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRefCountry_List()
		{
			AssertNotNull(Factory.New<ComplianceRule>().Lookups.RefCountry_List);
		}
	}
}
