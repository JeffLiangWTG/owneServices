using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using RefCusCodeListTypesCodes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes;

namespace Enterprise.Customs.CN.Business
{
	public static class CNRefTariffDataLoader
	{
		public static TariffView GetCIQTariff(BusinessObjectFactory factory, ZString ciqTariffCode, ZDateTime valuationDate, string parentTariffCode = null)
		{
			return ciqTariffCode.IsEmpty ? null : new TariffView.Loader(factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.China,
				Constants.UniversalReferenceConstants.CusTariffTypes.ChinaCIQTariff, ciqTariffCode, valuationDate, parentTariffCode);
		}

		public static TariffAttributeView[] GetCustomsTariffAdditionalInfoAttributes(BusinessObjectFactory factory, ZString tariffCode, ZDateTime valuationDate)
		{
			return factory.GetCachedValue("CNTariffAdditionalInfoAttributes" + tariffCode + valuationDate.ToString(), () =>
			{
				ZQuery filter = null;
				var tariff = new TariffView.Loader(factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.China, Universal.Constants.TariffTypes.HarmonizedSystem, tariffCode, valuationDate);
				if (tariff != null)
				{
					filter = new ZDBOnlyQuery(typeof(TariffAttributeView));
					filter.AddToFilter(TariffAttributeViewSchema.ZZ3_ParentTableType, tariff.ZZ1_TableType);
					filter.AddToFilter(TariffAttributeViewSchema.ZZ3_ZZ1_ParentTariffOrNationalCode, tariff.PK);
					filter.AddToFilter(TariffAttributeViewSchema.ZZ3_Name, SQLComparisonOperator.StartsWith, Constants.UniversalReferenceConstants.CusTariffAttributeName.AdditionalInfomation);
					filter.OrderBy = TariffAttributeView.Schema.ZZ3_Name;
				}
				else
				{
					filter = ZQuery.NoResultQuery;
				}
				return factory.Load<TariffAttributeView>(filter);
			});
		}

		public static ZString GetAdditionalElementDescription(this TariffView tariff, ZString additionalElementCode)
		{
			var result = ZString.Empty;

			if (tariff != null)
			{
				var cachedAdditionalElements = tariff.Factory.GetCachedValue("CN.AdditionalElements", () => new StringDictionary());

				if (!cachedAdditionalElements.ContainsKey(additionalElementCode))
				{
					foreach (DictionaryEntry pair in GetTariffRequiredAdditionalElements(tariff.Factory, tariff))
					{
						var code = pair.Key.ToString();
						if (!cachedAdditionalElements.ContainsKey(code))
						{
							cachedAdditionalElements.Add(code, pair.Value.ToString());
						}
					}
				}

				result = cachedAdditionalElements.ContainsKey(additionalElementCode) ? cachedAdditionalElements[additionalElementCode] : string.Empty;
			}

			return result;
		}

		public static void CacheTariffRequiredAdditionalElements(BusinessObjectFactory factory, IEnumerable<TariffView> tariffs)
		{
			var cachedTariffRequiredAdditionalElements = GetCachedTariffRequiredAdditionalElements(factory);
			var cachedTariffPKs = cachedTariffRequiredAdditionalElements.Select(x => x.Key);

			foreach (var pair in GetAdditionalElementsFromDatabase(factory, tariffs.Where(x => !cachedTariffPKs.Contains(x.PK))))
			{
				cachedTariffRequiredAdditionalElements.Add(pair.Key, pair.Value);
			}
		}

		static Dictionary<ZGuid, StringDictionary> GetCachedTariffRequiredAdditionalElements(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("CN.TariffRequiredAdditionalElements", () =>
			{
				return new Dictionary<ZGuid, StringDictionary>();
			});
		}

		static StringDictionary GetTariffRequiredAdditionalElements(BusinessObjectFactory factory, TariffView tariff)
		{
			var cachedTariffRequiredAdditionalElements = GetCachedTariffRequiredAdditionalElements(factory);

			if (!cachedTariffRequiredAdditionalElements.ContainsKey(tariff.PK))
			{
				foreach (var pair in GetAdditionalElementsFromDatabase(factory, new TariffView[] { tariff }))
				{
					cachedTariffRequiredAdditionalElements.Add(pair.Key, pair.Value);
				}
			}

			if (cachedTariffRequiredAdditionalElements.ContainsKey(tariff.PK))
			{
				return cachedTariffRequiredAdditionalElements[tariff.PK];
			}
			else
			{
				ErrorReporter.ReportOnce("CNTariffRequiredAdditionalElements_NotFound", string.Format("Cannot find Required Additional Elements For Tariff '{0}'", tariff.ZZ1_TariffCode));
				return new StringDictionary();
			}
		}

		static Dictionary<ZGuid, StringDictionary> GetAdditionalElementsFromDatabase(BusinessObjectFactory factory, IEnumerable<TariffView> tariffs)
		{
			var dictionary = new Dictionary<ZGuid, StringDictionary>();

			if (tariffs.Any())
			{
				var query = new ZDBOnlyQuery(typeof(TariffAttributeView));
				query.AddToFilter(TariffAttributeViewSchema.ZZ3_ZZ1_ParentTariffOrNationalCode, tariffs.Select(x => x.PK).Distinct());
				query.AddToFilter(TariffAttributeViewSchema.ZZ3_Name, SQLComparisonOperator.StartsWith, Constants.UniversalReferenceConstants.CusTariffAttributeName.AdditionalInfomation);

				var dynamicBOs = new DynamicBusinessObjectCollection(factory);

				var queryText = FormattableString.Invariant($@"SELECT ZZ3_ZZ1_ParentTariffOrNationalCode, ZZD_Code, ZZD_Description 
				FROM {TariffAttributeViewSchema.Constants.TableName} JOIN {ZZRefCusCodeListCombinedSchema.Constants.SqlSchemaName}.{ZZRefCusCodeListCombinedSchema.Constants.TableName} ON ZZ3_Value = ZZD_Code
				{query.GetAsWhereAndOrderByClause(false)}
				AND ZZD_CodeType = '{RefCusCodeListTypesCodes.CNAdditionalElements}' AND ZZD_CountryOrGrouping='{Core.Constants.CountryCodes.China}'");

				dynamicBOs.Load(queryText, query.Params);

				if (dynamicBOs.Any())
				{
					foreach (var g in dynamicBOs.GroupBy(x => new ZGuid(x[TariffAttributeViewSchema.ZZ3_ZZ1_ParentTariffOrNationalCode])))
					{
						var list = new StringDictionary();
						foreach (var dynamicBO in g)
						{
							var code = dynamicBO[ZZRefCusCodeListCombinedSchema.ZZD_Code].ToString();
							if (!list.ContainsKey(code))
							{
								list.Add(code, dynamicBO[ZZRefCusCodeListCombinedSchema.ZZD_Description].ToString());
							}
						}
						dictionary.Add(g.Key, list);
					}
				}
			}

			return dictionary;
		}
	}
}
