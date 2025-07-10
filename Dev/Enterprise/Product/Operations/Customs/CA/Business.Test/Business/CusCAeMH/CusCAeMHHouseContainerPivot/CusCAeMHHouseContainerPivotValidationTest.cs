using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusCAeMHHouseContainerPivotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBPA_BQ_Container()
		{
			var master = Factory.New<CusCAeMHMaster>();
			var container1 = master.Containers.AddNew();
			var container2 = master.Containers.AddNew();
			var house = master.HouseBills.AddNew();
			var pivot1 = house.Pivots.AddNew();
			pivot1.BPA_BQ_Container = container1.PK;
			var pivot2 = house.Pivots.AddNew();
			pivot2.BPA_BQ_Container = container1.PK;
			AssertHasError(pivot2.BPA_BQ_ContainerInfo, "This container is already linked to the house bill.");
			pivot2.BPA_BQ_Container = container2.PK;
			AssertNoError(pivot2.BPA_BQ_ContainerInfo, "This container is already linked to the house bill.");
		}
	}
}
