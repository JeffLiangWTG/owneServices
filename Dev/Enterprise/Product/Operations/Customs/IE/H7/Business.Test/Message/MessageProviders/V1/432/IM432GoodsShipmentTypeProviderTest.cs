using System;
using System.Collections.Generic;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1.Test
{
	sealed class IM432GoodsShipmentTypeProviderTest : DataProviderTestCase<IM432GoodsShipmentTypeProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>("Manifest header missing", () => new IM432GoodsShipmentTypeProvider(null));
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

		public void TestDatesPlaces()
		{
			var datesPlaces = Provider.DatesPlaces;
			CombineAssertions("DatesPlaces", () =>
			{
				Assert("DatesPlaces is LocationOfGoodsGNSSProvider", datesPlaces is LocationOfGoodsGNSSProvider);
				AssertSame("Is cached", datesPlaces, Provider.DatesPlaces);
			});
		}

		public void TestGoodsInformation()
		{
			var goodsInformation = Provider.GoodsInformation;
			CombineAssertions("GoodsInformation", () =>
			{
				Assert("GoodsInformation is GoodsInformation02Provider", goodsInformation is GoodsInformation02Provider);
				AssertSame("Is cached", goodsInformation, Provider.GoodsInformation);
			});
		}

		public void TestGovernmentAgencyGoodsItem()
		{
			var governmentAgencyGoodsItem = Provider.GovernmentAgencyGoodsItem;
			CombineAssertions("GovernmentAgencyGoodsItem", () =>
			{
				AssertEquals("Should have 2 documents", 2, governmentAgencyGoodsItem.Count);
				Assert("Should be IReadOnlyCollection<IM432GoodsShipmentItemTypeProvider>", governmentAgencyGoodsItem is IReadOnlyCollection<IM432GoodsShipmentItemTypeProvider>);
			});
		}

		protected override IM432GoodsShipmentTypeProvider GetProvider()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.PreviousDocuments.AddNew();
			bill.PreviousDocuments.AddNew();

			var pack = bill.Packs.AddNew();
			var packedItem1 = bill.PackedItems.AddNew();
			packedItem1.AsycudaPackPackedItemPivots.AddPivotFor(pack);
			var packedItem2 = bill.PackedItems.AddNew();
			packedItem2.AsycudaPackPackedItemPivots.AddPivotFor(pack);

			return new IM432GoodsShipmentTypeProvider(bill);
		}
	}
}
