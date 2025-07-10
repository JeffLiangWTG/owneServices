using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DocBuilder.Testing
{
	[TestedType(typeof(TemplateSection))]
	sealed class TemplateSectionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestTagIncludingCategory()
		{
			var templateSection = new TemplateSection("Gen:Invoice ,   Pick a section , pick a poke.  ", 45, 55);
			AssertEquals("templateSection.TypeCode", ConfigurableSectionTypeList.Codes.GenericSection, templateSection.TypeCode);
			AssertEquals("templateSection.SectionName", "Pick a section , pick a poke.", templateSection.SectionName);
			AssertEquals("templateSection.StartingRowNumber", 45, templateSection.StartingRowNumber);
			AssertEquals("templateSection.RowCount", 55, templateSection.RowCount);
			AssertEquals("templateSection.Category", "Invoice", templateSection.Category);
		}

		public void TestIsGenericSectionType()
		{
			var nonGenericTemplateSection = new TemplateSection("BOD, This is not a generic section.", 0, 0);
			AssertEquals("nonGenericTemplateSection.IsGenericSectionType", false, nonGenericTemplateSection.IsGenericSectionType);

			var genericTemplateSection = new TemplateSection("GEN, This is a generic section", 0, 0);
			AssertEquals("genericTemplateSection.IsGenericSectionType", true, genericTemplateSection.IsGenericSectionType);
		}

		public void TestSectionNameWithComma()
		{
			var templateSection = new TemplateSection("BOD, This is a body section, and it has a comma in the section name", 1, 2);
			AssertEquals("templateSection.TypeCode", "BOD", templateSection.TypeCode);
			AssertEquals("templateSection.SectionName", "This is a body section, and it has a comma in the section name", templateSection.SectionName);
			AssertEquals("templateSection.Category", "", templateSection.Category);
		}

		public void TestConstructor()
		{
			var templateSection = new TemplateSection(ConfigurableSectionTypeList.Codes.DocumentHeader + " , My Favourite Martian", 1, 2);
			AssertEquals("templateSection.TypeCode", ConfigurableSectionTypeList.Codes.DocumentHeader, templateSection.TypeCode);
			AssertEquals("templateSection.SectionName", "My Favourite Martian", templateSection.SectionName);
			AssertEquals("templateSection.StartingRowNumber", 1, templateSection.StartingRowNumber);
			AssertEquals("templateSection.RowCount", 2, templateSection.RowCount);
			AssertEquals("templateSection.Category", "", templateSection.Category);
			AssertEquals("templateSection.TemplateName", "", templateSection.TemplateName);
			AssertEquals("templateSection.FullLabel", "", templateSection.FullLabel);

			templateSection = new TemplateSection("abc, XyZ", 0, 0);
			AssertEquals("templateSection.TypeCode", "ABC", templateSection.TypeCode);
			AssertEquals("templateSection.SectionName", "XyZ", templateSection.SectionName);
			AssertEquals("templateSection.Category", "", templateSection.Category);
			AssertEquals("templateSection.TemplateName", "", templateSection.TemplateName);
			AssertEquals("templateSection.FullLabel", "", templateSection.FullLabel);

			templateSection = new TemplateSection("GEN:Warehouse PickingSlip, PageTotals + Running Totals [FR]", 4, 5, "Customized Document Elements [FRN]", "#ConfigurableSection:GEN:Warehouse PickingSlip, PageTotals + Running Totals [FR]");
			AssertEquals("templateSection.TypeCode", ConfigurableSectionTypeList.Codes.GenericSection, templateSection.TypeCode);
			AssertEquals("templateSection.SectionName", "PageTotals + Running Totals [FR]", templateSection.SectionName);
			AssertEquals("templateSection.StartingRowNumber", 4, templateSection.StartingRowNumber);
			AssertEquals("templateSection.RowCount", 5, templateSection.RowCount);
			AssertEquals("templateSection.Category", "Warehouse PickingSlip", templateSection.Category);
			AssertEquals("templateSection.TemplateName", "Customized Document Elements [FRN]", templateSection.TemplateName);
			AssertEquals("templateSection.FullLabel", "#ConfigurableSection:GEN:Warehouse PickingSlip, PageTotals + Running Totals [FR]", templateSection.FullLabel);
		}

		public void TestIsControlSection()
		{
			AssertEquals("BodySection should not be a Control Section.", false, new TemplateSection(ConfigurableSectionTypeList.Codes.BodySection, 0, 0).IsControlSection);
			AssertEquals("ConfigSection should be a Control Section.", true, new TemplateSection(ConfigurableSectionTypeList.Codes.ConfigSection, 0, 0).IsControlSection);
			AssertEquals("EndOfReport should be a Control Section.", true, new TemplateSection(ConfigurableSectionTypeList.Codes.EndOfReport, 0, 0).IsControlSection);
		}

		public void TestLastRow()
		{
			var templateSection = new TemplateSection(ConfigurableSectionTypeList.Codes.DocumentHeader, 10, 5);
			AssertEquals("LastRow", 15, templateSection.LastRowNumber);
		}

		public void TestValidation()
		{
			var templateSection = new TemplateSection("", 0, 0);
			AssertEquals(typeof(TemplateSectionValidation), templateSection.Validation.GetType());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new TemplateSection(ConfigurableSectionTypeList.Codes.DocumentHeader + " , My Favourite Martian", 1, 2);
		}
	}
}
