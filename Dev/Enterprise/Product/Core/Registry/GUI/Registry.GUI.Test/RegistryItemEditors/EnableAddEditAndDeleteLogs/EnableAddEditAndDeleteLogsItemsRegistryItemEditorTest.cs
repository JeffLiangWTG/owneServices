using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(EnableAddEditAndDeleteLogsItemsRegistryItemEditor))]
	sealed class EnableAddEditAndDeleteLogsItemsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new EnableAddEditAndDeleteLogsItemCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsHidden, new EnableAddEditAndDeleteLogsItemCollection());
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new EnableAddEditAndDeleteLogsItemsRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(EnableAddEditAndDeleteLogsItemsControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new EnableAddEditAndDeleteLogsItemCollection() };
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((EnableAddEditAndDeleteLogsItemsControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
	}
}
