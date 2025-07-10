using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(PersonMergePreviewItemsRegistryItemEditor))]
	sealed class PersonMergePreviewItemsRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new PersonMergePreviewItemCollectionRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, new PersonMergePreviewItemCollection());
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new PersonMergePreviewItemsRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(PersonMergePreviewItemsControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[] { new PersonMergePreviewItemCollection() };
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((PersonMergePreviewItemsControl)editorPane).ReadOnly;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor => RegistryItemEditor.EditorPaneAnchor.All;
	}
}
