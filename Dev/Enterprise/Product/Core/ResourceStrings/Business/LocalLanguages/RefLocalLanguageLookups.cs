using Enterprise.ZArchitecture.Core;

namespace Enterprise.ResourceStrings.Business
{
	public class RefLocalLanguageLookups : AutoRefLocalLanguageLookups
	{
		public RefLocalLanguageLookups(AutoRefLocalLanguage parent) : base(parent)
		{
		}

		public CodeDescriptionPairList LocalLanguagesList
		{
			get
			{
				return Factory.GetCachedValue("RefLocalLanguageLookups.LocalLanguagesList",
					() =>
					{
						var allActiveLanguages = LanguageHelper.GetAllActiveLanguages();
						var list = new CodeDescriptionPairList();
						foreach (var language in allActiveLanguages)
						{
							list.AddPair(language.PK, language.FullLanguageCode, language.RA_DescriptionMultilingual);
						}
						return list;
					});
			}
		}
	}
}
