using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(AWBExtraTextControlRegistryItemEditor))]
	sealed class AWBExtraTextControlRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CodeDescriptionPairListRegistryItem("TEST_REGISTRY_ITEM", null, null, null, 10, RegistryStorageFlags.System);
		}

		protected override RegistryItemEditor GetEditor()
		{
			CodeDescriptionPairListRegistryItem registryItem = new CodeDescriptionPairListRegistryItem("TEST_REGISTRY_ITEM", null, null, null, 3, RegistryStorageFlags.System);
			return new AWBExtraTextControlRegistryItemEditor(registryItem, new CodeDescriptionPairListRegistryDataType(10), new FreightDataRegistry.AWBGridRegistryEditorInfo());
		}

		protected override System.Type GetExpectedEditorPaneType()
		{
			return typeof(AWBExtraTextControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("Code", "Description");
			return new[] { list };
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((AWBExtraTextControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
