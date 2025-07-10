using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.JP.Common;

public static class JPRefCusCodeListTypes
{
	public static IBusinessObjectCollection GetJapanBondedAreaCodes(BusinessObjectFactory factory, ZString transportMode, string defaultCode = "")
	{
		var result = GetCachedCollectionWithDefaults(factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanBondedAreaCode, ZDateTime.Today);

		if (!string.IsNullOrEmpty(defaultCode))
		{
			result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.Code, PropertyNameForModuleTextFilter, new ZString(defaultCode), true));
		}

		result.FilterBusinessObjectDefaults.Add(FilterBusinessObjectDefault.Create(
				filterName: Universal.Constants.ZZRefCusCodeListFilters.TransportMode,
				propertyName: PropertyNameForModuleTextFilter,
				value: transportMode,
				comparisonOperator: ModuleTextFilter.ComparisonConstants.Exact
			));
		return result;
	}

	public static IBusinessObjectCollection GetJapanBondedAreaCodes(BusinessObjectFactory factory)
	{
		return GetCachedCollection(factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanBondedAreaCode, ZDateTime.Today);
	}

	public static ZZRefCusCodeListCombined GetJapanBondedAreaCode(BusinessObjectFactory factory, ZString code)
	{
		return GetJPCodeByType(factory, code, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanBondedAreaCode, ZDateTime.Today);
	}

	public static IBusinessObjectCollection GetSpecialCargoCodes(BusinessObjectFactory factory)
	{
		return GetCachedCollectionWithDefaults(factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanSpecialCargoCode, ZDateTime.Today, true);
	}

	public static ZZRefCusCodeListCombined GetSpecialCargoCode(BusinessObjectFactory factory, ZString code)
	{
		return GetJPCodeByType(factory, code, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanSpecialCargoCode, ZDateTime.Today);
	}

	static ZZRefCusCodeListCombined GetJPCodeByType(BusinessObjectFactory factory, ZString code, ZString codeType, ZDateTime date)
	{
		return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, code, Core.Constants.CountryCodes.Japan, codeType, date);
	}

	static ZZRefCusCodeListCombinedCollection GetCachedCollectionWithDefaults(BusinessObjectFactory factory, ZString codeType, ZDateTime dateOfValuation, bool descriptionToAdd = false)
	{
		var result = GetCachedCollection(factory, new[] { codeType }, dateOfValuation, null);
		result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.CountryOrGrouping, PropertyNameForModuleTextFilter, (ZString)Core.Constants.CountryCodes.Japan, false));
		result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.ListType, PropertyNameForModuleTextFilter, codeType, false));
		if (descriptionToAdd)
		{
			result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.Description, PropertyNameForModuleTextFilter, ZString.Empty));
		}
		return result;
	}

	static ZZRefCusCodeListCombinedCollection GetCachedCollection(BusinessObjectFactory factory, ZString codeType, ZDateTime date)
	{
		return ZZRefCusCodeListCombinedCollection.GetCachedCollection(factory, Core.Constants.CountryCodes.Japan, codeType, date);
	}

	static ZZRefCusCodeListCombinedCollection GetCachedCollection(BusinessObjectFactory factory, ZString[] codeTypes, ZDateTime dateOfValuation, RefCusCodeListAttributeFilter[] attrFilters)
	{
		return ZZRefCusCodeListCombinedCollection.GetCachedCollection(factory, Core.Constants.CountryCodes.Japan, codeTypes, dateOfValuation, attrFilters);
	}

	public static CodeDescriptionPairList GetCachedList(BusinessObjectFactory factory, ZString codeType) => RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.Japan, codeType, ZDateTime.Today);

	static readonly ZString PropertyNameForModuleTextFilter = (NoResString)"Property";
}
