using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(DropDownCodeDescriptionBoolRegistryItemEditor))]
	sealed class DropDownCodeDescriptionBoolRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new DropDownCodeDescriptionBoolRegistryItem("TEST_REGISTRY_ITEM", null, null, null, RegistryStorageFlags.System, (NoResString)"Enabled", new DropDownCodeDescriptionBoolCollection(), 3, null, null);
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new DropDownCodeDescriptionBoolRegistryItemEditor(new DropDownCodeDescriptionBoolRegistryDataType(3, null, null), null);
		}

		protected override System.Type GetExpectedEditorPaneType()
		{
			return typeof(DropDownCodeDescriptionBoolRegistryControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new DropDownCodeDescriptionBoolCollection(16, ManagerSecurityMappingDropDownProvider.MangerMappingCodeLookup, ManagerSecurityMappingDropDownProvider.MangerMappingDescriptionLookup);
			collection.Add("MANAGERSECURITY1", (NoResString)"DRM", true);
			return new[] { collection };
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((DropDownCodeDescriptionBoolRegistryControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
