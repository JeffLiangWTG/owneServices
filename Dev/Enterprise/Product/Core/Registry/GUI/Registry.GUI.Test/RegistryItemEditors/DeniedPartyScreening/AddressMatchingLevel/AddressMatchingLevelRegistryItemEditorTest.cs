using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(AddressMatchingLevelRegistryItemEditor))]
	sealed class AddressMatchingLevelRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor() => new AddressMatchingLevelRegistryItemEditor(new AddressMatchingLevelRegistryDataType(), null, null);

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((AddressMatchingLevelControl)editorPane).ReadOnly;

		protected override Type GetExpectedEditorPaneType() => typeof(AddressMatchingLevelControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new AddressMatchingLevelRegisrtyItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new AddressMatchingLevelBusinessObject());

		protected override object[] GetValidRegistryValues() => new object[] { new AddressMatchingLevelBusinessObject() };

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
	}
}
