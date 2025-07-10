using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(AutoratingViaPortRegistryItemEditor))]
	sealed class AutoratingViaPortRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
			=> new AutoratingViaPortRegistryItemEditor(RegistryItem.DataType, null, null);

		protected override bool GetEditorPaneEnabledState(Control editorPane)
			=> !((AutoratingViaPortControl)editorPane).ReadOnly;

		protected override Type GetExpectedEditorPaneType()
			=> typeof(AutoratingViaPortControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
			=> new AutoratingViaPortRegistryItem("", null, null, null, RegistryStorageFlags.System, AutoratingViaPortConfigurationCollection.Default);

		protected override object[] GetValidRegistryValues()
			=> new object[] { AutoratingViaPortConfigurationCollection.Default };

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
			=> RegistryItemEditor.EditorPaneAnchor.All;
	}
}
