using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class LanguageHelperTest : TestCaseWithFactory
	{
		public void TestGetBaseSystemLanguage()
		{
			var testLanguage1 = Factory.New<IRefLocalLanguage>();
			testLanguage1.RA_Code = "AA";
			testLanguage1.RA_RN_NKCountryCode = "CN";
			testLanguage1.RA_Description = "Test Language1";
			Factory.Save();

			var testLanguage2 = Factory.New<IRefLocalLanguage>();
			testLanguage2.RA_Code = "BB";
			testLanguage2.RA_RN_NKCountryCode = "CN";
			testLanguage2.RA_Description = "Test Language2";
			testLanguage2.RA_RA_ParentLanguage = testLanguage1.PK;
			Factory.Save();

			var testLanguage3 = Factory.New<IRefLocalLanguage>();
			testLanguage3.RA_Code = "CC";
			testLanguage3.RA_RN_NKCountryCode = "CN";
			testLanguage3.RA_Description = "Test Language3";
			testLanguage3.RA_RA_ParentLanguage = testLanguage2.PK;
			Factory.Save();

			var baseSystemLanguage = LanguageHelper.GetBaseSystemLanguage(testLanguage3.FullLanguageCode);
			AssertEquals("Should get the base system language code", Res.DefaultLanguage, baseSystemLanguage);
		}

		public void TestGetCustomLanguageByFullLanguageCodeOrDescription()
		{
			var testLanguage1 = Factory.New<IRefLocalLanguage>();
			testLanguage1.RA_Code = "AA";
			testLanguage1.RA_RN_NKCountryCode = "CN";
			testLanguage1.RA_Description = "Test Language1";
			testLanguage1.RA_IsActive = true;

			var testLanguage2 = Factory.New<IRefLocalLanguage>();
			testLanguage2.RA_Code = "BB";
			Factory.Save();

			var loadedLanguage = LanguageHelper.GetCustomLanguageByFullLanguageCodeOrDescription(testLanguage1.FullLanguageCode, Factory);
			AssertEquals("Should load testLanguage1", testLanguage1.PK, loadedLanguage.PK);

			loadedLanguage = LanguageHelper.GetCustomLanguageByFullLanguageCodeOrDescription("aa-cn", Factory);
			AssertEquals("Should load testLanguage1", testLanguage1.PK, loadedLanguage.PK);

			testLanguage1.RA_IsActive = false;
			Factory.Save();
			loadedLanguage = LanguageHelper.GetCustomLanguageByFullLanguageCodeOrDescription("aa-cn", Factory);
			AssertNull("Should not load inactive language", loadedLanguage);

			loadedLanguage = LanguageHelper.GetCustomLanguageByFullLanguageCodeOrDescription("bb", Factory);
			AssertEquals("Should load testLanguage2", testLanguage2.PK, loadedLanguage.PK);

			loadedLanguage = LanguageHelper.GetCustomLanguageByFullLanguageCodeOrDescription("aa- cn", Factory);
			AssertNull("No matched language to be loaded.", loadedLanguage);
		}

		public void TestGetCustomLanguageByLanguageCode()
		{
			var testLanguage = Factory.New<IRefLocalLanguage>();
			testLanguage.RA_Code = "AA";
			testLanguage.RA_RN_NKCountryCode = "CN";
			testLanguage.RA_Description = "Test Language1";
			Factory.Save();

			var loadedLanguage = LanguageHelper.GetCustomLanguageByLanguageCode(testLanguage.FullLanguageCode);
			AssertEquals("Should load the correct language", testLanguage.PK, loadedLanguage.PK);
		}

		public void TestActiveLanguageExistsForOLookUpEditType()
		{
			var testLanguage = Factory.New<IRefLocalLanguage>();
			testLanguage.RA_Code = "AA";
			testLanguage.RA_RN_NKCountryCode = "CN";
			testLanguage.RA_Description = "Test Language1";

			var testLanguage2 = Factory.New<IRefLocalLanguage>();
			testLanguage2.RA_Code = "BB";
			testLanguage2.RA_RN_NKCountryCode = "CN";
			testLanguage2.RA_IsActive = false;
			testLanguage2.RA_Description = "Test Language2";

			Factory.Save();
			var testLanguage3 = Factory.New<IRefLocalLanguage>();
			testLanguage3.RA_Code = "CC";
			testLanguage3.RA_RN_NKCountryCode = "CN";
			testLanguage3.RA_Description = "Test Language3";

			CombineAssertions(() =>
			{
				Assert("AA-CN should exists", LanguageHelper.ActiveLanguageExistsForOLookUpEditType(testLanguage.FullLanguageCode));
				Assert("BB-CN inactive", !LanguageHelper.ActiveLanguageExistsForOLookUpEditType(testLanguage2.FullLanguageCode));
				Assert("CC-CN not in db", !LanguageHelper.ActiveLanguageExistsForOLookUpEditType(testLanguage3.FullLanguageCode));
				Assert("empty", !LanguageHelper.ActiveLanguageExistsForOLookUpEditType(""));
				Assert("default Constants.Languages.English should exists", LanguageHelper.ActiveLanguageExistsForOLookUpEditType(Constants.Languages.English));
			});
		}
	}
}
