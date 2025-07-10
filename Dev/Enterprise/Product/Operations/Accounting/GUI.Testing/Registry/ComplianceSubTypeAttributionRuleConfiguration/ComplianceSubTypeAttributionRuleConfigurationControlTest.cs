using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(ComplianceSubTypeAttributionRuleConfigurationControl))]
	class ComplianceSubTypeAttributionRuleConfigurationControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new ComplianceSubTypeAttributionRuleConfigurationCollection();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((ComplianceSubTypeAttributionRuleConfigurationControl)control).ComplianceSubTypeAttributionRuleConfigurationGrid_ForTestOnly.ReadOnly;
		}

		public void TestGridColumnConfigurations()
		{
			using (var form = new ComplianceSubTypeAttributionRuleConfigurationControl())
			{
				form.Show();

				var grid = form.Controls.Find("ComplianceSubTypeAttributionRuleConfigurationGrid", true)[0] as ZGrid;
				AssertNotNull(grid);

				AssertGridColumnInfo(grid, "Country", typeof(ZCodeFindBoxColumnStyle), 80, true, false, true, "Select Country");
				AssertGridColumnInfo(grid, "SubType", typeof(ZDropEditColumnStyle), 80, false, false, true, null);
				AssertGridColumnInfo(grid, "Description", typeof(ZTextBoxColumnStyle), 80, false, false, true, null);
				AssertGridColumnInfo(grid, "DocumentTitle", typeof(ZTextBoxColumnStyle), 120, false, true, true, null);
				AssertGridColumnInfo(grid, "LedgerType", typeof(ZDropEditColumnStyle), 80, false, false, true, null);
				AssertGridColumnInfo(grid, "InvoiceType", typeof(ZDropEditColumnStyle), 80, false, false, true, null);
				AssertGridColumnInfo(grid, "TaxInvoiceRule", typeof(ZDropEditColumnStyle), 80, false, false, true, null);
				AssertGridColumnInfo(grid, "TaxIDCode", typeof(ZTextBoxColumnStyle), 80, false, false, true, null);
				AssertGridColumnInfo(grid, "OriginalRule", typeof(ZDropEditColumnStyle), 80, false, false, true, null);
				AssertGridColumnInfo(grid, "DisbursementRule", typeof(ZDropEditColumnStyle), 80, false, false, true, null);
				AssertGridColumnInfo(grid, "OrganisationLocation", typeof(ZDropEditColumnStyle), 80, false, false, true, null);
				AssertGridColumnInfo(grid, "TaxRegistrationType", typeof(ZDropEditColumnStyle), 80, false, false, true, null);
				AssertGridColumnInfo(grid, "SelfBillingRule", typeof(ZDropEditColumnStyle), 80, false, false, true, null);
				AssertGridColumnInfo(grid, "TaxRegistrationLocationRule", typeof(ZDropEditColumnStyle), 80, false, false, true, null);
				AssertGridColumnInfo(grid, "VATGroupRule", typeof(ZDropEditColumnStyle), 80, false, false, true, null);
				AssertGridColumnInfo(grid, "ParentTransactionSubType", typeof(ZDropEditColumnStyle), 80, false, false, true, null);
				AssertGridColumnInfo(grid, "ExporterExemption", typeof(ZDropEditColumnStyle), 80, false, false, true, null);
				AssertGridColumnInfo(grid, "RuleSetCode", typeof(ZTextBoxColumnStyle), 80, false, true, false, null);
				AssertGridColumnInfo(grid, "RuleSetDescription", typeof(ZTextBoxColumnStyle), 80, false, true, false, null);
				AssertGridColumnInfo(grid, "RequiredTaxSystem", typeof(ZDropEditColumnStyle), 53, false, false, true, null);
				AssertGridColumnInfo(grid, "ExcludedTaxSystem", typeof(ZDropEditColumnStyle), 53, false, false, true, null);
				AssertGridColumnInfo(grid, "RequiredRegistrationCode", typeof(ZDropEditColumnStyle), 80, false, false, true, null);
				AssertGridColumnInfo(grid, "ExcludedRegistrationCode", typeof(ZDropEditColumnStyle), 80, false, false, true, null);
				AssertGridColumnInfo(grid, "ThresholdApplies", typeof(ZCheckBoxColumnStyle), 80, false, false, true, null);
				AssertGridColumnInfo(grid, "SubTypeThresholdNotMet", typeof(ZDropEditColumnStyle), 80, false, false, true, null);
			}
		}

		void AssertGridColumnInfo(ZGrid grid, ZString columnName, Type expectedStyleType, ZInt expectedWidth, ZBool expectedMandatory, ZBool expectedReadOnly, ZBool expectedVisible, string expectedPopupCaption)
		{
			var columnInfo = grid.GetColumnStyle(columnName);

			AssertNotNull(columnName, columnInfo);
			AssertEquals($"{columnName} Style Type", expectedStyleType, columnInfo.ColumnStyleType);
			AssertEquals($"{columnName} Width", expectedWidth, columnInfo.Width);
			AssertEquals($"{columnName} IsMandatory", expectedMandatory, columnInfo.IsMandatory);
			AssertEquals($"{columnName} IsReadOnly", expectedReadOnly, columnInfo.IsReadOnly);
			AssertEquals($"{columnName} IsVisible", expectedVisible, columnInfo.IsVisible);
			AssertEquals($"{columnName} Popup Caption", expectedPopupCaption, (columnInfo as ZBaseFindBoxColumnStyleInfo)?.PopupCaption);
		}
	}
}
