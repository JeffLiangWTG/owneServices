using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CO.Manifest.Business.Testing
{
	sealed class AsycudaBillLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestGoods_LocationLookups()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			AssertType(typeof(OrgAddressCollection), bill.Lookups.GoodsLocations);
		}

		public void TestContainerModeList()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var list = bill.Lookups.ContainerModeList.GetAllCodes();

			AssertEquals(5, list.Length);
			AssertContainsExactElementsInExactOrder(new[] { "1", "3", "4", "5", "6" }, list);
		}
	}
}
