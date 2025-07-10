using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(CurrencyConverter))]
public class CurrencyConverterTest : CurrencyConverterWithDataProviderTest
{
	public void TestGetExchangeRate()
	{
		var currency = Factory.New<RefCurrency>();
		currency.RX_Code = "ZZZ";

		var declaration = Factory.New<JobDeclaration>();
		var nscExchangeRates = declaration.NonStandardExchangeRates;
		var rate = nscExchangeRates.AddNew();
		rate.CSI_RX_NKCurrency = currency.RX_Code;

		DummyProvider.DateOfValuationExposed = ZDateTime.BrettsBirthday;
		DummyProvider.FixedExchangeRateCurrencyCode = ZString.Empty;
		DummyProvider.NonStandardExchangeRates = nscExchangeRates;

		var currencyConverter = (CurrencyConverter)GetCurrencyConverter();

		rate.CSI_Value = 2.0m;
		AssertEquals(2.0m, currencyConverter.GetExchangeRate(currency, out var foundRateDate));
		AssertEquals(ZDateTime.BrettsBirthday, foundRateDate);

		DummyProvider.FixedExchangeRateCurrencyCode = currency.RX_Code;
		DummyProvider.FixedExchangeRate = 3.0m;
		AssertEquals(3.0m, currencyConverter.GetExchangeRate(currency));
	}

	public void TestIsNonStandardCurrency()
	{
		var standardCurrency = RefDataSetupTestHelper.SetupStandardCurrencyCodes(Factory);
		var nonStandardCurrency = "ZZZ";
		var currencyConverter = (CurrencyConverter)GetCurrencyConverter();
		DummyProvider.DateOfValuationExposed = ZDateTime.BrettsBirthday;

		Assert(!currencyConverter.IsNonStandardCurrency(ZString.Empty));
		Assert(!currencyConverter.IsNonStandardCurrency(standardCurrency));
		Assert(currencyConverter.IsNonStandardCurrency(nonStandardCurrency));
	}

	protected new DummyCurrencyConverter DummyProvider => (DummyCurrencyConverter)base.DummyProvider;

	protected override DummyCurrencyConverterDataProvider GetNewDummyProvider() => new DummyCurrencyConverter();

	protected override CurrencyConverterWithDataProvider GetCurrencyConverter() => new CurrencyConverter(Factory, DummyProvider);
}
