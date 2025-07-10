using System;
using CargoWise.Application;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.Integration.Accounting;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class MultipleChoiceBuilderTest : FilterBuilderTestWithTempFile
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDefault()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("MultipleChoiceWithDefault.xls", TestFilesSubFolder.ReportTestFiles);
			using (var testReport = new Report(new DocumentPack(), excelTemplate))
			{
				var testReportAnalyser = new ReportAnalyser(testReport);
				var testFilterCollectionBuilder = new FilterCollectionBuilder(testReportAnalyser.DataSourceParameters, testReportAnalyser.ValidatorPack, new StringTreeBuilder(testReport.FilterSheet).GetTree(), testReport.ReplaceSingleMacroNotInTemplateBody);
				testFilterCollectionBuilder.Build();

				AssertEquals("MultipleChoice of fields", 2, testFilterCollectionBuilder.IFilterCollection.Count);
				AssertEquals("Field with default", "Val 3", ((MultipleChoice)testFilterCollectionBuilder.IFilterCollection[1]).Value);
				AssertEquals("Field with default", "Val 3", ((MultipleChoice)testFilterCollectionBuilder.IFilterCollection[1]).DefaultExpression);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDefaultNew()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("MultipleChoiceWithDefault.xls", TestFilesSubFolder.ReportTestFiles);
			using (var testReport = new Report(new DocumentPack(), excelTemplate))
			{
				testReport.FilterSheet[9, 1] = "DefaultValue";
				testReport.FilterSheet[9, 2] = "Val 2";
				var testReportAnalyser = new ReportAnalyser(testReport);
				var testFilterCollectionBuilder = new FilterCollectionBuilder(testReportAnalyser.DataSourceParameters, testReportAnalyser.ValidatorPack, new StringTreeBuilder(testReport.FilterSheet).GetTree(), DummyEvaluator);
				testFilterCollectionBuilder.Build();

				AssertEquals("MultipleChoice of fields", 2, testFilterCollectionBuilder.IFilterCollection.Count);
				AssertEquals("Field with default", "Val 2", ((MultipleChoice)testFilterCollectionBuilder.IFilterCollection[1]).Value);
				AssertEquals("Field with default", "Val 2", ((MultipleChoice)testFilterCollectionBuilder.IFilterCollection[1]).DefaultExpression);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDefaultWithPaymentWebService()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("MultipleChoiceWithDefault.xls", TestFilesSubFolder.ReportTestFiles);
			using (var testReport = new Report(new DocumentPack(), excelTemplate))
			{
				testReport.FilterSheet[7, 2] = OnlyCurrentPeriodIfPayByWebServiceValidator.NoAgeing; // NON value must be on the list
				testReport.FilterSheet[10, 1] = "onlycurrentperiodifpaybywebservice"; // FilterBuilder.CurrentPeriodOnlyIfPayByWebService 

				// Payment Web Servise is Disabled by Default
				var testReportAnalyser = new ReportAnalyser(testReport);
				var testFilterCollectionBuilder = new FilterCollectionBuilder(testReportAnalyser.DataSourceParameters, testReportAnalyser.ValidatorPack, new StringTreeBuilder(testReport.FilterSheet).GetTree(), DummyEvaluator);
				testFilterCollectionBuilder.Build();

				AssertEquals("MultipleChoice of fields", 2, testFilterCollectionBuilder.IFilterCollection.Count);
				AssertEquals("Default", "Val 3", ((MultipleChoice)testFilterCollectionBuilder.IFilterCollection[1]).Value);
				AssertEquals("Default", "Val 3", ((MultipleChoice)testFilterCollectionBuilder.IFilterCollection[1]).DefaultExpression);

				// Mock Payment Web Service enabled
				var mockSupporter = new Mock<IAccounting>();
				mockSupporter.Setup(m => m.IsInvoicePaymentWebServiceEnabled(It.IsAny<Guid>())).Returns(true);
				using (ObjectFactory.Substitute<IAccounting>(mockSupporter.Object))
				{
					testReportAnalyser = new ReportAnalyser(testReport);
					testFilterCollectionBuilder = new FilterCollectionBuilder(testReportAnalyser.DataSourceParameters, testReportAnalyser.ValidatorPack, new StringTreeBuilder(testReport.FilterSheet).GetTree(), DummyEvaluator);
					testFilterCollectionBuilder.Build();

					AssertEquals("MultipleChoice of fields", 2, testFilterCollectionBuilder.IFilterCollection.Count);
					AssertEquals("No Ageing", OnlyCurrentPeriodIfPayByWebServiceValidator.NoAgeing, ((MultipleChoice)testFilterCollectionBuilder.IFilterCollection[1]).Value);
					AssertEquals("No Ageing", OnlyCurrentPeriodIfPayByWebServiceValidator.NoAgeing, ((MultipleChoice)testFilterCollectionBuilder.IFilterCollection[1]).DefaultExpression);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDefaultNotInList()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("MultipleChoiceWithDefault.xls", TestFilesSubFolder.ReportTestFiles);
			using (var testReport = new Report(new DocumentPack(), excelTemplate))
			{
				testReport.FilterSheet[9, 1] = "DefaultValue";
				testReport.FilterSheet[9, 2] = "NotInList";
				var testReportAnalyser = new ReportAnalyser(testReport);
				var testFilterCollectionBuilder = new FilterCollectionBuilder(testReportAnalyser.DataSourceParameters, testReportAnalyser.ValidatorPack, new StringTreeBuilder(testReport.FilterSheet).GetTree(), DummyEvaluator);
				testFilterCollectionBuilder.Build();

				AssertEquals("MultipleChoice of fields", 2, testFilterCollectionBuilder.IFilterCollection.Count);
				AssertEquals("Field with default", "", ((MultipleChoice)testFilterCollectionBuilder.IFilterCollection[1]).Value.ToString());
				AssertEquals("Field with default", "NotInList", ((MultipleChoice)testFilterCollectionBuilder.IFilterCollection[1]).DefaultExpression);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDefaultAsMacro()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("MultipleChoiceWithDefault.xls", TestFilesSubFolder.ReportTestFiles);
			using (var testReport = new Report(new DocumentPack(), excelTemplate))
			{
				testReport.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Val2", "Val 2"));
				testReport.FilterSheet[9, 1] = "DefaultValue";
				testReport.FilterSheet[9, 2] = "<Val2>";
				var testReportAnalyser = new ReportAnalyser(testReport);
				var testFilterCollectionBuilder = new FilterCollectionBuilder(testReportAnalyser.DataSourceParameters, testReportAnalyser.ValidatorPack, new StringTreeBuilder(testReport.FilterSheet).GetTree(), testReport.ReplaceSingleMacroNotInTemplateBody);
				testFilterCollectionBuilder.Build();

				AssertEquals("MultipleChoice of fields", 2, testFilterCollectionBuilder.IFilterCollection.Count);
				AssertEquals("Field with default", "Val 2", ((MultipleChoice)testFilterCollectionBuilder.IFilterCollection[1]).Value);
				AssertEquals("Field with default", "<Val2>", ((MultipleChoice)testFilterCollectionBuilder.IFilterCollection[1]).DefaultExpression);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCodeDescriptionPairList()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("MultipleChoiceWithDefault.xls", TestFilesSubFolder.ReportTestFiles);
			using (var testReport = new Report(new DocumentPack(), excelTemplate))
			{
				var testReportAnalyser = new ReportAnalyser(testReport);
				var testFilterCollectionBuilder = new FilterCollectionBuilder(testReportAnalyser.DataSourceParameters, testReportAnalyser.ValidatorPack, new StringTreeBuilder(testReport.FilterSheet).GetTree(), DummyEvaluator);
				testFilterCollectionBuilder.Build();

				AssertEquals("MultipleChoice of fields", 2, testFilterCollectionBuilder.IFilterCollection.Count);
				AssertEquals("Number of options", 5, ((MultipleChoice)testFilterCollectionBuilder.IFilterCollection[1]).List.Count);
				AssertEquals("Field with default", "Val 1", ((MultipleChoice)testFilterCollectionBuilder.IFilterCollection[1]).List[0].Code);
				AssertEquals("Field with default", "Option 1", ((MultipleChoice)testFilterCollectionBuilder.IFilterCollection[1]).List[0].Description);
				AssertEquals("Field with default", "Val 2", ((MultipleChoice)testFilterCollectionBuilder.IFilterCollection[1]).List[1].Code);
				AssertEquals("Field with default", "Option 2", ((MultipleChoice)testFilterCollectionBuilder.IFilterCollection[1]).List[1].Description);
				AssertEquals("Field with default", "Val 3", ((MultipleChoice)testFilterCollectionBuilder.IFilterCollection[1]).List[2].Code);
				AssertEquals("Field with default", "Option 3", ((MultipleChoice)testFilterCollectionBuilder.IFilterCollection[1]).List[2].Description);
				AssertEquals("Field with default", "Val 4", ((MultipleChoice)testFilterCollectionBuilder.IFilterCollection[1]).List[3].Code);
				AssertEquals("Field with default", "Option 4", ((MultipleChoice)testFilterCollectionBuilder.IFilterCollection[1]).List[3].Description);
				AssertEquals("Code only", "Code only", ((MultipleChoice)testFilterCollectionBuilder.IFilterCollection[1]).List[4].Code);
				AssertEquals("Description should be blank if not specified", "", ((MultipleChoice)testFilterCollectionBuilder.IFilterCollection[1]).List[4].Description);
				AssertEquals(MultipleChoice.Styles.DropDown, ((MultipleChoice)testFilterCollectionBuilder.IFilterCollection[1]).Style);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAllowInvalidCode()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("MultipleChoiceWithAllowInvalidCode.xls", TestFilesSubFolder.ReportTestFiles);
			using (var testReport = new Report(new DocumentPack(), excelTemplate))
			{
				var testReportAnalyser = new ReportAnalyser(testReport);
				var testFilterCollectionBuilder = new FilterCollectionBuilder(testReportAnalyser.DataSourceParameters, testReportAnalyser.ValidatorPack, new StringTreeBuilder(testReport.FilterSheet).GetTree(), DummyEvaluator);
				testFilterCollectionBuilder.Build();

				AssertEquals("MultipleChoice of fields", 3, testFilterCollectionBuilder.IFilterCollection.Count);
				AssertEquals("Number of options", 2, ((MultipleChoice)testFilterCollectionBuilder.IFilterCollection[1]).List.Count);
				AssertEquals("AllowInvalidCode", false, ((MultipleChoice)testFilterCollectionBuilder.IFilterCollection[1]).AllowInvalidCode);
				AssertEquals("Number of options", 3, ((MultipleChoice)testFilterCollectionBuilder.IFilterCollection[2]).List.Count);
				AssertEquals("AllowInvalidCode", true, ((MultipleChoice)testFilterCollectionBuilder.IFilterCollection[2]).AllowInvalidCode);
			}
		}
	}
}
