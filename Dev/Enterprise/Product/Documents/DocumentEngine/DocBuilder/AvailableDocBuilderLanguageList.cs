using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.DocBuilder
{
	public class AvailableDocBuilderLanguageList : CodeDescriptionPairList
	{
		public AvailableDocBuilderLanguageList(BusinessObjectFactory factory)
		{
			Load(factory);
		}

		void Load(BusinessObjectFactory factory)
		{
			foreach (var language in LanguageHelper.GetAllActiveLanguages())
			{
				var languageCode = language.FullLanguageCode;
				if (languageCode != Res.DefaultLanguage && IsLanguageAvailable(languageCode))
				{
					AddPair(languageCode, language.RA_DescriptionMultilingual);
				}
			}
			Sort();
		}

		internal static bool IsLanguageAvailable(string language)
		{
			if (Res.IsSystemDefinedEnglish(language))
			{
				return true;
			}
			else
			{
				SecurityCheckpoint security;
				Env.Security.DocBuilderLanguagesLookup.TryGetValue(language, out security);
				return security != null && security.IsAllowed;
			}
		}
	}
}
