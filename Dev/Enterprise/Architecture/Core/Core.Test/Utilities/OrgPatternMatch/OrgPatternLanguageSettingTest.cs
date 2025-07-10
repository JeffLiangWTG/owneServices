using System;
using Enterprise.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class OrgPatternLanguageSettingTest : TransactionedTestCase
	{
		public void TestGivenAllParametersShouldRetrieveSettingForLanguage()
		{
			var result = OrgPatternLanguageSetting.Get(Constants.Languages.Czech, "A", (NoResString)"B").Language;
			AssertEquals(Constants.Languages.Czech, result);
		}

		public void TestGivenCityAndCountryShouldRetrieveSettingForCity()
		{
			var setting = OrgPatternLanguageSetting.Get(Constants.Languages.English, "Sydney", (NoResString)"Australia");
			var result = setting.RemoveIgnoredOrganisationWords("Orange Sydney Australia XYZ");
			AssertEquals("ORANGE AUSTRALIA XYZ", result);
		}

		public void TestGivenLanguageShouldRetrieveSettingForLanguage()
		{
			var result = OrgPatternLanguageSetting.Get(Constants.Languages.Czech).Language;
			AssertEquals(Constants.Languages.Czech, result);
		}

		public void TestShouldAllowNumericCharactersInCodeGenerationIfRegistryItemIsSetToAllow()
		{
			SetAllowNumericCharactersInCodeGeneration(true);
			var result = OrgPatternLanguageSetting.Get(Constants.Languages.English).RemoveNonLetterChars("ABC123", true, false);
			AssertEquals("ABC123", result);
		}

		public void TestShouldNotAllowNumericCharactersInCodeGenerationIfRegistryItemIsSetToNotAllow()
		{
			SetAllowNumericCharactersInCodeGeneration(false);
			var result = OrgPatternLanguageSetting.Get(Constants.Languages.English).RemoveNonLetterChars("ABC123", true, false);
			AssertEquals("ABC", result);
		}

		public void TestShouldUseSingleFactoryInstance()
		{
			var setting1 = OrgPatternLanguageSetting.Get(Constants.Languages.English);
			var setting2 = OrgPatternLanguageSetting.Get(Constants.Languages.English);
			AssertSame(setting1, setting2);
		}

		static void SetAllowNumericCharactersInCodeGeneration(bool value)
		{
			OrganisationsDataRegistryProvider.Instance.AllowNumericCharactersInCodeGeneration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}
	}
}
