using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(InvoiceLineValuationIndicatorDropEditsUserControl))]
	sealed class InvoiceLineValuationIndicatorDropEditsUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var control = new InvoiceLineValuationIndicatorDropEditsUserControl())
			{
				var groupBox = control.FindSingleOrDefault<ZGroupBox>("OverrideValuationIndicatorsGroupBox");
				var partyDropEdit = groupBox.FindSingleOrDefault<ZDropEdit>("PartyRelationShipDropEdit");
				var restrictionsDropEdit = groupBox.FindSingleOrDefault<ZDropEdit>("RestrictionsDropEdit");
				var saleDropEdit = groupBox.FindSingleOrDefault<ZDropEdit>("SaleConditionsDropEdit");
				var disposalDropEdit = groupBox.FindSingleOrDefault<ZDropEdit>("DisposalAccrualDropEdit");

				CombineAssertions(() =>
				{
					AssertNotNull("OverrideValuationIndicatorsGroupBox", groupBox);
					AssertEquals("OverrideValuationIndicatorsGroupBox Caption", "Override Valuation Indicators", groupBox.CaptionResourceString.Caption);

					AssertNotNull("PartyRelationShipDropEdit", partyDropEdit);
					AssertEquals("PartyRelationShipDropEdit readonly", false, partyDropEdit.ReadOnly);
					AssertEquals("PartyRelationShipDropEdit binding", "FilteredInvoiceLines.JI_RelatedIndicator", partyDropEdit.BindTo);
					Assert("PartyRelationShipDropEdit SupportsEmptyCode", partyDropEdit.SupportsEmptyCode);
					AssertEquals("PartyRelationShipDropEdit Width", 360, partyDropEdit.Width);

					AssertNotNull("RestrictionsDropEdit", restrictionsDropEdit);
					AssertEquals("RestrictionsDropEdit readonly", false, restrictionsDropEdit.ReadOnly);
					AssertEquals("RestrictionsDropEdit binding", "FilteredInvoiceLines.ZG_RelatedIndicator2", restrictionsDropEdit.BindTo);
					Assert("RestrictionsDropEdit SupportsEmptyCode", restrictionsDropEdit.SupportsEmptyCode);
					AssertEquals("RestrictionsDropEdit Width", 360, restrictionsDropEdit.Width);

					AssertNotNull("SaleConditionsDropEdit", saleDropEdit);
					AssertEquals("SaleConditionsDropEdit readonly", false, saleDropEdit.ReadOnly);
					AssertEquals("SaleConditionsDropEdit binding", "FilteredInvoiceLines.ZG_RelatedIndicator3", saleDropEdit.BindTo);
					Assert("SaleConditionsDropEdit SupportsEmptyCode", saleDropEdit.SupportsEmptyCode);
					AssertEquals("SaleConditionsDropEdit Width", 360, saleDropEdit.Width);

					AssertNotNull("DisposalAccrualDropEdit", disposalDropEdit);
					AssertEquals("DisposalAccrualDropEdit readonly", false, disposalDropEdit.ReadOnly);
					AssertEquals("DisposalAccrualDropEdit binding", "FilteredInvoiceLines.ZG_RelatedIndicator4", disposalDropEdit.BindTo);
					Assert("DisposalAccrualDropEdit SupportsEmptyCode", disposalDropEdit.SupportsEmptyCode);
					AssertEquals("DisposalAccrualDropEdit Width", 360, disposalDropEdit.Width);
				});
			}
		}
	}
}
