using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.FlexCelInterface.Testing
{
	public class ImageToPDFConverterTest : TestCaseWithFactory
	{
		public void TestConvertingAnInvalidImageDoesntCauseAnException()
		{
			var converter = new ImageToPDFConverter();
			AssertNull("converter.ConvertToPDF(Encoding.ASCII.GetBytes(\"Invalid Image\"))", converter.ConvertToPDF(Encoding.ASCII.GetBytes("Invalid Image")));
		}

		public void TestIsPDF()
		{
			AssertEquals("ImageToPDFConverter.IsPDF(byte[] contents)", true, ImageToPDFConverter.IsPDF(Encoding.ASCII.GetBytes("%PDF <data> %EOF")));
			AssertEquals("ImageToPDFConverter.IsPDF(byte[] contents)", true, ImageToPDFConverter.IsPDF(Encoding.ASCII.GetBytes("%PDF <data> %EOF\n")));
			AssertEquals("ImageToPDFConverter.IsPDF(byte[] contents)", true, ImageToPDFConverter.IsPDF(Encoding.ASCII.GetBytes("%PDF <data> %EOF\n  \n\n\n    ")));

			AssertEquals("ImageToPDFConverter.IsPDF(byte[] contents)", false, ImageToPDFConverter.IsPDF(Encoding.ASCII.GetBytes(" %PDF <data> %EOF")));
			AssertEquals("ImageToPDFConverter.IsPDF(byte[] contents)", false, ImageToPDFConverter.IsPDF(Encoding.ASCII.GetBytes("%BMP <data> %EOF")));
			AssertEquals("ImageToPDFConverter.IsPDF(byte[] contents)", false, ImageToPDFConverter.IsPDF(Encoding.ASCII.GetBytes("%BMP <data> EOF")));
			AssertEquals("ImageToPDFConverter.IsPDF(byte[] contents)", false, ImageToPDFConverter.IsPDF(Encoding.ASCII.GetBytes("%PDF <data> %XXX")));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestIsPDFA()
		{
			var bytes = File.ReadAllBytes(TestCaseWithFactory.BaseSourcePath + @"Enterprise\Product\Documents\DocumentEngine\Testing\DocumentTestFiles\PDFNonA.pdf");
			Assert("Should be a valid PDF document", ImageToPDFConverter.IsPDF(bytes));
			Assert("Should not be PDF/A document", !ImageToPDFConverter.IsPDFA(bytes));

			bytes = File.ReadAllBytes(TestCaseWithFactory.BaseSourcePath + @"Enterprise\Product\Documents\DocumentEngine\Testing\DocumentTestFiles\PDFA.pdf");
			Assert("Should be a valid PDF document", ImageToPDFConverter.IsPDF(bytes));
			Assert("Should be a valid PDF/A document", ImageToPDFConverter.IsPDFA(bytes));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestIsTiff()
		{
			var bytesInvalidTiff1 = File.ReadAllBytes(TestCaseWithFactory.BaseSourcePath + @"Enterprise\Product\Documents\DocumentEngine\Testing\DocumentTestFiles\PDFNonA.pdf");
			var bytesInvalidTiff2 = File.ReadAllBytes(TestCaseWithFactory.BaseSourcePath + @"Enterprise\Product\Documents\DocumentEngine\Testing\DocumentTestFiles\DecimalGridStyles.xls");
			var bytesInvalidTiff3 = File.ReadAllBytes(TestCaseWithFactory.BaseSourcePath + @"Enterprise\Product\Documents\DocumentEngine\Testing\ReportTestFiles\TransparentPNG.png");
			var bytesInvalidTiff4 = File.ReadAllBytes(TestCaseWithFactory.BaseSourcePath + @"Enterprise\Product\Documents\DocumentEngine\Testing\ReportTestFiles\IAMNotATIFFButATextFile.tif");
			var bytesValidTiff = File.ReadAllBytes(TestCaseWithFactory.BaseSourcePath + @"Enterprise\Product\Documents\DocumentEngine\Testing\ReportTestFiles\TIFPage.tif");

			Assert("Should not be a valid TIFF document", !ImageToPDFConverter.IsTiff(bytesInvalidTiff1));
			Assert("Should not be a valid TIFF document", !ImageToPDFConverter.IsTiff(bytesInvalidTiff2));
			Assert("Should not be a valid TIFF document", !ImageToPDFConverter.IsTiff(bytesInvalidTiff3));
			Assert("Should not be a valid TIFF document", !ImageToPDFConverter.IsTiff(bytesInvalidTiff4));
			Assert("Should be a valid TIFF document", ImageToPDFConverter.IsTiff(bytesValidTiff));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestConvertToPDF()
		{
			using (var stream = File.OpenRead(TestCaseWithFactory.BaseSourcePath + @"Enterprise\Product\Documents\DocumentEngine\FlexCelInterface\Testing\Sample2PageTIFF.TIF"))
			{
				byte[] imageData = stream.CopyToByteArray();

				var converter = new ImageToPDFConverter();
				byte[] pdfData = converter.ConvertToPDF(imageData);

				AssertEquals("ImageToPDFConverter.IsPDF(pdfData)", true, ImageToPDFConverter.IsPDF(pdfData));

				string expectedFileName = "Sample2PageTIFFAsPDF.pdf";

				// Uncomment the following line if this test is failing and you want to see what the actual output is.
				//File.WriteAllBytes(@"C:\tmp\" + expectedFileName, pdfData);

				using (var expected = File.OpenRead(TestCaseWithFactory.BaseSourcePath + @"Enterprise\Product\Documents\DocumentEngine\FlexCelInterface\Testing\" + expectedFileName))
				{
					AssertMultilineASCIIEquals("Output PDF should be the same.", ImageToPDFConverterTest.GetStringForPDFComparison(expected), ImageToPDFConverterTest.GetStringForPDFComparison(pdfData));
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReadingIntoPDFFromMultiPageTIFF()
		{
			using (var sourceImage = Image.FromFile(TestCaseWithFactory.BaseSourcePath + @"Enterprise\Product\Documents\DocumentEngine\FlexCelInterface\Testing\Sample2PageTIFF.TIF"))
			{
				var converter = new ImageToPDFConverter();
				using (var actual = new MemoryStream())
				{
					int pages = converter.GetPagesAsPDF(sourceImage, actual);
					AssertEquals("pages", 2, pages);
					Assert(ImageToPDFConverter.IsPDF(actual.CopyToByteArray()));

					string expectedFileName = "Sample2PageTIFFAsPDF.pdf";

					// Uncomment the following line if this test is failing and you want to see what the actual output is.
					//SaveAndOpenStream(actual, @"C:\tmp\" + expectedFileName);

					using (var expected = File.OpenRead(TestCaseWithFactory.BaseSourcePath + @"Enterprise\Product\Documents\DocumentEngine\FlexCelInterface\Testing\" + expectedFileName))
					{
						AssertMultilineASCIIEquals("Output PDF should be the same.", ImageToPDFConverterTest.GetStringForPDFComparison(expected), ImageToPDFConverterTest.GetStringForPDFComparison(actual));
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReadingIntoPDFFromBitmap()
		{
			using (var sourceImage = Image.FromFile(TestCaseWithFactory.BaseSourcePath + @"Enterprise\Product\Documents\DocumentEngine\FlexCelInterface\Testing\SampleBitmap.bmp"))
			{
				var converter = new ImageToPDFConverter();
				using (var actual = new MemoryStream())
				{
					int pages = converter.GetPagesAsPDF(sourceImage, actual);
					AssertEquals("pages", 1, pages);
					Assert(ImageToPDFConverter.IsPDF(actual.CopyToByteArray()));

					string expectedFileName = "SampleBitmapAsPDF.pdf";

					// Uncomment the following line if this test is failing and you want to see what the actual output is.
					//SaveAndOpenStream(actual, @"C:\tmp\" + expectedFileName);

					using (var expected = File.OpenRead(TestCaseWithFactory.BaseSourcePath + @"Enterprise\Product\Documents\DocumentEngine\FlexCelInterface\Testing\" + expectedFileName))
					{
						AssertMultilineASCIIEquals("Output PDF should be the same.", ImageToPDFConverterTest.GetStringForPDFComparison(expected), ImageToPDFConverterTest.GetStringForPDFComparison(actual));
					}
				}
			}
		}

		public void TestIsPdf_WithInvalidBytes_ShouldNotThrow()
		{
			var bytes = System.Array.Empty<byte>();
			Assert(!ImageToPDFConverter.IsPDF(bytes));

			bytes = Encoding.ASCII.GetBytes("%PDF nyat nyat nyat");
			Assert(!ImageToPDFConverter.IsPDF(bytes));
		}

		public static string GetStringForPDFComparison(byte[] contents)
		{
			var result = Encoding.ASCII.GetString(contents);

			result = result.RemoveBetween("/Creator(", ")");
			result = result.RemoveBetween("/CreationDate (", ")");
			result = result.RemoveBetween("/CreationDate(", ")");
			result = result.RemoveBetween("/Producer(", ")");
			result = result.RemoveBetween("/ID [", "]");
			result = result.RemoveBetween("/ID[", "]");
			result = result.RemoveBetween("<pdf:Producer>", "</pdf:Producer>");
			result = result.RemoveBetween("<xmp:ModifyDate>", "</xmp:ModifyDate>");
			result = result.RemoveBetween("<xmp:CreateDate>", "</xmp:CreateDate>");
			result = result.RemoveBetween("<xmp:MetadataDate>", "</xmp:MetadataDate>");
			result = result.RemoveBetween("<xmpMM:DocumentID>", "</xmpMM:DocumentID>");
			result = result.RemoveBetween("<xmpMM:InstanceID>", "</xmpMM:InstanceID>");
			result = result.RemoveBetween("<stEvt:instanceID>", "</stEvt:instanceID>");
			result = result.RemoveBetween("<stEvt:softwareAgent>", "</stEvt:softwareAgent>");
			result = result.RemoveBetween("<stEvt:when>", "</stEvt:when>");
			result = result.RemoveBetween("/Type/Sig/", "/Type/SigRef/");

			return result;
		}

		public static IEnumerable<(string key, string value)> GetSignatureFromPDF(byte[] contents)
		{
			var stringContent = Encoding.ASCII.GetString(contents);
			var signature = Regex.Match(stringContent, @"<</Type/Sig/(.*?)<</Type/SigRef/").Groups[1].Value;
			var location = Regex.Match(signature, @"/Location\((.*?[^\\])\)").Groups[1].Value;
			var reason = Regex.Match(signature, @"/Reason\((.*?[^\\])\)").Groups[1].Value;
			var contactInfo = Regex.Match(signature, @"/ContactInfo\((.*?[^\\])\)").Groups[1].Value;

			yield return ("Location", location);
			yield return ("Reason", reason);
			yield return ("ContactInfo", contactInfo);
		}

		public static string GetStringForPDFComparison(Stream stream) => GetStringForPDFComparison(stream.CopyToByteArray());
	}
}
