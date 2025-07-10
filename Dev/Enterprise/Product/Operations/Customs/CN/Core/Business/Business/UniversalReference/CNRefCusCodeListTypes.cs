using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using ECC = Enterprise.Core.Constants;
using RefCusCodeListTypesCodes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes;

namespace Enterprise.Customs.CN.Business
{
	public static class CNRefCusCodeListTypes
	{
		public static ZZRefCusCodeListCombinedCollection GetCNPorts(BusinessObjectFactory factory, ZDateTime dateOfValuation)
		{
			return GetCachedCollection(factory, RefCusCodeListTypesCodes.Port, dateOfValuation);
		}

		public static ZZRefCusCodeListCombinedCollection GetDistrictList(BusinessObjectFactory factory, ZDateTime dateOfValuation)
		{
			return GetCachedCollection(factory, RefCusCodeListTypesCodes.DistrictCode, dateOfValuation);
		}

		public static ZZRefCusCodeListCombinedCollection GetRegionList(BusinessObjectFactory factory, ZDateTime dateOfValuation)
		{
			return GetCachedCollection(factory, RefCusCodeListTypesCodes.CNCIQDistricts, dateOfValuation);
		}

		public static ZZRefCusCodeListCombinedCollection GetStatesList(BusinessObjectFactory factory, ZDateTime dateOfValuation)
		{
			return GetCachedCollection(factory, RefCusCodeListTypesCodes.CNCIQStates, dateOfValuation);
		}

		public static ZZRefCusCodeListCombinedCollection GetCommodityInspectionList(BusinessObjectFactory factory, ZDateTime dateOfValuation)
		{
			return GetCachedCollection(factory, RefCusCodeListTypesCodes.CNCIQCommodityInspection, dateOfValuation);
		}

		public static ZZRefCusCodeListCombinedCollection GetCIQOfficeCodes(BusinessObjectFactory factory, ZDateTime dateOfValuation)
		{
			return GetCachedCollection(factory, RefCusCodeListTypesCodes.CNCIQOfficeCode, dateOfValuation);
		}

		public static ZZRefCusCodeListCombinedCollection GetCNCIQPorts(BusinessObjectFactory factory, ZDateTime dateOfValuation)
		{
			return GetCachedCollection(factory, RefCusCodeListTypesCodes.CNCIQPortOffices, dateOfValuation);
		}

		public static ZZRefCusCodeListCombinedCollection GetCusSupportingDocumentList(BusinessObjectFactory factory, ZDateTime dateOfValuation)
		{
			return GetCachedCollection(factory, RefCusCodeListTypesCodes.CNRequiredDocuments, dateOfValuation);
		}

		public static CodeDescriptionPairList GetApplicablePreferentialTradeAgreements(BusinessObjectFactory factory, ZDateTime dateOfValuation, ZString countryCode)
		{
			return RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(factory, ECC.CountryCodes.China, RefCusCodeListTypesCodes.CNPreferentialTradeAgreement, dateOfValuation, true, Constants.UniversalReferenceConstants.CusCodeListAttributeName.ApplicableCountry, new[] { countryCode });
		}

		public static ZZRefCusCodeListCombinedCollection GetCustomsOfficeList(BusinessObjectFactory factory, ZDateTime dateOfValuation)
		{
			return GetCachedCollection(factory, RefCusCodeListTypesCodes.CustomsOffice, dateOfValuation);
		}

