using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.Build;
using Enterprise.DocumentEngine.Testing;

namespace Enterprise.DocumentEngine.DocBuilder.Testing
{
	sealed class LegacyDocumentTemplateTranslationHelperTest : TestCaseWithFactory
	{
		public void TestGetAsmidByTemplatePath()
		{
			var templatePath = "\\Documents\\Test.xls";
			AssertEquals(6, TemplateSerializationHelper.GetAsmidByTemplatePath(templatePath));

			templatePath = "\\Documents\\EDI\\Test.xls";
			AssertEquals(7, TemplateSerializationHelper.GetAsmidByTemplatePath(templatePath));
		}

		public void TestGetTranslatableLabelsWithTranslateLegacyDocument()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[TranslateLegacyDocument]
{A}-[#SectionBody]
{B}-[TestCell]
{A}-[#EndOfReport]");

			var legacyDocumentTemplateTranslationHelper = new LegacyDocumentTemplateTranslationHelper();
			var reportTemplates = new Dictionary<string, byte[]>();
			reportTemplates.Add("TestTranslateLegacyDocument.xls", excelTemplate.GetAsByteArray());
			var labels = legacyDocumentTemplateTranslationHelper.GetUniqueLabels(Factory, reportTemplates);
			AssertResourceStringDataContainCaption_Context_SpecifiedKey("string in report content", labels, "TestCell", "TestTranslateLegacyDocument");
			Assert(legacyDocumentTemplateTranslationHelper.SheetNames.Contains("Document"));
			AssertEquals(true, legacyDocumentTemplateTranslationHelper.NeedTranslate("TestTranslateLegacyDocument.xls", excelTemplate.GetAsByteArray()));
		}

		public void TestGetTranslatableLabelsWithoutTranslateLegacyDocument()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
				@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[#SectionBody]
{B}-[TestCell]
{A}-[#EndOfReport]");

			var legacyDocumentTemplateTranslationHelper = new LegacyDocumentTemplateTranslationHelper();
			var reportTemplates = new Dictionary<string, byte[]>();
			reportTemplates.Add("Test.xls", excelTemplate.GetAsByteArray());
			var labels = legacyDocumentTemplateTranslationHelper.GetUniqueLabels(Factory, reportTemplates);
			AssertEquals("This template should not be translatable", 0, labels.Length);
			AssertEquals(false, legacyDocumentTemplateTranslationHelper.NeedTranslate("Test.xls", excelTemplate.GetAsByteArray()));
		}

		void AssertResourceStringDataContainCaption_Context_SpecifiedKey(string message, DataWithDocumentMacroUsage<ResourceStringData>[] resourceStrings, string caption, string contextKey)
		{
			AssertNotNull(message, Array.Find(resourceStrings, label => label.Data.Caption == caption
																		&& label.Data.MetaData.ContextClassName == contextKey
																		&& label.Data.Key.Contains(contextKey)));
		}
	}
}
