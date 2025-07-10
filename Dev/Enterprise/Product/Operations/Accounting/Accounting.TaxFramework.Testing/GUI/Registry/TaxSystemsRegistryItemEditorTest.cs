using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.TaxFramework.GUI.Testing
{
	[TestedType(typeof(TaxSystemsRegistryItemEditor))]
	public class TaxSystemsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		new class RegistryFormForTest : RegistryItemEditorTestCase.RegistryFormForTest
		{
			public RegistryFormForTest(IRegistryItem registryItem)
				: base(registryItem)
			{
			}

			public override void DisplayRegistryItem()
			{
				RegistriesTreeView.SelectedNode = RegistriesTreeView.Nodes[0];
				FallbackTreeView.SelectedNode = FallbackTreeView.Nodes[0];
			}
		}

		protected override RegistryItemEditorTestCase.RegistryFormForTest GetNewRegistryFormForTest(IRegistryItem registryItem)
		{
			return new RegistryFormForTest(registryItem);
		}

		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new TaxSystemsRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((TaxSystemsConfigurationControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(TaxSystemsConfigurationControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new TaxSystemsRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport);
		}

		protected override object[] GetValidRegistryValues()
		{
			TaxSystemsConfigurationCollection collection = new TaxSystemsConfigurationCollection();

			TaxSystemsConfiguration taxSystemsConfiguration = collection.AddNew();
			taxSystemsConfiguration.Code = "CW1TAXA1";
			taxSystemsConfiguration.Name = "CW1 TAX SYSTEM CODE1";
			taxSystemsConfiguration.Country = CountryCodes.Australia;
			taxSystemsConfiguration.TaxAuthorityType = AccountingMasterFilesTaxFrameworkConstants.TaxAuthorityTypeList.Municipal.Code;
			taxSystemsConfiguration.TaxSuperType = AccountingMasterFilesTaxFrameworkConstants.TaxSuperTypeList.SalesTax.Code;
			taxSystemsConfiguration.RegistrationLevel = AccountingMasterFilesTaxFrameworkConstants.TaxSystemRegistrationLevels.Branch.Code;
			taxSystemsConfiguration.IncludeInInvoceTotal = true;
			taxSystemsConfiguration.AdjustmentSign = AccountingMasterFilesTaxFrameworkConstants.TaxCalculationAdjustmentSigns.Positive.Code;
			taxSystemsConfiguration.TaxBaseCalculationMethod = AccountingMasterFilesTaxFrameworkConstants.TaxBaseCalculationMethods.InvoiceLineAmount.Code;
			taxSystemsConfiguration.TaxAmountCalculationMethod = AccountingMasterFilesTaxFrameworkConstants.TaxAmountCalculationMethods.BaseTimesRate.Code;
			taxSystemsConfiguration.ThresholdRule = AccountingMasterFilesTaxFrameworkConstants.ThresholdRulesForTaxCalculation.NoThreshold.Code;
			taxSystemsConfiguration.TaxRateSource = AccountingMasterFilesTaxFrameworkConstants.TaxRateSources.OrganisationFallbackToTaxGroup.Code;

			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
