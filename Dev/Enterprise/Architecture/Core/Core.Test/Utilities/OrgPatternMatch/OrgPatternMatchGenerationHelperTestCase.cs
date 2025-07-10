using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class OrgPatternMatchGenerationHelperTestCase : TestCase
	{
		//NB: 99% of the THIS FUNCTIONALITY IS FULLY TESTED IN MasterFiles.OrgPatternMatch. For the sake of brevity, the tests remain there.

		public void TestSingletonInstance()
		{
			OrgPatternMatchGenerationHelper helper = OrgPatternMatchGenerationHelper.Get(Constants.Languages.English, "", (NoResString)"");
			AssertEquals("English languageSettings", Constants.Languages.English, helper.languageSetting.Language);

			helper = OrgPatternMatchGenerationHelper.Get(Constants.Languages.German, "", (NoResString)"");
			AssertEquals("German languageSettings", Constants.Languages.German, helper.languageSetting.Language);

			helper = OrgPatternMatchGenerationHelper.Get(Constants.Languages.Japanese, "", (NoResString)"");
			AssertEquals("languageSettings", string.Empty, helper.languageSetting.Language);
		}

		public void TestCompanyNameSoundexWordsForDefaultLanguage()
		{
			OrgPatternMatchGenerationHelper defaultHelper = OrgPatternMatchGenerationHelper.Get(Constants.Languages.Japanese, "Sydney", (NoResString)"Australia");
			string[] cargoWiseWords = defaultHelper.CompanyNameSoundexWords("CargoWise edi (Australia) Pty Ltd", false);
			AssertEquals("cargowise 0", "C622", cargoWiseWords[0]);
			AssertEquals("cargowise 1", "E300", cargoWiseWords[1]);
			AssertEquals("cargowise 2", "A236", cargoWiseWords[2]);
			AssertEquals("cargowise 3", "", cargoWiseWords[3]);

			cargoWiseWords = defaultHelper.CompanyNameSoundexWords("中国造船業（株）", false);
			AssertEquals("japanese 0", "中", cargoWiseWords[0]);
			AssertEquals("japanese 1", "国", cargoWiseWords[1]);
			AssertEquals("japanese 2", "造", cargoWiseWords[2]);
			AssertEquals("japanese 3", "船", cargoWiseWords[3]);
			AssertEquals("japanese count", 6, cargoWiseWords.Length);
		}

		public void TestSuccinctCompanyNameForUnicodeCharacters()
		{
			OrgPatternMatchGenerationHelper defaultHelper = OrgPatternMatchGenerationHelper.Get(Constants.Languages.Japanese, "Sydney", (NoResString)"Australia");

			AssertEquals("English company name", "CARGOWISE EDI AUSTRALIA", defaultHelper.SuccinctCompanyName("CargoWise edi (Australia) Pty Ltd", false));

			string expectedBytes = "ミラー社";
			AssertEquals("English company name", expectedBytes, defaultHelper.SuccinctCompanyName("ミラー社", false));
		}
	}
}