		public static ZZRefCusCodeListCombinedCollection GetTransitionSiteList(BusinessObjectFactory factory, ZString customsOffice, ZDateTime dateOfValuation)
		{
			return GetCachedCollection(
				factory,
				new ZString[] {
					Constants.RefCusCodeListTypes.CNDesignatedSitesUnderSupervisionForImportedMeat,
					Constants.RefCusCodeListTypes.CNDesignatedSitesUnderSupervisionForImportedFrozenandFreshSeafoodProducts,
					Constants.RefCusCodeListTypes.CNDesignatedSitesUnderSupervisionForImportedGrain,
					Constants.RefCusCodeListTypes.CNDesignatedSitesUnderSupervisionForImportedFruit,
					Constants.RefCusCodeListTypes.CNDesignatedSitesUnderSupervisionForImportedEdibleAquaticAnimals,
					Constants.RefCusCodeListTypes.CNDesignatedSitesUnderSupervisionForImportedPlantSeedlings,
					Constants.RefCusCodeListTypes.CNDesignatedSitesUnderSupervisionForImportedLogs,
					Constants.RefCusCodeListTypes.CNIsolationSitesforQuarantineofImportedAnimals
				},
				dateOfValuation,
				new RefCusCodeListAttributeFilter[] { new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.CustomsOffice, JoinCondition.And, new[] { customsOffice }) }
			);
		}

		public static CodeDescriptionPairList GetTradeAgreementCodesSupportDeclarationOfOrigin(BusinessObjectFactory factory, ZDateTime dateOfValuation)
		{
			return RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(factory, ECC.CountryCodes.China, RefCusCodeListTypesCodes.CNPreferentialTradeAgreement,
				dateOfValuation, false, Constants.UniversalReferenceConstants.CusCodeListAttributeName.SupportsDeclarationOfOrigin, new ZString[] { UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes });
		}

		public static CodeDescriptionPairList GetSupportingDocumentsSupportsTSD(BusinessObjectFactory factory, ZDateTime dateOfValuation)
		{
			return RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(factory, ECC.CountryCodes.China, RefCusCodeListTypesCodes.CNRequiredDocuments,
				dateOfValuation, false, Constants.UniversalReferenceConstants.CusCodeListAttributeName.TSDSupported);
		}

		static ZZRefCusCodeListCombinedCollection GetCachedCollection(BusinessObjectFactory factory, ZString codeType, ZDateTime dateOfValuation)
		{
			var result = GetCachedCollection(factory, new[] { codeType }, dateOfValuation, null);
			result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.CountryOrGrouping, PropertyNameForModuleTextFilter, (ZString)ECC.CountryCodes.China, false));
			result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.ListType, PropertyNameForModuleTextFilter, codeType, false));
			result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.Description, PropertyNameForModuleTextFilter, ZString.Empty));
			return result;
		}

		static ZZRefCusCodeListCombinedCollection GetCachedCollection(BusinessObjectFactory factory, ZString[] codeTypes, ZDateTime dateOfValuation, RefCusCodeListAttributeFilter[] attrFilters)
		{
			return ZZRefCusCodeListCombinedCollection.GetCachedCollection(factory, ECC.CountryCodes.China, codeTypes, dateOfValuation, attrFilters);
		}

		static readonly ZString PropertyNameForModuleTextFilter = (NoResString)"Property";

		public static ZZRefCusCodeListCombinedCollection GetDangerousChemicalList(BusinessObjectFactory factory, ZDateTime dateOfValuation)
		{
			var dangerousChemicalCollection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(factory, ECC.CountryCodes.China, RefCusCodeListTypesCodes.CNDangerousChemical, dateOfValuation);
			dangerousChemicalCollection.Load();
			return dangerousChemicalCollection;
		}

		public static IEnumerable<ZString> GetDangerousChemicalCASList(BusinessObjectFactory factory, ZDateTime dateOfValuation)
		{
			return factory.GetCachedValue("CNDangerousChemicalCASList",
				() => GetDangerousChemicalList(factory, dateOfValuation).Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
		}

		public static IEnumerable<ZString> GetDangerousChemicalGoodsNameList(BusinessObjectFactory factory, ZDateTime dateOfValuation)
		{
			return factory.GetCachedValue("CNDangerousChemicalGoodsNameList",
				() =>
				{
					var dangerousChemicalList = GetDangerousChemicalList(factory, dateOfValuation).Cast<ZZRefCusCodeListCombined>();
					return dangerousChemicalList.Select(x => x.ZZD_Description).Union(dangerousChemicalList.SelectMany(x => x.GetAttributesValues(Constants.UniversalReferenceConstants.CusCodeListAttributeName.Alias)));
				});
		}
	}
}
