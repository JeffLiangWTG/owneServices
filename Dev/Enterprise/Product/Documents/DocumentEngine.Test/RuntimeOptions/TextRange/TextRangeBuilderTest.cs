using Enterprise.DocumentEngine.Testing.UtilityClasses;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class TextRangeBuilderTest : FilterBuilderTestWithTempFile
	{
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDefault()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("TextRangeFilterWithDefault.xls", TestFilesSubFolder.ReportTestFiles);
			using (var testReport = new Report(new DocumentPack(), excelTemplate))
			{
				var testReportAnalyser = new ReportAnalyser(testReport);
				var testFilterCollectionBuilder = new FilterCollectionBuilder(testReportAnalyser.DataSourceParameters, testReportAnalyser.ValidatorPack, new StringTreeBuilder(testReport.FilterSheet).GetTree(), DummyEvaluator);

				testFilterCollectionBuilder.Build();

				AssertEquals("Number of fields", 2, testFilterCollectionBuilder.IFilterCollection.Count);
				AssertEquals("Field with default from", "a", ((TextRangeField)(testFilterCollectionBuilder.IFilterCollection[1])).From);
				AssertEquals("Field with default to", "z", ((TextRangeField)(testFilterCollectionBuilder.IFilterCollection[1])).To);
			}
		}
	}
}
