using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1.Test
{
	public class SupplementaryUnitsProviderTest : DataProviderTestCase<SupplementaryUnitsProvider>
	{
		public void TestGrossMass()
		{
			AssertEquals(0m, new SupplementaryUnitsProvider(Factory.New<AsycudaPackedItem>()).GrossMass);
			SetUpTestData();
			AssertEquals(0m, Provider.GrossMass);
			packedItem.API_GrossWeight = 1000;
			pack.APA_WeightUQ = "KG";
			AssertEquals(1000m, Provider.GrossMass);
			pack.APA_WeightUQ = "G";
			AssertEquals("No effect when unit converted to KG with APA_WeightUQ", 1000m, Provider.GrossMass);
		}

		public void TestSupplementaryUnits()
		{
			AssertEquals(0m, Provider.SupplementaryUnits);
			packedItem.API_CustomsQty2 = 10;
			AssertEquals(10m, Provider.SupplementaryUnits);
		}

		protected override SupplementaryUnitsProvider GetProvider()
		{
			SetUpTestData();
			return new SupplementaryUnitsProvider(packedItem);
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
