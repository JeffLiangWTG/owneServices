using System.Threading;
using CargoWise.Organizations.PatternMatching;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Core
{
	public static class OrgPatternLanguageSetting
	{
		static IOrgPatternLanguageSettingFactory factoryInstance;

		public static IOrgPatternLanguageSettingFactory FactoryInstance
		{
			get { return LazyInitializer.EnsureInitialized(ref factoryInstance, () => new OrgPatternLanguageSettingFactory(AllowNumericCharactersInCodeGeneration)); }
		}

		public static IOrgPatternLanguageSetting Get(string languageCode)
		{
			return FactoryInstance.Get(languageCode);
		}

		public static IOrgPatternLanguageSetting Get(string languageCode, string city, MultilingualString country)
		{
			return FactoryInstance.Get(languageCode, city, country.GetUnresolvedString());
		}

		static bool AllowNumericCharactersInCodeGeneration()
		{
			return OrganisationsDataRegistryProvider.Instance.AllowNumericCharactersInCodeGeneration.Value;
		}
	}
}
