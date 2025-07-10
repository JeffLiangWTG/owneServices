using System.Drawing;
using System.IO;
using Enterprise.DocumentEngine.Imaging;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.PreviewableDocument.Testing
{
	sealed class RotateDocumentTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBasic()
		{
			string newFile = "";
			var path = BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\small_3pages.tif";
			var fileBytes = File.ReadAllBytes(path);
			var image = Image.FromStream(new MemoryStream(fileBytes));
			var pageSelector = new StandardImagePageSelector(image, path);
			try
			{
				Assert("Image", pageSelector.CurrentImage != null);
				AssertEquals("Page Count", 3, pageSelector.TotalPages);

				pageSelector.CurrentPageIndex = 0;
				var page0Height = pageSelector.CurrentImage.Height;
				var page0Width = pageSelector.CurrentImage.Width;
				pageSelector.CurrentPageIndex = 1;
				var page1Height = pageSelector.CurrentImage.Height;
				var page1Width = pageSelector.CurrentImage.Width;
				pageSelector.CurrentPageIndex = 2;
				var page2Height = pageSelector.CurrentImage.Height;
				var page2Width = pageSelector.CurrentImage.Width;

				pageSelector.CurrentPageIndex = 1;
				Assertion.Assert("page must not be square", page1Height != page1Width);

				newFile = RotateDocument.RotateActivePage(true, pageSelector);
				pageSelector.Dispose();
				image.Dispose();

				fileBytes = File.ReadAllBytes(newFile);
				image = Image.FromStream(new MemoryStream(fileBytes));
				pageSelector = new StandardImagePageSelector(image, newFile);

				pageSelector.CurrentPageIndex = 0;
				Assertion.AssertEquals(page0Height, pageSelector.CurrentImage.Height);
				Assertion.AssertEquals(page0Width, pageSelector.CurrentImage.Width);

				pageSelector.CurrentPageIndex = 1;
				Assertion.AssertEquals(page1Width, pageSelector.CurrentImage.Height);
				Assertion.AssertEquals(page1Height, pageSelector.CurrentImage.Width);

				pageSelector.CurrentPageIndex = 2;
				Assertion.AssertEquals(page2Height, pageSelector.CurrentImage.Height);
				Assertion.AssertEquals(page2Width, pageSelector.CurrentImage.Width);
			}
			finally
			{
				pageSelector.Dispose();
				image.Dispose();
				if (!string.IsNullOrEmpty(newFile))
				{
					System.IO.File.Delete(newFile);
				}
			}
		}
	}
}
