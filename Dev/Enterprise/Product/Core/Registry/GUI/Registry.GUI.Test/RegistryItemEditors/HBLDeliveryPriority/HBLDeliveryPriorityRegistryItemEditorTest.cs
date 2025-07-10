using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(HBLDeliveryPriorityRegistryItemEditor))]
	sealed class HBLDeliveryPriorityRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
			=> new HBLDeliveryPriorityRegistryItemEditor(RegistryItem.DataType, null, null);

		protected override bool GetEditorPaneEnabledState(Control editorPane)
			=> !((HBLDeliveryPriorityControl)editorPane).ReadOnly;

		protected override Type GetExpectedEditorPaneType()
			=> typeof(HBLDeliveryPriorityControl);

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
			=> new HBLDeliveryPriorityRegistryItem("", null, null, null, RegistryStorageFlags.System,RegistryOptions.IsOnlyForSupport);

		protected override object[] GetValidRegistryValues()
			=> new object[] { new HBLDeliveryPriorityConfigCollection() };

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
			=> RegistryItemEditor.EditorPaneAnchor.All;
	}
}
