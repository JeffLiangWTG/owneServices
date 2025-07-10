using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1.Test
{
	sealed class IM432GoodsInformationProviderTest : DataProviderTestCase<IM432GoodsInformationProvider>
	{
		public void TestGrossMassValue()
		{
			AssertEquals("GrossMassValue", 20M, Provider.GrossMassValue);
		}

		public void TestPackaging()
		{
			AssertEquals("Packaging", 100, Provider.Packaging);
		}

		protected override IM432GoodsInformationProvider GetProvider()
		{
			var bill = Factory.New<AsycudaBill>();

			var pack = bill.Packs.AddNew();
			pack.APA_PackQty = 100;
			pack.APA_Weight = 20;

			var packedItem = bill.PackedItems.AddNew();
			packedItem.AsycudaPackPackedItemPivots.AddPivotFor(pack);

			return new IM432GoodsInformationProvider(packedItem);
		}
	}
}
