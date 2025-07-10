using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(ComplianceSubTypeAttributionRuleConfigurationRegistryItemEditor))]
	public class ComplianceSubTypeAttributionRuleConfigurationRegistryItemEditorTest : RegistryItemEditorTestCase
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
				FallbackTreeView.SelectedNode = FallbackTreeView.Nodes[0].Nodes[0];
			}
		}

		protected override RegistryItemEditorTestCase.RegistryFormForTest GetNewRegistryFormForTest(IRegistryItem registryItem)
		{
			return new RegistryFormForTest(registryItem);
		}

		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new ComplianceSubTypeAttributionRuleConfigurationRegistryItemEditor(RegistryItem.DataType, new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ComplianceSubTypeAttributionRuleConfigurationControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ComplianceSubTypeAttributionRuleConfigurationControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ComplianceSubTypeAttributionRuleConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport);
		}

		protected override object[] GetValidRegistryValues()
		{
			ComplianceSubTypeAttributionRuleConfigurationCollection collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();

			ComplianceSubTypeAttributionRuleConfiguration complianceSubTypeAttributionRuleConfiguration = collection.AddNew();
			complianceSubTypeAttributionRuleConfiguration.Country = "PE";
			complianceSubTypeAttributionRuleConfiguration.SubType = "TXI";
			complianceSubTypeAttributionRuleConfiguration.LedgerType = "AR";
			complianceSubTypeAttributionRuleConfiguration.InvoiceType = "INV";
			complianceSubTypeAttributionRuleConfiguration.TaxInvoiceRule = "AMT";
			complianceSubTypeAttributionRuleConfiguration.OriginalRule = "ALL";
			complianceSubTypeAttributionRuleConfiguration.DisbursementRule = "ALL";
			complianceSubTypeAttributionRuleConfiguration.TaxRegistrationType = "DNI";

			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
