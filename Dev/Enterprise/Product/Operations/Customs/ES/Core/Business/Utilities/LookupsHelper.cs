using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business;

public static class LookupsHelper
{
	public static CodeDescriptionPairList RegionOfDestinationDropEditList(BusinessObjectFactory factory)
	{
		return factory.GetCachedValue("ESStateIslandCodes", () =>
		{
			var listSpain = CustomsFiscalTerritoriesList.GetSpainFullList(factory);
			var list = new CodeDescriptionPairList
			{
				new CodeDescriptionPair("55", (NoResString)"CEUTA"),
				new CodeDescriptionPair("56", (NoResString)"MELILLA")
			};
			var listResult = new CodeDescriptionPairList();
			listResult.AddRange(listSpain);
			listResult.AddRange(list);
			listResult.Sort();
			return listResult;
		});
	}

	public static Universal.ZZRefCusCodeListCombinedCollection RegionOfDestinationCodeFindBoxList(BusinessObjectFactory factory)
		=> Universal.ZZRefCusCodeListCombinedCollection.GetCachedCollection(factory, Core.Constants.CountryCodes.Spain, UniversalReferenceConstants.RefCusCodeListTypes.CL142, ZDateTime.Today);
}
