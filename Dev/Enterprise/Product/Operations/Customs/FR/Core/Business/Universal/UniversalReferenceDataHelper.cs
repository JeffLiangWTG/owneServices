using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Integration.Customs.FR;

namespace Enterprise.Customs.FR.Business
{
	public static class UniversalReferenceDataHelper
	{
		public static bool CheckFeeValueInThreshold(ZDecimal valueToCompare, CountComparisonConstants comparisonConstant, string ruleCode, BusinessObjectFactory factory, ZString dataGrouping, ZDateTime effectiveValuationDate, out decimal threshold, FeeValueToLookAt feeValueToLookAt)
		{
			var result = false;
			threshold = 0m;
			var fee = new RefCusTaxOrFee.Loader(factory).LoadMostRecentEffectiveTaxOrFeeFromCodeDate(dataGrouping, ruleCode, effectiveValuationDate);
			if (fee != null)
			{
				switch (feeValueToLookAt)
				{
					case FeeValueToLookAt.Minimum:
						threshold = fee.ZZF_Minimum;
						break;
					case FeeValueToLookAt.Maximum:
						threshold = fee.ZZF_Maximum;
						break;
					default:
						threshold = fee.ZZF_Threshold;
						break;
				}

				if (threshold != 0)
				{
					switch (comparisonConstant)
					{
						case CountComparisonConstants.GreaterThan:
							result = valueToCompare > threshold;
							break;
						case CountComparisonConstants.GreaterThanOrEqualTo:
							result = valueToCompare >= threshold;
							break;
						case CountComparisonConstants.LesserThan:
							result = valueToCompare < threshold;
							break;
						case CountComparisonConstants.LessThanOrEqualTo:
							result = valueToCompare <= threshold;
							break;
						default:
							break;
					}
				}
			}
			return result;
		}

		public enum CountComparisonConstants
		{
			LessThanOrEqualTo = 0,
			GreaterThanOrEqualTo = 1,
			GreaterThan = 2,
			LesserThan = 3
		}

		public enum FeeValueToLookAt
		{
			Minimum = 0,
			Maximum = 1,
			Threshold = 2
		}

		public static ZZRefCusCodeListCombinedCollection GetCustomsApprovedCountryList(BusinessObjectFactory factory, ZString dataGrouping)
		{
			var codeList = new ZString[] {
				EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17,
				EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU17,
				EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO17,
			};
			return ZZRefCusCodeListCombinedCollection.GetCachedCollection(factory, dataGrouping, codeList, ZDateTime.Today, null, false);
		}

		public static IEnumerable<VATApplicabilityView> GetVATApplicabilitiesWithSecondaryTradeGroup(BusinessObjectFactory factory, TariffView tariff, ZString dataGrouping, ZString[] tradeGroupsCodes, ZDateTime effectiveValuationDate)
		{
			var vatApplicabilities = tariff?.GetEffectiveVATApplicabilities(effectiveValuationDate)
					.Where(v => v.ZX5_ZZZ_NKDataGrouping == dataGrouping)
					.OrderBy(x => x.ZX5_ZZF_NKTaxOrFeeCode) ?? Enumerable.Empty<VATApplicabilityView>();

			var tradeGroupLoader = new CusRefTradeGroupView.Loader(factory);
			var tradeGroups = tradeGroupLoader.Load(Enterprise.Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(dataGrouping), tradeGroupsCodes, effectiveValuationDate);
			if (tradeGroups?.Any() ?? false)
			{
				var vatApplicabilitiesWithTradeGroup = vatApplicabilities.Where(x => tradeGroups.Select(y => y.PK).Contains(x.ZX5_ZZA_TradeGroup)).OrderBy(x => x.ZX5_ZZF_NKTaxOrFeeCode);
				vatApplicabilities = vatApplicabilitiesWithTradeGroup.Any() ? vatApplicabilitiesWithTradeGroup : vatApplicabilities;
			}
			return vatApplicabilities;
		}

		public static string GetChargePaymentOrDestinationID(BusinessObjectFactory factory, IHarbourJob bizObj)
		{
			var result = ZString.Empty;

			if (bizObj != null)
			{
				var effectiveValuationDate = bizObj.ValuationDate;
				foreach (var chargePaymentOrDestinationID in PaymentDestinationList(factory, bizObj).GetAllCodes())
				{
					var harbourRates = new RefHarbourRate.Loader(factory).Load(bizObj.HarbourType, bizObj.DataGrouping, bizObj.ContainerMode, effectiveValuationDate, chargePaymentOrDestinationID, bizObj.IsContainerised);
					if (harbourRates.Length > 0)
					{
						result = chargePaymentOrDestinationID;
						break;
					}
				}
			}

			return result;
		}

