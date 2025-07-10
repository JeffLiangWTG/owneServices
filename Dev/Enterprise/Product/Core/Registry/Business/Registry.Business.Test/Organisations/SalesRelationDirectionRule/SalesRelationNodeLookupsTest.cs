using CargoWise.EntityFramework.Testing;

namespace Enterprise.Registry.Business.Testing
{
	sealed class SalesRelationNodeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAllActivityTypes()
		{
			var lookups = new SalesRelationRuleNodeLookups(null);

			AssertNotNull(lookups.AllActivityTypes);
			AssertEquals(8, lookups.AllActivityTypes.Count);
		}
	}
}
