using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using CargoWise.Application;
using Enterprise.DocumentEngine.Imaging;
using Enterprise.DocumentScanning.Integration;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Business.Testing
{
	sealed class TiffToJpegConverterTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestConvertTiffToJpegThrowsDetailedInfo()
		{
			string imagePath = BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\colored.tif";

			try
			{
				var parameters = new EncoderParameters(2); // intentionally put here 2 - to provocate Image.Save(...) failing
				parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)100);

				using (var image = Image.FromFile(imagePath))
				{
					TiffToJpegConverter.TryShrinkImageByConvertingToJpeg(image, parameters);
				}
				Fail("Should throw exception for testing");
			}
			catch (ArgumentException ex)
			{
				Assert(ex.Message.Contains("PixelFormat"));
				Assert(ex.Message.Contains("RawFormat"));
				Assert(ex.Message.Contains("Width"));
				Assert(ex.Message.Contains("Height"));
				Assert(ex.Message.Contains("HorizontalResolution"));
				Assert(ex.Message.Contains("VerticalResolution"));

				AssertNotNull(ex.InnerException);
				Assert(typeof(ArgumentException).IsAssignableFrom(ex.InnerException.GetType()));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestConvertTiffToJpegThrowsExternalException()
		{
			using (var image = GetImageFileWithDisposedSourceStream())
			{
				//ExternalException is thrown when trying to save a file whose source stream has been closed
				AssertNull("Since an ExternalException was thrown and the image could not be converted, we should get a null", TiffToJpegConverter.TryShrinkImageByConvertingToJpeg(image));
			}
		}

		Image GetImageFileWithDisposedSourceStream()
		{
			var documentUtilities = ObjectFactory.Get<IDocumentUtilities>();
			using (var memoryStream = new MemoryStream(documentUtilities.GetFileAsBytes(BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\NotCompressed.tif")))
			{
				return Image.FromStream(memoryStream);
			}
		}
	}
}
