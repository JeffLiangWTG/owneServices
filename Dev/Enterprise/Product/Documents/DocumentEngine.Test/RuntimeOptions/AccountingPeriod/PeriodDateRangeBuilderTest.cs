using CargoWise.Types;
using Enterprise.DocumentEngine.Testing.UtilityClasses;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class PeriodDateRangeBuilderTest : FilterBuilderTestWithTempFile
	{
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPeriodDateRange()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("AccountingPeriodDateRange.xls", TestFilesSubFolder.ReportTestFiles);
			using (var testReport = new Report(new DocumentPack(), excelTemplate))
			{
				var testReportAnalyser = new ReportAnalyser(testReport);
				var testFilterCollectionBuilder = new FilterCollectionBuilder(testReportAnalyser.DataSourceParameters, testReportAnalyser.ValidatorPack, new StringTreeBuilder(testReport.FilterSheet).GetTree(), DummyEvaluator);
				testFilterCollectionBuilder.Build();

				AssertEquals("Number of fields", 3, testFilterCollectionBuilder.IFilterCollection.Count);
				AssertEquals(typeof(PeriodDateRangeField), testFilterCollectionBuilder.IFilterCollection[1].GetType());
				AssertNoErrors("Validation should be suspended during build", (PeriodDateRangeField)testFilterCollectionBuilder.IFilterCollection[1]);

				((PeriodDateRangeField)testFilterCollectionBuilder.IFilterCollection[1]).ValueHigh = ZDateTime.Empty;
				AssertHasError("Validation should be resumed after build", ((PeriodDateRangeField)testFilterCollectionBuilder.IFilterCollection[1]).ValueHighInfo, "At least one of the 'Some date' and 'Some date1' should have data.");
			}
		}
	}
}
