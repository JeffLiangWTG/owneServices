using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(WebCustomImagesRegistryItemEditor))]
	sealed class WebTrackerCustomImagesRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new WebCustomImagesRegistryItemEditor(new WebCustomImagesRegistryDataType(), WebDataRegistry.Instance.WebTrackerCustomImages);
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((WebCustomImagesControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(WebCustomImagesControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new WebCustomImagesRegistryItem("", WebDataRegistry.Instance.WebTrackerCustomImages.UrlsRegistryItem, null, null, null, RegistryStorageFlags.System);
		}

		public new void TestSetAndGetValueFromEditorPane()
		{
			var urls = GetValidRegistryValues().Cast<WebTrackerCustomImage[]>().SelectMany(x => x.Select(y => y.Url)).ToArray();
			WebDataRegistry.Instance.WebTrackerUrls.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, urls);
			base.TestSetAndGetValueFromEditorPane();
		}

		protected override object[] GetValidRegistryValues()
		{
			var image1 = new WebTrackerCustomImage("Image1.png", "webtracker1.com/", new byte[] { 0, 1, 2, 3 });
			var image2 = new WebTrackerCustomImage("Image2.png", "webtracker2.com/", new byte[] { 4, 5, 6, 7 });
			var image3 = new WebTrackerCustomImage("Image3.png", "webtracker3.com/", new byte[] { 8, 9, 10, 11, 12, 13, 14, 15 });

			return new object[]
				{
					Array.Empty<WebTrackerCustomImage>(),
					new WebTrackerCustomImage[] { image1 },
					new WebTrackerCustomImage[] { image1, image2, image3 },
				};
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
