using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	public class EDIOrgOpportunityValidationTest : BusinessObjectValidationTestCase
	{
		public void TestP8_DiscountAmount()
		{
			var opp = Factory.New<EDIOrgOpportunity>();
			opp.P8_DiscountAmount = 0;
			AssertNoNotifications(opp.P8_DiscountAmountInfo);

			opp.P8_DiscountAmount = 1;
			AssertNoNotifications(opp.P8_DiscountAmountInfo);

			opp.P8_DiscountAmount = -1;
			AssertHasError(opp.P8_DiscountAmountInfo, "Contracted cannot be negative.");
		}

		public void TestLocalCurrency()
		{
			EDIDataRegistry.Instance.OpportunityExchangeRateCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GlbCompany.CurrentCompany.PK.ToGuid());
			var opp = Factory.New<EDIOrgOpportunity>();
			opp.RunPreSaveValidation();
			AssertNoNotifications(opp.P8_Calc_ContractValueLocalCurrencyInfo);
			AssertNoNotifications(opp.P8_Calc_TotalLocalValueCurrencyInfo);

			EDIDataRegistry.Instance.OpportunityExchangeRateCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			opp.RunPreSaveValidation();
			AssertHasWarning(opp.P8_Calc_ContractValueLocalCurrencyInfo, "The Registry Item 'Opportunity Exchange Rate Company' is invalid, the exchange rate cannot be calculated.");
			AssertHasWarning(opp.P8_Calc_TotalLocalValueCurrencyInfo, "The Registry Item 'Opportunity Exchange Rate Company' is invalid, the exchange rate cannot be calculated.");
		}
	}
}