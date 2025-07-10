using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	sealed class GoodsMeasureProviderTest : DataProviderTestCase<GoodsMeasureProvider>
	{
		public void TestNewOrNull()
		{
			AssertNull("Return null when PackedItem is null", GoodsMeasureProvider.NewOrNull(null));
			AssertNotNull("Return provider when PackedItem is not null", GoodsMeasureProvider.NewOrNull(Factory.New<AsycudaPackedItem>()));
		}

		public void TestGrossMass()
		{
			AssertEquals("Gross Mass", 5m, goodsMeasureProvider.GrossMass);
			pack.APA_WeightUQ = "G";
			AssertEquals("No effect when unit converted to KG with APA_WeightUQ", 5m, goodsMeasureProvider.GrossMass);
			var nullPackProvider = GoodsMeasureProvider.NewOrNull(Factory.New<AsycudaPackedItem>());
			AssertEquals("Gross Mass", 0m, nullPackProvider.GrossMass);
		}

		public void TestNetMass()
		{
			AssertEquals("Net Mass", 0m, goodsMeasureProvider.NetMass);
		}

		public void TestSupplementaryUnits()
		{
			AssertEquals("Supplementary Units", 20m, goodsMeasureProvider.SupplementaryUnits);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			pack = bill.Packs.AddNew();
			pack.APA_Weight = 10;
			pack.APA_WeightUQ = "KG";

			var packedItem = bill.PackedItems.AddNew();
			packedItem.API_CustomsQty2 = 20;
			packedItem.API_GrossWeight = 5;
			packedItem.AsycudaPackPackedItemPivots.AddPivotFor(pack);

			goodsMeasureProvider = GoodsMeasureProvider.NewOrNull(packedItem);
		}
		AsycudaPack pack;
		GoodsMeasureProvider goodsMeasureProvider;

		protected override GoodsMeasureProvider GetProvider()
		{
			return goodsMeasureProvider;
		}
	}
}
