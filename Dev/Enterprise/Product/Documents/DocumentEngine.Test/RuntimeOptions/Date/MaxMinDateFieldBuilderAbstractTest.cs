using System;
using CargoWise.Types;
using Enterprise.DocumentEngine.Testing.UtilityClasses;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	abstract class MaxMinDateFieldBuilderAbstractTest : FilterBuilderTestWithTempFile
	{
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDefault()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting(DefaultXlsFileName, TestFilesSubFolder.ReportTestFiles);
			using (var testReport = new Report(new DocumentPack(), excelTemplate))
			{
				var testReportAnalyser = new ReportAnalyser(testReport);
				var testFilterCollectionBuilder = new FilterCollectionBuilder(testReportAnalyser.DataSourceParameters, testReportAnalyser.ValidatorPack, new StringTreeBuilder(testReport.FilterSheet).GetTree(), DummyEvaluator);
				testFilterCollectionBuilder.Build();
				AssertEquals("Number of fields", 3, testFilterCollectionBuilder.IFilterCollection.Count);
				AssertEquals("Type of field", ExpectedDateFieldType, testFilterCollectionBuilder.IFilterCollection[1].GetType());
				AssertEquals("Type of field", ExpectedDateFieldType, testFilterCollectionBuilder.IFilterCollection[2].GetType());
			}
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWithDefaults()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting(XlsFileWithDefault, TestFilesSubFolder.ReportTestFiles);
			using (var testReport = new Report(new DocumentPack(), excelTemplate))
			{
				var testReportAnalyser = new ReportAnalyser(testReport);
				var testFilterCollectionBuilder = new FilterCollectionBuilder(testReportAnalyser.DataSourceParameters, testReportAnalyser.ValidatorPack, new StringTreeBuilder(testReport.FilterSheet).GetTree(), DummyEvaluator);
				testFilterCollectionBuilder.Build();
				AssertEquals("Number of fields", 3, testFilterCollectionBuilder.IFilterCollection.Count);
				AssertEquals("First filter with default 18 Oct 2005", new ZDateTime(2005, 10, 18), GetValueOfFilter(testFilterCollectionBuilder, 1));
				AssertEquals("Second filter with default now", ZDateTime.Now.ToShortDateString(), GetValueOfFilter(testFilterCollectionBuilder, 2).ToShortDateString());
			}
		}

		protected abstract string DefaultXlsFileName { get; }

		protected abstract Type ExpectedDateFieldType { get; }

		protected abstract string XlsFileWithDefault { get; }

		protected abstract ZDateTime GetValueOfFilter(FilterCollectionBuilder testFilterCollectionBuilder, int index);
	}
}
