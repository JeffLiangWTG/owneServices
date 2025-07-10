using CargoWise.Types;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class DateBuilderTest : FilterBuilderTestWithTempFile
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDefault()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("DateFilterWithDefault.xls", TestFilesSubFolder.ReportTestFiles);
			using (var testReport = new Report(new DocumentPack(), excelTemplate))
			{
				var testReportAnalyser = new ReportAnalyser(testReport);
				var testFilterCollectionBuilder = new FilterCollectionBuilder(testReportAnalyser.DataSourceParameters, testReportAnalyser.ValidatorPack, new StringTreeBuilder(testReport.FilterSheet).GetTree(), DummyEvaluator);

				testFilterCollectionBuilder.Build();

				AssertEquals("Number of fields", 5, testFilterCollectionBuilder.IFilterCollection.Count);
				AssertEquals("Second filter with default ", new ZDateTime(2005, 1, 14), ((DateField)testFilterCollectionBuilder.IFilterCollection[2]).Value);
				AssertEquals("Third filter with default to now", ZDateTime.Now.ToShortDateString(), ((DateField)testFilterCollectionBuilder.IFilterCollection[3]).Value.ToShortDateString());
				AssertEquals("4rd filter with default to now", ZDateTime.Today.ToShortDateString(), ((DateField)testFilterCollectionBuilder.IFilterCollection[4]).Value.ToShortDateString());

				CombineAssertions("PickerFormat should be built", () =>
				{
					AssertEquals(DocEngineDatePickerFormats.Short, ((DateField)testFilterCollectionBuilder.IFilterCollection[1]).PickerFormat);
					AssertEquals(DocEngineDatePickerFormats.Short, ((DateField)testFilterCollectionBuilder.IFilterCollection[2]).PickerFormat);
					AssertEquals(DocEngineDatePickerFormats.Long, ((DateField)testFilterCollectionBuilder.IFilterCollection[3]).PickerFormat);
					AssertEquals(DocEngineDatePickerFormats.Short, ((DateField)testFilterCollectionBuilder.IFilterCollection[4]).PickerFormat);
				});
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestYearAndMonth()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("DateFilterYearAndMonthOnly.xls", TestFilesSubFolder.ReportTestFiles);
			using (var testReport = new Report(new DocumentPack(), excelTemplate))
			{
				var testReportAnalyser = new ReportAnalyser(testReport);
				var testFilterCollectionBuilder = new FilterCollectionBuilder(testReportAnalyser.DataSourceParameters, testReportAnalyser.ValidatorPack, new StringTreeBuilder(testReport.FilterSheet).GetTree(), DummyEvaluator);

				testFilterCollectionBuilder.Build();

				AssertEquals("Number of fields", 2, testFilterCollectionBuilder.IFilterCollection.Count);
				AssertEquals(DocEngineDatePickerFormats.YearAndMonth, ((DateField)testFilterCollectionBuilder.IFilterCollection[1]).PickerFormat);
			}
		}
	}
}
