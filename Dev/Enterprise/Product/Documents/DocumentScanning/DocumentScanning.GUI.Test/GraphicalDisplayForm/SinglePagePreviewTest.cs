using System.Drawing;
using System.Windows.Forms;
using Enterprise.DocumentScanning.GUI;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Imaging.Testing
{
	sealed class SinglePagePreviewTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPreview()
		{
			using (var dummySelector = new DummyIImagePageSelector(BaseSourcePath))
			using (var dummySelector2 = new DummyIImagePageSelector(BaseSourcePath))
			{
				PictureBox pictureBox = new PictureBox();
				pictureBox.Height = 50;
				pictureBox.Width = 50;

				Panel parentPanel = new Panel();
				parentPanel.Height = 50;
				parentPanel.Width = 50;

				SinglePagePreview previewObj = new SinglePagePreview(pictureBox, parentPanel);

				previewObj.Preview(dummySelector);
				Image originalImage = pictureBox.Image;
				Assert("image should be a thumbnail version of the original (ie smaller)", pictureBox.Image.Size.Width < dummySelector.CurrentImage.Size.Width);
				Assert("image should be a thumbnail version of the original (ie smaller)", pictureBox.Image.Size.Height < dummySelector.CurrentImage.Size.Height);

				dummySelector.CurrentPageIndex = 3;
				previewObj.Preview(dummySelector);
				Image secondImage = pictureBox.Image;
				Assert("image should be a thumbnail version of the original (ie smaller)", pictureBox.Image.Size.Width < dummySelector.CurrentImage.Size.Width);
				Assert("image should be a thumbnail version of the original (ie smaller)", pictureBox.Image.Size.Height < dummySelector.CurrentImage.Size.Height);
				Assert("A different image is in the picture box", originalImage != secondImage);

				previewObj.Preview(dummySelector2);
				Image thirdImage = pictureBox.Image;
				Assert("Image should be a thumbnail version of the original", pictureBox.Image.Size.Width < dummySelector2.CurrentImage.Size.Width);
				Assert("Image should be a thumbnail version of the original", pictureBox.Image.Size.Height < dummySelector2.CurrentImage.Size.Height);
				Assert("A different image is in the picture box", secondImage != thirdImage);
			}
		}
	}
}
