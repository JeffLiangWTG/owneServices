using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Security.ActiveDirectory.GUI.Test
{
	[TestedType(typeof(ADPasswordSettingsRegistryEditor))]
	class ADPasswordSettingsRegistryEditorTest : RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new ADPasswordSettingsRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.MustOverrideDefaultValue);

		protected override RegistryItemEditor GetEditor() => new ADPasswordSettingsRegistryEditor(RegistryItem.DataType, null, Factory);

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((ADPasswordSettingsRegistryControl)editorPane).ReadOnly;

		protected override Type GetExpectedEditorPaneType() => typeof(ADPasswordSettingsRegistryControl);

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;

		protected override object[] GetValidRegistryValues() => new object[] { new ADPasswordSettingsRegistryBusinessObject() };
	}
}
