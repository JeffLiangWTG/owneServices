using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Business.Testing
{
	using Enterprise.Accounting.Integration;

	internal class ExchangeRatesSettingsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestExchangeRatesSource()
		{
			ExchangeRatesSettings settings = new ExchangeRatesSettings(new ExRateSourceType[] { ExRateSourceType.Voyage });
			settings.ExchangeRatesSource = "XXX";
			AssertHasError(settings.ExchangeRatesSourceInfo, "Enter a valid Exchange Rates Source.");
			settings.ExchangeRatesSource = ExchangeRatesSourceList.Codes.CurrencyFile;
			AssertNoErrors(settings.ExchangeRatesSourceInfo);
			settings.ExchangeRatesSource = ExchangeRatesSourceList.Codes.SailingSchedule;
			AssertNoErrors(settings.ExchangeRatesSourceInfo);
			settings.ExchangeRatesSource = "";
			AssertHasError(settings.ExchangeRatesSourceInfo, "Please enter an Exchange Rates Source.");
			settings = new ExchangeRatesSettings(null);
			settings.ExchangeRatesSource = ExchangeRatesSourceList.Codes.CurrencyFile;
			AssertNoErrors(settings.ExchangeRatesSourceInfo);
			settings.ExchangeRatesSource = ExchangeRatesSourceList.Codes.SailingSchedule;
			AssertHasError(settings.ExchangeRatesSourceInfo, "Enter a valid Exchange Rates Source.");
			settings = new ExchangeRatesSettings(System.Array.Empty<ExRateSourceType>());
			settings.ExchangeRatesSource = ExchangeRatesSourceList.Codes.CurrencyFile;
			AssertNoErrors(settings.ExchangeRatesSourceInfo);
			settings.ExchangeRatesSource = ExchangeRatesSourceList.Codes.SailingSchedule;
			AssertHasError(settings.ExchangeRatesSourceInfo, "Enter a valid Exchange Rates Source.");
		}
	}
}