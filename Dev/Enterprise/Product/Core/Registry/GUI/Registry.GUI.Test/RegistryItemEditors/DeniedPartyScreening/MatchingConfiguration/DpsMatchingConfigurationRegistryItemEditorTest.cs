using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(DpsMatchingConfigurationRegistryItemEditor))]
	sealed class DpsMatchingConfigurationRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor() => new DpsMatchingConfigurationRegistryItemEditor(new DpsMatchingConfigurationRegistryDataType(), null, null);

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((DpsMatchingConfigurationControl)editorPane).ReadOnly;

		protected override Type GetExpectedEditorPaneType() => typeof(DpsMatchingConfigurationControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new DpsMatchingConfigurationRegisrtyItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new DpsMatchingConfigurationBusinessObject());

		protected override object[] GetValidRegistryValues() => new object[] { new DpsMatchingConfigurationBusinessObject() };

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
	}
}
