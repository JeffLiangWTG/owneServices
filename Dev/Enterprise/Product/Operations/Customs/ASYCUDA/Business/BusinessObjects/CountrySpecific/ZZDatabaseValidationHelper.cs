using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using static System.FormattableString;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeList;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class ZZDatabaseValidationHelper
	{
		ZZDatabaseValidationHelper(ZString country, BusinessObjectFactory factory)
		{
			this.factory = factory;
			this.country = country;
		}

		readonly BusinessObjectFactory factory;
		readonly ZString country;

		public ZZDatabaseValidationHelper(AsycudaManifestHeader header) : this(header.AMA_RN_NKCountry, header.Factory)
		{
			Header = Argument.NotNull(header, nameof(header));
		}
		protected readonly AsycudaManifestHeader Header;

		public static ZZDatabaseValidationHelper GetDefaultValidationHelper(BusinessObjectFactory factory) => new (Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, factory);

		internal void CheckIsMandatoryWhenMatchingAttribute(ZPropertyInfo info, string zzdCode, string zzeName, string valueToMatchAttribute)
		{
			var errorMessageWhenEmpty = IsMandatoryWhenMatchingAttribute(zzdCode, zzeName, valueToMatchAttribute);
			if (!errorMessageWhenEmpty.IsEmpty)
			{
				info.AddMessageError(errorMessageWhenEmpty);
			}
		}

		internal ZString IsMandatoryWhenMatchingAttribute(string zzdCode, string zzeName, string valueToMatchAttribute)
		{
			return IsMandatoryWhenAttributeMatches(country, zzdCode, zzeName, valueToMatchAttribute);
		}

		internal ZString HasMandatoryCusCode(string fieldIdentifier, JobDocAddress parentJDA)
		{
			return HasMandatoryCusCode(fieldIdentifier, parentJDA.Address?.Header);
		}

		internal ZString HasMandatoryCusCode(ZString fieldIdentifier, OrgHeader parentOrg)
		{
			if (country.IsEmpty)
			{
				return ZString.Empty;
			}

			var result = new List<ZString>();
			var cusCodeRequiredAttribute = GetCusCodesRequired(country, fieldIdentifier);
			if (cusCodeRequiredAttribute != null)
			{
				foreach (var attrib in cusCodeRequiredAttribute)
				{
					var code = attrib.ZZE_Value;
					if (parentOrg == null || parentOrg.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(country, new[] { code }).IsEmpty)
					{
						result.Add(ValidationConstants.RequiresCustomsCode(code, country));
					}
				}
			}
			return result.Count == 0 ? string.Empty : ValidationConstants.PartyRequiresCustomsCode(fieldIdentifier.ToProperCase(), new ZStringBuilder(result).ToStringWithNewLineBetweenAppends());
		}

		internal IEnumerable<ZZRefCusCodeListAttributeCombined> GetCusCodesRequired(ZString countryCode, string fieldIdentifier)
		{
			IEnumerable<ZZRefCusCodeListAttributeCombined> mandatoryCusCodes = null;
			var zzd = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, fieldIdentifier, countryCode, RefCusCodeListTypes.Codes.ManifestValidationRule, ZDateTime.Today);
			if (zzd != null)
			{
				mandatoryCusCodes = zzd.Attributes.OfType<ZZRefCusCodeListAttributeCombined>().Where(a => a.ZZE_ZXE_NKName.EqualsIgnoringCase(ManifestValidationRuleCodes.MandatoryOrgCusCode));
				if (mandatoryCusCodes.Any())
				{
					// Country is M, doesn't matter about the global setting
					return mandatoryCusCodes;
				}
			}

			var zzdAllCountries = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, fieldIdentifier, Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, RefCusCodeListTypes.Codes.ManifestValidationRule, ZDateTime.Today);
			if (zzdAllCountries != null)
			{
				mandatoryCusCodes = zzdAllCountries.Attributes.OfType<ZZRefCusCodeListAttributeCombined>().Where(a => a.ZZE_ZXE_NKName.EqualsIgnoringCase(ManifestValidationRuleCodes.MandatoryOrgCusCode));
			}
			return mandatoryCusCodes;
		}

		internal ZzVesselWithRadioAndCarrierRule CompileRulesForCheckingZzVesselAndCarrier(string transportMode)
		{
			ZzVesselWithRadioAndCarrierRule rule = null;
			var zzd = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, ManifestValidationRuleCodes.ZZVESSELANDCARRIER, country, RefCusCodeListTypes.Codes.ManifestValidationRule, ZDateTime.Today);
			if (zzd != null)
			{
				rule = new ZzVesselWithRadioAndCarrierRule(country, false, false, false);
				// Need to apply the rule, generally.
				// But what about the specifics of radio and carrier code, are both of those demanded for this country?
				foreach (var zze in zzd.Attributes.Cast<ZZRefCusCodeListAttributeCombined>().Where(x => x.ZZE_ZXE_NKName.EqualsIgnoringCase(ManifestValidationRuleCodes.MANDATORYZZQUALITY)))
				{
					switch (zze.ZZE_Value)
					{
						case ManifestValidationRuleCodes.Carrier when zze.IsTransportModeApplied(transportMode):
							rule.IsCarrierCodeNeeded = true;
							break;
						case ManifestValidationRuleCodes.RadioCallSign:
							rule.IsRadioNeeded = true;
							break;
						case ManifestValidationRuleCodes.CccInList when zze.IsTransportModeApplied(transportMode):
							rule.CccShouldBeInList = true;
							break;
					}
				}
			}
			return rule;
		}

		internal class ZzVesselWithRadioAndCarrierRule
		{
			public readonly string CountryCode;
			public bool IsRadioNeeded { get; set; }
			public bool IsCarrierCodeNeeded { get; set; }
			public bool CccShouldBeInList { get; set; }

			public ZzVesselWithRadioAndCarrierRule(ZString countryCode, bool isRadioNeeded, bool isCarrierCodeNeeded, bool cccShouldBeInList)
			{
				CountryCode = countryCode;
				IsCarrierCodeNeeded = isCarrierCodeNeeded;
				IsRadioNeeded = isRadioNeeded;
				CccShouldBeInList = cccShouldBeInList;
			}
		}

		public void CheckIsMandatoryFor(ZPropertyInfo info, string validationRule)
		{
			if (info.Value.IsEmpty)
			{
				CheckIsMandatoryForValidationRuleWhenTheRelatedValueIsEmpty(info, validationRule);
			}
		}

		public void CheckIsMandatoryForValidationRuleWhenTheRelatedValueIsEmpty(ZPropertyInfo info, string validationRule)
		{
			var errorMessageWhenEmpty = IsMandatoryFor(validationRule);
			if (!errorMessageWhenEmpty.IsEmpty)
			{
				info.AddMessageError(errorMessageWhenEmpty);
			}
		}

		ZString IsMandatoryFor(string fieldIdentifier)
		{
			var errorMessage = ZString.Empty;

			if (GetMandatoryFieldsInZZ)
			{
				errorMessage = IsMandatoryForOneCountry(factory, country, fieldIdentifier);
			}
			else if (MandatoryFields.TryGetValue(fieldIdentifier, out var config) && (config.NeedsToCheck?.Invoke() ?? true))
			{
				errorMessage = config.ErrorMessage;
			}
			return errorMessage;
		}

		internal protected virtual bool GetMandatoryFieldsInZZ => true;

		IReadOnlyDictionary<string, MandatoryValidationRule> MandatoryFields => mandatoryFields ??= GetMandatoryFieldsCore();
		IReadOnlyDictionary<string, MandatoryValidationRule> mandatoryFields;

		protected virtual Dictionary<string, MandatoryValidationRule> GetMandatoryFieldsCore() => new ()
		{
			{ ManifestValidationRuleCodes.Consignee, new MandatoryValidationRule(Res.GetString("ZZDatabaseValidationHelper|Consignee", "A Consignee is required"), () => true) },
			{ ManifestValidationRuleCodes.OfficeCode, new MandatoryValidationRule(Res.GetString("ZZDatabaseValidationHelper|OfficeCode", "A Customs Office Code is required"), () => true) },
			{ ManifestValidationRuleCodes.EstimatedDepartureTime, new MandatoryValidationRule(Res.GetString("ZZDatabaseValidationHelper|EstimatedDepartureTime", "An Estimated Departure Time is required"), () => true) },
			{ ManifestValidationRuleCodes.ShipmentType, new MandatoryValidationRule(Res.GetString("ZZDatabaseValidationHelper|ShipmentType", "A Shipment Type is required"), () => true) },
			{ ManifestValidationRuleCodes.CurrentUserEmailAddress, new MandatoryValidationRule(Res.GetString("ZZDatabaseValidationHelper|CurrentUserEmailAddress", "An email address is required for the current user"), () => true) },
			{ ManifestValidationRuleCodes.Nature, new MandatoryValidationRule(Res.GetString("ZZDatabaseValidationHelper|Nature", "A Nature is required"), () => true) },
			{ ManifestValidationRuleCodes.FinalDestination, new MandatoryValidationRule(Res.GetString("ZZDatabaseValidationHelper|FinalDestination", "A Final Destination is required"), () => true) },
		};

		public static ZString IsMandatoryForOneCountry(BusinessObjectFactory factory, ZString countryCode, string fieldIdentifier)
		{
			Func<ZZRefCusCodeListAttributeCombined, bool> matchAttribute = (a) => a.ZZE_ZXE_NKName.EqualsIgnoringCase(ManifestValidationRuleCodes.Mandatory);
			return IsMandatoryFor(factory, countryCode, fieldIdentifier, matchAttribute);
		}

		internal void CheckIsMandatoryWhenAttributeMatches(ZPropertyInfo info, string zzdName, string attributeName, string valueToMatchAttribute)
		{
			var errorMessageWhenEmpty = IsMandatoryWhenAttributeMatches(country, zzdName, attributeName, valueToMatchAttribute);
			if (!errorMessageWhenEmpty.IsEmpty)
			{
				info.AddMessageError(errorMessageWhenEmpty);
			}
		}

		ZString IsMandatoryWhenAttributeMatches(ZString countryCode, string zzdName, string attributeName, string valueToMatchAttribute)
		{
			Func<ZZRefCusCodeListAttributeCombined, bool> matchAttribute = (a) => a.ZZE_ZXE_NKName.EqualsIgnoringCase(ManifestValidationRuleCodes.Mandatory) || (a.ZZE_ZXE_NKName.EqualsIgnoringCase(attributeName) && a.ZZE_Value.EqualsIgnoringCase(valueToMatchAttribute));
			return IsMandatoryFor(factory, countryCode, zzdName, matchAttribute);
		}

		internal static ZBool IsMandatoryForOneCountryWhenAttributeMatches(BusinessObjectFactory factory, ZString countryCode, string zzdName, string attributeName, string valueToMatchAttribute)
		{
			var zzd = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, zzdName, countryCode, RefCusCodeListTypes.Codes.ManifestValidationRule, ZDateTime.Today);
			return zzd?.Attributes.OfType<ZZRefCusCodeListAttributeCombined>().Any(matchAttribute) ?? false;

			bool matchAttribute(ZZRefCusCodeListAttributeCombined a) => a.ZZE_ZXE_NKName.EqualsIgnoringCase(attributeName) && a.ZZE_Value.EqualsIgnoringCase(valueToMatchAttribute);
		}

		internal static ZBool IsMandatoryForOneCountryWhenTransportModeMatches(BusinessObjectFactory factory, ZString countryCode, string zzdName, string transportMode)
		{
			var zzd = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, zzdName, countryCode, RefCusCodeListTypes.Codes.ManifestValidationRule, ZDateTime.Today);
			return zzd != null && RefTransportModesHelper.IsTransportModeApplied(zzd, transportMode);
		}

		static ZString IsMandatoryFor(BusinessObjectFactory factory, ZString countryCode, string fieldIdentifier, Func<ZZRefCusCodeListAttributeCombined, bool> matchAttribute)
		{
			if (!countryCode.IsEmpty)
			{
				var zzd = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, fieldIdentifier, countryCode, RefCusCodeListTypes.Codes.ManifestValidationRule, ZDateTime.Today);
				if (zzd != null && zzd.Attributes.OfType<ZZRefCusCodeListAttributeCombined>().Any(matchAttribute))
				{
					// Country is M, doesn't matter about the global setting
					return ValidationConstants.FieldIsMandatory(zzd.ZZD_Description, countryCode);
				}

				var zzdAllCountries = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, fieldIdentifier, Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, RefCusCodeListTypes.Codes.ManifestValidationRule, ZDateTime.Today);
				if (zzdAllCountries != null && zzdAllCountries.Attributes.OfType<ZZRefCusCodeListAttributeCombined>().Any(matchAttribute))
				{
					// Mandatory globally, but does this country have an exception?
					var isExpresslyOptional = zzd != null && zzd.Attributes.OfType<ZZRefCusCodeListAttributeCombined>().Any(a => a.ZZE_ZXE_NKName.EqualsIgnoringCase(ManifestValidationRuleCodes.Optional));
					return isExpresslyOptional ? ZString.Empty : ValidationConstants.FieldIsMandatory(zzdAllCountries.ZZD_Description);
				}
			}
			return ZString.Empty;
		}

		internal void CheckIsMandatoryWhenTransportModeMatches(ZPropertyInfo info, string zzdName, string transportMode)
		{
			var errorMessageWhenEmpty = IsMandatoryWhenTransportModeMatches(zzdName, transportMode);
			if (!errorMessageWhenEmpty.IsEmpty)
			{
				info.AddMessageError(errorMessageWhenEmpty);
			}
		}

		internal ZString IsMandatoryWhenTransportModeMatches(string zzdName, string transportMode)
		{
			return IsMandatoryWhenTransportModeMatches(country, zzdName, transportMode);
		}

		ZString IsMandatoryWhenTransportModeMatches(ZString countryCode, string zzdName, string transportMode)
		{
			var errorMessage = ZString.Empty;

			if (GetMandatoryFieldsInZZ)
			{
				errorMessage = IsMandatoryForTransportMode(countryCode, zzdName, transportMode);
			}
			else if (MandatoryFields.TryGetValue(zzdName, out var config)
				&& (config.NeedsToCheck?.Invoke() ?? true)
				&& (config.TransportModes?.Contains(transportMode, StringComparer.OrdinalIgnoreCase) ?? true))
			{
				errorMessage = config.ErrorMessage;
			}
			return errorMessage;
		}

		internal void CheckIsMandatoryForOneCountryWhenTransportModeMatches(ZPropertyInfo info, string zzdName, string transportMode)
		{
			CheckIsMandatoryForOneCountryWhenTransportModeMatches(info, country, zzdName, transportMode);
		}

		internal void CheckIsMandatoryForOneCountryWhenTransportModeMatches(ZPropertyInfo info, ZString countryCode, string zzdName, string transportMode)
		{
			if (info.Value.IsEmpty)
			{
				var errorMessageWhenEmpty = IsMandatoryWhenTransportModeMatches(countryCode, zzdName, transportMode);
				if (!errorMessageWhenEmpty.IsEmpty)
				{
					info.AddMessageError(errorMessageWhenEmpty);
				}
			}
		}

		ZString IsMandatoryForTransportMode(ZString countryCode, string fieldIdentifier, string transportMode)
		{
			var result = ZString.Empty;
			if (!countryCode.IsEmpty)
			{
				var zzd = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, fieldIdentifier, countryCode, RefCusCodeListTypes.Codes.ManifestValidationRule, ZDateTime.Today);

				if (zzd != null && (RefTransportModesHelper.IsTransportModeApplied(zzd, transportMode) || zzd.Attributes.OfType<ZZRefCusCodeListAttributeCombined>().Any(a => a.ZZE_ZXE_NKName.EqualsIgnoringCase(ManifestValidationRuleCodes.Mandatory))))
				{
					result = ValidationConstants.FieldIsMandatory(zzd.ZZD_Description, countryCode);
				}
				else
				{
					var zzdAllCountries = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, fieldIdentifier, Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, RefCusCodeListTypes.Codes.ManifestValidationRule, ZDateTime.Today);
					if (zzdAllCountries != null && (RefTransportModesHelper.IsTransportModeApplied(zzdAllCountries, transportMode) || zzdAllCountries.Attributes.OfType<ZZRefCusCodeListAttributeCombined>().Any(a => a.ZZE_ZXE_NKName.EqualsIgnoringCase(ManifestValidationRuleCodes.Mandatory))))
					{
						var isExpresslyOptional = zzd != null && zzd.Attributes.OfType<ZZRefCusCodeListAttributeCombined>().Any(a => a.ZZE_ZXE_NKName.EqualsIgnoringCase(ManifestValidationRuleCodes.Optional));
						result = isExpresslyOptional ? ZString.Empty : ValidationConstants.FieldIsMandatory(zzdAllCountries.ZZD_Description);
					}
				}
			}
			return result;
		}

		public static IEnumerable<ApplicationBusinessProvider> GetNVCApplicationBusinessProviders(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("NVCC_Countries", () =>
			{
				var result = new List<ApplicationBusinessProvider>();
				result.AddRange(GetAsycudaManifestCountries(factory, RefCusCodeListTypes.Codes.NVC).SelectMany(x => ApplicationBusinessProvider.GetApplicationBusinessProviders(factory, x.ZZD_Code)));
				result.AddRange(GetApplicationProviderManifestCountries(factory, ApplicationCodeTypeList.Codes.Consolidator));
				return result.Distinct();
			});
		}

		public static IEnumerable<ApplicationBusinessProvider> GetVOCApplicationBusinessProviders(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("VOCC_Countries", () =>
			{
				var result = new List<ApplicationBusinessProvider>();
				result.AddRange(GetAsycudaManifestCountries(factory, RefCusCodeListTypes.Codes.VOC).SelectMany(x => ApplicationBusinessProvider.GetApplicationBusinessProviders(factory, x.ZZD_Code)));
				result.AddRange(GetApplicationProviderManifestCountries(factory, ApplicationCodeTypeList.Codes.ShippingLine));
				return result.Distinct();
			});
		}

		public static IEnumerable<ApplicationBusinessProvider> GetBCDApplicationBusinessProviders(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("BCDC_Countries", () =>
			{
				var result = new List<ApplicationBusinessProvider>();
				result.AddRange(GetApplicationProviderManifestCountries(factory, ApplicationCodeTypeList.Codes.TWBriefCustomsDeclaration));
				return result.Distinct();
			});
		}

		public static IEnumerable<ApplicationBusinessProvider> GetLVCApplicationBusinessProviders(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("LVCC_Countries", () =>
			{
				var result = new List<ApplicationBusinessProvider>();
				result.AddRange(GetApplicationProviderManifestCountries(factory, ApplicationCodeTypeList.Codes.EuH7));
				result.AddRange(GetApplicationProviderManifestCountries(factory, ApplicationCodeTypeList.Codes.EuH7V1));
				result.AddRange(GetApplicationProviderManifestCountries(factory, ApplicationCodeTypeList.Codes.EuH7V2));
				return result.Distinct();
			});
		}

		static IEnumerable<ApplicationBusinessProvider> GetApplicationProviderManifestCountries(BusinessObjectFactory factory, ZString manifestStyle)
		{
			return ApplicationBusinessProvider.GetManifestApplicationBusinessProvidersForActiveManifestTypes(factory, manifestStyle);
		}

		public static IEnumerable<ZZRefCusCodeListCombined> GetAsycudaManifestCountries(BusinessObjectFactory factory, ZString typeCode)
		{
			return CountryHelper.SupportedAsycudaCountryCodesList(factory).Where(c => c.HasAttribute(typeCode));
		}

		internal void CheckIsMandatoryFor(ZPropertyInfo propertyInfo, IEnumerable<ManifestValidationRule> mandatoryRules)
		{
			if (Header != null)
			{
				var rules = mandatoryRules.ToArray();
				var countryName = Header.CountryName;
				var messsageErrors = new List<ZString>();
				var areAllMandatoryRulesApplied = true;
				foreach (var mandatoryRule in rules)
				{
					var mandatoryOnPropertyInfo = mandatoryRule.MandatoryOnPropertyInfoGetter?.Invoke(Header);
					if (mandatoryOnPropertyInfo != null)
					{
						var mandatoryOn = mandatoryOnPropertyInfo.Value.ToString();
						if (IsMandatoryForOneCountryWhenAttributeMatches(factory, country, mandatoryRule.MandatoryField,
							mandatoryRule.MandatoryRuleCode, mandatoryOn))
						{
							messsageErrors.Add(Invariant($"{mandatoryOnPropertyInfo.Description} is {mandatoryOn}"));
						}
						else
						{
							areAllMandatoryRulesApplied = false;
							break;
						}
					}
					else
					{
						areAllMandatoryRulesApplied = false;
						break;
					}
				}

				if (areAllMandatoryRulesApplied && messsageErrors.Any())
				{
					var msg = ZString.Join(" and ", messsageErrors.ToArray());
					propertyInfo.AddMessageError(Invariant($"{propertyInfo.Description} is compulsory when {msg} for {country} ({countryName})."));
				}
			}
		}

		public class MandatoryValidationRule
		{
			public MandatoryValidationRule(string errorMessage, Func<bool> needsToCheck, string[] transportModes = null)
			{
				ErrorMessage = errorMessage;
				NeedsToCheck = needsToCheck;
				TransportModes = transportModes;
			}

			public string ErrorMessage { get; set; }
			public Func<bool> NeedsToCheck { get; set; }
			public string[] TransportModes { get; set; }
		}

		public class ManifestValidationRule
		{
			public ManifestValidationRule(ZString mandatoryField, ZString mandatoryRuleCode, Func<AsycudaManifestHeader, ZPropertyInfo> mandatoryOnPropertyInfoGetter)
			{
				MandatoryField = mandatoryField;
				MandatoryRuleCode = mandatoryRuleCode;
				MandatoryOnPropertyInfoGetter = mandatoryOnPropertyInfoGetter;
			}
			public ZString MandatoryField { get; }
			public ZString MandatoryRuleCode { get; }
			public Func<AsycudaManifestHeader, ZPropertyInfo> MandatoryOnPropertyInfoGetter { get; }
		}
	}
}
