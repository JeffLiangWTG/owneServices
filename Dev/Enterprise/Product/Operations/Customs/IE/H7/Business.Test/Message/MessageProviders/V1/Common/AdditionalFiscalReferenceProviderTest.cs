using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1.Test
{
	public class AdditionalFiscalReferenceProviderTest : DataProviderTestCase<AdditionalFiscalReferenceProvider>
	{
		public void TestType()
		{
			AssertEquals("FR5", Provider.Type);
		}

		public void TestNumber()
		{
			AssertEquals(string.Empty, Provider.Number);
			bill.ABL_SellerRegNo = "1123";
			AssertEquals("1123", Provider.Number);
		}

		protected override AdditionalFiscalReferenceProvider GetProvider()
		{
			SetUpTestData();
			return new AdditionalFiscalReferenceProvider(bill);
		}

		void SetUpTestData()
		{
			if (bill == null)
			{
				bill = Factory.New<AsycudaBill>();
			}
		}
		AsycudaBill bill;
	}
}
