using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DocBuilder.Testing
{
	sealed class ReportTemplateTranslationHelperTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetTranslatableLabels()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("TestExportReportLabelString.xls", TestFilesSubFolder.ReportTestFiles);
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromExcelTemplate(Factory, excelTemplate);

			var reportTemplateTranslationHelper = new ReportTemplateTranslationHelper();
			var reportTemplates = new Dictionary<string, byte[]>();
			reportTemplates.Add(excelTemplate.FullTemplateSourceLocation, template.SO_Template);
			var labels = reportTemplateTranslationHelper.GetUniqueLabels(Factory, reportTemplates);

			AssertResourceStringDataContainCaption_Context_SpecifiedKey("string in ColumnHeading Heading Text", labels, "Charge Code", "TestExportReportLabelString");
			AssertResourceStringDataContainCaption_Context_SpecifiedKey("string in ColumnHeading Desctiption", labels, "Charge Description", "TestExportReportLabelString");
			AssertResourceStringDataContainCaption_Context_SpecifiedKey("string in report content", labels, "Document Header", "TestExportReportLabelString");
			AssertResourceStringDataContainCaption_Context_SpecifiedKey("string in filter tab", labels, "Show Transaction Header Descriptions filter", "TestExportReportLabelString");
			AssertResourceStringDataContainCaption_Context_SpecifiedKey("string in filter tab - multiple selection lookup", labels, "Vessel", "TestExportReportLabelString");
			AssertResourceStringDataContainCaption_Context_SpecifiedKey("string in groupbys tab", labels, "GroupByField", "TestExportReportLabelString");
			AssertResourceStringDataContainCaption_Context_SpecifiedKey("string in constants tab", labels, "Third para blah", "TestExportReportLabelString");
			AssertResourceStringDataContainCaption_Context_SpecifiedKey("string in sort tab", labels, "Perfectly valid sort order", "TestExportReportLabelString");
			AssertResourceStringDataContainCaption_Context_SpecifiedKey("string in optional templates tab", labels, "GLTransactionList", "TestExportReportLabelString");
			AssertResourceStringDataContainCaption_Context_SpecifiedKey("string in fields tab", labels, "Test Fields", "TestExportReportLabelString");

			Assert(reportTemplateTranslationHelper.SheetNames.Contains("GLTransactionList"));
			Assert(reportTemplateTranslationHelper.ReportTitles.Contains("GL TransactionList"));
			Assert(reportTemplateTranslationHelper.SheetNames.Contains("SecondGLTransactionList"));
			Assert(!reportTemplateTranslationHelper.SheetNames.Contains("Filters"));
			Assert(!reportTemplateTranslationHelper.SheetNames.Contains("Groupbys"));
			Assert(!reportTemplateTranslationHelper.SheetNames.Contains("Constants"));
			Assert(!reportTemplateTranslationHelper.SheetNames.Contains("Fields"));
			Assert(!reportTemplateTranslationHelper.SheetNames.Contains("Optional Templates"));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetTranslatableLabelsWithDisableTranslateOptionInTemplate()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("TestExportReportLabelStringWithDisableTranslateOption.xls", TestFilesSubFolder.ReportTestFiles);
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromExcelTemplate(Factory, excelTemplate);

			var reportTemplateTranslationHelper = new ReportTemplateTranslationHelper();
			var reportTemplates = new Dictionary<string, byte[]>();
			reportTemplates.Add(excelTemplate.FullTemplateSourceLocation, template.SO_Template);
			var labels = reportTemplateTranslationHelper.GetUniqueLabels(Factory, reportTemplates);
			AssertEquals("This template should not be translatable", 0, labels.Length);
		}

		public void TestGetAllResStringsUsedByBizOFields()
		{
			var reportTemplateTranslationHelper = new ReportTemplateTranslationHelper();
			var labels = reportTemplateTranslationHelper.GetAllResStringsUsedByBizOFields(Factory);

			Assert("Format only", labels.Count == 1);
		}

		void AssertResourceStringDataContainCaption_Context_SpecifiedKey(string message, DataWithDocumentMacroUsage<ResourceStringData>[] resourceStrings, string caption, string contextKey)
		{
			AssertNotNull(message, Array.Find(resourceStrings, label => label.Data.Caption == caption
											&& label.Data.MetaData.ContextClassName == contextKey
											&& label.Data.Key.Contains(contextKey)));
		}
	}
}
