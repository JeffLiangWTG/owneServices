using System;
using System.Windows.Forms;
using Enterprise.Accounting.GUI;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(ComplianceDocumentRePrintRestrictionRegistryItemEditor))]
	public class ComplianceDocumentRePrintRestrictionRegistryItemEditorTest : RegistryItemEditorTestCase
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
			return new ComplianceDocumentRePrintRestrictionRegistryItemEditor(RegistryItem.DataType, new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((ComplianceDocumentRePrintRestrictionControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(ComplianceDocumentRePrintRestrictionControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new ComplianceDocumentRePrintRestrictionRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.Default);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new ComplianceDocumentRePrintRestrictionCollection();
			var complianceSubTypeDependencyConfiguration = collection.AddNew();
			complianceSubTypeDependencyConfiguration.OrganizationCategory = "NAT";
			complianceSubTypeDependencyConfiguration.NumberOfReprintAllowed = 1;
			complianceSubTypeDependencyConfiguration.ComplianceDocumentMenu = "AAA";
			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
