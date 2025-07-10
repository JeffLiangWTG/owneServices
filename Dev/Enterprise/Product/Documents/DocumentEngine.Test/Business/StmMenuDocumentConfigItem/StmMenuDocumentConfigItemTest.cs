using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Business.Testing
{
	[TestedType(typeof(StmMenuDocumentConfigItem))]
	sealed class StmMenuDocumentConfigItemTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTemplateSection()
		{
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
@"{A}-[#config]
{A}-[Name=TemplateWithGenericSections]
{A}-[#ConfigurableSection:BOD, Section Body 1]
{B}-[#SectionBody]
{B}-[This is Section Body 1.]
{A}-[#ConfigurableSection:GEN, Generic Section 1]
{B}-[This is Generic Section 1.]
{A}-[#ConfigurableSection:PFT, Page Footer 1]
{B}-[#PageFooter]
{B}-[This is Page Footer 1.]
{A}-[#EndOfReport]");

			var config = ConfigurableTemplateTestHelper.CreateDocumentConfig(template);
			var configItem = ConfigurableTemplateTestHelper.AddFromTemplateSection(config, "Generic Section 1");
			AssertNotNull(configItem);
			AssertNotNull(configItem.TemplateSection);
			AssertEquals("configItem.TemplateSection.IsGenericSectionType", true, configItem.TemplateSection.IsGenericSectionType);
			AssertEquals("configItem.TemplateSection.IsGenericSectionType", ConfigurableSectionTypeList.Codes.GenericSection, configItem.TemplateSection.TypeCode);
			AssertEquals("configItem.TemplateSection.IsGenericSectionType", "Generic Section 1", configItem.TemplateSection.SectionName);
		}

		public void TestTemplateSectionWithParentConfigFromNonConfigurableTemplate()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "TemplateWithGenericSections",
@"{A}-[#config]
{A}-[Name=NonConfigurableTemplate]
{B}-[#SectionBody]
{B}-[This is Section Body 1.]
{A}-[#EndOfReport]");

			var config = ConfigurableTemplateTestHelper.CreateDocumentConfig(template);
			var configItem = config.ConfigItems.AddFromTemplateSection(new TemplateSection("BOD, Section Body 1", 0, 0));
			AssertNotNull(configItem);
			AssertNull(configItem.TemplateSection);
		}

		public void TestTemplateSectionWithNoParentConfig()
		{
			var configItem = Factory.New<StmMenuDocumentConfigItem>();
			AssertNotNull(configItem);
			AssertNull(configItem.TemplateSection);
		}

		public void TestIsGenericSectionType()
		{
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateForTesting(Factory);
			var config = ConfigurableTemplateTestHelper.CreateDocumentConfig(template);

			var nonGenericConfigItem = ConfigurableTemplateTestHelper.AddFromTemplateSection(config, "Section Body 1");
			AssertEquals("nonGenericConfigItem.IsGenericSectionType", false, nonGenericConfigItem.IsGenericSectionType);

			var genericConfigItem = ConfigurableTemplateTestHelper.AddFromTemplateSection(config, "Generic Section 1");
			AssertEquals("nonGenericConfigItem.IsGenericSectionType", true, genericConfigItem.IsGenericSectionType);
		}

		public void TestPropertiesReadOnly()
		{
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateForTesting(Factory);
			var config = ConfigurableTemplateTestHelper.CreateDocumentConfig(template);

			var nonGenericConfigItem = ConfigurableTemplateTestHelper.AddFromTemplateSection(config, "Section Body 1");
			AssertEquals(true, nonGenericConfigItem.S4_SectionItemNameInfo.ReadOnly);
			AssertEquals(true, nonGenericConfigItem.S4_PrintOrderInfo.ReadOnly);
			AssertEquals(true, nonGenericConfigItem.S4_SectionTypeInfo.ReadOnly);
			AssertEquals(false, nonGenericConfigItem.S4_FilterListInfo.ReadOnly);

			var genericConfigItem = ConfigurableTemplateTestHelper.AddFromTemplateSection(config, "Generic Section 1");
			AssertEquals(true, genericConfigItem.S4_SectionItemNameInfo.ReadOnly);
			AssertEquals(true, genericConfigItem.S4_PrintOrderInfo.ReadOnly);
			AssertEquals(false, genericConfigItem.S4_SectionTypeInfo.ReadOnly);
			AssertEquals(false, genericConfigItem.S4_FilterListInfo.ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			StmMenuTemplatePivotBase menuTemplatePivot = factory.NewWithValidTestData<StmMenuTemplatePivotBase>();
			menuTemplatePivot.SI_SU = factory.NewWithValidTestData<StmMenuItemBase>().PK;
			menuTemplatePivot.SI_SO = factory.NewWithValidTestData<StmTemplateBase>().PK;
			StmMenuDocumentConfig config = factory.NewWithValidTestData<StmMenuDocumentConfig>();
			config.S3_SI = menuTemplatePivot.PK;
			return config.ConfigItems.AddNew();
		}
	}
}
