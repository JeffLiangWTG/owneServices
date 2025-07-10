using System;
using System.Windows.Forms;
using Enterprise.Client.UPE.Registry.Business;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Registry.GUI.Testing
{
	[TestedType(typeof(ShippingAgentRegistryItemEditor))]
	public class ShippingAgentRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		new class RegistryFormForTest : RegistryItemEditorTestCase.RegistryFormForTest
		{
			public RegistryFormForTest(IRegistryItem registryItem) : base(registryItem)
			{
			}

			public override void DisplayRegistryItem()
			{
				RegistriesTreeView.SelectedNode = RegistriesTreeView.Nodes[0];
				FallbackTreeView.SelectedNode = FallbackTreeView.Nodes[0].Nodes[0];
			}
		}

		protected override RegistryItemEditorTestCase.RegistryFormForTest GetNewRegistryFormForTest(IRegistryItem registryItem) => new RegistryFormForTest(registryItem);
		protected override RegistryItemEditor GetEditor() => new ShippingAgentRegistryItemEditor(RegistryItem.DataType, null, null);
		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((ShippingAgentControl)editorPane).ReadOnly;
		protected override Type GetExpectedEditorPaneType() => typeof(ShippingAgentControl);
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new ShippingAgentRegistryItem("NAME", "CATERGORY", "CAPTION", "HINT");
		protected override object[] GetValidRegistryValues() => new object[] { new ShippingAgentObject() };
		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
	}
}
