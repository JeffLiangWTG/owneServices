using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	internal class EdiOrgOpportunityExValidationTest : BusinessObjectValidationTestCase
	{
		public void TestEOM_GlobalPotential()
		{
			var opp = Factory.New<EDIOrgOpportunity>();
			opp.OrgOpportunityEx.EOM_GlobalPotential = 0;
			AssertNoNotifications(opp.OrgOpportunityEx.EOM_GlobalPotentialInfo);

			opp.OrgOpportunityEx.EOM_GlobalPotential = 1;
			AssertNoNotifications(opp.OrgOpportunityEx.EOM_GlobalPotentialInfo);

			opp.OrgOpportunityEx.EOM_GlobalPotential = -1;
			AssertHasError(opp.OrgOpportunityEx.EOM_GlobalPotentialInfo, "Global Reach cannot be negative.");
		}

		public void TestEOM_RX_NKLifetimeValueCurrency()
		{
			var opp = Factory.New<EDIOrgOpportunity>();
			opp.OrgOpportunityEx.EOM_RX_NKLifetimeValueCurrency = "AUD";
			AssertNoNotifications(opp.OrgOpportunityEx.EOM_GlobalPotentialInfo);
			opp.OrgOpportunityEx.EOM_RX_NKLifetimeValueCurrency = "USD";
			AssertNoNotifications(opp.OrgOpportunityEx.EOM_GlobalPotentialInfo);

			opp.OrgOpportunityEx.EOM_RX_NKLifetimeValueCurrency = "";
			AssertHasError(opp.OrgOpportunityEx.EOM_RX_NKLifetimeValueCurrencyInfo, "Please enter a value.");

			opp.OrgOpportunityEx.EOM_RX_NKLifetimeValueCurrency = "US1";
			AssertHasError(opp.OrgOpportunityEx.EOM_RX_NKLifetimeValueCurrencyInfo, "Enter a valid selection.");
		}

		public void TestLocalCurrency()
		{
			EDIDataRegistry.Instance.OpportunityExchangeRateCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GlbCompany.CurrentCompany.PK.ToGuid());
			var opp = Factory.New<EDIOrgOpportunity>();
			opp.RunPreSaveValidation();
			AssertNoNotifications(opp.OrgOpportunityEx.EOM_Calc_LifetimeValueOver3YearsLocalCurrencyInfo);

			EDIDataRegistry.Instance.OpportunityExchangeRateCompany.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			opp.RunPreSaveValidation();
			AssertHasWarning(opp.OrgOpportunityEx.EOM_Calc_LifetimeValueOver3YearsLocalCurrencyInfo, "The Registry Item 'Opportunity Exchange Rate Company' is invalid, the exchange rate cannot be calculated.");
		}
	}
}