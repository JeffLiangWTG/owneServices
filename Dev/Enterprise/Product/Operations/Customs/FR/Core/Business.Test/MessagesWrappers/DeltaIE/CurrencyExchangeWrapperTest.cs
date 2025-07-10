namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class CurrencyExchangeWrapperTest : Customs.Business.Testing.DataProviderTestCase<CurrencyExchangeWrapper>
	{
		protected override CurrencyExchangeWrapper GetProvider()
		{
			return new CurrencyExchangeWrapper();
		}

		public void TestInternalCurrencyUnit()
		{
			AssertEquals("InternalCurrencyUnit should be equal to EUR.", "EUR", Provider.InternalCurrencyUnit);
		}
	}
}
