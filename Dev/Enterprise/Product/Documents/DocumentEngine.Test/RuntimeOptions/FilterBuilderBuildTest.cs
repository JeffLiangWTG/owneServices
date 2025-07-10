using System.Linq;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class FilterBuilderBuildTest : TempFileTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReadOnlyIf()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("MultipleFiltersWithReadOnlyIf.xls", TestFilesSubFolder.ReportTestFiles);
			using (var testReport = new Report(new DocumentPack(), excelTemplate))
			{
				var testReportAnalyser = new ReportAnalyser(testReport);
				var testFilterCollectionBuilder = new FilterCollectionBuilder(testReportAnalyser.DataSourceParameters, testReportAnalyser.ValidatorPack, new StringTreeBuilder(testReport.FilterSheet).GetTree(), DummyEvaluator);
				testFilterCollectionBuilder.Build();

				AssertNotEquals("ReadOnlyIf Relation should be set", null, testFilterCollectionBuilder.IFilterCollection.FirstOrDefault(e => ((FilterField)e).HasReadOnlyIfFilter));
			}
		}

		string DummyEvaluator(Match match)
		{
			return match.Value;
		}
	}
}
