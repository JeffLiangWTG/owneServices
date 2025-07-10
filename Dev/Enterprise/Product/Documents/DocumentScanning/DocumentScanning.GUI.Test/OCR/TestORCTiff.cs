using System.IO;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.OCR.Testing
{
	sealed class TestOCRTiff : TestCase
	{
		OCRTiff ocrTiff;

		readonly string TestFilePath = BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.GUI\OCR\test-tifs\";

		[ExpectNoExceptions]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNewFromFile()
		{
			ocrTiff = new OCRTiff(TestFilePath + "ocr-Rocky.tif");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReadRocky()
		{
			ocrTiff = new OCRTiff(TestFilePath + "ocr-Rocky.tif");
			Assertion.AssertEquals("Rocky", ocrTiff.Text());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReadRawRocky()
		{
			ocrTiff = new OCRTiff(TestFilePath + "ocr-Rocky.tif");
			Assertion.AssertEquals("Rocky\x0c\x20\x0a\x0d\x00", ocrTiff.RawText());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReadBullwinkle()
		{
			ocrTiff = new OCRTiff(TestFilePath + "ocr-Bullwinkle.tif");
			// The OCR engine insists on recognising it as "Bul*i*winkle" not "Bul*l*winkle".
			Assertion.AssertEquals("Buliwinkle", ocrTiff.Text());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReadUTF8CurlyQuotes()
		{
			ocrTiff = new OCRTiff(TestFilePath + "ocr-curly-quotes.tif");
			// SourceSafe doesn't work with Unicode, so we have to use these escapes
			// and keep the source file in Western European/CRLF encoding.
			Assertion.AssertEquals("\u2018sgl\u2019\n\u201Cdbl\u201D", ocrTiff.Text());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCompareMSBandLSB()
		{
			OCRTiff ocrTiffLSB = new OCRTiff(TestFilePath + "lsb.tif");
			OCRTiff ocrTiffMSB = new OCRTiff(TestFilePath + "msb.tif");

			Assertion.AssertEquals("Identical images should produce identical text, regardless of byte order",
				ocrTiffLSB.Text(),
				ocrTiffMSB.Text()
				);
		}

		public void TestSwapTwoBytes()
		{
			Assertion.AssertEquals(0xabcd, OCRTiff.SwapBytes(OCRTiff.ByteOrder.BO_BIG_ENDIAN, 0xab, 0xcd));
			Assertion.AssertEquals(0xabcd, OCRTiff.SwapBytes(OCRTiff.ByteOrder.BO_LITTLE_ENDIAN, 0xcd, 0xab));
		}

		public void TestSwapFourBytes()
		{
			Assertion.AssertEquals(0xdeadbeef, OCRTiff.SwapBytes(OCRTiff.ByteOrder.BO_BIG_ENDIAN, 0xde, 0xad, 0xbe, 0xef));
			Assertion.AssertEquals(0xdeadbeef, OCRTiff.SwapBytes(OCRTiff.ByteOrder.BO_LITTLE_ENDIAN, 0xef, 0xbe, 0xad, 0xde));
		}

		public void TestBeautify()
		{
			Assertion.AssertEquals("Should trim trailing crud", "Rocky", OCRTiff.BeautifyRawOCRText("Rocky  \x0c\x09\x0b\x0d\x0a\x00"));
			Assertion.AssertEquals("Should not touch whitespace in the middle", "Rocky\nBullwinkle", OCRTiff.BeautifyRawOCRText("Rocky\nBullwinkle\n\x00"));
			Assertion.AssertEquals("Should clean form feeds", "Rocky\nBullwinkle", OCRTiff.BeautifyRawOCRText("Rocky\x0c \rBullwinkle\n\x00"));
		}

		public void TestDeleteOnDestroy()
		{
			string tmpFileName = Env.GetTempFileName();
			Assertion.Assert(File.Exists(tmpFileName));

			// Use using to make sure it's IDisposable
			using (OCRTiff ocrTiff = new OCRTiff(tmpFileName, true))
			{
			}
			Assertion.Assert("Should delete temp file " + tmpFileName + " when constructor flag is true", !File.Exists(tmpFileName));
		}

		public void TestDontDeleteOnDestroy()
		{
			string tmpFileName = Env.GetTempFileName();
			Assertion.Assert(File.Exists(tmpFileName));

			// Use using to make sure it's IDisposable
			using (OCRTiff ocrTiff = new OCRTiff(tmpFileName, false))
			{
			}
			Assertion.Assert("Should not delete temp file " + tmpFileName + " when constructor flag is false", File.Exists(tmpFileName));

			File.Delete(tmpFileName);
		}
	}
}
