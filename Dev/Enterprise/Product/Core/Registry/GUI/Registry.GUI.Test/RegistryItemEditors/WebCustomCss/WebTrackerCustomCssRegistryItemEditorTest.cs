using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(WebCustomCssRegistryItemEditor))]
	sealed class WebTrackerCustomCssRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new WebCustomCssRegistryItemEditor(new WebCustomCssRegistryDataType(), WebDataRegistry.Instance.WebTrackerCustomCss);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return editorPane.Enabled;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(WebCustomCssControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new WebCustomCssRegistryItem("", WebDataRegistry.Instance.WebTrackerCustomCss.UrlsRegistryItem, null, null, null, RegistryStorageFlags.System);
		}

		[RequiresSTA]
		public new void TestSetAndGetValueFromEditorPane()
		{
			var urls = GetValidRegistryValues().Cast<WebTrackerCustomCss[]>().SelectMany(x => x.Select(y => y.Url)).ToArray();
			WebDataRegistry.Instance.WebTrackerUrls.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, urls);
			base.TestSetAndGetValueFromEditorPane();
		}

		protected override object[] GetValidRegistryValues()
		{
			var css1 = new WebTrackerCustomCss("webtracker1.com/", "body { color: red; }");
			var css2 = new WebTrackerCustomCss("webtracker2.com/", "body { color: blue; }");
			var css3 = new WebTrackerCustomCss("webtracker3.com/", "body { color: green; }");

			return new object[]
			{
				new WebTrackerCustomCss[] { css1 },
				new WebTrackerCustomCss[] { css1, css2, css3 },
			};
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}
	}
}
