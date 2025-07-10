using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	internal class ClientIncidentQuoteValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCIQ_RX_NKCurrency()
		{
			var quote = Factory.New<ClientIncidentQuote>();
			AssertEquals("Precondition: empty currency", ZString.Empty, quote.CIQ_RX_NKCurrency);
			quote.Validation.ValidateCIQ_RX_NKCurrency();
			AssertNoErrors("Precondition: no errors", quote.CIQ_RX_NKCurrencyInfo);

			quote.CIQ_CancellationFee = 1;
			quote.Validation.ValidateCIQ_RX_NKCurrency();
			AssertHasErrors("Error at no currency but has cancellation fee", quote.CIQ_RX_NKCurrencyInfo);
			quote.CIQ_CancellationFee = 0;

			quote.CIQ_QuoteAmount = 1;
			quote.Validation.ValidateCIQ_RX_NKCurrency();
			AssertHasErrors("Error at no currency but has monthly quote amount", quote.CIQ_RX_NKCurrencyInfo);
			quote.CIQ_QuoteAmount = 0;

			quote.CIQ_OneoffUpfront = 1;
			quote.Validation.ValidateCIQ_RX_NKCurrency();
			AssertHasErrors("Error at no currency but has one-off amount", quote.CIQ_RX_NKCurrencyInfo);
			quote.CIQ_OneoffUpfront = 0;

			quote.CIQ_HeadStartSurcharge = 1;
			quote.Validation.ValidateCIQ_RX_NKCurrency();
			AssertHasErrors("Error at no currency but has head start surcharge", quote.CIQ_RX_NKCurrencyInfo);
			quote.CIQ_HeadStartSurcharge = 0;

			quote.CIQ_ExpressDeliverySurcharge = 1;
			quote.Validation.ValidateCIQ_RX_NKCurrency();
			AssertHasErrors("Error at no currency but has express del. surcharge", quote.CIQ_RX_NKCurrencyInfo);
			quote.CIQ_ExpressDeliverySurcharge = 0;

			quote.Validation.ValidateCIQ_RX_NKCurrency();
			AssertNoErrors("Precondition: no errors", quote.CIQ_RX_NKCurrencyInfo);
			quote.CIQ_RX_NKCurrency = "111";
			quote.Validation.ValidateCIQ_RX_NKCurrency();
			AssertHasErrors("Error at invalid currency", quote.CIQ_RX_NKCurrencyInfo);

			quote.CIQ_RX_NKCurrency = "AUD";
			quote.Validation.ValidateCIQ_RX_NKCurrency();
			AssertNoErrors("No errors at valid currency", quote.CIQ_RX_NKCurrencyInfo);
		}
	}
}