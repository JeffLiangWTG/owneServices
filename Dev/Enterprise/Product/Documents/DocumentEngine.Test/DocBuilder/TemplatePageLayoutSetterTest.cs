using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DocBuilder.Testing
{
	sealed class TemplatePageLayoutSetterTest : TestCase
	{
		public void TestSetToPortrait()
		{
			var template = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[Hello World]
{A}-[#EndOfReport]");

			using (var excelInterface = new ExcelInterface(template.GetAsByteArray()))
			{
				var workSheet = excelInterface.WorkSheets[0];
				var setter = new TemplatePageLayoutSetter();
				setter.SetPageLayout(workSheet, DocumentConfigPageStyleList.Codes.Portrait);

				AssertMultilineASCIIEquals("PageStyle should be Portrait.",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[Hello World]
{A}-[#EndOfReport]",
					workSheet.ToString());
			}
		}

		public void TestSetToLandscape()
		{
			var template = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[Hello World]
{A}-[#EndOfReport]");

			using (var excelInterface = new ExcelInterface(template.GetAsByteArray()))
			{
				var workSheet = excelInterface.WorkSheets[0];
				var setter = new TemplatePageLayoutSetter();
				setter.SetPageLayout(workSheet, DocumentConfigPageStyleList.Codes.Landscape);

				AssertMultilineASCIIEquals("PageStyle should be Landscape.",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[PAGESTYLE=Landscape]
{A}-[#SectionBody]
{B}-[Hello World]
{A}-[#EndOfReport]",
					workSheet.ToString());
			}
		}

		public void TestSetToContinuous()
		{
			var template = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[Hello World]
{A}-[#EndOfReport]");

			using (var excelInterface = new ExcelInterface(template.GetAsByteArray()))
			{
				var workSheet = excelInterface.WorkSheets[0];
				var setter = new TemplatePageLayoutSetter();
				setter.SetPageLayout(workSheet, DocumentConfigPageStyleList.Codes.Continuous);

				AssertMultilineASCIIEquals("PageStyle should be Continuous.",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[PAGESTYLE=Continuous]
{A}-[#SectionBody]
{B}-[Hello World]
{A}-[#EndOfReport]",
					workSheet.ToString());
			}
		}

		public void TestSetToContinuousWithNoMargins()
		{
			var template = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[Hello World]
{A}-[#EndOfReport]");

			using (var excelInterface = new ExcelInterface(template.GetAsByteArray()))
			{
				var workSheet = excelInterface.WorkSheets[0];
				var setter = new TemplatePageLayoutSetter();
				setter.SetPageLayout(workSheet, DocumentConfigPageStyleList.Codes.ContinuousWithNoMargin);

				AssertMultilineASCIIEquals("PageStyle should be Continuous.",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[PAGESTYLE=Continuous]
{A}-[#SectionBody]
{B}-[Hello World]
{A}-[#EndOfReport]",
					workSheet.ToString());

				var margins = excelInterface.Xls.GetPrintMargins();
				AssertEquals("margins.Left", 0.0, margins.Left);
				AssertEquals("margins.Right", 0.0, margins.Right);
				AssertEquals("margins.Top", 0.0, margins.Top);
				AssertEquals("margins.Bottom", 0.0, margins.Bottom);
				AssertEquals("margins.Header", 0.0, margins.Header);
				AssertEquals("margins.Footer", 0.0, margins.Footer);
			}
		}

		public void TestSetToLabel()
		{
			var template = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[Hello World]
{A}-[#EndOfReport]");

			using (var excelInterface = new ExcelInterface(template.GetAsByteArray()))
			{
				var workSheet = excelInterface.WorkSheets[0];
				var setter = new TemplatePageLayoutSetter();
				setter.SetPageLayout(workSheet, DocumentConfigPageStyleList.Codes.Label);

				AssertMultilineASCIIEquals("PageStyle should be Label.",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[PAGESTYLE=Label]
{A}-[#SectionBody]
{B}-[Hello World]
{A}-[#EndOfReport]",
					workSheet.ToString());

				var margins = excelInterface.Xls.GetPrintMargins();
				AssertEquals("margins.Left", 0.0, margins.Left);
				AssertEquals("margins.Right", 0.0, margins.Right);
				AssertEquals("margins.Top", 0.0, margins.Top);
				AssertEquals("margins.Bottom", 0.0, margins.Bottom);
				AssertEquals("margins.Header", 0.0, margins.Header);
				AssertEquals("margins.Footer", 0.0, margins.Footer);
			}
		}

		public void TestPageStylesInAllLanguages()
		{
			foreach (var language in DataFile.GetAvailableLanguages())
			{
				using (Res.TemporarilySwitchLanguage(language))
				{
					TestSetToPortrait();
					TestSetToLandscape();
					TestSetToContinuous();
					TestSetToContinuousWithNoMargins();
					TestSetToLabel();
				}
			}
		}
	}
}
