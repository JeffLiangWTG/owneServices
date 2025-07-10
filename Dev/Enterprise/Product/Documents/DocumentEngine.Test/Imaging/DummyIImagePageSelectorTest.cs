using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using CargoWise.IO;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Imaging.Testing
{
	sealed class DummyIImagePageSelectorTest : TransactionedTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBasic()
		{
			using (var selector = new DummyIImagePageSelector(BaseSourcePath))
			{
				Assert("Image", selector.CurrentImage != null);
				AssertEquals("Total Pages", 5, selector.TotalPages);
				AssertEquals("Current page index", 0, selector.CurrentPageIndex);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCurrentPageIndex()
		{
			using (var selector = new DummyIImagePageSelector(BaseSourcePath))
			{
				AssertEquals("Current page index", 0, selector.CurrentPageIndex);
				var oldImage = (Image)selector.CurrentImage.Clone();

				selector.CurrentPageIndex = 1;
				var newImage = (Image)selector.CurrentImage.Clone();
				AssertEquals("Current page index", 1, selector.CurrentPageIndex);

				Assert("The two frame bytes aren't equal", !IsCurrentPageEqual(newImage, oldImage));
			}
		}

		static bool IsCurrentPageEqual(Image first, Image second)
		{
			var originalFrameFile = Temp.GetTempFileNameWithExtension("tif");
			// this call to save saves only the ACTIVE FRAME
			first.Save(originalFrameFile, ImageFormat.Tiff);

			var newFrameFile = Temp.GetTempFileNameWithExtension("tif");
			// this call to save saves only the ACTIVE FRAME
			second.Save(newFrameFile, ImageFormat.Tiff);

			var originalImageBytes = ImageToByteArray(originalFrameFile);
			var newImageBytes = ImageToByteArray(newFrameFile);

			if (File.Exists(originalFrameFile))
			{
				File.Delete(originalFrameFile);
			}

			if (File.Exists(newFrameFile))
			{
				File.Delete(newFrameFile);
			}

			return originalImageBytes.SequenceEqual(newImageBytes);
		}

		static byte[] ImageToByteArray(string filename)
		{
			using (var image = Image.FromFile(filename))
			using (var stream = new MemoryStream())
			{
				image.Save(stream, image.RawFormat);
				return stream.ToArray();
			}
		}
	}
}
