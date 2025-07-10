using Enterprise.DocumentEngine.Testing.UtilityClasses;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class ExactTextBuilderTest : FilterBuilderTestWithTempFile
	{
		public void TestCanBuild()
		{
			ExactTextBuilder builder = new ExactTextBuilder(null, Factory, DummyEvaluator, ReportRunningType.Report);
			AssertEquals("CanBuild should return true for 'ExactText' filter type", true, builder.CanBuild("ExactText"));
			AssertEquals("CanBuild should return true for 'ExactText' filter type", true, builder.CanBuild("exacttext"));
			AssertEquals("CanBuild should return true for 'ExactText' filter type", true, builder.CanBuild("eXaCtTeXt"));
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDefault()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("ExactTextFilterWithDefault.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report testReport = new Report(new DocumentPack(), excelTemplate))
			{
				testReport.PrepareForRender();
				testReport.ColumnHeadingManager.DefaultTemplateConfigurationManager.Load(testReport.ColumnHeadingManager.LinkedFilterFields, testReport.GroupByCollection, testReport.SortOrderCollection, testReport.OrientationManager, testReport.Parent);
				CollectionOfIFilter testFilterCollection = testReport.FilterCollection;
				AssertEquals("Number of fields", 2, testFilterCollection.Count);
				AssertEquals("Filter should be of ExactTextField type", typeof(ExactTextField), testFilterCollection[1].GetType());
				AssertEquals("Field with default ", "Hello", ((ExactTextField)(testFilterCollection[1])).Value);
			}
		}
	}
}
