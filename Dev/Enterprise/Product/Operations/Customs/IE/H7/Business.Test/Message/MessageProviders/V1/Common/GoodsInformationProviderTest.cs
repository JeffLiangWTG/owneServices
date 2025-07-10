using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1.Test
{
	public class GoodsInformationProviderTest : DataProviderTestCase<GoodsInformationProvider>
	{
		public void TestDescriptionOfGoods()
		{
			AssertEquals(string.Empty, Provider.DescriptionOfGoods);
			packedItem.API_GoodsDescription = "description";
			AssertEquals("description", Provider.DescriptionOfGoods);
		}

		public void TestCommodityCode()
		{
			AssertEquals(string.Empty, Provider.CommodityCode);
			packedItem.API_Tariff = "1230990";
			AssertEquals("123099", Provider.CommodityCode);
		}

		public void TestSupplementaryUnits()
		{
			SetUpTestData();
			packedItem.API_GrossWeight = 10;
			pack.APA_WeightUQ = "KG";
			packedItem.API_CustomsQty2 = 5;
			CombineAssertions(() =>
			{
				AssertType<SupplementaryUnitsProvider>(Provider.SupplementaryUnits);
				AssertEquals(10m, Provider.SupplementaryUnits.GrossMass);
				AssertEquals(5m, Provider.SupplementaryUnits.SupplementaryUnits);
			});
		}

		public void TestPackagingNumberOfPackages()
		{
			AssertEquals("empty String when pack is null", string.Empty, new GoodsInformationProvider(Factory.New<AsycudaPackedItem>()).PackagingNumberOfPackages);
			SetUpTestData();
			AssertEquals("0", Provider.PackagingNumberOfPackages);
			pack.APA_PackQty = 10;
			AssertEquals("10", Provider.PackagingNumberOfPackages);
		}

		protected override GoodsInformationProvider GetProvider()
		{
			SetUpTestData();
			return new GoodsInformationProvider(packedItem);
		}

		void SetUpTestData()
		{
			if (packedItem == null)
			{
				var header = Factory.New<AsycudaManifestHeader>();
				bill = header.Bills.AddNew();
				pack = bill.Packs.AddNew();
				packedItem = bill.PackedItems.AddNew();
				packedItem.AsycudaPackPackedItemPivots.AddPivotFor(pack);
			}
		}
		AsycudaBill bill;
		AsycudaPack pack;
		AsycudaPackedItem packedItem;
	}
}
