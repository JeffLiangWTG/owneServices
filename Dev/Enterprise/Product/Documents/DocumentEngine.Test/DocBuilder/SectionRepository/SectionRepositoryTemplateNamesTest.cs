using NUnit.Framework;

namespace Enterprise.DocumentEngine.DocBuilder.Testing
{
	sealed class SectionRepositoryTemplateNamesTest : TestCase
	{
		public void TestGetLanguageCodeFromTemplateName()
		{
			CombineAssertions(() =>
			{
				AssertEquals("EN", SectionRepositoryTemplateNames.GetLanguageCodeFromTemplateName("System Document Elements"));
				AssertEquals("EN", SectionRepositoryTemplateNames.GetLanguageCodeFromTemplateName("System Document Elements []"));
				AssertEquals("ZH-CN", SectionRepositoryTemplateNames.GetLanguageCodeFromTemplateName("System Document Elements [ZH-CN]"));
				AssertEquals("ES-ES", SectionRepositoryTemplateNames.GetLanguageCodeFromTemplateName("Customized Document Elements [ES-ES]"));
				AssertEquals("EN", SectionRepositoryTemplateNames.GetLanguageCodeFromTemplateName("Customized Document Elements"));
				AssertEquals("EN", SectionRepositoryTemplateNames.GetLanguageCodeFromTemplateName("Customized Document Elements []"));
				AssertEquals("EN", SectionRepositoryTemplateNames.GetLanguageCodeFromTemplateName(""));
				AssertEquals("EN", SectionRepositoryTemplateNames.GetLanguageCodeFromTemplateName("blablabla"));
			});
		}
	}
}
