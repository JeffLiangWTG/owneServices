using CargoWise.EntityFramework.Testing;

namespace Enterprise.CommissionManagement.Business.Testing
{
	internal class CommissionLineSourceGroupingLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCompanies()
		{
			var grouping = new CommissionLineGroupingForTest(Factory);
			grouping.Init(new[] { Factory.New<ViewCommissionLine>() });
			var lookups = grouping.Lookups;
			AssertNotNull(lookups.Companies);
		}
	}
}
