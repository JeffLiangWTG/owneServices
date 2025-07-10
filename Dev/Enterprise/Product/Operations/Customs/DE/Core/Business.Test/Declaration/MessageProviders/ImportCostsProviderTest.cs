using static Enterprise.Core.Constants;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class ImportCostsProviderTest : Customs.Business.Testing.DataProviderTestCase<ImportCostsProvider>
	{
		public void TestValue()
		{
			AssertEquals(45.10m, dataProvider.Value);
		}

		public void TestCurrencyCode()
		{
			AssertEquals(CurrencyCodes.UnitedStates, dataProvider.CurrencyCode);
		}

		public void TestCurrencyRateAgreedFlag()
		{
			Assert(dataProvider.CurrencyRateAgreedFlag);
		}

		public void TestCurrencyRateAgreedFlag_IsFixed()
		{
			dataProvider = new ImportCostsProvider(45.10m, CurrencyCodes.UnitedStates, 0.89m, false);
			AssertEquals(false, dataProvider.CurrencyRateAgreedFlag);
		}

		public void TestCurrencyRate()
		{
			AssertEquals(1.012345679m, dataProvider.CurrencyRate);
		}

		public void TestCurrencyRate_IsFixed()
		{
			dataProvider = new ImportCostsProvider(45.10m, CurrencyCodes.UnitedStates, 0.89m, false);
			AssertEquals(decimal.Zero, dataProvider.CurrencyRate);
		}

		protected override void SetUp()
		{
			dataProvider = new ImportCostsProvider(45.10m, CurrencyCodes.UnitedStates, 1.0123456789m, true);
		}
		ImportCostsProvider dataProvider;

		protected override ImportCostsProvider GetProvider() => dataProvider;
	}
}
