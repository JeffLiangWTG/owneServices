using System;
using System.Windows.Forms;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Registry.GUI;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(IntercompanyEventConfigurationRegistryItemEditor))]
	public class IntercompanyEventConfigurationRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		new class RegistryFormForTest : RegistryItemEditorTestCase.RegistryFormForTest
		{
			public RegistryFormForTest(IRegistryItem registryItem)
				: base(registryItem)
			{ }

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
			return new IntercompanyEventConfigurationRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((IntercompanyEventConfigurationControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(IntercompanyEventConfigurationControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new IntercompanyEventConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default, new IntercompanyEventConfiguration());
		}

		protected override object[] GetValidRegistryValues()
		{
			IntercompanyEventConfiguration copy = new IntercompanyEventConfiguration();

			return new object[] { copy };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
