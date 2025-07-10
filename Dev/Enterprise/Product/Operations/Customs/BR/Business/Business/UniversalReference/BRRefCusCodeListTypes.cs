using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using ECC = Enterprise.Core.Constants;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public static class BRRefCusCodeListTypes
	{
		public static CodeDescriptionPairList GetReasonTypeList(BusinessObjectFactory factory)
		{
			return RefCusCodeListTypes.GetCachedList(factory, ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.ReasonType, ZDateTime.Today);
		}

		public static CodeDescriptionPairList GetFinancialInstitutionList(BusinessObjectFactory factory)
		{
			return RefCusCodeListTypes.GetCachedList(factory, ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BankCode, ZDateTime.Today);
		}

		public static ZZRefCusCodeListCombinedCollection GetFinancialInstitutionCollection(BusinessObjectFactory factory)
		{
			return ZZRefCusCodeListCombinedCollection.GetCachedCollection(factory, ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BankCode, ZDateTime.Today);
		}

		public static ZZRefCusCodeListCombinedCollection GetReasonCollection(BusinessObjectFactory factory)
		{
			return ZZRefCusCodeListCombinedCollection.GetCachedCollection(factory, ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.ReasonType, ZDateTime.Today);
		}

		public static CodeDescriptionPairList GetCustomsOfficeList(BusinessObjectFactory factory, string languageCode = "")
		{
			return RefCusCodeListTypes.GetCachedList(factory, ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today, languageCode: languageCode);
		}

		public static CodeDescriptionPairList GetCustomsEnclosureList(BusinessObjectFactory factory, string languageCode = "")
		{
			return RefCusCodeListTypes.GetCachedList(factory, ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today, languageCode: languageCode);
		}

		public static CodeDescriptionPairList GetPackagesTypesList(BusinessObjectFactory factory, string languageCode = "")
		{
			return RefCusCodeListTypes.GetCachedList(factory, ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, ZDateTime.Today, languageCode: languageCode);
		}

		public static CodeDescriptionPairList GetDuimpLegalBaseList(BusinessObjectFactory factory, string languageCode = "")
		{
			return RefCusCodeListTypes.GetCachedList(factory, ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRDuimpLegalBase, ZDateTime.Today, languageCode: languageCode);
		}

		public static CodeDescriptionPairList GetWarehousingSectorList(BusinessObjectFactory factory, ZString customsOffice, ZString customsEnclosure, string languageCode = "")
		{
			if (!customsOffice.IsEmpty && !customsEnclosure.IsEmpty)
			{
				var filterCode = $"{customsOffice};{customsEnclosure};";
				return factory.GetCachedValue("BR_WarehousingSectorList_" + filterCode, () =>
				{
					var result = new CodeDescriptionPairList();
					var warehousingSectors = RefCusCodeListTypes.GetCachedList(factory, ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRWarehousingSectorsCode, ZDateTime.Today, languageCode: languageCode)
						.Cast<ICodeDescription>().Where(t => t.Code.StartsWith(filterCode));

					foreach (var warehousingSector in warehousingSectors)
					{
						result.AddPairIfNotExist(warehousingSector.Code.Replace(filterCode, ""), warehousingSector.Description);
					}
					result.Sort();
					return result;
				});
			}
			return new CodeDescriptionPairList();
		}

		public static ZZRefCusCodeListCombinedCollection GetIssuingAgencyList(BusinessObjectFactory factory)
		{
			return ZZRefCusCodeListCombinedCollection.GetCachedCollection(factory, ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRIssuingAgency, ZDateTime.Today);
		}

		public static CodeDescriptionPairList GetConsentingBodyList(BusinessObjectFactory factory)
		{
			return RefCusCodeListTypes.GetCachedList(factory, ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRConsentingBodyCode, ZDateTime.Today);
		}

		public static CodeDescriptionPairList GetExchangeHedgePaymentMethodList(BusinessObjectFactory factory)
		{
			return RefCusCodeListTypes.GetCachedList(factory, ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRExchangeHedgePaymentCode, ZDateTime.Today);
		}

		public static CodeDescriptionPairList GetTaxRegimeList(BusinessObjectFactory factory, ZString messageType, ZString messageSubType, ZDateTime date, ZString category)
		{
			var taxationRegimePairList = RefCusCodeListTypes.GetCachedList(factory, ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRTaxationRegimeCode, date);

			if (messageType == MessageTypeList.Codes.LIC)
			{
				return taxationRegimePairList;
			}
			else
			{
				return factory.GetCachedValue($"BRTaxRegimeList+{category}+{messageType}+{messageSubType}+{date:yyyy/dd/MM}", () =>
				{
					var selectedCodes = BRRefCusProcedure.GetRefCusProcedureList(factory, category, messageType, messageSubType, date).Select(x => x.ZZ6_PreviousProcedureCode).Distinct().ToList();

					var codePairList = new CodeDescriptionPairList();
					selectedCodes.ForEach(x => codePairList.AddPairIfNotExist(x, taxationRegimePairList.GetDescriptionFromCode(x)));
					codePairList.Sort();
					return codePairList;
				});
			}
		}

		public static CodeDescriptionPairList GetLegalBaseList(BusinessObjectFactory factory, ZString messageType, ZString messageSubType, ZString taxRegime, ZDateTime date, ZString category, ZString codeType)
		{
			var legalBaseRegimePairList = RefCusCodeListTypes.GetCachedList(factory, ECC.CountryCodes.Brazil, codeType, date);

			if (messageType == MessageTypeList.Codes.LIC)
			{
				return legalBaseRegimePairList;
			}
			else
			{
				return factory.GetCachedValue($"BRLegalBaseList+{messageType}+{messageSubType}+{taxRegime}+{codeType}+{date:yyyy/dd/MM}+{category}", () =>
				{
					var selectedCodes = RefCusProcedureCollection.LoadConcessionsForCountryShipmentTypeProcedureCodePreviousProceduresCode(factory, ECC.CountryCodes.Brazil, messageType, messageSubType, taxRegime, date, category).Select(x => x.ZZ6_Concession).Distinct().ToList();

					var codePairList = new CodeDescriptionPairList();
					selectedCodes.ForEach(x => codePairList.AddPairIfNotExist(x, legalBaseRegimePairList.GetDescriptionFromCode(x)));
					codePairList.Sort();
					return codePairList;
				});
			}
		}

		public static CodeDescriptionPairList GetExTariffLegalActList(BusinessObjectFactory factory)
		{
			return RefCusCodeListTypes.GetCachedList(factory, ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRExTariffLegalAct, ZDateTime.Today);
		}

		public static CodeDescriptionPairList GetIPITaxRegimeMethodList(BusinessObjectFactory factory)
		{
			return RefCusCodeListTypes.GetCachedList(factory, ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRTaxRegimeIPI, ZDateTime.Today);
		}

		public static CodeDescriptionPairList GetLegalActIssuingAuthorityList(BusinessObjectFactory factory)
		{
			return RefCusCodeListTypes.GetCachedList(factory, ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRLegalActIssuingAuthority, ZDateTime.Today);
		}

		public static CodeDescriptionPairList GetRefCusCodeListMATMPList(BusinessObjectFactory factory)
		{
			return RefCusCodeListTypes.GetCachedList(factory, ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRCustomsReasonTemporaryAdmissionCode, ZDateTime.Today);
		}

		public static CodeDescriptionPairList GetTariffAgreementCodeList(BusinessObjectFactory factory, ZDateTime date)
		{
			return RefCusCodeListTypes.GetCachedList(factory, ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRTariffAgreementCode, date);
		}

		public static ZZRefCusCodeListCombined GetTariffAgreementCode(BusinessObjectFactory factory, ZString code, ZDateTime date)
		{
			return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, code, ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRTariffAgreementCode, date);
		}

		public static ZString GetCustomsStatusByCustomsCode(BusinessObjectFactory factory, ZString customsCode, ZString prefix)
		{
			var date = ZDateTime.Today;
			var customsCodes = factory.GetCachedValue($"GetCustomsStatusByCustomsCode_{customsCode}_{date}", () =>
			{
				var attributeFilter = new[] { new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.CustomsCode, SQLComparisonOperator.Equal, customsCode) };
				return ZZRefCusCodeListCombined.Loader.Load(factory, ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, date, attributeFilter).Select(x => x.ZZD_Code);
			});
			return customsCodes.FirstOrDefault(x => x.StartsWith(prefix));
		}

		public static CodeDescriptionPairList GetICMSTaxRegimeMethodList(BusinessObjectFactory factory)
		{
			return RefCusCodeListTypes.GetCachedList(factory, ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRTaxRegimeICMS, ZDateTime.Today);
		}

		public static CodeDescriptionPairList GetICMSLegalBaseList(BusinessObjectFactory factory)
		{
			return RefCusCodeListTypes.GetCachedList(factory, ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRLegalBaseICMS, ZDateTime.Today);
		}

		public static CodeDescriptionPairList GetEntryStatusList(BusinessObjectFactory factory, string languageCode = null)
		{
			return RefCusCodeListTypes.GetCachedList(factory, ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, ZDateTime.Today, languageCode: languageCode);
		}

		public static ZString GetTariffAgreementCodeByTypeAndAgreementCodeInImportEntry(BusinessObjectFactory factory, ZString agreementType, ZString agreementCodeInImportEntry)
		{
			var date = ZDateTime.Today;
			return factory.GetCachedValue($"GetTariffAgreementCode_{agreementType}_{agreementCodeInImportEntry}_{date}", () =>
			{
				var attributeFilter = new RefCusCodeListAttributeFilter[]
				{
					new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.Type, SQLComparisonOperator.Equal, agreementType),
					new RefCusCodeListAttributeFilter(Constants.RefCusCodeList.Attributes.AgreementCodeInImportEntry, SQLComparisonOperator.Equal, agreementCodeInImportEntry)
				};
				var codeList = ZZRefCusCodeListCombined.Loader.Load(factory, ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRTariffAgreementCode, date, attributeFilter);
				return codeList.FirstOrDefault()?.ZZD_Code ?? ZString.Empty;
			});
		}
	}
}
