using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Business
{
	public static class IncoTermCodeListHelper
	{
		public static CodeDescriptionPairList GetCachedIncoTermListEU(this BusinessObjectFactory factory, bool isUCC6)
		{
			var cacheKey = "IncoTermListEU|" + (isUCC6 ? "UCC6" : "NO_UCC6");
			return factory.GetCachedValue(cacheKey, () => IncoTermListEU(isUCC6));
		}

		public static CodeDescriptionPairList IncoTermListEU(bool isUCC6)
		{
			var defaultIncoTermsCodeDescriptionPairList = new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms);
			if (isUCC6)
			{
				defaultIncoTermsCodeDescriptionPairList.AddPairIfNotExist(Constants.IncoTerms.Other, Constants.IncoTerms.Descriptions.DefaultCodeDescriptionPairs[Constants.IncoTerms.Other] ?? Constants.IncoTerms.Descriptions.Other);
			}
			return defaultIncoTermsCodeDescriptionPairList;
		}
	}
}
