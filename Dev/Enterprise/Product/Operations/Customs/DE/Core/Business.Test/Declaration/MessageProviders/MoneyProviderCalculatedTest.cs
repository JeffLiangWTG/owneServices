namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class MoneyProviderCalculatedTest : Customs.Business.Testing.DataProviderTestCase<MoneyProviderCalculated>
	{
		public void TestValue()
		{
			AssertEquals(123456789.10m, dataProvider.Value);
		}

		public void TestCurrencyCode()
		{
			AssertEquals("INR", dataProvider.CurrencyCode);
		}

		protected override void SetUp()
		{
			dataProvider = new MoneyProviderCalculated("INR", 123456789.10m);
		}
		MoneyProviderCalculated dataProvider;

		protected override MoneyProviderCalculated GetProvider() => dataProvider;
	}
}
