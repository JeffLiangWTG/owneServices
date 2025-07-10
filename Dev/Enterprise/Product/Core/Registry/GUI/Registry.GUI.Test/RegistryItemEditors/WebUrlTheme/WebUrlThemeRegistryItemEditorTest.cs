using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(WebUrlThemeRegistryItemEditor))]
	sealed class WebUrlThemeRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new WebUrlThemeRegistryItemEditor(new WebThemeUrlRegistryDataType(), null, Factory);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((WebUrlThemeControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(WebUrlThemeControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new WebThemeUrlRegistryItem("", null, null, null, RegistryStorageFlags.System, new WebThemeUrlCollection());
		}

		protected override object[] GetValidRegistryValues()
		{
			WebThemeUrlCollection collection = new WebThemeUrlCollection();
			collection.Add(new WebThemeUrl()
			{
				ThemeName = "Standard",
				Url = "http://localhost"
			});
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
