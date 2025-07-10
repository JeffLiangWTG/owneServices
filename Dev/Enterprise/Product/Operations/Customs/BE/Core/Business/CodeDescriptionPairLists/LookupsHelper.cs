using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business;

public static class LookupsHelper
{
	public static CodeDescriptionPairList GetBELanguageList(BusinessObjectFactory factory)
	{
		return factory.GetCachedValue("BE.Business.GetBELanguageList", () =>
		{
			var result = new CodeDescriptionPairList();
			var languageList = LanguageHelper.GetDefaultLanguageForOLookUpEditType();
			result.AddPair(Core.Constants.CountryCodes.Netherlands, languageList.GetValueSafe(SharedConstants.Languages.Dutch));
			result.AddPair(Core.Constants.CountryCodes.France, languageList.GetValueSafe(SharedConstants.Languages.French));
			result.AddPair(Core.Constants.CountryCodes.Germany, languageList.GetValueSafe(SharedConstants.Languages.German));
			result.AddPair(SharedConstants.Languages.English, languageList.GetValueSafe(SharedConstants.Languages.English));
			return result;
		});
	}
}
