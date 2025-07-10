using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	sealed class TransportCostsProviderTest : DataProviderTestCase<TransportCostsProvider>
	{
		public void TestAmount()
		{
			transportCostsProvider = new TransportCostsProvider(100, "USD");
			AssertEquals(100m, transportCostsProvider.Amount);

			bill = CreateBill(10.222m, 10, "EUR", "EUR", "AUD");
			transportCostsProvider = TransportCostsProvider.NewOrNull(bill);
			AssertEquals(20.22m, transportCostsProvider.Amount);

			bill.PackedItems.AddNew();
			bill.PackedItems.AddNew();
			transportCostsProvider = TransportCostsProvider.NewOrNullPerPackedItem(bill);
			AssertEquals(10.11m, transportCostsProvider.Amount);
		}

		public void TestAmountIsRoundedToTwoDecimals()
		{
			bill = CreateBill(0, 10.1111m, "EUR", "EUR", "AUD");
			bill.PackedItems.AddNew();

			transportCostsProvider = TransportCostsProvider.NewOrNull(bill);
			AssertEquals(10.11m, transportCostsProvider.Amount);

			transportCostsProvider = TransportCostsProvider.NewOrNullPerPackedItem(bill);
			AssertEquals(10.11m, transportCostsProvider.Amount);

			bill.ABL_TransportValue = 10.1188m;

			transportCostsProvider = TransportCostsProvider.NewOrNull(bill);
			AssertEquals(10.12m, transportCostsProvider.Amount);

			transportCostsProvider = TransportCostsProvider.NewOrNullPerPackedItem(bill);
			AssertEquals(10.12m, transportCostsProvider.Amount);
		}

		public void TestCurrency()
		{
			transportCostsProvider = new TransportCostsProvider(100, "USD");
			AssertEquals("USD", transportCostsProvider.Currency);

			bill = CreateBill(10.222m, 10, "EUR", "EUR", "AUD");
			transportCostsProvider = TransportCostsProvider.NewOrNull(bill);
			AssertEquals("EUR", transportCostsProvider.Currency);

			bill.PackedItems.AddNew();
			bill.PackedItems.AddNew();
			transportCostsProvider = TransportCostsProvider.NewOrNullPerPackedItem(bill);
			AssertEquals("EUR", transportCostsProvider.Currency);
		}

		public void TestWhenAmountIsZero()
		{
			bill = CreateBill(0, 0, "EUR", "EUR", "AUD");
			transportCostsProvider = TransportCostsProvider.NewOrNull(bill);

			CombineAssertions(() =>
			{
				AssertNotNull(transportCostsProvider);
				AssertEquals(0m, transportCostsProvider.Amount);
				AssertNull(transportCostsProvider.Currency);
			});

			bill.PackedItems.AddNew();
			bill.PackedItems.AddNew();
			transportCostsProvider = TransportCostsProvider.NewOrNullPerPackedItem(bill);

			CombineAssertions(() =>
			{
				AssertNotNull(transportCostsProvider);
				AssertEquals(0m, transportCostsProvider.Amount);
				AssertNull(transportCostsProvider.Currency);
			});
		}

		protected sealed override TransportCostsProvider GetProvider()
		{
			return transportCostsProvider;
		}

		TransportCostsProvider transportCostsProvider;
		AsycudaBill bill;

		AsycudaBill CreateBill(decimal insuranceValue, decimal transportValue, string insuranceCurrency, string transportCurrency, string goodsCurrency)
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_InsuranceValue = insuranceValue;
			bill.ABL_TransportValue = transportValue;
			bill.ABL_RX_NKInsuranceValueCurrency = insuranceCurrency;
			bill.ABL_RX_NKTransportValueCurrency = transportCurrency;
			bill.ABL_RX_NKGoodsValueCurrency = goodsCurrency;
			return bill;
		}
	}
}
