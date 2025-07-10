namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class AdditionsAndDeductionsProviderTest : Customs.Business.Testing.DataProviderTestCase<AdditionsAndDeductionsProvider>
	{
		public void TestCode()
		{
			AssertEquals("Code", "CT1", Provider.Code);
		}

		public void TestAmount()
		{
			AssertEquals("Amount", 31m, Provider.Amount);
		}

		protected override AdditionsAndDeductionsProvider GetProvider() => new AdditionsAndDeductionsProvider("CT1", 31m);
	}
}
