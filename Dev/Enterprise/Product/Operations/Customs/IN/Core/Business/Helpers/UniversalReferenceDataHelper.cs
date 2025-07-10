using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes;

namespace Enterprise.Customs.IN.Business;

public static class UniversalReferenceDataHelper
{
	public static ZZRefCusCodeListCombinedCollection GetCustomsOfficeCollection(BusinessObjectFactory factory, bool isAir, ZDateTime? date = null)
	{
		var additionalFilter = new ZQuery(ZZRefCusCodeListCombinedSchema.ZZD_IsAir, isAir);
		var collection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(factory, additionalFilter, Core.Constants.CountryCodes.India, new ZString[] { RefCusCodeListTypes.CustomsOffice }, date ?? ZDateTime.Today, null);
		var comparisonOperator = isAir ? ModuleTextFilter.ComparisonConstants.Exact : ModuleTextFilter.ComparisonConstants.NotEqual;
		collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.TransportMode, "Property", (ZString)RefTransportModeList.Codes.AIR));
		collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.TransportMode, "ComparisonOperator", new ZString(comparisonOperator), isRemovable: false));
		return collection;
	}

	public static ZZRefCusCodeListCombined GetCustomsOffice(BusinessObjectFactory factory, ZString customsOffice, ZDateTime date)
	{
		return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, customsOffice, Core.Constants.CountryCodes.India, RefCusCodeListTypes.CustomsOffice, date);
	}

	public static ZZRefCusCodeListCombined GetSWControl(BusinessObjectFactory factory, ZString controlResultCode, ZDateTime date)
	{
		return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, controlResultCode, Core.Constants.CountryCodes.India, RefCusCodeListTypes.SingleWindowControl, date);
	}

	public static ZZRefCusCodeListCombinedCollection GetSupportingDocumentTypeCollection(BusinessObjectFactory factory, ZDateTime? date = null)
	{
		return ZZRefCusCodeListCombinedCollection.GetCachedCollection(factory, Core.Constants.CountryCodes.India, RefCusCodeListTypes.IndiaSupportingDocument, date ?? ZDateTime.Today);
	}

	public static CodeDescriptionPairList GetCustomsUnitOfQuantityList(BusinessObjectFactory factory, ZDateTime date)
	{
		return Enterprise.Customs.Universal.RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.India, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, date);
	}
}
