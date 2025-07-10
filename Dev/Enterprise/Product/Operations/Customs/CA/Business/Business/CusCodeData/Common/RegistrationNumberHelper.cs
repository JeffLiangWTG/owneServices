using System;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public static class RegistrationNumberHelper
	{
		public static void DefaultSafeFoodLicence(ZString codeType, Action<ZString> valueSetter, CodeDescriptionPairList list)
		{
			if (codeType == RegistrationNumberHelper.SafeFoodForCanadiansLicence && list.Count > 0)
			{
				valueSetter(list[0].Code.ToUpper());
			}
		}

		public static bool IsConfirmationCFIALPCO(BusinessObjectFactory factory, ZString codeName)
		{
			if (!codeName.IsEmpty)
			{
				var code = LoadCFIALPCOType(factory, codeName);
				var format = code?.Attributes.GetAttributeValue(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CFIAFormat) ?? ZString.Empty;
				if (format == Confirmation)
				{
					return true;
				}
			}
			return false;
		}

		public static CodeDescriptionPairList GetCY_DataListForSafeFoodForCanadiansLicence(BusinessObjectFactory factory, OrgHeader effectiveImporter)
		{
			if (effectiveImporter != null)
			{
				return factory.GetCachedValue("SafeFoodLicensesRefNumbers_" + effectiveImporter.OH_Code, () =>
				{
					var result = new CodeDescriptionPairList();
					foreach (var oneItem in OrgImpAddInfo.Get(effectiveImporter).SafeFoodLicenses)
					{
						result.AddPair(oneItem.CY_Code, oneItem.CY_Data);
					}
					return result;
				});
			}
			return new CodeDescriptionPairList();
		}

		public static void ValidateCFIARegNum(CusCodeData regNum)
		{
			Validate(regNum.Factory, regNum.CY_DataInfo, regNum.CY_Code, regNum.CY_Data, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CFIAAIRSRegistrationType);
		}

		public static void ValidateCFIARegNum(CusCALPCO lpco)
		{
			Validate(lpco.Factory, lpco.CLP_RefNoInfo, lpco.CLP_Type, lpco.CLP_RefNo, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CFIALPCOType);
		}

		static void Validate(BusinessObjectFactory factory, ZPropertyInfo propertyInfo, ZString codeName, ZString regNum, ZString codeType)
		{
			if (!codeName.IsEmpty)
			{
				var code = LoadCFIARefCusCodeList(factory, codeName, codeType);
				var format = code?.Attributes.GetAttributeValue(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CFIAFormat) ?? ZString.Empty;
				if (format == Confirmation)
				{
					if (regNum != YesNoList.Codes.Yes)
					{
						propertyInfo.AddMessageError(ShouldIndicateY);
					}
				}
				else if (format == Alpha)
				{
					if (!Regex.IsMatch(regNum, @"^[a-zA-Z0-9 .-]+$"))
					{
						propertyInfo.AddWarning(ShouldBeAlphanumeric);
					}
				}
				else if (format == Numeric)
				{
					if (!ZDecimal.TryParse(regNum, out _))
					{
						propertyInfo.AddMessageError(ShouldBeNumericOnly);
					}
				}

				if (codeName == SafeFoodForCanadiansLicence)
				{
					ListValidation.WarnIfInvalidCode(propertyInfo, SafeFoodLicenseNotListed);
				}
			}
		}

		static ZZRefCusCodeListCombined LoadCFIARefCusCodeList(BusinessObjectFactory factory, ZString codeName, ZString codeType)
		{
			var date = ZDateTime.UtcToday;
			return factory.GetCachedValue(string.Format("{0}_{1}_{2}", codeName, codeType, date.ToShortDateString()), () =>
			{
				return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, codeName, Core.Constants.CountryCodes.Canada, codeType, date);
			});
		}

		public static ZZRefCusCodeListCombined LoadCFIAAirRegistrationType(BusinessObjectFactory factory, ZString codeName) => LoadCFIARefCusCodeList(factory, codeName, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CFIAAIRSRegistrationType);

		public static ZZRefCusCodeListCombined LoadCFIALPCOType(BusinessObjectFactory factory, ZString codeName) => LoadCFIARefCusCodeList(factory, codeName, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CFIALPCOType);

		public static void ValidateCFIADIFURN(CusCALPCO lpco)
		{
			var cfia = lpco.Parent as CFIAPGAHeader;
			if (cfia != null)
			{
				var code = LoadCFIALPCOType(lpco.Factory, lpco.CLP_Type);
				if (code != null)
				{
					var dif = lpco.CLP_DIFRefNumberOrLocation;
					var dematerializedCountryCodes = code.Attributes.GetAttributeValue(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CFIADematerializedCountry);
					if (dif.IsEmpty)
					{
						if (code.Attributes.HasAttribute(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CFIAMaterialized, YesNoList.Codes.Yes)
							&& (dematerializedCountryCodes.IsEmpty || cfia.RN_NKCountryOfSource.IsEmpty || !dematerializedCountryCodes.Contains(cfia.RN_NKCountryOfSource)))
						{
							lpco.CLP_DIFRefNumberOrLocationInfo.AddMessageError(RequireDIFURN);
						}
					}
					else
					{
						if (code.Attributes.HasAttribute(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CFIAMaterialized, YesNoList.Codes.No)
							|| (!dematerializedCountryCodes.IsEmpty && !cfia.RN_NKCountryOfSource.IsEmpty && dematerializedCountryCodes.Contains(cfia.RN_NKCountryOfSource)))
						{
							lpco.CLP_DIFRefNumberOrLocationInfo.AddMessageError(NotRequireDIFURN);
						}
					}
				}
			}
		}

		public const string Alpha = "A";
		public const string Numeric = "N";
		public const string Confirmation = "C";
		public const string SafeFoodForCanadiansLicence = "893";
		public const string Materialized = "M";

		internal static MultilingualString SafeFoodLicenseNotListed => ResString.GetMultilingualString("FC5AFEFA-03A5-49DF-8D97-5F365E7CA896", "Safe Food License is not listed under the importer.To add go to the Importer Organization > Details > Config > Canada > CA Details");

		internal static MultilingualString ShouldIndicateY
		{
			get
			{
				return ResString.GetMultilingualString("158C3750-2EC6-4D0C-866E-98A3A5A410BC", "The registration number field must be a confirmation indicated by a Y.");
			}
		}

		internal static MultilingualString NotRequireDIFURN
		{
			get
			{
				return ResString.GetMultilingualString("DB593C3C-48FE-4C37-9275-FC57B700036E", "De-materialized LPCO does not require DIF URN.");
			}
		}

		internal static MultilingualString RequireDIFURN
		{
			get
			{
				return ResString.GetMultilingualString("32EECDBC-723B-48DD-BB06-84BD5A852930", "Materialized LPCO’s require a DIF URN.");
			}
		}

		internal static MultilingualString ShouldBeNumericOnly
		{
			get
			{
				return ResString.GetMultilingualString("CA975051-7AD9-413D-AEAE-724462531C0C", "The number should be numeric only.");
			}
		}

		internal static MultilingualString ShouldBeAlphanumeric
		{
			get
			{
				return ResString.GetMultilingualString("D7A4043B-9179-4BF9-8F98-E21FBCD01AC3", "The number should be alphanumeric, dots, space, or hyphens.");
			}
		}
	}
}
