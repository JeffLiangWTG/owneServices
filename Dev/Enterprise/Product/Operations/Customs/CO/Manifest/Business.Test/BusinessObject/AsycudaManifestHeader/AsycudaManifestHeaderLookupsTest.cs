using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CO.Manifest.Business.Testing
{
	class AsycudaManifestHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAMA_MessageStatusList()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var list = header.Lookups.MessageStatusList.GetAllCodes();

			AssertEquals(4, list.Length);
			AssertContainsExactElementsInAnyOrder(new[] { "ACP", "ERR", "SNT", "CAN" }, list);
			AssertCollectionNotContains(new[] { "NOT", "UNK", "UPD", "REG", "AWA", "WRN" }, list);
		}

		public void TestDeliveryModeList()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var list = header.Lookups.DeliveryModeList.GetAllCodes();

			AssertEquals(4, list.Length);
			AssertContainsExactElementsInExactOrder(new[] { "1", "2", "3", "4" }, list);
		}
	}
}
