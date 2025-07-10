using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusCAeMHHouseContainerPivotLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLookupLists()
		{
			var master = Factory.New<CusCAeMHMaster>();
			var container1 = master.Containers.AddNew();
			var container2 = master.Containers.AddNew();
			var house = master.HouseBills.AddNew();
			var pivot = house.Pivots.AddNew();
			AssertEquals(3, pivot.Lookups.Containers.Count);
			Assert(pivot.Lookups.Containers.Contains(container1));
			Assert(pivot.Lookups.Containers.Contains(container2));
		}
	}
}
