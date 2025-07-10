using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Testing
{
	sealed class ZImageListDpiScalingHelperTest : TestCase
	{
#if !WINZOR

		public void TestSetScaledImagesFromImageListStreamer()
		{
			var imageList = new ImageList();
			var resources = new System.ComponentModel.ComponentResourceManager(typeof(ZRichTextBoxToolBar));
			imageList.ImageStream = (ImageListStreamer)resources.GetObject("ToolBarImageList.ImageStream");

			AssertEquals(16, imageList.Images.Count);
			AssertEquals(16, imageList.ImageSize.Width);
			AssertEquals(16, imageList.ImageSize.Height);

			using (ControlDpiScalingHelper.OverrideDPI_ForTesting(192, 192))
			{
				var imageList2 = new ImageList();
				ZImageListDpiScalingHelper.SetScaledImagesFromImageListStreamer(imageList2, (ImageListStreamer)resources.GetObject("ToolBarImageList.ImageStream"));

				AssertEquals(16, imageList2.Images.Count);
				AssertEquals(32, imageList2.ImageSize.Width);
				AssertEquals(32, imageList2.ImageSize.Height);
			}
		}

#endif
	}
}
