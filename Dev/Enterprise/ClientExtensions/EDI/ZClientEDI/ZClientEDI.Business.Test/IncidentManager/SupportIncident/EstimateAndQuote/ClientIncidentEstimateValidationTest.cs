using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	internal class ClientIncidentEstimateValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCIE_RX_NKCurrency()
		{
			var estimate = Factory.New<ClientIncidentEstimate>();
			AssertEquals("Precondition: empty currency", ZString.Empty, estimate.CIE_RX_NKCurrency);
			estimate.Validation.ValidateCIE_RX_NKCurrency();
			AssertNoErrors("Precondition: no errors", estimate.CIE_RX_NKCurrencyInfo);

			estimate.CIE_CancellationFee = 1;
			estimate.Validation.ValidateCIE_RX_NKCurrency();
			AssertHasErrors("Error at no currency but has cancellation fee", estimate.CIE_RX_NKCurrencyInfo);
			estimate.CIE_CancellationFee = 0;

			estimate.CIE_MinEstimateMonthly = 1;
			estimate.Validation.ValidateCIE_RX_NKCurrency();
			AssertHasErrors("Error at no currency but has min estimate monthly amount", estimate.CIE_RX_NKCurrencyInfo);
			estimate.CIE_MinEstimateMonthly = 0;

			estimate.CIE_MaxEstimateMonthly = 1;
			estimate.Validation.ValidateCIE_RX_NKCurrency();
			AssertHasErrors("Error at no currency but has max estimate monthly amount", estimate.CIE_RX_NKCurrencyInfo);
			estimate.CIE_MaxEstimateMonthly = 0;

			estimate.CIE_MinEstimateOneoff = 1;
			estimate.Validation.ValidateCIE_RX_NKCurrency();
			AssertHasErrors("Error at no currency but has min estimate one-off amount", estimate.CIE_RX_NKCurrencyInfo);
			estimate.CIE_MinEstimateOneoff = 0;

			estimate.CIE_MaxEstimateOneoff = 1;
			estimate.Validation.ValidateCIE_RX_NKCurrency();
			AssertHasErrors("Error at no currency but has max estimate one-off amount", estimate.CIE_RX_NKCurrencyInfo);
			estimate.CIE_MaxEstimateOneoff = 0;

			estimate.Validation.ValidateCIE_RX_NKCurrency();
			AssertNoErrors("Precondition: no errors", estimate.CIE_RX_NKCurrencyInfo);
			estimate.CIE_RX_NKCurrency = "111";
			estimate.Validation.ValidateCIE_RX_NKCurrency();
			AssertHasErrors("Error at invalid currency", estimate.CIE_RX_NKCurrencyInfo);

			estimate.CIE_RX_NKCurrency = "AUD";
			estimate.Validation.ValidateCIE_RX_NKCurrency();
			AssertNoErrors("No errors at valid currency", estimate.CIE_RX_NKCurrencyInfo);
		}
	}
}