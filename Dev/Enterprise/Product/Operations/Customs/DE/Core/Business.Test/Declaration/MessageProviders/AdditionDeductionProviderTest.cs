using CargoWise.Types;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class AdditionDeductionProviderTest : Customs.Business.Testing.DataProviderTestCase<AdditionDeductionProvider>
	{
		public void TestValue()
		{
			AssertEquals(45.13m, dataProvider.Value);
		}

		public void TestCurrencyCode()
		{
			AssertEquals(CurrencyCodes.UnitedStates, dataProvider.CurrencyCode);
		}

		public void TestCurrencyRateAgreedFlag()
		{
			Assert(dataProvider.CurrencyRateAgreedFlag);
		}

		public void TestCurrencyRateAgreedFlag_NotFixed()
		{
			dataProvider = new AdditionDeductionProvider(ImportChargeCodeList.Codes._017, 45.10m, CurrencyCodes.UnitedStates, false, 0.89m, exchangeRateDate, exchangeRateUserEnterable: false, 0.02m);
			AssertEquals(false, dataProvider.CurrencyRateAgreedFlag);
		}

		public void TestCurrencyRate()
		{
			AssertEquals(1.012345679m, dataProvider.CurrencyRate);
		}

		public void TestCurrencyRate_NotFixed()
		{
			dataProvider = new AdditionDeductionProvider(ImportChargeCodeList.Codes._017, 45.10m, CurrencyCodes.UnitedStates, false, 0.89m, exchangeRateDate, exchangeRateUserEnterable: false, 0.02m);
			AssertEquals(decimal.Zero, dataProvider.CurrencyRate);
		}

		public void TestCurrencyRateIATA()
		{
			AssertEquals(false, dataProvider.CurrencyRateIATA);
		}

		public void TestCurrencyRateIATA_IsIATA()
		{
			dataProvider = new AdditionDeductionProvider(ImportChargeCodeList.Codes._017, 45.10m, CurrencyCodes.UnitedStates, currencyRateIATA: true, 0.89m, exchangeRateDate, true, 0.02m);
			Assert(dataProvider.CurrencyRateIATA);
		}

		public void TestCurrencyRateDate()
		{
			AssertEquals(exchangeRateDate, dataProvider.CurrencyRateDate);
		}

		public void TestCurrencyRateDate_NotFixed()
		{
			dataProvider = new AdditionDeductionProvider(ImportChargeCodeList.Codes._017, 45.10m, CurrencyCodes.UnitedStates, false, 0.89m, exchangeRateDate, exchangeRateUserEnterable: false, 0.02m);
			AssertNull(dataProvider.CurrencyRateDate);
		}

		public void TestCurrencyRateDate_Empty()
		{
			dataProvider = new AdditionDeductionProvider(ImportChargeCodeList.Codes._017, 45.10m, CurrencyCodes.UnitedStates, false, 0.89m, ZDate.Empty, exchangeRateUserEnterable: true, 0.02m);
			AssertNull(dataProvider.CurrencyRateDate);
		}

		public void TestType()
		{
			AssertEquals(ImportChargeCodeList.Codes._010, dataProvider.Type);
		}

		public void TestPercentage()
		{
			AssertEquals(0.03m, dataProvider.Percentage);
		}

		public void TestPercentage_Less0()
		{
			dataProvider = new AdditionDeductionProvider(ImportChargeCodeList.Codes._010, 45.10m, CurrencyCodes.UnitedStates, false, 0.89m, exchangeRateDate, true, -0.02m);
			AssertEquals(decimal.Zero, dataProvider.Percentage);
		}

		public void TestPercentage_WrongChargeType()
		{
			dataProvider = new AdditionDeductionProvider(ImportChargeCodeList.Codes._017, 45.10m, CurrencyCodes.UnitedStates, false, 0.89m, exchangeRateDate, true, 0.02m);
			AssertEquals(decimal.Zero, dataProvider.Percentage);
		}

		protected override void SetUp()
		{
			exchangeRateDate = new ZDate(2020, 11, 10);
			dataProvider = new AdditionDeductionProvider(ImportChargeCodeList.Codes._010, 45.125m, CurrencyCodes.UnitedStates, false, 1.0123456789m, exchangeRateDate, true, 0.025m);
		}
		AdditionDeductionProvider dataProvider;
		ZDate exchangeRateDate;

		protected override AdditionDeductionProvider GetProvider() => dataProvider;
	}
}
