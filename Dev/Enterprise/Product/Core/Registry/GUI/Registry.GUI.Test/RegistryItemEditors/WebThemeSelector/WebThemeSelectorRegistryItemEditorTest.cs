using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(WebThemeSelectorRegistryItemEditor))]
	sealed class WebThemeSelectorRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new WebThemeSelectorRegistryItemEditor(new WebThemeCustomObjectRegistryDataType(), null, Factory);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((WebThemeSelectorControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(WebThemeSelectorControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new WebThemeCustomObjectRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.PreserveTestValue, new WebThemeCustomObjectCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			var imageCollection = new WebCustomThemeImageBusinessObjectCollection();
			imageCollection.Add(new WebCustomThemeImageBusinessObject()
			{
				Data = new byte[] { 0, 1, 2, 3 },
				ImageName = "Image1.png"
			});

			WebThemeCustomObjectCollection collection = new WebThemeCustomObjectCollection();
			WebThemeCustomObject themeObject = new WebThemeCustomObject();
			themeObject.CSS = "";
			themeObject.ThemeName = "Test Theme";
			themeObject.ImageCollection.Add(imageCollection.First<WebCustomThemeImageBusinessObject>());
			collection.Add(themeObject);

			return new object[]
			{
				collection
			};
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
