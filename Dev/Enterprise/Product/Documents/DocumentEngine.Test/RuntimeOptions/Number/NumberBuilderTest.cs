using System;
using CargoWise.Types;
using Enterprise.DocumentEngine.Testing.UtilityClasses;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class NumberBuilderTest : FilterBuilderTestWithTempFile
	{
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDefault()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("NumberFieldWithDefault.xls", TestFilesSubFolder.ReportTestFiles);
			using (var testReport = new Report(new DocumentPack(), excelTemplate))
			{
				var testReportAnalyzer = new ReportAnalyser(testReport);
				var testFilterCollectionBuilder = new FilterCollectionBuilder(testReportAnalyzer.DataSourceParameters, testReportAnalyzer.ValidatorPack, new StringTreeBuilder(testReport.FilterSheet).GetTree(), DummyEvaluator);

				testFilterCollectionBuilder.Build();

				AssertEquals("Number of fields", 6, testFilterCollectionBuilder.IFilterCollection.Count);
				AssertEquals("Field with default", 42.33m, ((NumberField)testFilterCollectionBuilder.IFilterCollection[1]).Value);
				AssertEquals("Field without default", DBNull.Value, ((NumberField)testFilterCollectionBuilder.IFilterCollection[2]).Value);
				AssertEquals("Field with year default", ZDateTime.Now.Year, ((NumberField)testFilterCollectionBuilder.IFilterCollection[3]).Value);

				AssertEquals("Field without decimal places", 2, ((NumberField)testFilterCollectionBuilder.IFilterCollection[1]).DecimalPlaces);
				AssertEquals("Field with decimal places", 3, ((NumberField)testFilterCollectionBuilder.IFilterCollection[2]).DecimalPlaces);
				AssertEquals("Field with min value", 123.45m, ((NumberField)testFilterCollectionBuilder.IFilterCollection[4]).MinValue);
				AssertEquals("Field without min value", (decimal)int.MinValue, ((NumberField)testFilterCollectionBuilder.IFilterCollection[5]).MinValue);
				AssertEquals("Field with max value", 99.88m, ((NumberField)testFilterCollectionBuilder.IFilterCollection[5]).MaxValue);
				AssertEquals("Field without max value", (decimal)int.MaxValue, ((NumberField)testFilterCollectionBuilder.IFilterCollection[4]).MaxValue);
				AssertEquals("Field with group separators", true, ((NumberField)testFilterCollectionBuilder.IFilterCollection[4]).ShowGroupSeparators);
				AssertEquals("Field with no group separators", false, ((NumberField)testFilterCollectionBuilder.IFilterCollection[5]).ShowGroupSeparators);
				AssertEquals("Field without group separators", true, ((NumberField)testFilterCollectionBuilder.IFilterCollection[3]).ShowGroupSeparators);
			}
		}
	}
}
