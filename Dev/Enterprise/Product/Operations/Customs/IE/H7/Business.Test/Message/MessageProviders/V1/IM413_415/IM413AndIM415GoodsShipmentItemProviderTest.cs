using System.Linq;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1.Test
{
	public class IM413AndIM415GoodsShipmentItemProviderTest : DataProviderTestCase<IM413AndIM415GoodsShipmentItemProvider>
	{
		public void TestGoodsItemNumber()
		{
			var bill = Factory.New<AsycudaBill>();
			var firstPackedItem = bill.PackedItems.AddNew();
			var firstItemProvider = new IM413AndIM415GoodsShipmentItemProvider(firstPackedItem);
			AssertEquals("first Item on bill", "1", firstItemProvider.GoodsItemNumber);

			var secondPackedItem = bill.PackedItems.AddNew();
			var secondItemProvider = new IM413AndIM415GoodsShipmentItemProvider(secondPackedItem);
			AssertEquals("second Item on bill", "2", secondItemProvider.GoodsItemNumber);
			secondPackedItem.API_LineNo = 199;

			AssertEquals("second Item on bill with API_LineNo specifically changed", "199", secondItemProvider.GoodsItemNumber);
		}

		public void TestAdditionalProcedure()
		{
			SetUpTestData();
			bill.ABL_Procedure = "Procedure";
			AssertContainsExactElementsInAnyOrder(new[] { "Procedure" }, Provider.AdditionalProcedure);
		}

		public void TestDocumentsAuthorisations()
		{
			SetUpTestData();
			bill.ABL_UCRNumber = "1123";
			packedItem.PreviousDocuments.AddNew();
			CombineAssertions(() =>
			{
				AssertType<DocumentAuthorisationsProvider>(Provider.DocumentsAuthorisations);
				AssertEquals("1123", Provider.DocumentsAuthorisations.ReferenceNumberUCR);
				AssertEquals(1, Provider.DocumentsAuthorisations.PreviousDocuments.Count);
			});
		}

		public void TestParties()
		{
			SetUpTestData();
			bill.ABL_SellerRegNo = "1123";
			bill.ABL_ShipperName = "ShipperName";
			CombineAssertions(() =>
			{
				AssertType<Parties03Provider>(Provider.Parties);
				AssertEquals("1123", Provider.Parties.AdditionalFiscalReference.Single().Number);
				AssertEquals("FR5", Provider.Parties.AdditionalFiscalReference.Single().Type);
				AssertEquals("ShipperName", Provider.Parties.Exporter.Name);
			});
		}

		public void TestGoodsInformation()
		{
			AssertType<GoodsInformationProvider>(Provider.GoodsInformation);
		}

		public void TestTaxes()
		{
			AssertEquals(0, Provider.Taxes.Count);
		}

		public void TestItemAmountInvoicedIntrinsicValue()
		{
			CombineAssertions(() =>
			{
				AssertType<ItemAmountInvoicedIntrinsicValueProvider>(Provider.ItemAmountInvoicedIntrinsicValue);
				AssertEquals(10.22m, Provider.ItemAmountInvoicedIntrinsicValue.ItemAmount.Amount);
			});
		}

		protected override IM413AndIM415GoodsShipmentItemProvider GetProvider()
		{
			SetUpTestData();
			return new IM413AndIM415GoodsShipmentItemProvider(packedItem);
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
				packedItem.API_GoodsValue = 10.222m;
			}
		}
		AsycudaBill bill;
		AsycudaPack pack;
		AsycudaPackedItem packedItem;
	}
}
