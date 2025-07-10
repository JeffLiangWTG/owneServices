using Enterprise.DocumentEngine.Testing.UtilityClasses;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class NumberNotInRangeBuilderTest : FilterBuilderTest
	{
		public void TestGetFilterField()
		{
			var filterBuilder = new NumberNotInRangeBuilder(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			AssertEquals(typeof(NumberNotInRangeField), filterBuilder.NewField().GetType());
		}

		public void TestCanBuild1()
		{
			var filterBuilder = new NumberNotInRangeBuilder(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			AssertEquals(true, filterBuilder.CanBuild("number not in range"));
		}

		public void TestCanBuild2()
		{
			var filterBuilder = new NumberNotInRangeBuilder(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			AssertEquals(false, filterBuilder.CanBuild("number in range"));
		}

		public void TestCanBuild3()
		{
			var filterBuilder = new NumberNotInRangeBuilder(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
			AssertEquals(false, filterBuilder.CanBuild("number not range"));
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDefault()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("NumberNotInRangeFilterWithDefault.xls", TestFilesSubFolder.ReportTestFiles);
			using (var testReport = new Report(new DocumentPack(), excelTemplate))
			{
				var testReportAnalyser = new ReportAnalyser(testReport);
				var testFilterCollectionBuilder = new FilterCollectionBuilder(testReportAnalyser.DataSourceParameters, testReportAnalyser.ValidatorPack, new StringTreeBuilder(testReport.FilterSheet).GetTree(), DummyEvaluator);

				testFilterCollectionBuilder.Build();

				AssertEquals("Number of fields", 2, testFilterCollectionBuilder.IFilterCollection.Count);
				AssertEquals("Field with default from", 1m, ((NumberNotInRangeField)(testFilterCollectionBuilder.IFilterCollection[1])).From);
				AssertEquals("Field with default to", 10m, ((NumberNotInRangeField)(testFilterCollectionBuilder.IFilterCollection[1])).To);
			}
		}

		protected override FilterBuilder GetFilterBuilderToTest() => new NumberNotInRangeBuilder(new ValidatorPack(), Factory, DummyEvaluator, ReportRunningType.Report);
	}
}
