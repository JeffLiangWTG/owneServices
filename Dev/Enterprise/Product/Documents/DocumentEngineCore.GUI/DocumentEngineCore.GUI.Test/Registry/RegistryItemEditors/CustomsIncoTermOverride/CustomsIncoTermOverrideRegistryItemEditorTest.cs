using System;
using System.Windows.Forms;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.GUI.Registry.Testing
{
	[TestedType(typeof(CustomsIncoTermOverrideRegistryItemEditor))]
	sealed class CustomsIncoTermOverrideRegistryItemEditorTest : RegistryItemEditorTestCase
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

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CustomsIncoTermOverrideCollectionRegistryItem("", null, null, null, RegistryOptions.Default);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new CustomsIncoTermOverrideRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CustomsIncoTermOverrideControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new CustomsIncoTermOverrideCollection() };
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CustomsIncoTermOverrideControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
