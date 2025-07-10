using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business
{
	public static class Extensions
	{
		public static IEnumerable<T> GetCachedAggregatedData<T>(this BusinessObjectFactory factory, ref CachedProperty<IEnumerable<T>> cachedProperty, Func<string[]> getKeys, Func<IEnumerable<T>> getBizObjs)
			where T : BusinessObject
		{
			return factory.GetValue(ref cachedProperty, () => GetAggregatedData(getKeys(), getBizObjs()));
		}

		public static IEnumerable<T> GetAggregatedData<T>(IReadOnlyList<string> keys, IEnumerable<T> bizObjs)
			where T : BusinessObject
		{
			var dictionary = new Dictionary<MergeKey, T>();
			if (keys.Count > 0)
			{
				foreach (T bizObj in bizObjs)
				{
					dictionary.AddMergeKey(keys, bizObj);
				}
			}
			return dictionary.Values;
		}

		public static void AddMergeKey<T>(this Dictionary<MergeKey, T> dictionary, IReadOnlyList<string> keys, T bizObj)
			where T : BusinessObject
		{
			var mergeKey = new MergeKey();
			foreach (string key in keys)
			{
				mergeKey.Add((IZType)bizObj[key]);
			}
			dictionary[mergeKey] = bizObj;
		}

		public static ZString GetEoriNumber(this OrgHeader header, ZString countryCode) => header?.CustomsCodes.GetCustomsRegNo(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, countryCode) ?? ZString.Empty;

		public static ZString GetEoriDetails(this OrgHeader orgHeader, bool errorOnMultiple = false, params ZString[] countriesToIgnore) => orgHeader?.GetConcatenatedSingleOrgCusCodeIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, errorOnMultiple, countriesToIgnore) ?? ZString.Empty;

		public static ZString GetICS2EoriDetails(this OrgHeader orgHeader, bool errorOnMultiple = false) => orgHeader?.GetEoriDetails(errorOnMultiple, countriesToIgnore: [Core.Constants.CountryCodes.UnitedKingdom]) ?? ZString.Empty;

		public static ZString GetEoriDetailsForSpecificCountry(this OrgHeader orgHeader) => orgHeader?.GetConcatenatedSingleOrgCusCode(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori) ?? ZString.Empty;

		public static ZString GetEoriNumberWithFallback()
		{
			var result = GlbBranch.CurrentBranch.OrgProxy?.GetEoriDetails() ?? ZString.Empty;
			if (result.IsEmpty)
			{
				result = GlbCompany.CurrentCompany.OrgProxy?.GetEoriDetails() ?? ZString.Empty;
			}
			return result;
		}

		public static ZString GetEORIFromAddress(this OrgAddress address)
		{
			var result = ZString.Empty;
			if (address != null)
			{
				result = address.CustomsCodes.GetCustomsRegNo(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, address.OA_RN_NKCountryCode);
				if (result.IsEmpty)
				{
					result = address.Header.CustomsCodes.GetCustomsRegNo(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, address.OA_RN_NKCountryCode);
				}
			}

			return result;
		}

		public static ZBool HasSameEori(this OrgHeader parentOrg, OrgHeader otherOrg)
		{
			if (parentOrg == null || otherOrg == null)
			{
				return false;
			}

			if (parentOrg.PK == otherOrg.PK)
			{
				return true;
			}

			var parentEori = parentOrg.GetConcatenatedSingleOrgCusCodeIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);
			var otherEori = otherOrg.GetConcatenatedSingleOrgCusCodeIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);

			return !parentEori.IsEmpty && parentEori == otherEori;
		}

		public static ZBool HasSameEoriOrTcu(this OrgHeader parentOrg, OrgHeader otherOrg)
		{
			if (HasSameEori(parentOrg, otherOrg))
			{
				return true;
			}

			if (parentOrg == null || otherOrg == null)
			{
				return false;
			}

			var parentTcuCollection = parentOrg.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU).Select(x => x.OK_RN_NKCodeCountry + x.OK_CustomsRegNo);
			var otherTcuCollection = otherOrg.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU).Select(x => x.OK_RN_NKCodeCountry + x.OK_CustomsRegNo);

			return parentTcuCollection.Intersect(otherTcuCollection).Any();
		}

		public static ZString GetConcatenatedSingleOrgCusCodeIgnoringCountry(this OrgHeader orgHeader, ZString codeType, bool errorOnMultiple = false, params ZString[] countriesToIgnore)
		{
			var result = ZString.Empty;
			var cusCodes = orgHeader?.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(codeType);
			if (cusCodes != null && cusCodes.Length != 0)
			{
				if (countriesToIgnore is { Length: > 0 })
				{
					cusCodes = cusCodes.Where(x => !countriesToIgnore.Contains(x.OK_RN_NKCodeCountry)).ToArray();
				}
				if (cusCodes.Length == 1)
				{
					var orgCusCode = cusCodes[0];
					result = orgCusCode.OK_CustomsRegNo.AddPrefixToNumber(orgCusCode.OK_RN_NKCodeCountry);
				}
				else if (errorOnMultiple)
				{
					result = $"* multiple {codeType} *";
				}
			}

			return result;
		}

		public static ZString GetConcatenatedSingleOrgCusCode(this OrgHeader orgHeader, ZString codeType)
		{
			var result = ZString.Empty;
			var cusCodes = orgHeader?.CustomsCodes.GetOrgCusCodesForCodeAndCountry(codeType, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			if (cusCodes != null)
			{
				if (cusCodes.Length == 0)
				{
					cusCodes = orgHeader.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(codeType);
					result = cusCodes.Length > 0 ? cusCodes.First().OK_CustomsRegNo.AddPrefixToNumber(cusCodes[0].OK_RN_NKCodeCountry) : ZString.Empty;
				}
				else
				{
					var orgCusCode = cusCodes[0];
					result = orgCusCode.OK_CustomsRegNo.AddPrefixToNumber(orgCusCode.OK_RN_NKCodeCountry);
				}
			}
			return result;
		}

		public static ZString AddPrefixToNumber(this ZString number, ZString prefix) => number.IsEmpty ? number : (ZString)(prefix + number);

		public static ZBool IsACountryEligibleToACommonTransitProcedure(this RefCountry country)
		{
			return country.IsPartOfTradeGroup(Core.Constants.Customs.Universal.RefCusTradeGroup.Codes.EUCommonTransitProcedure);
		}

		/// <summary>
		/// True if "country" is part of the EU Special Fiscal Territories, that means a part of the customs territory of
		/// the Union where the provisions of Council Directive 2006/112/EC of 28 November 2006 on the common system of value added tax or
		/// Council Directive 2008/118/EC of 16 December 2008 concerning the general arrangements for excise duty and repealing Directive 92/12/EEC do not apply.
		/// </summary>
		/// <param name="country"></param>
		/// <returns></returns>
		public static ZBool IsASpecialTerritoryOfTheCommunity(this RefCountry country)
		{
			return country.IsPartOfTradeGroup(UniversalReferenceConstants.RefCusTradeGroups.Groups.EUSpecialFiscalTerritories);
		}

		public static ZBool HasSpecialTerritoriesOfTheCommunity(this RefCountry country)
		{
			return country.IsPartOfTradeGroup(UniversalReferenceConstants.RefCusTradeGroups.Groups.EUSpecialFiscalTerritoryCountries);
		}

		public static ZBool IsEFTA(this RefCountry country)
		{
			return country.IsPartOfTradeGroup(UniversalReferenceConstants.RefCusTradeGroups.Groups.EFTA);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Key strings")]
		public static ZBool IsPartOfTradeGroup(this RefCountry country, ZString tradeGroup)
		{
			var result = ZBool.False;

			if (country != null)
			{
				var factory = country.Factory;
				var loader = factory.GetCachedValue("CusRefTradeGroupView.Loader->IsACountryOfThisTradeGroup", () => new CusRefTradeGroupView.Loader(factory));

				result = loader.IsCountryPartOfTradeGroup(country.Code, tradeGroup, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDateTime.Today);
			}

			return result;
		}

		public static bool IsIntegerRequiredUnitOfQuantity(this BusinessObjectFactory factory, ZString unitOfQuantity) => IntegerRequiredUnitOfQuantities(factory).Contains(unitOfQuantity);

		static ImmutableHashSet<string> IntegerRequiredUnitOfQuantities(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("A7A3C057-A2D2-4603-9CA9-89D658222916", () => ImmutableHashSet.Create(
				Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfItems,
				Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfItemsPerFlask,
				Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfCells,
				Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfPairs
			));
		}

		public static ZString GetCustomsRegNoIgnoringCountry(this OrgHeader orgHeader, string codeType)
		{
			var result = ZString.Empty;
			var cusCodes = orgHeader?.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(codeType);
			if (cusCodes != null && cusCodes.Length == 1)
			{
				result = cusCodes[0].OK_CustomsRegNo;
			}
			return result;
		}

		public static ZString GetCustomsRegNoIgnoringCountry(this OrgAddress orgAddress, string codeType)
		{
			var result = ZString.Empty;
			var cusCodes = orgAddress?.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(codeType);
			if (cusCodes != null && cusCodes.Length == 1)
			{
				result = cusCodes[0].OK_CustomsRegNo;
			}
			return result;
		}

		public static ZString GetCustomsRegNoIgnoringCountryPrefixed(this OrgHeader orgHeader, string codeType)
		{
			var result = ZString.Empty;
			var cusCodes = orgHeader?.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(codeType);
			if (cusCodes != null && cusCodes.Length == 1)
			{
				result = cusCodes[0].OK_RN_NKCodeCountry + cusCodes[0].OK_CustomsRegNo;
			}
			return result;
		}

		public static ZString GetVATCodeType(this ZString countryCode)
		{
			CountryVatCodeType.Value.TryGetValue(countryCode, out string vatCodeType);
			return vatCodeType;
		}

		public static ZString GetVATRegistrationNumber(this OrgHeader orgHeader, ZString countryCode)
		{
			var vatCodeType = countryCode.GetVATCodeType();
			return vatCodeType.IsEmpty ? ZString.Empty : (orgHeader?.CustomsCodes.GetCustomsRegNo(vatCodeType, countryCode) ?? ZString.Empty);
		}

		public static ZString GetVATRegistrationNumberWithCountryCodePrefix(this OrgHeader orgHeader, ZString countryCode)
		{
			var vatNumber = orgHeader.GetVATRegistrationNumber(countryCode);
			if (!vatNumber.IsEmpty)
			{
				switch (countryCode)
				{
					case Core.Constants.CountryCodes.Greece:
						vatNumber = "EL" + vatNumber;
						break;

					default:
						vatNumber = countryCode + vatNumber;
						break;
				}
			}
			return vatNumber;
		}

		public static Lazy<ImmutableDictionary<string, string>> CountryVatCodeType => new Lazy<ImmutableDictionary<string, string>>(() =>
		{
			var dataTable = DataUtils.GetDataTableFromQuery(Db.Connection, (NoResString)"SELECT CountryCode, TaxBusinessRegistrationCode FROM EUTaxBusinessRegistrationCodes()");
			return dataTable.AsEnumerable().ToImmutableDictionary(row => row[0].ToString(), row => row[1].ToString());
		}, true);

		public static Type GetCorrectEUTypeForCountryCode(this CountrySpecificTypeDecider typeDecider, ZString countryCode, Type defaultTypeForUnsupportedCountry)
		{
			Type result = null;
			if (ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnionOrInheritsFromEU(Enterprise.Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode)))
			{
				result = typeDecider.GetTypeForCountryCode(countryCode);
			}
			return result ?? defaultTypeForUnsupportedCountry;
		}

		/// <summary>
		/// Returns Fallback Value when value is empty
		/// </summary>
		/// <param name="value">Value to evaluate</param>
		/// <param name="fallbackValue">Fallback value to return when value is empty</param>
		/// <returns></returns>
		public static ZString FallbackTo(this ZString value, ZString fallbackValue) => !value.IsEmpty ? value : fallbackValue;

		public static ZBool EqualsAny(this ZString str, IEnumerable<ZString> compareValues)
		{
			return compareValues.Any(x => x.Equals(str));
		}

		public static OrgCusCode[] GetAllVatNumbers(this OrgHeader orgHeader)
		{
			var listVATNumbers = new List<OrgCusCode>();
			foreach (OrgCusCode orgCusCode in orgHeader.CustomsCodes)
			{
				var codeType = GetVATCodeType(orgCusCode.OK_RN_NKCodeCountry);
				if (!codeType.IsEmpty && codeType.Equals(orgCusCode.OK_CodeType))
				{
					listVATNumbers.Add(orgCusCode);
				}
			}
			return listVATNumbers.ToArray();
		}

		public static string Direction(this ICanBeImportOrExport canBeImportOrExport) => canBeImportOrExport == null ? string.Empty : canBeImportOrExport.IsExport && canBeImportOrExport.IsImport
				? UniversalReferenceConstants.RefCusCodeListDirectionType.Both
				: canBeImportOrExport.IsExport
					? UniversalReferenceConstants.RefCusCodeListDirectionType.Export
					: UniversalReferenceConstants.RefCusCodeListDirectionType.Import;

		public static int GetNumberOfSignificantDigits(this ZDecimal inputValue) => inputValue.Truncate().ToString().Length + inputValue.DecimalPlaces;

		public static string GetIdentificationNumber(this OrgHeader orgHeader)
		{
			string identificationNumber = null;
			var firstEoriCusCode = orgHeader?.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori).FirstOrDefault();
			if (firstEoriCusCode != null)
			{
				identificationNumber = firstEoriCusCode.OK_RN_NKCodeCountry + firstEoriCusCode.OK_CustomsRegNo;
			}
			else
			{
				var firstTcuCusCode = orgHeader?.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU).FirstOrDefault();
				if (firstTcuCusCode != null)
				{
					identificationNumber = firstTcuCusCode.OK_RN_NKCodeCountry + firstTcuCusCode.OK_CustomsRegNo;
				}
			}

			return identificationNumber;
		}
	}
}
