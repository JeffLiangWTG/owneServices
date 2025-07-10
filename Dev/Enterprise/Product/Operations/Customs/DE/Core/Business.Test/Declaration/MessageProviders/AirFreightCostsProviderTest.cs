using CargoWise.Types;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class AirFreightCostsProviderTest : Customs.Business.Testing.DataProviderTestCase<AirFreightCostsProvider>
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
			dataProvider = new AirFreightCostsProvider(45.10m, CurrencyCodes.UnitedStates, 0.89m, true, exchangeRateDate, false);
			AssertEquals(false, dataProvider.CurrencyRateAgreedFlag);
		}

		public void TestCurrencyRate()
		{
			AssertEquals(0.89m, dataProvider.CurrencyRate);
		}

		public void TestCurrencyRateIATA()
		{
			Assert(dataProvider.CurrencyRateIATA);
		}

		public void TestCurrencyRateIATA_IsIATA()
		{
			dataProvider = new AirFreightCostsProvider(45.10m, CurrencyCodes.UnitedStates, 0.89m, false, exchangeRateDate, true);
			AssertEquals(false, dataProvider.CurrencyRateIATA);
		}

		public void TestCurrencyRateDate()
		{
			AssertEquals(exchangeRateDate, dataProvider.CurrencyRateDate);
		}

		public void TestCurrencyRateDate_IsFixed()
		{
			dataProvider = new AirFreightCostsProvider(45.10m, CurrencyCodes.UnitedStates, 0.89m, true, exchangeRateDate, false);
			AssertNull(dataProvider.CurrencyRateDate);
		}

		public void TestCurrencyRateDate_Empty()
		{
			dataProvider = new AirFreightCostsProvider(45.10m, CurrencyCodes.UnitedStates, 0.89m, true, ZDate.Empty, false);
			AssertNull(dataProvider.CurrencyRateDate);
		}

		protected override void SetUp()
		{
			exchangeRateDate = new ZDate(2020, 11, 10);
			dataProvider = new AirFreightCostsProvider(45.10m, CurrencyCodes.UnitedStates, 0.89m, true, exchangeRateDate, true);
		}
		AirFreightCostsProvider dataProvider;
		ZDate exchangeRateDate;

		protected override AirFreightCostsProvider GetProvider() => dataProvider;
	}
}
