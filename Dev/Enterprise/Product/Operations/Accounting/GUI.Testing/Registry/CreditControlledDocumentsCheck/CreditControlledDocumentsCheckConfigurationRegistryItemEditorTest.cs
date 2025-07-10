using System;
using System.Windows.Forms;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(CreditControlledDocumentsCheckConfigurationRegistryItemEditor))]
	public class CreditControlledDocumentsCheckConfigurationRegistryItemEditorTest : RegistryItemEditorTestCase
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
			return new CreditControlledDocumentsCheckConfigurationRegistryItemEditor(RegistryItem.DataType, new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CreditControlledDocumentsCheckConfigurationControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CreditControlledDocumentsCheckConfigurationControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CreditControlledDocumentsCheckConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.Company);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new CreditControlledDocumentsCheckConfigurationCollection();

			CreditControlledDocumentsCheckConfiguration configuration = collection.AddNew();
			configuration.InvoiceType = OrgARTermsLookups.InvoiceTypes.All.Code;
			configuration.NumberOfDaysOverdue = 1;
			configuration.Amount = 0;
			configuration.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			configuration.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;

			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
