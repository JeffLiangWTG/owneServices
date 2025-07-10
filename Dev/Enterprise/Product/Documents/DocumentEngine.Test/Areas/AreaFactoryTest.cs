using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Areas.Testing
{
	sealed class AreaFactoryTest : TempFileTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectException(typeof(DocumentEngineException))]
		public void TestThrowExceptionWhenFindingBadText()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("DateFilterWithDefault.xls", TestFilesSubFolder.ReportTestFiles);
			using (var testReport = new Report(new DocumentPack(), excelTemplate))
			{
				var testReportAnalyser = new ReportAnalyser(testReport);
				AssertEquals(null, AreaFactory.InstantiateArea(10, 20, testReport, "#Hello"));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRethrowDocumentEngineExceptionWhenAreaContructorThrowsDocumentEngineException()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("DateFilterWithDefault.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(new DocumentPack(), excelTemplate))
			{
				var reportAnalyser = new ReportAnalyser(report);
				AssertExceptionThrown<DocumentEngineException>("Should throw a DocumentEngineException", () => AreaFactory.InstantiateArea(10, 20, report, "#SectionBody:"));
				AssertExceptionThrown<DocumentEngineException>("Should throw a DocumentEngineException", () => AreaFactory.InstantiateArea(10, 20, report, "#Footer:"));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInstantiatingAnArea()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("DateFilterWithDefault.xls", TestFilesSubFolder.ReportTestFiles);
			using (var testReport = new Report(new DocumentPack(), excelTemplate))
			{
				var testReportAnalyser = new ReportAnalyser(testReport);
				AssertEquals(typeof(ConfigArea), AreaFactory.InstantiateArea(10, 20, testReport, "#Config").GetType());
			}
		}
	}
}
