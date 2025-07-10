using System.Drawing;
using System.Drawing.Imaging;
using Enterprise.DocumentEngine.PreviewableDocument;
using Moq;
using NUnit.Framework;
using static CargoWise.PdfiumWrapper.Testing.ImageTestingHelpers;

namespace Enterprise.DocumentEngine.Imaging.Testing
{
	sealed class PreviewableDocumentImageReaderTest : TestCase
	{
		public void TestCurrentPage()
		{
			using (var tempFile = CreateImageFile(ImageFormat.Tiff, Color.Red, Color.Empty, Color.Lime, Color.Aqua, Color.Blue, Color.Fuchsia))
			{
				var document = new PreviewableImageDocument(tempFile.Filename);
				using (var imageReader = new PreviewableDocumentImageReader(document))
				{
					AssertEquals("The default value should be zero", 0, imageReader.PageSelector.CurrentPageIndex);
					AssertEquals("First page: Red", Color.Red, AsNamedColor(GetCenterPixelColor(imageReader.PageSelector.CurrentImage)));

					imageReader.PageSelector.CurrentPageIndex = 1;
					AssertEquals("Second page: Empty", Color.Empty, AsNamedColor(GetCenterPixelColor(imageReader.PageSelector.CurrentImage)));

					imageReader.PageSelector.CurrentPageIndex = 2;
					AssertEquals("Third page: Green (aka Lime)", Color.Lime, AsNamedColor(GetCenterPixelColor(imageReader.PageSelector.CurrentImage)));

					imageReader.PageSelector.CurrentPageIndex = 0;
					AssertEquals("Should return to zero", 0, imageReader.PageSelector.CurrentPageIndex);
					AssertEquals("First page: Red", Color.Red, AsNamedColor(GetCenterPixelColor(imageReader.PageSelector.CurrentImage)));
				}
			}
		}

		public void TestTotalPages()
		{
			var firstDocument = new Mock<IPreviewableDocument>();
			firstDocument.Setup(m => m.NumberOfPages).Returns(1);
			using (var pageSelector = new PreviewableDocumentImageReader(firstDocument.Object))
			{
				AssertEquals("Should return the number of pages of the document", 1, pageSelector.PageSelector.TotalPages);
			}

			var secondDocument = new Mock<IPreviewableDocument>();
			secondDocument.Setup(m => m.NumberOfPages).Returns(99);
			using (var pageSelector = new PreviewableDocumentImageReader(secondDocument.Object))
			{
				AssertEquals("Should return the number of pages of the document", 99, pageSelector.PageSelector.TotalPages);
			}
		}

		public void TestCachesPage()
		{
			using (var tempFile = CreateImageFile(ImageFormat.Tiff, Color.Red, Color.Yellow, Color.Lime, Color.Aqua, Color.Blue, Color.Fuchsia))
			{
				using (var document = new PreviewableImageDocument(tempFile.Filename))
				using (var imageReader = new PreviewableDocumentImageReader(document))
				{
					imageReader.PageSelector.CurrentPageIndex = 1;

					var firstImage = imageReader.PageSelector.CurrentImage;
					AssertSame("We should be returning the smae image where we can to save memory", firstImage, imageReader.PageSelector.CurrentImage);

					imageReader.PageSelector.CurrentPageIndex = 2;
					AssertNotEquals("Different page, should be different image", firstImage, imageReader.PageSelector.CurrentImage); // Make sure a new image is created

					imageReader.PageSelector.CurrentPageIndex = 1;
					AssertSame("We should have cached the first image", firstImage, imageReader.PageSelector.CurrentImage);
				}
			}
		}
	}
}
