using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(NewsSectionRegistryItemEditor))]
	sealed class NewsSectionRegistryItemEditorTest : Enterprise.Registry.GUI.Testing.RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new NewsSectionRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((NewsSectionControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(NewsSectionControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new NewsSectionRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new NewsSectionCollection();
			var item1 = collection.AddNew();
			item1.LayoutPanelID = "Bottom-Left";
			item1.SectionID = NewsSectionTypeList.Codes.ClientNews;

			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
