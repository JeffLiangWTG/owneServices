using System.IO;
using CargoWise.BuildTools;

namespace Enterprise.DocumentEngine.Testing.UtilityClasses
{
	public static class UnitTestingConstants
	{
		public static string TestCustomisableTemplateFilePath
		{
			get { return Path.Combine(TestDocumentsFilesDir, "CustomisableSectionTest.xls"); }
		}

		public static string TestDocumentExcelTemplateFilePath
		{
			get { return Path.Combine(TestDocumentsFilesDir, "DummyBusinessObjectAsDataSource.xls"); }
		}

		public static string TestDocumentDigitalSignatureFilePath
		{
			get { return Path.Combine(BuildConstants.LocalEnterprisePath, @"Enterprise\Product\Documents\DocumentEngine\Testing\DigitalSignature\"); }
		}

		public static string TestDocumentsFilesDir
		{
			get { return Path.Combine(BuildConstants.LocalEnterprisePath, @"Enterprise\Product\Documents\DocumentEngine\Testing\DocumentTestFiles"); }
		}

		public static string TestFilesDir
		{
			get { return Path.Combine(BuildConstants.LocalEnterprisePath, @"Enterprise\Product\Documents\DocumentEngine\Testing\ReportTestFiles\"); }
		}

		public static string TestDocumentScanningFilesPath
		{
			get { return Path.Combine(BuildConstants.LocalEnterprisePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\"); }
		}

		public static string TestReportExcelTemplateFilePath
		{
			get { return Path.Combine(TestFilesDir, "UDF with defaults.xls"); }
		}

		public static string DifferentHorizontalAndVerticalResolutionFile
		{
			get { return Path.Combine(BuildConstants.LocalEnterprisePath, @"Enterprise\Product\Documents\DocumentEngine\Core\FileFormatUtilities\Testing\DifferentHorizontalAndVerticalResolutionSinglePage.tif"); }
		}

		public static string MultipageWithLandscapeFirstPageFile
		{
			get { return Path.Combine(BuildConstants.LocalEnterprisePath, @"Enterprise\Product\Documents\DocumentEngine\Core\FileFormatUtilities\Testing\MultipageWithLandscapeFirstPage.tif"); }
		}

		public static string TestNewSerializedReportFile
		{
			get { return Path.Combine(BuildConstants.LocalEnterprisePath, @"Enterprise\Product\Documents\DocumentEngine\Core\FileFormatUtilities\Testing\NewSerializedReport.bin"); }
		}
	}
}
