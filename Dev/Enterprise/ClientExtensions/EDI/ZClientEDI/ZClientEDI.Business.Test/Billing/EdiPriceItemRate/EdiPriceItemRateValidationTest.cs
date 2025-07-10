using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	internal class EdiPriceItemRateValidationTest : BusinessObjectValidationTestCase
	{
		public void TestIsCurrencyUnique()
		{
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			priceHeader.L6_RX_NKCurrency = "USD";

			var priceItem = priceHeader.Items.AddNew();
			priceItem.L7_RX_NKCurrency = "AUD";

			var rate1 = priceItem.CurrencyRates.AddNew();
			rate1.PIR_RX_NKCurrency = "USD";
			AssertNoErrors(rate1.PIR_RX_NKCurrencyInfo);

			rate1.PIR_RX_NKCurrency = "AUD";
			AssertNoErrors(rate1.PIR_RX_NKCurrencyInfo);

			rate1.PIR_RX_NKCurrency = "NZD";
			AssertNoErrors(rate1.PIR_RX_NKCurrencyInfo);

			var rate2 = priceItem.CurrencyRates.AddNew();
			rate2.PIR_RX_NKCurrency = "EUR";
			AssertNoErrors(rate2.PIR_RX_NKCurrencyInfo);

			rate2.PIR_RX_NKCurrency = "NZD";
			AssertHasErrors(rate2.PIR_RX_NKCurrencyInfo);
		}

		public void TestNoConflictWithPriceItemCurrency()
		{
			var priceItem = Factory.New<ClientLicencePriceItem>();
			priceItem.L7_RX_NKCurrency = "GBP";

			var rate = priceItem.CurrencyRates.AddNew();
			rate.Validation.ValidateAll();
			AssertHasRowError(rate, "Should not add price in other currencies when price item already has a currency.");

			priceItem.L7_RX_NKCurrency = "";
			rate.Validation.ValidateAll();
			AssertNoRowError(rate, "Should not add price in other currencies when price item already has a currency.");
		}
	}
}