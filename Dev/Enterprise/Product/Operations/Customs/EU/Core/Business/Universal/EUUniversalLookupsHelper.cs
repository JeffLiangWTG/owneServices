using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.EU.Business
{
	public static class EUUniversalLookupsHelper
	{
		public static CodeDescriptionPairList GetCachedListMatchAllAttributes(this BusinessObjectFactory factory, ZString country, ZString codeType, KeyValuePair<ZString, ZString>[] attributeNameValuePairs, IncludeParentDataGroupingOptions includeParentDataGrouping = IncludeParentDataGroupingOptions.Union)
		{
			return RefCusCodeListTypes.GetCachedListMatchAllAttributes(factory, country, codeType, ZDateTime.Today, attributeNameValuePairs, includeParentDataGrouping: includeParentDataGrouping);
		}

		public static CodeDescriptionPairList GetCachedListMatchAllAttributes(this BusinessObjectFactory factory, ZString country, ZString codeType, ZDateTime date, KeyValuePair<ZString, ZString>[] attributeNameValuePairs, IncludeParentDataGroupingOptions includeParentDataGrouping = IncludeParentDataGroupingOptions.Union)
		{
			return RefCusCodeListTypes.GetCachedListMatchAllAttributes(factory, country, codeType, date, attributeNameValuePairs, includeParentDataGrouping: includeParentDataGrouping);
		}

		public static CodeDescriptionPairList GetAdditionalInformationList(this BusinessObjectFactory factory, ZString country, ZString level, ZString direction, bool ignoreLevel = false, IncludeParentDataGroupingOptions includeParentDataGrouping = IncludeParentDataGroupingOptions.Union)
		{
			return GetCachedList(factory, country, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, level, direction, ignoreLevel, includeParentDataGrouping);
		}

		public static ZZRefCusCodeListCombinedCollection GetUCCAdditionalInformationList(this BusinessObjectFactory factory, ZQuery additionalFilter, ZString country, ZString[] codeTypes, ZString level, bool ignoreLevel = false, IncludeParentDataGroupingOptions includeParentDataGrouping = IncludeParentDataGroupingOptions.ChildFirstThenParent)
		{
			var date = ZDateTime.Today;
			var attributeFilters = new List<RefCusCodeListAttributeFilter>();
			if (!ignoreLevel && level != UniversalReferenceConstants.RefCusCodeListLevelType.Both)
			{
				var levelValue = level == UniversalReferenceConstants.RefCusCodeListLevelType.Item
					? UniversalReferenceConstants.RefCusCodeListAttributes.Values.Item
					: UniversalReferenceConstants.RefCusCodeListAttributes.Values.Header;
				attributeFilters.Add(new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.Level, JoinCondition.And, new ZString[] { levelValue }));
			}

			ZZRefCusCodeListCombinedCollection cachedCollection = null;
			switch (includeParentDataGrouping)
			{
				case IncludeParentDataGroupingOptions.ChildOnly:
					cachedCollection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(factory, additionalFilter, country, codeTypes, date, attributeFilters, false);
					break;
				case IncludeParentDataGroupingOptions.ChildFirstThenParent:
					cachedCollection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(factory, additionalFilter, country, codeTypes, date, attributeFilters, false);
					cachedCollection.Load();
					if (!cachedCollection.Any())
					{
						cachedCollection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(factory, additionalFilter, country, codeTypes, date, attributeFilters, true);
					}
					break;
				case IncludeParentDataGroupingOptions.Union:
					cachedCollection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(factory, additionalFilter, country, codeTypes, date, attributeFilters, true);
					break;
				default:
					cachedCollection = new ZZRefCusCodeListCombinedCollection(factory);
					break;
			}
			if (!cachedCollection.IsLoaded)
			{
				cachedCollection.Load();
			}

			return cachedCollection;
		}

		public static CodeDescriptionPairList GetTranNatureList(this BusinessObjectFactory factory, ZString country)
		{
			var result = GetCachedList(factory, country, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, UniversalReferenceConstants.RefCusCodeListLevelType.Both, UniversalReferenceConstants.RefCusCodeListDirectionType.Both, includeParentDataGrouping: IncludeParentDataGroupingOptions.ChildFirstThenParent);
			return result;
		}

		public static CodeDescriptionPairList GetValuationMethodList(this BusinessObjectFactory factory, ZString country)
		{
			return GetCachedList(factory, country, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ValuationMethod, UniversalReferenceConstants.RefCusCodeListLevelType.Both, UniversalReferenceConstants.RefCusCodeListDirectionType.Both, includeParentDataGrouping: IncludeParentDataGroupingOptions.ChildFirstThenParent);
		}

		public static ZZRefCusCodeListCombinedCollection GetSupportingDocumentList(this BusinessObjectFactory factory, ZString dataGroupingCode, ZString direction, ZString level, IEnumerable<RefCusCodeListAttributeFilter> additionalAttributeFilters = null, IncludeParentDataGroupingOptions includeParentDataGroupings = IncludeParentDataGroupingOptions.Union)
		{
			var attrFilterKeyPart = additionalAttributeFilters != null ? string.Join(";", additionalAttributeFilters.Select(x => x.Key)) : string.Empty;

			return factory.GetCachedValue(string.Join("_", "SupportingDocumentList", dataGroupingCode, direction, level, attrFilterKeyPart, includeParentDataGroupings), () =>
			{
				var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
				var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;

				ZString[] codeTypes = null;
				if (direction == UniversalReferenceConstants.RefCusCodeListDirectionType.Both)
				{
					codeTypes = new ZString[] { exportCodeType, importCodeType };
				}
				else if (direction == UniversalReferenceConstants.RefCusCodeListDirectionType.Export)
				{
					codeTypes = new ZString[] { exportCodeType };
				}
				else if (direction == UniversalReferenceConstants.RefCusCodeListDirectionType.Import)
				{
					codeTypes = new ZString[] { importCodeType };
				}

				var attributeFilters = new List<RefCusCodeListAttributeFilter>();

				if (level == UniversalReferenceConstants.RefCusCodeListLevelType.Header)
				{
					attributeFilters.Add(new RefCusCodeListAttributeFilter(
						RefCusCodeListAttributeTypes.Codes.Level,
						JoinCondition.And,
						true,
						new ZString[] { UniversalReferenceConstants.RefCusCodeListAttributes.Values.Item },
						UniversalReferenceConstants.RefCusCodeListAttributes.Values.Header));
				}
				else if (level == UniversalReferenceConstants.RefCusCodeListLevelType.Item)
				{
					attributeFilters.Add(new RefCusCodeListAttributeFilter(
						RefCusCodeListAttributeTypes.Codes.Level,
						JoinCondition.And,
						true,
						values: UniversalReferenceConstants.RefCusCodeListAttributes.Values.Item));
				}

				if (additionalAttributeFilters != null)
				{
					attributeFilters.AddRange(additionalAttributeFilters);
				}

				return ZZRefCusCodeListCombinedCollection.GetCachedCollection(factory, dataGroupingCode, codeTypes, ZDateTime.Today, attributeFilters, includeParentDataGroupings);
			});
		}

		public static ZZRefCusCodeListCombinedCollection GetSupportingDocumentList(this BusinessObjectFactory factory, ZString dataGroupingCode)
			=> GetSupportingDocumentList(factory, new[] { dataGroupingCode });

		public static ZZRefCusCodeListCombinedCollection GetSupportingDocumentList(this BusinessObjectFactory factory, ZString[] dataGroupingCodes)
		{
			return ZZRefCusCodeListCombinedCollection.GetCachedCollection(
				factory,
				dataGroupingCodes,
				new ZString[] { Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection },
				ZDateTime.Today,
				Array.Empty<RefCusCodeListAttributeFilter>()
				);
		}

		public static ZZRefCusCodeListCombinedCollection GetSupportingDocumentListWithPermitAttribute(this BusinessObjectFactory factory, ZString dataGroupingCode, string direction = UniversalReferenceConstants.RefCusCodeListDirectionType.Both)
		{
			ZString[] codeTypes = null;
			if (direction == UniversalReferenceConstants.RefCusCodeListDirectionType.Both)
			{
				codeTypes = new ZString[] { Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection };
			}
			else if (direction == UniversalReferenceConstants.RefCusCodeListDirectionType.Export)
			{
				codeTypes = new ZString[] { Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection };
			}
			else if (direction == UniversalReferenceConstants.RefCusCodeListDirectionType.Import)
			{
				codeTypes = new ZString[] { Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection };
			}

			return ZZRefCusCodeListCombinedCollection.GetCachedCollection(
				factory,
				dataGroupingCode,
				codeTypes,
				ZDateTime.Today,
				new RefCusCodeListAttributeFilter[] { new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.Permit, JoinCondition.And) }
				);
		}

		public static ZZRefCusCodeListCombined GetSupportingDocumentCode(this BusinessObjectFactory factory, ZString dataGroupingCode, ZString direction, ZString code)
		{
			if (direction == UniversalReferenceConstants.RefCusCodeListDirectionType.Both)
			{
				return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, code, dataGroupingCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, ZDateTime.Today)
					?? ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, code, dataGroupingCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, ZDateTime.Today);
			}
			else if (direction == UniversalReferenceConstants.RefCusCodeListDirectionType.Export)
			{
				return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, code, dataGroupingCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, ZDateTime.Today);
			}
			return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, code, dataGroupingCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, ZDateTime.Today);
		}

		public static ZZRefCusCodeListCombined GetAdditionalInformationCode(this BusinessObjectFactory factory, ZString country, ZString code, bool includeParentDataGrouping = true)
		{
			return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, code, country, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, ZDateTime.Today, includeParentDataGrouping: includeParentDataGrouping);
		}

		public static ZZRefCusCodeListCombined GetAdditionalInformationCode(this BusinessObjectFactory factory, ZString country, ZString code, ZString subType, ZString direction, bool includeParentDataGrouping = true)
		{
			if (direction == UniversalReferenceConstants.RefCusCodeListDirectionType.Import)
			{
				var codeType = AdditionalInfoLookupsHelper.GetImportRefCusCodeListType(subType);
				return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, code, country, codeType, ZDateTime.Today, includeParentDataGrouping: includeParentDataGrouping);
			}
			else if (direction == UniversalReferenceConstants.RefCusCodeListDirectionType.Export)
			{
				var codeType = AdditionalInfoLookupsHelper.GetUCCExportRefCusCodeListType(subType);
				return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, code, country, codeType, ZDateTime.Today, includeParentDataGrouping: includeParentDataGrouping);
			}
			return null;
		}

		public static CodeDescriptionPairList GetAgreedPlaceCodeList(this BusinessObjectFactory factory, ZString countryCode)
		{
			return factory.GetCachedValue("AgreedPlaceCodeList_" + countryCode, () =>
			{
				var result = new CodeDescriptionPairList();
				var incoTermKeys = ZZRefCusCodeListCombined.Loader.Load(factory, countryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.IncoTermKey, ZDateTime.Now, includeParentDataGrouping: false);
				if (incoTermKeys.Length == 0)
				{
					incoTermKeys = ZZRefCusCodeListCombined.Loader.Load(
						factory,
						Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
						Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.IncoTermKey,
						ZDateTime.Now, includeParentDataGrouping: false);
				}
				result.AddRange(incoTermKeys);
				result.Sort();
				return result;
			});
		}

		public static CodeDescriptionPairList GetCachedList(BusinessObjectFactory factory, ZString country, ZString codeType, ZString level, ZString direction, bool ignoreLevel = false, IncludeParentDataGroupingOptions includeParentDataGrouping = IncludeParentDataGroupingOptions.Union)
		{
			var attributeNameValuePairs = new Dictionary<ZString, ZString>();

			if (!ignoreLevel && level != UniversalReferenceConstants.RefCusCodeListLevelType.Both)
			{
				var levelValue = level == UniversalReferenceConstants.RefCusCodeListLevelType.Item
					? UniversalReferenceConstants.RefCusCodeListAttributes.Values.Item
					: UniversalReferenceConstants.RefCusCodeListAttributes.Values.Header;

				attributeNameValuePairs.Add(RefCusCodeListAttributeTypes.Codes.Level, levelValue);
			}

			if (direction != UniversalReferenceConstants.RefCusCodeListDirectionType.Both)
			{
				var directionValue = direction == UniversalReferenceConstants.RefCusCodeListDirectionType.Import
					? UniversalReferenceConstants.RefCusCodeListAttributes.Values.Import
					: UniversalReferenceConstants.RefCusCodeListAttributes.Values.Export;

				attributeNameValuePairs.Add(RefCusCodeListAttributeTypes.Codes.Direction, directionValue);
			}

			return RefCusCodeListTypes.GetCachedListMatchAllAttributes(factory, country, codeType, ZDateTime.Today, attributeNameValuePairs.ToArray(), includeParentDataGrouping: includeParentDataGrouping);
		}

		public static IEnumerable<CusRefRateCodeView> GetCachedRatesByType(this BusinessObjectFactory factory, ZString dataGrouping, params ZString[] rateTypes)
		{
			var cacheKey = FormattableString.Invariant($"EU.RefCusRateCodeList.{dataGrouping}.{string.Join("_", rateTypes)}");

			return factory.GetCachedValue(cacheKey, delegate
			{
				var result = CusRefRateCodeView.Loader.Load(factory, dataGrouping, new RateCodeLoadCriteria() { RateTypesToInclude = rateTypes });
				return result;
			});
		}

		public static CodeDescriptionPairList GetWorstCaseAdditionalCodes(this BusinessObjectFactory factory, ZString dataGrouping, ZDateTime effectiveDate)
		{
			var attrs = new Dictionary<ZString, ZString> { { Core.Constants.Customs.Universal.RefCusCodeList.Attributes.DefaultRate, ZString.Empty } };
			return RefCusCodeListTypes.GetCachedListMatchAllAttributes(factory, dataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, effectiveDate, attrs.ToArray(), includeParentDataGrouping: IncludeParentDataGroupingOptions.ChildFirstThenParent);
		}

		public static ZZRefCusCodeListCombinedCollection GetEXNATCountryList(this BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("EU.Business.EUUniversalLookupsHelper.GetEXNATCountryList",
				() =>
				{
					var dataGroupingCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
					var codeType = EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EXNAT;
					var collection = new ZZRefCusCodeListCombinedCollection(factory, dataGroupingCode, codeType, ZDateTime.Today);
					collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.CountryOrGrouping, "Property", (ZString)dataGroupingCode, false));
					collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.ListType, "Property", (ZString)codeType, false));

					return collection;
				});
		}

		public static ZString GetUCCAuthorizationCodeCustomsValue(BusinessObjectFactory factory, ZString cw1Value)
		{
			return factory.GetCachedValue("EU.Business.EUUniversalLookupsHelper.GetUCCAuthorizationCodeList|" + cw1Value,
				() =>
				{
					var dataGroupingCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
					var mapType = UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU;

					var query = new ZQuery(RefCusMapSchema.ZZM_ZZP_NKMapType, mapType);
					query.AddToFilter(RefCusMapSchema.ZZM_ZZZ_NKDataGrouping, dataGroupingCode);
					query.AddToFilter(RefCusMapSchema.ZZM_CW1orCommercialValue, cw1Value);
					return factory.LoadTop1<RefCusMap>(query)?.ZZM_CustomsValue ?? ZString.Empty;
				});
		}

		public static RefCusMap[] GetEuropeanUnionAuthorizationCustomsCodeMaps(BusinessObjectFactory factory, ZString currentCountryCode)
		{
			var date = ZDateTime.Today;
			return factory.GetCachedValue($"RefCusMap_EUN_{currentCountryCode}_EUNAU_{date}", () =>
			{
				var query = new ZQuery(RefCusMapSchema.ZZM_ZZP_NKMapType, UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU);
				if (currentCountryCode.IsEmpty)
				{
					query.AddToFilter(RefCusMapSchema.ZZM_ZZZ_NKDataGrouping, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
				}
				else
				{
					query.AddToFilter(RefCusMapSchema.ZZM_ZZZ_NKDataGrouping, new string[2] { Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, currentCountryCode });
				}
				query.AddToFilter(RefCusMapSchema.ZZM_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, date);
				query.AddToFilter(RefCusMapSchema.ZZM_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, date);
				return factory.Load<RefCusMap>(query);
			});
		}

		public static CodeDescriptionPairList GetCountryAddressPostcodeOnlyList(this BusinessObjectFactory factory, string dataGrouping = null, ZDateTime? date = default)
		{
			return RefCusCodeListTypes.GetCachedList(factory,
				country: dataGrouping ?? Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				codeType: UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL148,
				date: date ?? ZDateTime.Today);
		}

		public static CodeDescriptionPairList GetCL030XMLErrorCodeListOnlyList(this BusinessObjectFactory factory, string dataGrouping = null, ZDateTime? date = default)
		{
			return RefCusCodeListTypes.GetCachedList(factory,
				country: dataGrouping ?? Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				codeType: UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL030,
				date: date ?? ZDateTime.Today);
		}

		public static CodeDescriptionPairList GetCL180FunctionalErrorCodeListOnlyList(this BusinessObjectFactory factory, string dataGrouping = null, ZDateTime? date = default)
		{
			return RefCusCodeListTypes.GetCachedList(factory,
				country: dataGrouping ?? Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				codeType: UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL180,
				date: date ?? ZDateTime.Today);
		}

		public static CodeDescriptionPairList Get104IMCodeListOnlyList(this BusinessObjectFactory factory, string dataGrouping = null, ZDateTime? date = default)
		{
			return RefCusCodeListTypes.GetCachedList(factory,
				country: dataGrouping ?? Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				codeType: Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CCIMethodOfPayment,
				date: date ?? ZDateTime.Today);
		}

		public static CodeDescriptionPairList GetCachedCountryNC008List(this BusinessObjectFactory factory, ZString dataGroupingCode)
		{
			var date = ZDateTime.Today;
			var key = string.Join("|", "EU.Business.EUUniversalLookupsHelper.GetCachedCountryNC008List", dataGroupingCode, date.ToShortDateString());
			return factory.GetCachedValue(key, () =>
			{
				var countryList = new CodeDescriptionPairList();

				var codeList = RefCusCodeListTypes.GetCachedListMatchAllAttributes(factory,
					dataGroupingCode,
					UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008,
					date,
					attributeNameValuePairs: null,
					includeParentDataGrouping: IncludeParentDataGroupingOptions.ChildFirstThenParent);

				if (codeList.Count > 0)
				{
					countryList = codeList;
				}
				else
				{
					countryList.AddRange(new RefCountryCollection(factory, new ZQuery(RefCountrySchema.RN_IsActive, true)));
					if (dataGroupingCode == Core.Constants.CountryCodes.UnitedKingdom)
					{
						countryList.AddPairIfNotExist(Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes, Res.GetString("EB1622C4-6610-4E08-8F4C-E51D5EB9689C", "Northern Ireland"));
					}
				}
				countryList.SortByDescription();
				return countryList;
			});
		}
	}
}
