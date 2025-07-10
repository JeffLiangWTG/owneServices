using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	internal class EdiPriceHeaderExchangeRateValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckPHE_GroupCode()
		{
			var priceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			var rate1 = priceHeader.ExchangeRates.AddNew();
			var rate2 = priceHeader.ExchangeRates.AddNew();

			rate1.PHE_GroupCode = "@@@";
			AssertHasError(rate1.PHE_GroupCodeInfo, "Enter a valid selection.");

			rate1.PHE_GroupCode = "STL";
			rate1.PHE_RX_NKCurrency = "USD";
			rate1.PHE_Rate = 1;
			AssertNoErrors(rate1);

			rate2.PHE_GroupCode = "STL";
			rate2.PHE_RX_NKCurrency = "USD";
			rate2.PHE_Rate = 1;
			rate2.RunPreSaveValidation();
			AssertHasError(rate2.PHE_GroupCodeInfo, "Group Code / Currency must be unique.");

			rate2.PHE_RX_NKCurrency = "AUD";
			rate2.RunPreSaveValidation();
			AssertNoErrors(rate2);
		}

		public void TestCheckPHE_RX_NKCurrency()
		{
			var priceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			var rate1 = priceHeader.ExchangeRates.AddNew();

			rate1.PHE_RX_NKCurrency = "";
			AssertHasError(rate1.PHE_RX_NKCurrencyInfo, "Please enter a value.");

			rate1.PHE_RX_NKCurrency = "@@@";
			AssertHasError(rate1.PHE_RX_NKCurrencyInfo, "Enter a valid selection.");

			rate1.PHE_RX_NKCurrency = "USD";
			AssertNoErrors(rate1.PHE_RX_NKCurrencyInfo);
		}

		public void TestCheckPHE_Rate()
		{
			var priceHeader = Factory.NewWithValidTestData<ClientLicencePriceHeader>();
			var rate1 = priceHeader.ExchangeRates.AddNew();

			rate1.PHE_Rate = 0;
			AssertHasError(rate1.PHE_RateInfo, "value cannot be zero.");

			rate1.PHE_Rate = -1;
			AssertHasError(rate1.PHE_RateInfo, "value cannot be negative.");

			rate1.PHE_Rate = 1;
			AssertNoErrors(rate1.PHE_RateInfo);
		}
	}
}