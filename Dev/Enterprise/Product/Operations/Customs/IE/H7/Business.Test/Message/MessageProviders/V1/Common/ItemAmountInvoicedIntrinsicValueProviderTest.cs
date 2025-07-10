using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1.Test
{
	public class ItemAmountInvoicedIntrinsicValueProviderTest : DataProviderTestCase<ItemAmountInvoicedIntrinsicValueProvider>
	{
		public void TestItemAmount()
		{
			AssertEquals(0m, Provider.ItemAmount.Amount);
			AssertEquals("EUR", Provider.ItemAmount.Currency);

			packedItem.API_GoodsValue = 100;
			packedItem.API_RX_NKGoodsValueCurrency = "USD";
			var provider = GetProvider();
			AssertEquals(100m, provider.ItemAmount.Amount);
			AssertEquals("USD", provider.ItemAmount.Currency);
		}

		public void TestTransportCosts()
		{
			SetUpTestData();
			bill.ABL_InsuranceValue = 15;
			bill.ABL_TransportValue = 5;
			bill.ABL_RX_NKInsuranceValueCurrency = "EUR";
			bill.ABL_RX_NKTransportValueCurrency = "EUR";
			bill.ABL_RX_NKGoodsValueCurrency = "AUD";

			bill.PackedItems.AddNew();
			AssertEquals("total 2 items to apportion to", 2, bill.PackedItems.Count);

			CombineAssertions(() =>
			{
				AssertType<TransportCostsProvider>(Provider.TransportCosts);
				AssertEquals(10m, Provider.TransportCosts.Amount);
				AssertEquals("EUR", Provider.TransportCosts.Currency);
			});
		}

		public void TestTransportDocuments()
		{
			SetUpTestData();
			var transportDoc = packedItem.AdditionalDocuments.AddNew();
			transportDoc.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			transportDoc.CSI_Code = "CODE";
			transportDoc.CSI_ReferenceNumber = "12321";

			var nonTransportDoc = packedItem.AdditionalDocuments.AddNew();
			nonTransportDoc.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;

			var result = Provider.TransportDocuments.ToArray();
			CombineAssertions(() =>
			{
				AssertEquals(1, result.Length);
				AssertType<DocumentProvider>(result[0]);
				AssertEquals("CODE", result[0].Type);
				AssertEquals("12321", result[0].Reference);
			});
		}

		protected override ItemAmountInvoicedIntrinsicValueProvider GetProvider()
		{
			SetUpTestData();
			return new ItemAmountInvoicedIntrinsicValueProvider(packedItem);
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
