using CargoWise.EntityFramework.Testing;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ResourceStrings.Module.Testing.LocalLanguages
{
	class LanguageEDocsViaUniversalXmlSupportTest : TestCaseWithFactory
	{
		public void TestLoadLanguageFromCode()
		{
			var testLanguage1 = Factory.New<IRefLocalLanguage>();
			testLanguage1.RA_Code = "AA";
			testLanguage1.RA_RN_NKCountryCode = "CN";
			testLanguage1.RA_Description = "Test Language1";

			var testLanguage2 = Factory.New<IRefLocalLanguage>();
			testLanguage2.RA_Code = "AA";
			testLanguage2.RA_RN_NKCountryCode = "DE";
			testLanguage2.RA_Description = "Test Language2";
			Factory.Save();

			var loader = new LocalLanguageEDocsViaUniversalXmlSupport();
			AssertEquals(testLanguage1.PK, loader.LoadBusinessObjectFromCode(Factory, "AA-CN")?.PK);
			AssertEquals(testLanguage2.PK, loader.LoadBusinessObjectFromCode(Factory, "AA-DE")?.PK);
		}

		public void TestTryLoadLanguageNotInDB()
		{
			var loader = new LocalLanguageEDocsViaUniversalXmlSupport();
			AssertEquals(null, loader.LoadBusinessObjectFromCode(Factory, "XX-12"));
		}
	}
}
