using CargoWise.Types;
using Enterprise.DocumentScanning;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Test
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
	internal static class TestFileHelper
	{
		public static byte[] SamplePDF => samplePDF;
		static ZBlob samplePDF => DocumentUtilities.GetFileAsBytes(TestCase.BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Sample.PDF");

		public static byte[] File1MB => file1MB;
		static ZBlob file1MB => DocumentUtilities.GetFileAsBytes(TestCase.BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\1MB.dat");

		public static byte[] File200KB => file200KB;
		static ZBlob file200KB => DocumentUtilities.GetFileAsBytes(TestCase.BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\200kb.tif");

		public static byte[] UnsupportedFormat => unsupportedFormat;
		static ZBlob unsupportedFormat => DocumentUtilities.GetFileAsBytes(TestCase.BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\UnsupportedFormat.docx");
	}
}
