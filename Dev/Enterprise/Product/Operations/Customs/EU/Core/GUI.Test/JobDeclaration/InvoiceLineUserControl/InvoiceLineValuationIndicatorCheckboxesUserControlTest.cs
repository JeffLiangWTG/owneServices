using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(InvoiceLineValuationIndicatorCheckboxesUserControl))]
	sealed class InvoiceLineValuationIndicatorCheckboxesUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var control = new InvoiceLineValuationIndicatorCheckboxesUserControl())
			{
				var partyCheckbox = control.FindSingleOrDefault<ZCheckBox>("TabPartyRelationShipCheckBox");
				var restrictionsCheckbox = control.FindSingleOrDefault<ZCheckBox>("TabRestrictionsShipCheckBox");
				var saleCheckbox = control.FindSingleOrDefault<ZCheckBox>("TabSaleConditionsShipCheckBox");
				var disposalCheckbox = control.FindSingleOrDefault<ZCheckBox>("TabDisposalAccrualShipCheckBox");

				CombineAssertions(() =>
				{
					AssertNotNull("PartyRelationShipCheckBox", partyCheckbox);
					AssertEquals("PartyRelationShipCheckBox visible", true, partyCheckbox.Visible);
					AssertEquals("PartyRelationShipCheckBox binding", "FilteredInvoiceLines.RelatedIndicator", partyCheckbox.BindTo);

					AssertNotNull("RestrictionsShipCheckBox", restrictionsCheckbox);
					AssertEquals("RestrictionsShipCheckBox visible", true, restrictionsCheckbox.Visible);
					AssertEquals("RestrictionsShipCheckBox binding", "FilteredInvoiceLines.RelatedIndicator2", restrictionsCheckbox.BindTo);

					AssertNotNull("SaleConditionsShipCheckBox", saleCheckbox);
					AssertEquals("SaleConditionsShipCheckBox visible", true, saleCheckbox.Visible);
					AssertEquals("SaleConditionsShipCheckBox binding", "FilteredInvoiceLines.RelatedIndicator3", saleCheckbox.BindTo);

					AssertNotNull("DisposalAccrualShipCheckBox", disposalCheckbox);
					AssertEquals("DisposalAccrualShipCheckBox visible", true, disposalCheckbox.Visible);
					AssertEquals("DisposalAccrualShipCheckBox binding", "FilteredInvoiceLines.RelatedIndicator4", disposalCheckbox.BindTo);
				});
			}
		}
	}
}
