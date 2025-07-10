using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.FR.H7.Business.Testing
{
	sealed class H7BillLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestShipmentTypeList()
		{
			var bill = Factory.New<H7Bill>();
			var codeList = bill.Lookups.ShipmentTypes;

			CombineAssertions(() =>
			{
				AssertEquals("There are four elements in the list", 4, codeList.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "AIM", "ACO", "DIM", "DCO" }, codeList.GetAllCodes());
			});
		}
	}
}
