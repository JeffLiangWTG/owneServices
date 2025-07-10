using System.Drawing;
using System.Linq;
using CargoWise.IO;
using NUnit.Framework;

namespace Enterprise.Barcode.Business.Testing
{
	[DatCapabilityRequirement("SOURCE_CODE")]
	sealed class EnterpriseScannerTest : TestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			_resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
		}

		protected override void TearDown()
		{
			base.TearDown();
			_resourceRetriever.Dispose();
		}

		class EnterpriseScannerForTest : EnterpriseScanner
		{
			public Bitmap Rotate_Exposed(Bitmap input)
			{
				return RotateImageBy90(input);
			}
		}

		public void TestScansRotatedBarcodes()
		{
			using (var image = Image.FromFile(GetImagePath("LandscapeImageWithBarcode.TIF")))
			using (var bmp = new Bitmap(image))
			{
				var scanner = new EnterpriseScannerForTest { RotateImage = true };
				AssertEquals("It should be able to read the barcode, even though its been tilted", "^CON=C17AMUC00059915;MAN;|", scanner.Read(bmp).Single());
			}
		}

		public void TestRotate()
		{
			var scanner = new EnterpriseScannerForTest();
			using (var input = new Bitmap(200, 100))
			{
				input.SetResolution(500, 100);
				using (var output = scanner.Rotate_Exposed(input))
				{
					AssertEquals(100, output.Width);
					AssertEquals(200, output.Height);
					AssertEquals(500F, output.HorizontalResolution);
					AssertEquals(100F, output.VerticalResolution);
				}
			}
		}

		/// <summary>
		/// This TestCase tests all parts of the BarcodeScanning project, due to the difficulty that exists
		/// in determining if output values are correct until the last stage of processing.
		/// </summary>
		public void TestBarcodeScanner()
		{
			using (var testImage = new Bitmap(GetImagePath("code128_unittest.bmp")))
			{
				var scanner = new EnterpriseScannerForTest { RotateImage = false };
				var barcodes = scanner.Read(testImage).ToList();

				Assert("Barcode 'Code128B-CorrectlyWorks' should be found", barcodes.IndexOf("Code128B-CorrectlyWorks") >= 0);
				Assert("Barcode '0000000128' should be found", barcodes.IndexOf("0000000128") >= 0);
				Assert("Barcode 'FIRST' should be found", barcodes.IndexOf("FIRST") >= 0);
				Assert("Barcode 'SECOND' should be found", barcodes.IndexOf("SECOND") >= 0);
			}
		}

		public void TestBarcodeReader()
		{
			using (var testImage = new Bitmap(GetImagePath("code128_simple.bmp")))
			{
				var imageData = ImageProcessor.GetImageDataFromBitmap(testImage, out int imageHeight, out int imageWidth);
				var reader = new BarcodeReader();
				var result = reader.ReadBarcode(imageData, imageHeight, imageWidth).Text;
				AssertEquals("Barcode should be correctly read", "Hello World", result);
			}
		}

		public void TestInputBitmapUntouched()
		{
			using (var testImage = new Bitmap(GetImagePath("code128_simple.bmp")))
			{
				int originalWidth = testImage.Width;
				int originalHeight = testImage.Height;
				Assert("Test requires non-square bitmap", originalHeight != originalWidth);

				(new BarcodeScanner()).ExtractBarcodes(testImage);
				AssertEquals(originalWidth, testImage.Width);
				AssertEquals(originalHeight, testImage.Height);
			}
		}

		string GetImagePath(string filename) => _resourceRetriever.SaveResourceToFile(filename);

		EmbeddedResourceRetriever _resourceRetriever;
	}
}
