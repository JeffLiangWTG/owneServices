using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Internal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.Business
{
	public static class ReferenceDataProvider
	{
		public static CodeDescriptionPairList GetExportConsumptionTaxExemptionCode(BusinessObjectFactory factory) => JPRefCusCodeListTypes.GetCachedList(factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanExportConsumptionTaxExemptionCode);

		public static ZZRefCusCodeListCombinedCollection GetDutyExemptionRefundCodeList(BusinessObjectFactory factory)
		{
			return GetRefCusCodeLisCollection(factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DutyExemptionRefundCode);
		}

		public static ZZRefCusCodeListCombinedCollection GetDutyExemptionCode(BusinessObjectFactory factory)
		{
			return GetRefCusCodeLisCollection(factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DutyExemptionCode);
		}

		static ZZRefCusCodeListCombinedCollection GetRefCusCodeLisCollection(BusinessObjectFactory factory, string refCusCodeListType)
		{
			var today = ZDateTime.Today;
			var result = factory.GetCachedValue($"JP.ReferenceDataProvider.GetRefCusCodeLisCollection-{today.ToShortDateString()}-{refCusCodeListType}", () =>
			{
				var refCusCodeListCollection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(factory, Core.Constants.CountryCodes.Japan, refCusCodeListType, today);
				return refCusCodeListCollection;
			});

			result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.ListType, "Property", new ZString(refCusCodeListType), false));
			result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.CountryOrGrouping, "Property", new ZString(Core.Constants.CountryCodes.Japan), false));

			return result;
		}

		public static CodeDescriptionPairList GetPreferenceList(BusinessObjectFactory factory)
		{
			return GetReferenceData(factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanCertificateOfOriginType1);
		}

		public static CodeDescriptionPairList GetOriginCertifierList(BusinessObjectFactory factory)
		{
			return GetReferenceData(factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanCertificateOfOriginType2);
		}

		static CodeDescriptionPairList GetReferenceData(BusinessObjectFactory factory, string refCusCodeListType)
		{
			var result = new CodeDescriptionPairList();
			var today = ZDateTime.Today;
			var codes = factory.GetCachedValue($"JP.ReferenceDataProvider.{refCusCodeListType}-{today}", () =>
				ZZRefCusCodeListCombined.Loader.Load(factory, Core.Constants.CountryCodes.Japan, refCusCodeListType, today));
			result.AddRange(codes);
			result.Sort();
			return result;
		}

		public static CodeDescriptionPairList GetCertificateOfOriginCertifierList(BusinessObjectFactory factory, ZString preference, ZString declarationType)
		{
			var result = new CodeDescriptionPairList();
			var today = ZDateTime.Today;

			var attributesForCOOT1 = new RefCusCodeListAttribute.Loader(factory)
				.Load(Core.Constants.CountryCodes.Japan, today, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanCertificateOfOriginType1, preference, RefCusCodeListAttributeTypes.Codes.Preference)
				.Select(x => x.ZZE_Value)
				.ToArray();

			if (attributesForCOOT1.Length == 0)
			{
				return result;
			}

			var attributeFilter = new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.Preference, SQLComparisonOperator.Contains, attributesForCOOT1).Filter;
			var codes = factory.GetCachedValue($"JP.JobComInvoiceLineLookups.CertificateOfOriginCertifierList-{preference}-{today}", () =>
				ZZRefCusCodeListCombined.Loader.Load(factory, Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanCertificateOfOriginType3, today, attributeFilter));
			result.AddRange(codes);

			if (declarationType.ToString().In(JPImportDeclarationTypeList.Codes.J, JPImportDeclarationTypeList.Codes.P, JPImportDeclarationTypeList.Codes.R))
			{
				result.RemoveCode("7");
				result.RemoveCode("M");
			}

			result.Sort();
			return result;
		}

		public static ICodeDescriptionPairList GetConcessionOrderList(BusinessObjectFactory factory, ZGuid tariffKey, ZString primaryPreference)
		{
			var result = new CodeDescriptionPairList();

			var applicabilities = GetCusRefApplicabilityViews(factory, tariffKey, primaryPreference);

			foreach (var applicability in applicabilities)
			{
				result.AddPairIfNotExist(applicability.ZZT_OrderNumber, applicability.ZZT_OrderNumber);
			}

			return result;
		}

		public static ICodeDescriptionPairList GetTradeControlOrderAppendixList(BusinessObjectFactory factory, bool isExport = true)
		{
			return factory.GetCachedValue($"JP.TradeControlOrderAppendixList-{ZDateTime.Today.ToISO8601ShortDateString()}-{isExport}", () =>
			{
				var result = new CodeDescriptionPairList();
				var codeType = isExport ? Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanExportTradeControlOrdinanceAppendix : Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanImportTradeControlOrdinanceAppendix;
				var codes = ZZRefCusCodeListCombined.Loader
					.Load(factory, Core.Constants.CountryCodes.Japan, codeType, ZDateTime.Today)
					.OrderBy(x => x.ZZD_Code);

				result.AddPairsIfNotExist(codes);
				return result;
			});
		}

		static CusRefApplicabilityView[] GetCusRefApplicabilityViews(BusinessObjectFactory factory, ZGuid tariffKey, ZString primaryPreference)
		{
			var today = ZDateTime.Today;

			return factory.GetCachedValue($"JP.PreferenceListProvider.ApplicabilityViews-{today}-{tariffKey}-{primaryPreference}", () =>
			{
				var preferenceQuery = new ZQuery(CusRefPreferenceViewSchema.ZZS_Preference, primaryPreference);
				preferenceQuery.AddToFilter(CusRefPreferenceViewSchema.ZZS_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.Japan);
				preferenceQuery.OrderBy = CusRefPreferenceViewSchema.Constants.ZZS_SystemCreateTimeUtc + " desc";

				var currentPreference = factory.LoadTop1<CusRefPreferenceView>(preferenceQuery);
				if (currentPreference is null)
				{
					return Array.Empty<CusRefApplicabilityView>();
				}

				var rateQuery = new ZDBOnlySubQuery(typeof(RefCusRate), RefCusRateSchema.PK)
					.AddToFilter(RefCusRateSchema.ZZ2_ZZ1_Tariff, tariffKey)
					.AddToFilter(RefCusRateSchema.ZZ2_ZZS_Preference, currentPreference.PK)
					.AddToFilter(RefCusRateSchema.ZZ2_StartDate, SQLComparisonOperator.LessThanOrEqualTo, today)
					.AddToFilter(RefCusRateSchema.ZZ2_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, today);

				var applicabilityQuery = new ZDBOnlyQuery(typeof(CusRefApplicabilityView));
				applicabilityQuery.AddSubQuery(RefCusApplicabilitySchema.ZZT_ZZ2_Rate, (ZDBOnlySubQuery)rateQuery, JoinCondition.And);
				applicabilityQuery.AddToFilter(RefCusApplicabilitySchema.ZZT_StartDate, SQLComparisonOperator.LessThanOrEqualTo, today);
				applicabilityQuery.AddToFilter(RefCusApplicabilitySchema.ZZT_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, today);

				return factory.Load<CusRefApplicabilityView>(applicabilityQuery);
			});
		}
	}
}
