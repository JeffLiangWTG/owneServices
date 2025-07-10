using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using static Enterprise.Customs.CA.Business.UniversalReferenceConstants;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class LookupsHelper : Integration.Customs.CA.ILookupsHelper
	{
		public static CodeDescriptionPairList StatesOfOriginBase(BusinessObjectFactory factory, bool isExport, ZString origin)
		{
			CodeDescriptionPairList result;
			if (isExport)
			{
				result = factory.GetCachedValue<CanadianProvinceList>();
			}
			else
			{
				switch (origin)
				{
					case Core.Constants.CountryCodes.Canada:
						result = factory.GetCachedValue<CanadianProvinceList>();
						break;
					case Core.Constants.CountryCodes.UnitedStates:
						result = factory.GetCachedValue<USStatesList>();
						break;
					default:
						result = factory.GetCachedValue("EmptyCodeDescriptionPairList", () => new CodeDescriptionPairList());
						break;
				}
			}
			return result;
		}

		static IEnumerable<ZString> GetTradeGroups(BusinessObjectFactory factory, ZString countryOfOrigin, ZDateTime dateOfValuation, CodeDescriptionPairList ttList)
		{
			var loader = new CusRefTradeGroupView.Loader(factory);
			var tradeGroups = loader.Load(Core.Constants.CountryCodes.Canada, ttList.GetAllCodesZString(), dateOfValuation);
			IEnumerable<CusRefTradeGroupView> filteredTradeGroups = tradeGroups;
			if (!countryOfOrigin.IsEmpty)
			{
				filteredTradeGroups = tradeGroups.Where(view => view.TradeGroupCountries.Any(y => y.ZZB_RN_NKTradeGroupCountryCode == countryOfOrigin && y.ZZB_StartDate <= dateOfValuation && y.ZZB_EndDate >= dateOfValuation));
			}
			var result = filteredTradeGroups.Select(view => view.ZZA_TradeGroup);
			return result;
		}

		public static CodeDescriptionPairList TreatmentCodesByOriginAndExport(BusinessObjectFactory factory, ZString origin, ZString export, ZString tradeZone, ZDateTime? dateOfValuation = null)
		{
			dateOfValuation ??= ZDateTime.Today;
			return factory.GetCachedValue(string.Format(CultureInfo.InvariantCulture, "CA|LookupsHelper|TreatmentCodesByOriginAndExport|{0}|{1} {2} {3:dd-MMM-yy}", origin, export, tradeZone, dateOfValuation), () =>
			{
				var result = new CodeDescriptionPairList();
				var originCodes = new Dictionary<ZString, string>();
				var list = TreatmentCodes(factory);

				IEnumerable<ZString> originValidCodes;
				if (!origin.IsEmpty)
				{
					if (origin == Core.Constants.CountryCodes.Canada)
					{
						return list;
					}
					originValidCodes = GetTradeGroups(factory, origin, dateOfValuation.Value, list);
					if (originValidCodes.Any())
					{
						foreach (var validCode in originValidCodes)
						{
							if (list[validCode] is ICodeDescription pair && !originCodes.ContainsKey(validCode))
							{
								originCodes.Add(pair.Code, pair.Description);
							}
						}
					}
				}
				else if (export.IsEmpty)
				{
					return list;
				}

				originValidCodes = export.IsEmpty ? null : GetTradeGroups(factory, export, dateOfValuation.Value, list);
				if (!export.IsEmpty && export != origin && tradeZone.IsEmpty && originValidCodes.Any())
				{
					if (origin.IsEmpty)
					{
						foreach (var validCode in originValidCodes)
						{
							if (list[validCode] is ICodeDescription pair)
							{
								result.AddPairIfNotExist(pair.Code, pair.Description);
							}
						}
					}
					else
					{
						foreach (var originCode in originCodes)
						{
							if (originValidCodes.Contains(originCode.Key) ||
								(EquivalentTreatmentCodes.TryGetValue(originCode.Key, out var equivalents) && equivalents.Any(equivalent => originValidCodes.Contains(equivalent))))
							{
								result.AddPairIfNotExist(originCode.Key, originCode.Value);
							}
						}

						if (result.Count == 0)
						{
							originCodes.ForEach(x => result.AddPair(x.Key, x.Value));
							foreach (var validCode in originValidCodes)
							{
								if (list[validCode] is ICodeDescription pair)
								{
									result.AddPairIfNotExist(pair.Code, pair.Description);
								}
							}
						}
					}
				}
				else
				{
					originCodes.ForEach(x => result.AddPairIfNotExist(x.Key, x.Value));
				}

				return result;
			});
		}

		static Dictionary<ZString, ZString[]> EquivalentTreatmentCodes
		{
			get
			{
				if (equivalentTreatmentCodes == null)
				{
					equivalentTreatmentCodes = new Dictionary<ZString, ZString[]>();
					equivalentTreatmentCodes.Add(TariffTreatmentCodes.Codes.UnitedStates, new ZString[] { TariffTreatmentCodes.Codes.Mexico });
					equivalentTreatmentCodes.Add(TariffTreatmentCodes.Codes.Mexico, new ZString[] { TariffTreatmentCodes.Codes.UnitedStates });
					equivalentTreatmentCodes.Add(TariffTreatmentCodes.Codes.Australia, new ZString[] { TariffTreatmentCodes.Codes.NewZealand, TariffTreatmentCodes.Codes.CommonwealthCaribbeanCountries, TariffTreatmentCodes.Codes.LeastDevelopedCountry });
				}
				return equivalentTreatmentCodes;
			}
		}
		[ThreadStatic]
		static Dictionary<ZString, ZString[]> equivalentTreatmentCodes;

		public static CodeDescriptionPairList TreatmentCodes(BusinessObjectFactory factory)
		{
			return UniversalReferenceDataHelper.GetPreferenceListByCountry(factory, Core.Constants.CountryCodes.Canada);
		}

		ICodeDescriptionPairList Integration.Customs.CA.ILookupsHelper.TreatmentCodes(BusinessObjectFactory factory) => TreatmentCodes(factory);

		public static CodeDescriptionPairList CFIAStatesOfOrigin(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<USStatesList>();
		}

		public static CodeDescriptionPairList CanadianProvinces(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<CanadianProvinceList>();
		}

		public static CACFIAEndUseCodesCollection CFIAEndUseCodes(BusinessObjectFactory factory)
		{
			return new CACFIAEndUseCodesCollection(factory);
		}

		public static CACFIAMiscCodesCollection CFIAMiscIDCodes(BusinessObjectFactory factory)
		{
			return new CACFIAMiscCodesCollection(factory);
		}

		public static ValueForDutyCodes ValueForDutyCodes(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<ValueForDutyCodes>();
		}

		public static CodeDescriptionPairList ImportReasonCodes(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<ImportReasonCodes>();
		}

		public static CodeDescriptionPairList GSTStatusCodes(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<GSTStatusCodes>();
		}

		public static CodeDescriptionPairList ETExemptionCodes(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<ExciseTaxExemptionCodes>();
		}

		public static CodeDescriptionPairList CA_PGAIndicatorList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<YesNoList>();
		}

		public static CodeDescriptionPairList CasualImportCommodityList(BusinessObjectFactory factory)
		{
			return RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.Canada, RefCusCodeListType.Codes.CasualImportCommodity, ZDateTime.Today);
		}

		public static ZString GetCasualImportCommodityType(BusinessObjectFactory factory, ZString commodity)
		{
			return factory.GetCachedValue($"GetCasualImportCommodityType|{commodity}",
				() => RefCusCodeListAttributeTypes.GetAttributeValuesFor(factory, Core.Constants.CountryCodes.Canada, RefCusCodeListType.Codes.CasualImportCommodity, ZDateTime.Today, commodity, RefCusCodeListAttributes.Names.Type).FirstOrDefault());
		}
	}
}
