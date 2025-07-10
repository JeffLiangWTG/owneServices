using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	class FilterBuilderTestWithTempFile : TempFileTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGroupOption()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("MultipleFiltersWithGroups.xls", TestFilesSubFolder.ReportTestFiles);
			using (var testReport = new Report(new DocumentPack(), excelTemplate))
			{
				var testReportAnalyser = new ReportAnalyser(testReport);
				var testFilterCollectionBuilder = new FilterCollectionBuilder(testReportAnalyser.DataSourceParameters, testReportAnalyser.ValidatorPack, new StringTreeBuilder(testReport.FilterSheet).GetTree(), DummyEvaluator);

				testFilterCollectionBuilder.Build();

				AssertEquals("Filter Groups collection should contain 3 groups", 3, testFilterCollectionBuilder.IFilterCollection.FilterGroups.Count);
				AssertEquals("Filter groups collection should contain '' group", true, testFilterCollectionBuilder.IFilterCollection.FilterGroups.ContainsCode(""));
				AssertEquals("Filter groups collection should contain 'One' group", true, testFilterCollectionBuilder.IFilterCollection.FilterGroups.ContainsCode("One"));
				AssertEquals("Filter groups collection should contain 'Two' group", true, testFilterCollectionBuilder.IFilterCollection.FilterGroups.ContainsCode("Two"));
				AssertNullOrEmpty("Filter groups should contain 'Primary Filters' Description", testFilterCollectionBuilder.IFilterCollection.FilterGroups[""].Description);
				AssertEquals("Filter groups should contain 'Filter Group One' Description", true, testFilterCollectionBuilder.IFilterCollection.FilterGroups["One"].Description == "Filter Group One");
				AssertEquals("Filter groups should contain 'Filter Group Two' Description", true, testFilterCollectionBuilder.IFilterCollection.FilterGroups["Two"].Description == "Filter Group Two");
			}
		}

		public virtual string DummyEvaluator(Match match) => match.Value;
	}
}
