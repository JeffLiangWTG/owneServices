using System.Linq;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class PermitTypeChecklistBuilderTest : FilterBuilderTestWithTempFile
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGeneralUsage()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("PermitTypeChecklistFilter.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(new DocumentPack(), excelTemplate))
			{
				var reportAnalyser = new ReportAnalyser(report);
				var filterCollectionBuilder = new FilterCollectionBuilder(reportAnalyser.DataSourceParameters, reportAnalyser.ValidatorPack, new StringTreeBuilder(report.FilterSheet).GetTree(), DummyEvaluator);
				filterCollectionBuilder.Build();

				AssertEquals(false, filterCollectionBuilder.HasErrors);
				AssertEquals("Number of fields", 2, filterCollectionBuilder.IFilterCollection.Count);
				AssertType(typeof(PermitTypeChecklistField), filterCollectionBuilder.IFilterCollection[1]);

				var field = (PermitTypeChecklistField)filterCollectionBuilder.IFilterCollection[1];
				AssertEquals("XX_Type", field.FieldName);
				AssertEquals("XX_SubType", field.SubTypeField);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMissingParameters()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("PermitTypeChecklistFilter_MissingParameters.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(new DocumentPack(), excelTemplate))
			{
				var reportAnalyser = new ReportAnalyser(report);
				var filterCollectionBuilder = new FilterCollectionBuilder(reportAnalyser.DataSourceParameters, reportAnalyser.ValidatorPack, new StringTreeBuilder(report.FilterSheet).GetTree(), DummyEvaluator);
				filterCollectionBuilder.Build();

				AssertEquals(true, filterCollectionBuilder.HasErrors);
				AssertContainsExactElementsInAnyOrder(
					new[]
					{
						"Error Building Filters from Tree: The filter 'Permit Type/Sub Type' requires the parameter 'SubTypeField'."
					},
					filterCollectionBuilder.Errors.Select(x => x.Message));
			}
		}
	}
}
