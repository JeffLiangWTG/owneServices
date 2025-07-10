using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants.Customs.Universal;
using static Enterprise.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.ES.Business
{
	public static class CustomsFiscalTerritoriesList
	{
		public static CodeDescriptionPairList GetSpainFullList(BusinessObjectFactory factory)
		{
			return factory == null ? new CodeDescriptionPairList() : factory.GetCachedValue("ESStateIslandCodes", () =>
			{
				var listSpain = GetSpainList(factory);
				var listSpainA = GetNorthAfricanSpainList(factory);
				var listSpainC = GetCanaryIslandsList(factory);
				var listResult = new CodeDescriptionPairList();
				listResult.AddRange(listSpain);
				listResult.AddRange(listSpainA);
				listResult.AddRange(listSpainC);
				listResult.Sort();
				return listResult;
			});
		}
		public static CodeDescriptionPairList GetSpainList(BusinessObjectFactory factory) => GetList(factory, Core.Constants.CountryCodes.Spain);
		public static CodeDescriptionPairList GetNorthAfricanSpainList(BusinessObjectFactory factory) => GetList(factory, NorthAfricanSpainCode);
		const string NorthAfricanSpainCode = Core.Constants.CountryCodes.Spain + "A";
		public static CodeDescriptionPairList GetCanaryIslandsList(BusinessObjectFactory factory) => GetList(factory, CanaryIslandsCode);
		const string CanaryIslandsCode = Core.Constants.CountryCodes.Spain + "C";
		static CodeDescriptionPairList GetList(BusinessObjectFactory factory, string code) => factory == null ?
				new CodeDescriptionPairList() :
				EUUniversalLookupsHelper.GetCachedList(factory, code, RefCusCodeListTypes.Codes.CustomsFiscalTerritories, null, Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusCodeListDirectionType.Both, true, IncludeParentDataGroupingOptions.ChildOnly);
	}
}
