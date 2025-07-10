using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1.Test
{
	public class ValuationInforamtionProviderTest : DataProviderTestCase<ValuationInformationProvider>
	{
		public void TestTransportCosts()
		{
			SetUpTestData();
			bill.ABL_RX_NKTransportValueCurrency = "USD";
			bill.ABL_InsuranceValue = 100;
			bill.ABL_TransportValue = 2;
			CombineAssertions(() =>
			{
				AssertType<TransportCostsProvider>(Provider.TransportCosts);
				AssertEquals(102m, Provider.TransportCosts.Amount);
				AssertEquals("USD", Provider.TransportCosts.Currency);
			});
		}

		public void TestTransportDocuments()
		{
			SetUpTestData();
			var transportDoc = bill.AdditionalDocuments.AddNew();
			transportDoc.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			transportDoc.CSI_Code = "CODE";
			transportDoc.CSI_ReferenceNumber = "12321";

			var nonTransportDoc = bill.AdditionalDocuments.AddNew();
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

		protected override ValuationInformationProvider GetProvider()
		{
			SetUpTestData();
			return new ValuationInformationProvider(bill);
		}

		void SetUpTestData()
		{
			if (bill == null)
			{
				var header = Factory.New<AsycudaManifestHeader>();
				bill = header.Bills.AddNew();
			}
		}
		AsycudaBill bill;
	}
}
