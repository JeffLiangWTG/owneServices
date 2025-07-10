using Enterprise.DocumentEngine.Testing.UtilityClasses;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class AccountingPeriodsRangeBuilderTest : FilterBuilderTestWithTempFile
	{
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAccountingPeriodsRange()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("AccountingPeriodRangeFilter.xls", TestFilesSubFolder.ReportTestFiles);
			using (var testReport = new Report(new DocumentPack(), excelTemplate))
			{
				var testReportAnalyser = new ReportAnalyser(testReport);
				var testFilterCollectionBuilder = new FilterCollectionBuilder(testReportAnalyser.DataSourceParameters, testReportAnalyser.ValidatorPack, new StringTreeBuilder(testReport.FilterSheet).GetTree(), DummyEvaluator);
				testFilterCollectionBuilder.Build();

				AssertEquals("Number of fields", 2, testFilterCollectionBuilder.IFilterCollection.Count);
				AssertEquals(typeof(AccountingPeriodsRangeField), testFilterCollectionBuilder.IFilterCollection[1].GetType());
			}
		}
	}
}
