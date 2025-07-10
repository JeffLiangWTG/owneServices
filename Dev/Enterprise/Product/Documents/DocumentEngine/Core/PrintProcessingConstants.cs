using System.IO;
using CargoWise.BuildTools;

namespace Enterprise.DocumentEngine
{
	public enum OutputFormatType
	{
		FAX = 1,
		PDF = 2,
		TIF = 3,
		Unspecified = 4,
		PDFAcrobat5 = 5,
		PDFA = 6,
		HTML = 7,
		HTMF = 8,
		PDFC = 9,
		XLS = 10,
		XLSX = 11
	}

	public static class PrintProcessingConstants
	{
		public const string NEWLINE = "\r\n";
		public const int WAIT_INTERVAL = 200; // milliseconds

#if DEBUG
		#region TestImagingFiles
		public static string TestWorkingDirectory => Path.Combine(BuildConstants.LocalEnterprisePath, @"Enterprise", "Product", "Documents", "PrintProcessing", "PrintProcessing.Test", "TestHelper", "TestFiles", " ").Trim();
		public const string TestPDFFileName = "id_8B56B1D7-1E37-4ed8-8FC1-395D44FE0EC7.PDF";
		public const string TestPDFFileNameWithUnicodeChar = "TestPDFFileNameWithUnicodeChar确.PDF";

		public const string TestXLSFileName = "id_8B56B1D7-1E37-4ed8-8FC1-395D44FE0EC6.XLS";
		public const string TestXLSXFileName = "id_8B56B1D7-1E37-4ed8-8FC1-395D44FE0EC6.XLSX";
		public const string TestCTLFileName = "id_8B56B1D7-1E37-4ed8-8FC1-395D44FE0EC6.CTL";
		public const string TestPDFMultiPageFile = "id_8B56B1D7-1E37-4ed8-8FC1-395D44FE0EM7.PDF";
		public const string TestPDFMultiPageFileWithUnicodeChar = "TestPDFMultiPageFileWithUnicodeChar%2.PDF";
		public const string Test8bppTifFileName = "id_8B56B1D7-1E37-4ed8-8FC1-395D44FE0EM7.TIF";
		public const string TestTiffRawFormatFileName = "id_8B56B1D7-1E37-4ed8-8FC1-395D44FE0EF7.TIF";
		public const string TestMissingFrameTifFileName = "MissingFrames.TIF";
		public static string TestMultipageTifFullPath => TestWorkingDirectory + Test8bppTifFileName;
		public const string TestFaxTifFileName = "TestGeneratedReport.TIF";
		public static string TestFaxTifFullPath => TestWorkingDirectory + TestFaxTifFileName;
		public const string TestMultipageLandscapeFileName = "MultipageWithLandscapePages.tif";
		public const string TestSingleLandscapeFileName = "SingleLandscape.tif";
		public const string TestMultipageFirstPageLandscapeFileName = "MultipageWithLandscapeFirstPage.tif";

		public const string Test4bppTifFileName = "NoCompression4bpp.tif";
		public static string Test4bppTifFullPath => TestWorkingDirectory + Test4bppTifFileName;

		public const string TestMixedbppTifFileName = "MixedBppAndCompression.tif";
		public static string TestMixedbppTifFullPath => TestWorkingDirectory + TestMixedbppTifFileName;

		public const string Test32bppTifFileName = "truecolour.tif";
		public static string Test32bppTifFullPath => TestWorkingDirectory + Test32bppTifFileName;
		public const string TestBadImageGDIExternalExceptionFileName = "badImageGDIExternalException.tif";
		public const string TestHtmlFileName = "TestHtml.html";
		public const string TestPngFileName = "TestPng.png";

		public const string TestMergedPDFFileName = "MergedPdf.PDF";
		public static string TestMergedPDFFileFullPath => TestWorkingDirectory + TestMergedPDFFileName;

		public const string TestTxtFileName = "Test.txt";
		public static string TestTxtFileFullPath => TestWorkingDirectory + TestTxtFileName;

		public const string TestSmallTifFileName = "SmallTif.tif";
		public static string TestSmallTifFileFullPath => TestWorkingDirectory + TestSmallTifFileName;

		#endregion
		#region TestReportEngineFiles
		public static readonly string Small = TestWorkingDirectory + "Small.xls";
		public static readonly string CorruptFileFullPath = TestWorkingDirectory + "CorruptFile.xls";
		public static readonly string TestSixPageSpreadsheet = TestWorkingDirectory + "SixPages.xls";
		public const string TestGeneratedReport = "TestGeneratedReport.xls";
		public const string TestGeneratedReportWith2Sheets = "TestGeneratedReportWith2Sheets.xls";
		public const string TestSerialisedChartOfAccounts = "SerialisedChartOfAccounts.xml";
		public const string TestEnterpriseDocument = "Document.XLS";
		public const string TestCorruptedPDF = "CORRUPTED_File.PDF";
		public const string TestHangingPDF = "HANG_File.PDF";
		public static readonly string TestFitToPageSpreadsheetFullPath = TestWorkingDirectory + "FitToPage.xls";
		public static readonly string TestTwoPageDocWithColourLogo = TestWorkingDirectory + "TwoPageDocWithColourLogo.xls";
		#endregion
#endif
	}
}