		public static ZString GetLocalHarbourFeeCode(BusinessObjectFactory factory, IHarbourJob bizObj, RefHarbourRate fee, ZDateTime dateOfValuation)
		{
			var refCusMap = ZZRefCusMapCombined.MapCW1CodeToCustomsCode(factory, fee.ZXF_ZZZ_NKDataGrouping, RefCusMapTypeList.Codes.HAFEE, fee.ZXF_Port, dateOfValuation);

			if (refCusMap.IsEmpty)
			{
				refCusMap = bizObj.IsDCN ? HarbourFeeCodes.Codes.V905 : HarbourFeeCodes.Codes.P635;
			}

			return refCusMap;
		}

		public static CodeDescriptionPairList PaymentDestinationList(BusinessObjectFactory factory, IHarbourJob bizObj)
		{
			var unloco = bizObj?.BarrierPort ?? ZString.Empty;
			var customsOffice = bizObj?.CustomsOffice ?? ZString.Empty;

			return factory.GetCachedValue(string.Concat("FR.PaymentDestinationList", customsOffice, unloco), () =>
			{
				var list = new CodeDescriptionPairList();
				var portTaxCollection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(factory,
					Core.Constants.CountryCodes.France,
					new ZString[] { UniversalReferenceConstants.RefCusCodeListTypes.Codes.PortTaxCombination },
					ZDateTime.Today,
					new RefCusCodeListAttributeFilter[]
					{
						new RefCusCodeListAttributeFilter(UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.CustomsOffice, JoinCondition.And, new[] { customsOffice }),
						new RefCusCodeListAttributeFilter(UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Port, JoinCondition.And, new[] { unloco })
					});

				portTaxCollection.Load();
				foreach (ZZRefCusCodeListCombined portTax in portTaxCollection)
				{
					list.AddPairIfNotExist(portTax.GetAttribute(UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.THI), portTax.GetAttribute(UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation));
				}
				list.Sort();

				return list;
			});
		}

		public static CodeDescriptionPairList GetCodeList(this BusinessObjectFactory factory, ZString dataGrouping, ZString codeType, ZString direction)
		{
			var		attributeNameValuePairs = new Dictionary<ZString, ZString>();
			if (direction == SharedJobMessageTypeList.Codes.Export)
			{
				attributeNameValuePairs.Add(IsExportAttribute, Core.Constants.BooleanTrueString);
			}
			else if (direction == SharedJobMessageTypeList.Codes.Import)
			{
				attributeNameValuePairs.Add(IsImportAttribute, Core.Constants.BooleanTrueString);
			}
			return RefCusCodeListTypes.GetCachedListMatchAllAttributes(factory, dataGrouping, codeType, ZDateTime.Today, attributeNameValuePairs.ToArray());
		}

		public static ZDecimal GetDv1Threshold(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("UniversalReferenceDataHelper|FR|DV1", () =>
			{
				var loader = new RefCusTaxOrFee.Loader(factory);
				return loader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.France, "DV1", ZDateTime.Today, ZDateTime.Today)?.ZZF_Threshold ?? ZDecimal.Zero;
			});
		}

		public static RefCusRateType[] GetNationalRateTypes(BusinessObjectFactory factory, ZString region)
		{
			var result = RefCusRateType.Loader.GetRateTypesByDataGrouping(factory, Core.Constants.CountryCodes.France);
			if (!IsRegionApplyingGrantingOfSea(region))
			{
				result = result.Where(x => !IsGrantingOfSea(x.ZZR_RateType)).ToArray();
			}
			return result;
		}

		public static CodeDescriptionPairList GetCachedPackageTypeList(BusinessObjectFactory factory)
			=> Universal.RefCusCodeListTypes.GetCachedList(factory,
														Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
														Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
														ZDateTime.Today);

		const string IsExportAttribute = "IsExport";
		const string IsImportAttribute = "IsImport";

		public static bool IsGrantingOfSea(string rateType) => rateType == FRConstants.GrantingOfSeaRateTypes.ExternalGrantingOfSea || rateType == FRConstants.GrantingOfSeaRateTypes.RegionalGrantingOfSea;

		public static bool IsRegionApplyingGrantingOfSea(string region) => region != FRDomesticOverseasTerritories.Codes.CONTI && region != FRDomesticOverseasTerritories.Codes.CORSE;
	}
}
