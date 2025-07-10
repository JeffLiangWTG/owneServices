using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.Testing;

namespace Enterprise.DocumentEngine.DocBuilder.Testing
{
	sealed class CoverSheetTemplateTranslationHelperTest : TestCaseWithFactory
	{
		public void TestGetTranslatableLabels()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[#SectionBody]
{B}-[TestCell]
{A}-[#EndOfReport]");

			var coverSheetTemplateTranslationHelper = new CoverSheetTemplateTranslationHelper();
			var coverSheetTemplates = new Dictionary<string, byte[]>();
			coverSheetTemplates.Add("TestCoverSheet.xls", excelTemplate.GetAsByteArray());
			var labels = coverSheetTemplateTranslationHelper.GetUniqueLabels(Factory, coverSheetTemplates);
			AssertResourceStringDataContainCaption_Context_SpecifiedKey("string in cover sheet content", labels, "TestCell", "TestCoverSheet");
			AssertEquals(true, coverSheetTemplateTranslationHelper.SheetNames.Contains("Document"));
			AssertEquals(true, coverSheetTemplateTranslationHelper.NeedTranslate("TestCoverSheet.xls", excelTemplate.GetAsByteArray()));
		}

		void AssertResourceStringDataContainCaption_Context_SpecifiedKey(string message, DataWithDocumentMacroUsage<ResourceStringData>[] resourceStrings, string caption, string contextKey)
		{
			AssertNotNull(message, Array.Find(resourceStrings, label => label.Data.Caption == caption
																		&& label.Data.MetaData.ContextClassName == contextKey
																		&& label.Data.Key.Contains(contextKey)));
		}
	}
}
