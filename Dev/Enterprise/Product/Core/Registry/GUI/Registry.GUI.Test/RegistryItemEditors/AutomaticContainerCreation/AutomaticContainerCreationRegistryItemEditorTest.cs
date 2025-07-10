using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(AutomaticContainerCreationRegistryItemEditor))]
	sealed class AutomaticContainerCreationRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor() => new AutomaticContainerCreationRegistryItemEditor(new AutomaticContainerCreationRegistryDataType(), null, null);

		protected override bool GetEditorPaneEnabledState(Control editorPane) => !((AutomaticContainerCreationControl)editorPane).ReadOnly;

		protected override Type GetExpectedEditorPaneType() => typeof(AutomaticContainerCreationControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel() => new AutomaticContainerCreationRegistryItem("", null, null, null);

		protected override object[] GetValidRegistryValues() => new object[] { new AutomaticContainerCreation() };
	}
}
