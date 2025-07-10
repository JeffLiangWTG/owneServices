using Enterprise.DocumentEngine.Testing.UtilityClasses;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class RegistrationCodeBuilderTest : FilterBuilderTestWithTempFile
	{
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRegistrationCode()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("RegistrationCodeFilter.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(new DocumentPack(), excelTemplate))
			{
				var reportAnalyser = new ReportAnalyser(report);
				var filterCollectionBuilder = new FilterCollectionBuilder(reportAnalyser.DataSourceParameters, reportAnalyser.ValidatorPack, new StringTreeBuilder(report.FilterSheet).GetTree(), DummyEvaluator);
				filterCollectionBuilder.Build();

				Assert(!filterCollectionBuilder.HasErrors);
				AssertEquals("Number of fields", 2, filterCollectionBuilder.IFilterCollection.Count);
				AssertEquals(typeof(RegistrationCodeField), filterCollectionBuilder.IFilterCollection[1].GetType());
			}

			excelTemplate = new ExcelTemplateForUnitTesting("RegistrationCodeFilter_NoFieldNameCodeCountry.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(new DocumentPack(), excelTemplate))
			{
				var reportAnalyser = new ReportAnalyser(report);
				var filterCollectionBuilder = new FilterCollectionBuilder(reportAnalyser.DataSourceParameters, reportAnalyser.ValidatorPack, new StringTreeBuilder(report.FilterSheet).GetTree(), DummyEvaluator);
				filterCollectionBuilder.Build();

				AssertEquals(1, filterCollectionBuilder.Errors.Count);
				var error = filterCollectionBuilder.Errors[0];
				AssertEquals(@"Error Building Filters from Tree: The filter 'Some registration code' requires the parameter 'FieldNameCodeCountry'.", error.Message);
				AssertEquals("Number of fields", 0, filterCollectionBuilder.IFilterCollection.Count);
			}

			excelTemplate = new ExcelTemplateForUnitTesting("RegistrationCodeFilter_NoFieldNameCustomType.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(new DocumentPack(), excelTemplate))
			{
				var reportAnalyser = new ReportAnalyser(report);
				var filterCollectionBuilder = new FilterCollectionBuilder(reportAnalyser.DataSourceParameters, reportAnalyser.ValidatorPack, new StringTreeBuilder(report.FilterSheet).GetTree(), DummyEvaluator);
				filterCollectionBuilder.Build();

				AssertEquals(1, filterCollectionBuilder.Errors.Count);
				var error = filterCollectionBuilder.Errors[0];
				AssertEquals(@"Error Building Filters from Tree: The filter 'Some registration code' requires the parameter 'FieldNameCustomType'.", error.Message);
				AssertEquals("Number of fields", 0, filterCollectionBuilder.IFilterCollection.Count);
			}
		}
	}
}
