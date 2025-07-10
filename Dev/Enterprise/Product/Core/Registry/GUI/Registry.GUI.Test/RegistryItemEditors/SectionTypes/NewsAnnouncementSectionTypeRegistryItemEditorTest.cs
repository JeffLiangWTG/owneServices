using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(NewsAnnouncementSectionTypeRegistryItemEditor))]
	sealed class NewsAnnouncementSectionTypeRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new NewsAnnouncementSectionTypeRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((NewsAnnouncementSectionTypeControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(NewsAnnouncementSectionTypeControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new NewsAnnouncementSectionTypeRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new NewsAnnouncementSectionTypeCollection();
			var item1 = collection.AddNew();
			item1.Code = "TST";
			item1.Description = (NoResString)"Some description";
			item1.OrderItemsBy = NewsSectionSortTypeList.Codes.Alphabetically;

			return new object[] { collection };
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
