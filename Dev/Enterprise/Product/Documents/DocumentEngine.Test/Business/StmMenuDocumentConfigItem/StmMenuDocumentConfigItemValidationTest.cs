using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;

namespace Enterprise.DocumentEngine.Business.Testing
{
	sealed class StmMenuDocumentConfigItemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckS4_SectionAndAgainAfterAddingANewSection()
		{
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN, Generic Section 1]
{B}-[This is Generic Section 1.]
{A}-[#EndOfReport]");
			Factory.Save();

			var config = ConfigurableTemplateTestHelper.CreateDocumentConfig(template);
			var configItem1 = config.ConfigItems.AddFromTemplateSection(new TemplateSection("GEN, Generic Section 1", 0, 0));
			configItem1.Validation.ValidateS4_SectionType();
			AssertEquals("configItem1.HasErrors", false, configItem1.HasErrors);

			Factory.ClearCachedValue<TemplateSectionCollection.CacheManager>("TemplateSectionCollectionCacheManager");
			template.SO_Template = DocumentEngineTestHelper.CreateTemplateFromString(
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN, Generic Section 1]
{B}-[This is Generic Section 1.]
{A}-[#ConfigurableSection:GEN, Generic Section 2]
{B}-[This is Generic Section 2.]
{A}-[#EndOfReport]");
			Factory.Save();

			var configItem2 = config.ConfigItems.AddFromTemplateSection(new TemplateSection("GEN, Generic Section 2", 0, 0));
			configItem2.Validation.ValidateS4_SectionType();
			AssertEquals("configItem2.HasErrors", false, configItem2.HasErrors);
		}

		public void TestValidateTemplate()
		{
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN, SectionName}
{B}-[This is Generic Section 1.]
{A}-[#ConfigurableSection:GEN,LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLL]
{B}-[This is Generic Section 2.]
{A}-[#ConfigurableSection:GEN,AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA]
{B}-[This is Generic Section 3.]
{A}-[#EndOfReport]");

			AssertHasError(template.SO_TemplateInfo,
				@"The following section names are too long. They must not exceed 128 characters:

LLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLL

AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
		}

		public void TestValidateS4_SectionTypeWithSystemAndUserCustomizableTemplate()
		{
			var systemTemplate = StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.System);
			var userTemplate = DocumentEngineTestHelper.CreateTemplateFromString(Factory, SectionRepositoryTemplateNames.User, ConfigurableTemplateTestHelper.ConfigurableStripsForTesting);

			var config = ConfigurableTemplateTestHelper.CreateDocumentConfig(systemTemplate);

			var configItem = config.ConfigItems.AddFromTemplateSection(new TemplateSection("GEN, Generic Section 1", 0, 0));
			configItem.Validation.ValidateS4_SectionType();
			AssertEquals("configItem.HasErrors", false, configItem.HasErrors);
		}

		public void TestValidateS4_SectionType()
		{
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateForTesting(Factory);
			var config = ConfigurableTemplateTestHelper.CreateDocumentConfig(template);

			var invalidConfigItem = ConfigurableTemplateTestHelper.AddFromTemplateSection(config, "Section Body 1");
			invalidConfigItem.S4_SectionType = "XXX";
			invalidConfigItem.Validation.ValidateS4_SectionType();
			AssertEquals("configItem.HasErrors", true, invalidConfigItem.HasErrors);
			AssertHasError("configItem.S4_SectionTypeInfo", invalidConfigItem.S4_SectionTypeInfo, "Invalid Section Type.");

			var configItem = ConfigurableTemplateTestHelper.AddFromTemplateSection(config, "Section Body 1");
			configItem.Validation.ValidateS4_SectionType();
			AssertEquals("configItem.HasErrors", false, configItem.HasErrors);

			var invalidGenericConfigItem = ConfigurableTemplateTestHelper.AddFromTemplateSection(config, "Generic Section 1");
			invalidGenericConfigItem.S4_SectionType = ConfigurableSectionTypeList.Codes.BodySection;
			invalidGenericConfigItem.Validation.ValidateS4_SectionType();
			AssertEquals("configItem.HasErrors", true, invalidGenericConfigItem.HasErrors);
			AssertHasError("configItem.S4_SectionTypeInfo", invalidGenericConfigItem.S4_SectionTypeInfo, "Invalid Section Type.");

			var genericConfigItem = ConfigurableTemplateTestHelper.AddFromTemplateSection(config, "Generic Section 1");
			genericConfigItem.Validation.ValidateS4_SectionType();
			AssertEquals("configItem.HasErrors", false, genericConfigItem.HasErrors);
		}
	}
}
