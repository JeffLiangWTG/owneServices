using System.Collections.Generic;
using System.Drawing;
using CargoWise.IO;
using Moq;
using NUnit.Framework;

namespace Enterprise.Barcode.Business.Testing
{
	sealed class BarcodeScannerTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBarcodeScanner_File()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var testImage = resourceRetriever.SaveResourceToFile("code128_unittest.bmp");
				var reader = new BarcodeScanner();
				var result = reader.ExtractBarcodes(testImage);

				Assert("Barcode 'Code128B-CorrectlyWorks' should be found", result.IndexOf("Code128B-CorrectlyWorks") >= 0);
				Assert("Barcode '0000000128' should be found", result.IndexOf("0000000128") >= 0);
				Assert("Barcode 'FIRST' should be found", result.IndexOf("FIRST") >= 0);
				Assert("Barcode 'SECOND' should be found", result.IndexOf("SECOND") >= 0);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBarcodeScanner_Image()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var testImage = resourceRetriever.SaveResourceToFile("code128_unittest.bmp");
				var reader = new BarcodeScanner();
				var result = reader.ExtractBarcodes(testImage);

				Assert("Barcode 'Code128B-CorrectlyWorks' should be found",
					result.IndexOf("Code128B-CorrectlyWorks") >= 0);
				Assert("Barcode '0000000128' should be found", result.IndexOf("0000000128") >= 0);
				Assert("Barcode 'FIRST' should be found", result.IndexOf("FIRST") >= 0);
				Assert("Barcode 'SECOND' should be found", result.IndexOf("SECOND") >= 0);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestExtractBarcodesThrowNoExceptionWhenResultIsNull()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var testImage = resourceRetriever.SaveResourceToFile("code128_unittest.bmp");
				using (var inputImage = (Bitmap)Bitmap.FromFile(testImage))
				{
					var reader = new BarcodeScannerForTest();
					var scanner = new Mock<IBarcodeScanner>(MockBehavior.Strict);
					reader.Scanner = scanner.Object;

					scanner.Setup(m => m.Read(It.IsAny<Bitmap>())).Returns((IEnumerable<string>)null);

					var result = reader.ExtractBarcodes(inputImage);
					AssertNotNull(result);
					AssertEquals(0, result.Count);
				}
			}
		}

		class BarcodeScannerForTest : BarcodeScanner
		{
			internal override IBarcodeScanner GetBarcodeScanner()
			{
				return Scanner;
			}

			internal IBarcodeScanner Scanner { private get; set; }
		}
	}
}
