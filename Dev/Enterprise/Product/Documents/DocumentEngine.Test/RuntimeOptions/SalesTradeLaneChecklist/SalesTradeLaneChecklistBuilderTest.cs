using System.Linq;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class SalesTradeLaneChecklistBuilderTest : FilterBuilderTestWithTempFile
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGeneralUsage()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("SalesTradeLaneChecklistFilter.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(new DocumentPack(), excelTemplate))
			{
				var reportAnalyser = new ReportAnalyser(report);
				var filterCollectionBuilder = new FilterCollectionBuilder(reportAnalyser.DataSourceParameters, reportAnalyser.ValidatorPack, new StringTreeBuilder(report.FilterSheet).GetTree(), DummyEvaluator);
				filterCollectionBuilder.Build();

				AssertEquals(false, filterCollectionBuilder.HasErrors);
				AssertEquals("Number of fields", 2, filterCollectionBuilder.IFilterCollection.Count);
				AssertType(typeof(SalesTradeLaneChecklistField), filterCollectionBuilder.IFilterCollection[1]);

				var field = (SalesTradeLaneChecklistField)filterCollectionBuilder.IFilterCollection[1];
				AssertEquals("XX_Product", field.FieldName);
				AssertEquals("XX_Mode", field.ModeField);
				AssertEquals("XX_Type", field.TypeField);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMissingParameters()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("SalesTradeLaneChecklistFilter_MissingParameters.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(new DocumentPack(), excelTemplate))
			{
				var reportAnalyser = new ReportAnalyser(report);
				var filterCollectionBuilder = new FilterCollectionBuilder(reportAnalyser.DataSourceParameters, reportAnalyser.ValidatorPack, new StringTreeBuilder(report.FilterSheet).GetTree(), DummyEvaluator);
				filterCollectionBuilder.Build();

				AssertEquals(true, filterCollectionBuilder.HasErrors);
				AssertContainsExactElementsInAnyOrder(
					new[]
					{
						"Error Building Filters from Tree: The filter 'Trade Lane' requires the parameter 'ModeField'."
					},
					filterCollectionBuilder.Errors.Select(x => x.Message));
			}
		}
	}
}
