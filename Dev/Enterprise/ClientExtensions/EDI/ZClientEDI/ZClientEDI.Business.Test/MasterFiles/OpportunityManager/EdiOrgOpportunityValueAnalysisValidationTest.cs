using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	internal class EdiOrgOpportunityValueAnalysisValidationTest : BusinessObjectValidationTestCase
	{
		public void TestLocalCurrency()
		{
			EDIDataRegistry.Instance.OpportunityExchangeRateCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GlbCompany.CurrentCompany.PK.ToGuid());
			var opp = Factory.New<EDIOrgOpportunity>();
			opp.RunPreSaveValidation();
			var ediOrgOpportunityValueAnalysis = opp.ValueAnalysisCollection[0];
			AssertNoNotifications(ediOrgOpportunityValueAnalysis.EOV_Calc_LocalValueCurrencyInfo);

			EDIDataRegistry.Instance.OpportunityExchangeRateCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			opp.RunPreSaveValidation();
			AssertHasWarning(ediOrgOpportunityValueAnalysis.EOV_Calc_LocalValueCurrencyInfo, "The Registry Item 'Opportunity Exchange Rate Company' is invalid, the exchange rate cannot be calculated.");
		}
	}
}