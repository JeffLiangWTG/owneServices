using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	public class ExchangeRateToleranceValidationTest : TestCaseWithFactory
	{
		public void TestValidCurrency()
		{
			var exchangeRateTolerance = CreateItemAndCollection();

			AssertNoErrors(exchangeRateTolerance.CurrencyInfo);

			exchangeRateTolerance.Currency = "";
			AssertHasError(exchangeRateTolerance.CurrencyInfo, "Please enter a Currency.");

			exchangeRateTolerance.Currency = "AAA";
			AssertHasError(exchangeRateTolerance.CurrencyInfo, "Enter a valid selection.");

			exchangeRateTolerance.Currency = ExchangeRateToleranceLookups.AllCurrencyCode;
			AssertNoErrors(exchangeRateTolerance.CurrencyInfo);

			exchangeRateTolerance.Currency = "AUD";
			AssertNoErrors(exchangeRateTolerance.CurrencyInfo);

			var exchangeRateToleranceDuplicated = exchangeRateTolerance.ParentCollection.AddNew();
			exchangeRateToleranceDuplicated.Currency = "AUD";
			AssertHasError(exchangeRateToleranceDuplicated.CurrencyInfo, "Only single configuration per currency should be allowed.");
		}

		public void TestValidExchangeRateTolerancePercentage()
		{
			var exchangeRateTolerance = CreateItemAndCollection();
			AssertNoErrors(exchangeRateTolerance.ExchangeRateTolerancePercentageInfo);

			exchangeRateTolerance.ExchangeRateTolerancePercentage = 0;
			AssertNoErrors(exchangeRateTolerance.ExchangeRateTolerancePercentageInfo);

			exchangeRateTolerance.ExchangeRateTolerancePercentage = 1;
			AssertNoErrors(exchangeRateTolerance.ExchangeRateTolerancePercentageInfo);

			exchangeRateTolerance.ExchangeRateTolerancePercentage = -1;
			AssertHasError(exchangeRateTolerance.ExchangeRateTolerancePercentageInfo, "Value should be between 0 to 100.");

			exchangeRateTolerance.ExchangeRateTolerancePercentage = 100;
			AssertNoErrors(exchangeRateTolerance.ExchangeRateTolerancePercentageInfo);

			exchangeRateTolerance.ExchangeRateTolerancePercentage = 101;
			AssertHasError(exchangeRateTolerance.ExchangeRateTolerancePercentageInfo, "Value should be between 0 to 100.");

			exchangeRateTolerance.ExchangeRateTolerancePercentage = 55;
			AssertNoErrors(exchangeRateTolerance.ExchangeRateTolerancePercentageInfo);
		}

		ExchangeRateTolerance CreateItemAndCollection()
		{
			var coll = new ExchangeRateToleranceCollection();
			return coll.AddNew();
		}
	}
}
