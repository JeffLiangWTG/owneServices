using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public static class UniversalLookupsHelper
	{
		public static ZZRefCusCodeListCombinedCollection GetCountryList(this BusinessObjectFactory factory, ZString dataGroupingCode, ZString codeType)
		{
			var collection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(factory, dataGroupingCode, new[] { codeType }, ZDateTime.Today, Array.Empty<RefCusCodeListAttributeFilter>(), RefCusCodeListTypes.IncludeParentDataGroupingOptions.ChildFirstThenParent);
			collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.CountryOrGrouping, "Property", dataGroupingCode, false));
			collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.ListType, "Property", codeType, false));
			return collection;
		}

		public static CodeDescriptionPairList GetCL234List(this NctsHeader header)
		{
			string countryCode = header.DefaultDataGroupingCode;
			countryCode = countryCode == Core.Constants.CountryCodes.Switzerland ? RefDataGroupingCodes.EuropeanUnionEUN : countryCode;
			return RefCusCodeListTypes.GetCachedList(header.Factory, countryCode, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL234, ZDateTime.Today);
		}

		public static CodeDescriptionPairList GetCountryCL505List(this NctsHeader header) => RefCusCodeListTypes.GetCachedList(header.Factory, header.DefaultDataGroupingCode, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL505, ZDateTime.Today);

		public static ZZRefCusCodeListCombinedCollection GetCountryNC008Collection(this BusinessObjectFactory factory, ZString dataGroupingCode)
		{
			return factory.GetCachedValue($"EU.Ncts.GetCountryNC008Collection_{dataGroupingCode}", () =>
			{
				var countryCollection = GetCountryList(factory, dataGroupingCode, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008);
				countryCollection.Load();
				return countryCollection.Count > 0 ? countryCollection : GetCountryList(factory, RefDataGroupingCodes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008);
			});
		}

		public static CodeDescriptionPairList GetCL010CountryCodes(this BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("EU.Ncts.GetCL010CodeList_EUN", () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddRange(ZZRefCusCodeListCombined.Loader.Load(factory, RefDataGroupingCodes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, ZDateTime.Today, includeParentDataGrouping: false));
				return result;
			});
		}

		public static CodeDescriptionPairList GetCountryC0009List(this BusinessObjectFactory factory, ZString dataGroupingCode)
		{
			return factory.GetCachedValue($"EU.Ncts.GetC0009CodeList_{dataGroupingCode}", () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddRange(ZZRefCusCodeListCombined.Loader.Load(factory, dataGroupingCode, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009, ZDateTime.Today));
				if (result.Count == 0)
				{
					result.AddRange(ZZRefCusCodeListCombined.Loader.Load(factory, RefDataGroupingCodes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0009, ZDateTime.Today));
				}
				result.Sort();
				return result;
			});
		}

		public static ZZRefCusCodeListCombinedCollection GetNCNATCountryList(this BusinessObjectFactory factory) =>
			GetCountryList(factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NCNAT);

		public static CodeDescriptionPairList GetEUCommunityCountryCodesList(this BusinessObjectFactory factory, ZString dataGroupingCode)
		{
			return RefCusCodeListTypes.GetCachedList(factory, dataGroupingCode, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, ZDateTime.Today);
		}

		public static CodeDescriptionPairList GetCL229EUGuaranteeTypeCTC(this BusinessObjectFactory factory) =>
			RefCusCodeListTypes.GetCachedList(factory, RefDataGroupingCodes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL229, ZDateTime.Today);

		public static CodeDescriptionPairList GetCL230EUGuaranteeTypeEUNonTIR(this BusinessObjectFactory factory) =>
			RefCusCodeListTypes.GetCachedList(factory, RefDataGroupingCodes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL230, ZDateTime.Today);

		public static CodeDescriptionPairList GetCountryCodesCTC(this BusinessObjectFactory factory) => RefCusCodeListTypes.GetCachedList(factory,
			Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
			UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL112,
			ZDateTime.Today);

		public static CodeDescriptionPairList GetCountryCodesNCTSCountryOutsideCustomsSecurityAgreementArea(this BusinessObjectFactory factory) => RefCusCodeListTypes.GetCachedList(factory,
			Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
			UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL247,
			ZDateTime.Today);

		public static CodeDescriptionPairList GetNctsPreviousDocumentUnionGoodsCode(this BusinessObjectFactory factory) => RefCusCodeListTypes.GetCachedList(factory,
			Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
			UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL178,
			ZDateTime.Today);

		public static CodeDescriptionPairList GetCL172CustomsOfficeDestinationCodes(this BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("EU.Ncts.GetCL172CustomsOfficeDestinationCodes", () =>
			{
				var result = new CodeDescriptionPairList();
				var customsOfficeOfDestinationCodeCollection = EUCustomsOfficeCodeCollection.AllEuropeanUnionAndCtCountriesCustomsOfficesWithRequiredRoles(factory, EuOfficeCodesTypes.Codes.OfficeOfDestination);
				customsOfficeOfDestinationCodeCollection.Load();
				result.AddRange(customsOfficeOfDestinationCodeCollection);
				return result;
			});
		}

		public static CodeDescriptionPairList GetCL294CustomsOfficeExitCodes(this BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("EU.Ncts.GetCL294OfficeOfExitCodes", () =>
			{
				var result = new CodeDescriptionPairList();
				var customsOfficeExitCodeCollection = EUCustomsOfficeCodeCollection.AllEuropeanUnionAndCtCountriesCustomsOfficesWithRequiredRoles(factory, EuOfficeCodesTypes.Codes.OfficeOfExit);
				customsOfficeExitCodeCollection.Load();
				result.AddRange(customsOfficeExitCodeCollection);
				return result;
			});
		}

		public static CodeDescriptionPairList GetCL740(this BusinessObjectFactory factory) =>
			RefCusCodeListTypes.GetCachedList(factory, RefDataGroupingCodes.EuropeanUnionEUN, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL740, ZDateTime.Today);
	}
}
