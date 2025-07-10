using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(WebThemeRegistryItemEditor))]
	sealed class WebTrackerThemeRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		public void TestUpdateValueFromEditorPaneDoesNotUpdateRegistryValue()
		{
			using (Control editorPane = Editor.NewWinFormsEditorPane())
			{
				var themes = (WebTrackerTheme[])GetValidRegistryValues()[1];
				Editor.SetValueFromEditorPane(editorPane, themes);
				var values = (WebTrackerTheme[])Editor.GetValueFromEditorPane(editorPane);

				AssertEquals("Before editor pane value is updated", themes[0], values[0]);

				values[0].Code = "CUS";

				AssertNotEquals("After editor pane value is updated", themes[0], values[0]);
			}
		}

		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new WebThemeRegistryItemEditor(new WebThemeRegistryDataType(), WebDataRegistry.Instance.WebTrackerTheme);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((WebThemeControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(WebThemeControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new WebThemeRegistryItem("", WebDataRegistry.Instance.WebTrackerTheme.UrlsRegistryItem, null, null, null, RegistryStorageFlags.System);
		}

		public new void TestSetAndGetValueFromEditorPane()
		{
			var urls = GetValidRegistryValues().Cast<WebTrackerTheme[]>().SelectMany(x => x.Select(y => y.Url)).ToArray();
			WebDataRegistry.Instance.WebTrackerUrls.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, urls);
			base.TestSetAndGetValueFromEditorPane();
		}

		protected override object[] GetValidRegistryValues()
		{
			var theme0 = new WebTrackerTheme("", "STD");
			var theme1 = new WebTrackerTheme("webtracker1.com/", "STD");
			var theme2 = new WebTrackerTheme("webtracker2.com/", "ALT");
			var theme3 = new WebTrackerTheme("webtracker3.com/", "CLS");

			return new object[]
				{
					new WebTrackerTheme[] { theme0, theme1 },
					new WebTrackerTheme[] { theme0, theme1, theme2, theme3 },
				};
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.TopLeft; }
		}

		#endregion
	}
}
