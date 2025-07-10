using System.Collections.Generic;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1.Test
{
	sealed class IM432GoodsShipmentItemTypeProviderTest : DataProviderTestCase<IM432GoodsShipmentItemTypeProvider>
	{
		public void TestGoodsItemNumber()
		{
			var bill = Factory.New<AsycudaBill>();
			var firstPackedItem = bill.PackedItems.AddNew();
			var firstItemProvider = new IM432GoodsShipmentItemTypeProvider(firstPackedItem);
			AssertEquals("first Item on bill", "1", firstItemProvider.GoodsItemNumber);

			var secondPackedItem = bill.PackedItems.AddNew();
			var secondItemProvider = new IM432GoodsShipmentItemTypeProvider(secondPackedItem);
			AssertEquals("second Item on bill", "2", secondItemProvider.GoodsItemNumber);
			secondPackedItem.API_LineNo = 199;

			AssertEquals("second Item on bill with API_LineNo specifically changed", "199", secondItemProvider.GoodsItemNumber);
		}

		public void TestDocumentsAuthorisations()
		{
			var documentsAuthorisations = Provider.DocumentsAuthorisations;
			CombineAssertions("DocumentsAuthorisations", () =>
			{
				AssertEquals("Should have 2 documents", 2, documentsAuthorisations.Count);
				Assert("Should be IReadOnlyCollection<DocumentProvider>", documentsAuthorisations is IReadOnlyCollection<DocumentProvider>);
			});
		}

		public void TestGoodsInformation()
		{
			var goodsInformation = Provider.GoodsInformation;
			CombineAssertions("GoodsInformation", () =>
			{
				Assert("GoodsInformation is IM432GoodsInformationProvider", goodsInformation is IM432GoodsInformationProvider);
				AssertSame("Is cached", goodsInformation, Provider.GoodsInformation);
			});
		}

		protected override IM432GoodsShipmentItemTypeProvider GetProvider()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_ManifestQty = 15;
			var pack = bill.Packs.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			packedItem.PreviousDocuments.AddNew();
			packedItem.PreviousDocuments.AddNew();
			packedItem.AsycudaPackPackedItemPivots.AddPivotFor(pack);

			return new IM432GoodsShipmentItemTypeProvider(packedItem);
		}
	}
}
