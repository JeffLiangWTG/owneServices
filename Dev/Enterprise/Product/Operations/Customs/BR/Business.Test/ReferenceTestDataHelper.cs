using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ECC = Enterprise.Core.Constants;

namespace Enterprise.Customs.BR.Business.Testing
{
	public static class ReferenceTestDataHelper
	{
		public static void CreateRefCusProcedureForExport(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateRefCusProcedure("BR", "099", "80001", ZString.Empty, ZString.Empty, ZString.Empty, Common.BR.BRJobMessageTypeList.Codes.Export);
			helper.CreateRefCusProcedure("BR", "099", "80002", ZString.Empty, ZString.Empty, ZString.Empty, Common.BR.BRJobMessageTypeList.Codes.Export);
			helper.CreateRefCusProcedure("AU", "099", "80003", ZString.Empty, ZString.Empty, ZString.Empty, Common.BR.BRJobMessageTypeList.Codes.Export);

			factory.Save();
		}

		public static void CreateCustomsOfficeCodes(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office Code");
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CO00001", "Customs Office Test1", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CO00002", "Customs Office Test2", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			factory.Save();
		}

		public static void CreateCustomsEnclosureCodes(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(ECC.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Customs Enclosure");
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "CE00001", "Customs Enclosure Test1", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "CE00002", "Customs Enclosure Test2", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			factory.Save();
		}

		public static void CreateWarehousingSectorsCodes(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);

			helper.CreateNewOrGetExistingCusCodeType(ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRWarehousingSectorsCode, "Warehousing Sectors");

			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRWarehousingSectorsCode, "CO00001;CE00001;001", "Warehousing Sector Test1", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRWarehousingSectorsCode, "CO00002;CE00002;002", "Warehousing Sector Test2", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRWarehousingSectorsCode, "CO00001;CE00002;003", "Warehousing Sector Test3", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRWarehousingSectorsCode, "CO00002;CE00001;004", "Warehousing Sector Test4", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRWarehousingSectorsCode, "CO00001;CE00001;005", "Warehousing Sector Test5", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRWarehousingSectorsCode, "006", "Incorrect format code", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));

			factory.Save();
		}

		public static void CreateRefCusCodeListMISCC_BNKTestData(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);

			helper.CreateNewOrGetExistingCusCodeType(ECC.Customs.Universal.RefCusCodeListTypes.Codes.ReasonType, "Reason for Importing without Exchange Hedge", ECC.CountryCodes.Brazil);
			helper.CreateNewOrGetExistingCusCodeType(ECC.Customs.Universal.RefCusCodeListTypes.Codes.BankCode, "Bank Code", ECC.CountryCodes.Brazil);
			helper.CreateNewOrGetExistingCusCodeType(ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRExchangeHedgePaymentCode, "Exchange Hedge Method of Payment", ECC.CountryCodes.Brazil);
			factory.Save();

			//Reason type
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.ReasonType, "30", "INVESTIMENTO DE CAPITAL ESTRANGEIRO", new ZDateTime(1996, 03, 15), ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.ReasonType, "31", "OPERACOES DE DOMICILIADOS NO EXTERIOR", new ZDateTime(1996, 03, 15), ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.ReasonType, "99", "OUTRAS IMPORTACOES SEM COBERTURA CAMBIAL", new ZDateTime(1996, 03, 15), ZDateTime.MaxSmallDateTimeValue);
			//Bank Code
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BankCode, "1", "AID - AGENCY FOR INTERNATIONAL DEVELOPMENT (AGENCIA PARA O DESENVOLVIMENTO INTERNACIONAL) - ESTADOS UNIDOS", new ZDateTime(1996, 03, 15), ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BankCode, "3", "BAD - BANQUE AFRICAINE DE DEVELOPMENT (BANCO AFRICANO DE DESENVOLVIMENTO)", new ZDateTime(1996, 03, 15), ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BankCode, "4", "BID - INTER-AMERICAN DEVELOPMENT BANK (BANCO INTERAMERICANO DE DESENVOLVIMENTO)", new ZDateTime(1996, 03, 15), ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BankCode, "99", "OUTROS NAO ESPECIFICADOS", new ZDateTime(1996, 03, 15), ZDateTime.MaxSmallDateTimeValue);
			//Payment Method
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRExchangeHedgePaymentCode, "60", "PAGAMENTO ANTECIPADO TOTAL OU PREPONDERANTE", new ZDateTime(1996, 03, 15), ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRExchangeHedgePaymentCode, "70", "PAGAMENTO A VISTA TOTAL OU PREPONDERANTE - CARTA DE CREDITO", new ZDateTime(1996, 03, 15), ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRExchangeHedgePaymentCode, "99", "PAGAMENTO A VISTA TOTAL OU PREPONDERANTE - OUTROS", new ZDateTime(1996, 03, 15), ZDateTime.MaxSmallDateTimeValue);
			factory.Save();
		}

		public static void CreateReferenceDataForTariffAgreementCode(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BRTariffAgreementCode, "Tariff Agreement");

			var codelist = helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BRTariffAgreementCode, "ASGPC", "ASGPC", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10));
			helper.CreateNewOrGetExistingCusCodeListAttribute(codelist.PK, Constants.RefCusCodeList.Attributes.Type, Constants.TariffAgreementTypes.SGPC);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codelist.PK, Constants.RefCusCodeList.Attributes.LegalActInImportEntry, "DEC/EXEC 5106/2004");

			codelist = helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BRTariffAgreementCode, "CO99", "CO99", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10));
			helper.CreateNewOrGetExistingCusCodeListAttribute(codelist.PK, Constants.RefCusCodeList.Attributes.Type, Constants.TariffAgreementTypes.OMC);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codelist.PK, Constants.RefCusCodeList.Attributes.LegalActInImportEntry, "386");

			codelist = helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BRTariffAgreementCode, "MX99", "MX99", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10));
			helper.CreateNewOrGetExistingCusCodeListAttribute(codelist.PK, Constants.RefCusCodeList.Attributes.Type, Constants.TariffAgreementTypes.Aladi);
			helper.CreateNewOrGetExistingCusCodeListAttribute(codelist.PK, Constants.RefCusCodeList.Attributes.AgreementCodeInImportEntry, "336");
			helper.CreateNewOrGetExistingCusCodeListAttribute(codelist.PK, Constants.RefCusCodeList.Attributes.LegalActInImportEntry, "Protocolo Aladi 5902");
			helper.CreateNewOrGetExistingCusCodeListAttribute(codelist.PK, Constants.RefCusCodeList.Attributes.AgreementCodeInImportEntry, "336");

			codelist = helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.BRTariffAgreementCode, "AR99", "AR99", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10));
			helper.CreateNewOrGetExistingCusCodeListAttribute(codelist.PK, Constants.RefCusCodeList.Attributes.Type, "XXXX");
			factory.Save();
		}

		public static void CreatePreferenceViewForPrimaryPreferenceList(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreatePreferenceView(Constants.RatePreferenceType.Normal, "General Rate", ECC.CountryCodes.Brazil);
			helper.CreatePreferenceView(Constants.RatePreferenceType.ExTariff, "Ex Tariff", ECC.CountryCodes.Brazil);
			helper.CreatePreferenceView(Constants.RatePreferenceType.FreeTradeAgreement, "Free Trade Agreeent Rate", ECC.CountryCodes.Brazil);
			helper.CreatePreferenceView(Constants.RatePreferenceType.ReducedRate, "Reduced Rate", ECC.CountryCodes.Brazil);
			helper.CreatePreferenceView(Constants.RatePreferenceType.ReductionMargin, "Reduction (Margin)", ECC.CountryCodes.Brazil);
			factory.Save();
		}

		public static void CreateNCMTETariffBRCharacteristic(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(ECC.CountryCodes.Brazil, "HSN");
			var tariff = helper.LoadOrCreateNewTariff(ECC.CountryCodes.Brazil, tariffType.PK, "56049000", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NCMTE, "ATT_2557", Universal.Constants.ProfileQuestion.AnswerDataTypes.List);

			tariff = helper.LoadOrCreateNewTariff(ECC.CountryCodes.Brazil, tariffType.PK, "99999999", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));

			var att2558 = helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NCMTE, "ATT_2558", Universal.Constants.ProfileQuestion.AnswerDataTypes.String);
			att2558.ZB1_IsMandatory = true;
			att2558.ZB1_IsExport = true;
			att2558.ZB1_IsImport = false;

			var att2559 = helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NCMTE, "ATT_2559", Universal.Constants.ProfileQuestion.AnswerDataTypes.String);
			att2559.ZB1_IsMandatory = true;
			att2559.ZB1_IsExport = true;
			att2559.ZB1_IsImport = false;

			var att2560 = helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NCMTE, "ATT_2560", Universal.Constants.ProfileQuestion.AnswerDataTypes.String);
			att2560.ZB1_IsMandatory = true;
			att2560.ZB1_IsExport = false;
			att2560.ZB1_IsImport = true;

			factory.Save();
		}

		public static void CreateNVETariffBRCharacteristic(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(ECC.CountryCodes.Brazil, "HSN");
			var tariff = helper.LoadOrCreateNewTariff(ECC.CountryCodes.Brazil, tariffType.PK, "56049000", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NVE, "AA", Universal.Constants.ProfileQuestion.AnswerDataTypes.List).ZB1_IsMandatory = true;

			var valuesList = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("0001","Ductil com nodularidade até 80%"),
				new KeyValuePair<string, string>("9999","Outros"),
			};

			tariff = helper.LoadOrCreateNewTariff(ECC.CountryCodes.Brazil, tariffType.PK, "11111111", ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10));
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NVE, "BA", Universal.Constants.ProfileQuestion.AnswerDataTypes.List, values: valuesList).ZB1_IsMandatory = true;
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.NVE, "BB", Universal.Constants.ProfileQuestion.AnswerDataTypes.List).ZB1_IsMandatory = true;
			helper.LoadOrCreateNewTariff(ECC.CountryCodes.Brazil, tariffType.PK, "00000000", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));

			factory.Save();
		}

		public static void CreateLPCTTariffBRCharacteristic(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(ECC.CountryCodes.Brazil, "HSN");
			var tariff = helper.LoadOrCreateNewTariff(ECC.CountryCodes.Brazil, tariffType.PK, "56049000", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.LPCT, "ATT_4507", Universal.Constants.ProfileQuestion.AnswerDataTypes.List);

			tariff = helper.LoadOrCreateNewTariff(ECC.CountryCodes.Brazil, tariffType.PK, "99999999", ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5));
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.LPCT, "ATT_4508", Universal.Constants.ProfileQuestion.AnswerDataTypes.String).ZB1_IsMandatory = true;
			helper.CreateRefCusTariffBRCharacteristic(tariff, Constants.Profile.Types.LPCT, "ATT_4509", Universal.Constants.ProfileQuestion.AnswerDataTypes.String).ZB1_IsMandatory = true;

			factory.Save();
		}

		public static void CreateSiscomexUsageEntryFees(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);

			var startDate = new ZDateTime(2019, 1, 1);
			var endDate = new ZDateTime(2079, 6, 6);

			helper.CreateRefCusTaxOrFeeType(ECC.Customs.Universal.RefCusTaxOrFee.Types.SiscomexUsageEntryFee, "Siscomex Usage Entry Fee");
			factory.Save();
			var taxOrFee1 = helper.CreateTaxOrFee("ENT1", 38.56m, ECC.CountryCodes.Brazil, 115.67m, 192.79m, ECC.Customs.Universal.RefCusTaxOrFee.Types.SiscomexUsageEntryFee,
				startDate, endDate, "Siscomex Usage Entry Fee. 115.67 Plus 38.56 per addition up to 2 Additions");
			taxOrFee1.ZZF_Threshold = 2;

			var taxOrFee2 = helper.CreateTaxOrFee("ENT2", 30.85m, ECC.CountryCodes.Brazil, 192.79m, 285.34m, ECC.Customs.Universal.RefCusTaxOrFee.Types.SiscomexUsageEntryFee,
				startDate, endDate, "Siscomex Usage Entry Fee. 192.79 Plus 30.85 per addition from 3 to 5 Additions");
			taxOrFee2.ZZF_Threshold = 5;

			var taxOrFee3 = helper.CreateTaxOrFee("ENT3", 23.14m, ECC.CountryCodes.Brazil, 285.34m, 401.04m, ECC.Customs.Universal.RefCusTaxOrFee.Types.SiscomexUsageEntryFee,
				startDate, endDate, "Siscomex Usage Entry Fee. 285.34 Plus 23.14 per addition from 6 to 10 Additions");
			taxOrFee3.ZZF_Threshold = 10;

			var taxOrFee4 = helper.CreateTaxOrFee("ENT4", 15.42m, ECC.CountryCodes.Brazil, 401.04m, 555.24m, ECC.Customs.Universal.RefCusTaxOrFee.Types.SiscomexUsageEntryFee,
				startDate, endDate, "Siscomex Usage Entry Fee. 401.04 Plus 15.42 per addition from 11 to 29 Additions");
			taxOrFee4.ZZF_Threshold = 20;

			var taxOrFee5 = helper.CreateTaxOrFee("ENT5", 7.71m, ECC.CountryCodes.Brazil, 555.24m, 786.54m, ECC.Customs.Universal.RefCusTaxOrFee.Types.SiscomexUsageEntryFee,
				startDate, endDate, "Siscomex Usage Entry Fee. 555.24 Plus 7.71 per addition from 21 to 50 Additions");
			taxOrFee5.ZZF_Threshold = 50;

			var taxOrFee6 = helper.CreateTaxOrFee("ENT6", 3.86m, ECC.CountryCodes.Brazil, 786.54m, 39189.68m, ECC.Customs.Universal.RefCusTaxOrFee.Types.SiscomexUsageEntryFee,
				startDate, endDate, "Siscomex Usage Entry Fee. 786.54 Plus 3.86 per addition from 50 to 9999 Additions");
			taxOrFee6.ZZF_Threshold = 9999;

			factory.Save();
		}

		public static void CreateAfrmmTaxes(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);

			helper.CreateRefCusTaxOrFeeType(ECC.Customs.Universal.RefCusTaxOrFee.Types.AfrmmTax, "Freight Surcharge for Renewal of the Merchant Marine (AFRMM)");
			factory.Save();
			var taxOrFee1 = helper.CreateTaxOrFee("FMM1", 10m, "BR", 0, 0, Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.AfrmmTax, ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5), "'AFRMM tax for long haul navigations");
			taxOrFee1.ZZF_Threshold = 2;

			var taxOrFee2 = helper.CreateTaxOrFee("FMM2", 11m, "BR", 0, 0, Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.AfrmmTax, ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(-1), "'AFRMM 2 tax for long haul navigations");
			taxOrFee2.ZZF_Threshold = 5;

			var taxOrFee3 = helper.CreateTaxOrFee("FMM3", 12m, "BR", 0, 0, Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.AfrmmTax, ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(-1), "'AFRMM 3 tax for long haul navigations");
			taxOrFee3.ZZF_Threshold = 10;

			var taxOrFee4 = helper.CreateTaxOrFee("FMM4", 16m, "BR", 0, 0, Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.AfrmmTax, ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5), "'AFRMM 4 tax for long haul navigations");
			taxOrFee4.ZZF_Threshold = 20;

			var taxOrFee5 = helper.CreateTaxOrFee("FMM5", 19m, "BR", 0, 0, Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.AfrmmTax, ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(-1), "'AFRMM 5 tax for long haul navigations");
			taxOrFee5.ZZF_Threshold = 50;

			var taxOrFee6 = helper.CreateTaxOrFee("FMM6", 23m, "BR", 0, 0, Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.AfrmmTax, ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(-1), "'AFRMM 6 tax for long haul navigations");
			taxOrFee6.ZZF_Threshold = 9999;

			var taxOrFee7 = helper.CreateTaxOrFee("TSUM", 10m, "BR", 0, 0, Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.AfrmmTax, ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(-1), "'TSUM tax for long haul navigations");
			taxOrFee7.ZZF_Threshold = 9999;

			factory.Save();
		}

		public static void CreateTariffAndRates(BusinessObjectFactory factory, string tariffCode, decimal rateValue,
			string tariffType = Universal.Constants.TariffTypes.HarmonizedSystem,
			string rateType = Universal.Constants.RateTypes.Duty,
			string rateCode = Constants.RateCodes.ImportDuty,
			string preference = null, string tradeGroupCountry = "CA",
			string customsValueFormula = null)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var dataGrouping = ECC.CountryCodes.Brazil;
			var refTariffType = helper.CreateNewOrGetExistingTariffType(dataGrouping, tariffType);
			var refRateType = helper.CreateNewOrGetExistingRateType(dataGrouping, rateType, rateType);

			if (!string.IsNullOrEmpty(customsValueFormula))
			{
				refRateType.ZZR_CustomsValueFormula = customsValueFormula;
			}

			var refRateCode = helper.LoadOrCreateNewCusRateCode(factory, rateCode, refRateType.PK);
			var tradeGroup = helper.LoadOrCreateTradeGroup(dataGrouping, "ALL", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			if (!tradeGroup.TradeGroupCountries.Any(x => x.ZZB_RN_NKTradeGroupCountryCode == tradeGroupCountry))
			{
				helper.AddCountry(tradeGroup, tradeGroupCountry, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			}

			var preferencePK = ZGuid.Empty;
			if (!string.IsNullOrEmpty(preference))
			{
				preferencePK = helper.CreatePreferenceForCountry(preference, preference, dataGrouping).PK;
			}
			factory.Save();

			var tariff = helper.LoadOrCreateNewTariff(dataGrouping, refTariffType.PK, tariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rate = helper.CreateRate(tariff, refRateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, $"VFD * {rateValue}/100", preferencePK, rateValue.ToString(), dataGrouping);
			helper.CreateCusApplicability(rate, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

			factory.Save();
		}

		public static void CreateRefCusCodeList(BusinessObjectFactory factory, KeyValuePair<string, string> typeAndDescription, IList<KeyValuePair<string, string>> codesAndDescriptions)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(typeAndDescription.Key, typeAndDescription.Value);
			foreach (var code in codesAndDescriptions)
			{
				helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, typeAndDescription.Key, code.Key, code.Value, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			}
			factory.Save();
		}

		public static void CreateRefCusMap(BusinessObjectFactory factory, ZString mapType, ZString mapTypeDescrition, List<KeyValuePair<string, string>> cw1ToCustomsList, string direction = "BTH")
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateCusMapType(mapType, direction, mapTypeDescrition, true);
			foreach (var item in cw1ToCustomsList)
			{
				helper.CreateCusMap(mapType, item.Key, item.Value, ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10), ECC.CountryCodes.Brazil);
			}
			factory.Save();
		}

		public static void CreateReferenceDataForTaxRegimeList(BusinessObjectFactory factory, string shipmentType = null)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);

			var testCusProcedure = factory.New<RefCusProcedure>();
			testCusProcedure.ZZ6_ProcedureCode = "01";
			testCusProcedure.ZZ6_ZZZ_NKDataGrouping = ECC.CountryCodes.Brazil;
			testCusProcedure.ZZ6_PreviousProcedureCode = "1";
			testCusProcedure.ZZ6_Description = "Test1";
			testCusProcedure.ZZ6_ShipmentType = shipmentType ?? BRJobMessageTypeList.Codes.ImportSiscomex;
			testCusProcedure.ZZ6_StartDate = ZDateTime.Now.AddDays(-5);
			testCusProcedure.ZZ6_EndDate = ZDateTime.Now.AddDays(5);
			testCusProcedure.ZZ6_Category = ChargeTypesList.Codes.DTY;

			var testCusProcedure1 = factory.New<RefCusProcedure>();
			testCusProcedure1.ZZ6_ProcedureCode = "01";
			testCusProcedure1.ZZ6_PreviousProcedureCode = "2";
			testCusProcedure1.ZZ6_Description = "Test2";
			testCusProcedure1.ZZ6_ShipmentType = shipmentType ?? BRJobMessageTypeList.Codes.ImportSiscomex;
			testCusProcedure1.ZZ6_ZZZ_NKDataGrouping = ECC.CountryCodes.Brazil;
			testCusProcedure1.ZZ6_StartDate = ZDateTime.Now.AddDays(-5);
			testCusProcedure1.ZZ6_EndDate = ZDateTime.Now.AddDays(5);
			testCusProcedure1.ZZ6_Category = ChargeTypesList.Codes.DTY;

			var testCusProcedure2 = factory.New<RefCusProcedure>();
			testCusProcedure2.ZZ6_ProcedureCode = "01";
			testCusProcedure2.ZZ6_PreviousProcedureCode = "3";
			testCusProcedure2.ZZ6_Description = "Test3";
			testCusProcedure2.ZZ6_ShipmentType = shipmentType ?? BRJobMessageTypeList.Codes.ImportSiscomex;
			testCusProcedure2.ZZ6_ZZZ_NKDataGrouping = ECC.CountryCodes.Brazil;
			testCusProcedure2.ZZ6_StartDate = ZDateTime.Now.AddDays(-5);
			testCusProcedure2.ZZ6_EndDate = ZDateTime.Now.AddDays(5);
			testCusProcedure2.ZZ6_Category = ChargeTypesList.Codes.DTY;

			var testCusProcedure3 = factory.New<RefCusProcedure>();
			testCusProcedure3.ZZ6_ProcedureCode = "01";
			testCusProcedure3.ZZ6_ZZZ_NKDataGrouping = ECC.CountryCodes.Brazil;
			testCusProcedure3.ZZ6_PreviousProcedureCode = "1";
			testCusProcedure3.ZZ6_Description = "Test4";
			testCusProcedure3.ZZ6_ShipmentType = shipmentType ?? BRJobMessageTypeList.Codes.ImportSiscomex;
			testCusProcedure3.ZZ6_StartDate = ZDateTime.Now.AddDays(-5);
			testCusProcedure3.ZZ6_EndDate = ZDateTime.Now.AddDays(5);
			testCusProcedure3.ZZ6_Category = Constants.RateTypes.PIS;

			var testCusProcedure4 = factory.New<RefCusProcedure>();
			testCusProcedure4.ZZ6_ProcedureCode = "01";
			testCusProcedure4.ZZ6_PreviousProcedureCode = "2";
			testCusProcedure4.ZZ6_Description = "Test5";
			testCusProcedure4.ZZ6_ShipmentType = shipmentType ?? BRJobMessageTypeList.Codes.ImportSiscomex;
			testCusProcedure4.ZZ6_ZZZ_NKDataGrouping = ECC.CountryCodes.Brazil;
			testCusProcedure4.ZZ6_StartDate = ZDateTime.Now.AddDays(-5);
			testCusProcedure4.ZZ6_EndDate = ZDateTime.Now.AddDays(5);
			testCusProcedure4.ZZ6_Category = Constants.RateTypes.PIS;

			var testCusProcedure5 = factory.New<RefCusProcedure>();
			testCusProcedure5.ZZ6_ProcedureCode = "01";
			testCusProcedure5.ZZ6_PreviousProcedureCode = "2";
			testCusProcedure5.ZZ6_Description = "Test5";
			testCusProcedure5.ZZ6_ShipmentType = shipmentType ?? BRJobMessageTypeList.Codes.ImportSiscomex;
			testCusProcedure5.ZZ6_ZZZ_NKDataGrouping = ECC.CountryCodes.Brazil;
			testCusProcedure5.ZZ6_StartDate = new ZDateTime(2022, 1, 1);
			testCusProcedure5.ZZ6_EndDate = new ZDateTime(2022, 12, 31);
			testCusProcedure5.ZZ6_Category = Constants.RateTypes.IPI;

			var testCusProcedure6 = factory.New<RefCusProcedure>();
			testCusProcedure6.ZZ6_ProcedureCode = "01";
			testCusProcedure6.ZZ6_PreviousProcedureCode = "2";
			testCusProcedure6.ZZ6_Description = "Test5";
			testCusProcedure6.ZZ6_ShipmentType = shipmentType ?? BRJobMessageTypeList.Codes.ImportSiscomex;
			testCusProcedure6.ZZ6_ZZZ_NKDataGrouping = ECC.CountryCodes.Brazil;
			testCusProcedure6.ZZ6_StartDate = new ZDateTime(2022, 1, 1);
			testCusProcedure6.ZZ6_EndDate = new ZDateTime(2022, 12, 31);
			testCusProcedure6.ZZ6_Category = Constants.RateTypes.Cofins;

			var testCusProcedure7 = factory.New<RefCusProcedure>();
			testCusProcedure7.ZZ6_ProcedureCode = "01";
			testCusProcedure7.ZZ6_PreviousProcedureCode = "2";
			testCusProcedure7.ZZ6_Description = "Test5";
			testCusProcedure7.ZZ6_ShipmentType = shipmentType ?? BRJobMessageTypeList.Codes.ImportSiscomex;
			testCusProcedure7.ZZ6_ZZZ_NKDataGrouping = ECC.CountryCodes.Brazil;
			testCusProcedure7.ZZ6_StartDate = new ZDateTime(2022, 1, 1);
			testCusProcedure7.ZZ6_EndDate = new ZDateTime(2022, 12, 31);
			testCusProcedure7.ZZ6_Category = Constants.RateTypes.ICMS;

			helper.CreateNewOrGetExistingCusCodeType(ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRTaxationRegimeCode, "BR Taxation Regime");
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRTaxationRegimeCode, "1", "RECOLHIMENTO INTEGRAL", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRTaxationRegimeCode, "2", "IMUNIDADE", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRTaxationRegimeCode, "3", "ISENCAO", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));

			factory.Save();
		}

		public static void CreateReferenceDataForDutyLegalBaseList(BusinessObjectFactory factory, string taxRegimeCode)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);

			var testCusProcedure1 = factory.New<RefCusProcedure>();
			testCusProcedure1.ZZ6_ProcedureCode = MessageSubTypeList.Codes._01;
			testCusProcedure1.ZZ6_PreviousProcedureCode = taxRegimeCode;
			testCusProcedure1.ZZ6_Concession = "01";
			testCusProcedure1.ZZ6_ShipmentType = BRJobMessageTypeList.Codes.ImportSiscomex;
			testCusProcedure1.ZZ6_ZZZ_NKDataGrouping = ECC.CountryCodes.Brazil;
			testCusProcedure1.ZZ6_StartDate = ZDateTime.Now.AddDays(-5);
			testCusProcedure1.ZZ6_EndDate = ZDateTime.Now.AddDays(5);
			testCusProcedure1.ZZ6_Description = "Test";
			testCusProcedure1.ZZ6_Category = Constants.ProcedureCategories.Duty;

			var testCusProcedure2 = factory.New<RefCusProcedure>();
			testCusProcedure2.ZZ6_ProcedureCode = MessageSubTypeList.Codes._01;
			testCusProcedure2.ZZ6_PreviousProcedureCode = taxRegimeCode;
			testCusProcedure2.ZZ6_Concession = "02";
			testCusProcedure2.ZZ6_ShipmentType = BRJobMessageTypeList.Codes.ImportSiscomex;
			testCusProcedure2.ZZ6_ZZZ_NKDataGrouping = ECC.CountryCodes.Brazil;
			testCusProcedure2.ZZ6_StartDate = ZDateTime.Now.AddDays(-5);
			testCusProcedure2.ZZ6_EndDate = ZDateTime.Now.AddDays(5);
			testCusProcedure2.ZZ6_Description = "Test";
			testCusProcedure1.ZZ6_Category = Constants.ProcedureCategories.Duty;

			helper.CreateNewOrGetExistingCusCodeType(ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRLegalBaseCode, "BR Legal Base Regime");
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRLegalBaseCode, "01", "LIVROS, JORNAIS E PERIODICOS - CF/88, ART.150,VI,D - LEI 8032/90, ART.2,II,A - LEI 8402/92, ART 1,IV", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRLegalBaseCode, "02", "PAPEL DEST. A IMPRESSAO D/LIVROS, JORNAIS E PERIODICOS - CF/88,ART.150,VI,D - L. 8032/90,ART.2,II,A - L. 8402/92,ART.1,IV", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));

			factory.Save();
		}

		public static void CreateReferenceDataForPISLegalBaseList(BusinessObjectFactory factory, string taxRegimeCode)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);

			var testCusProcedure1 = factory.New<RefCusProcedure>();
			testCusProcedure1.ZZ6_ProcedureCode = MessageSubTypeList.Codes._01;
			testCusProcedure1.ZZ6_PreviousProcedureCode = taxRegimeCode;
			testCusProcedure1.ZZ6_Concession = "01";
			testCusProcedure1.ZZ6_ShipmentType = BRJobMessageTypeList.Codes.ImportSiscomex;
			testCusProcedure1.ZZ6_ZZZ_NKDataGrouping = ECC.CountryCodes.Brazil;
			testCusProcedure1.ZZ6_StartDate = ZDateTime.Now.AddDays(-5);
			testCusProcedure1.ZZ6_EndDate = ZDateTime.Now.AddDays(5);
			testCusProcedure1.ZZ6_Description = "Test";
			testCusProcedure1.ZZ6_Category = Constants.ProcedureCategories.PisCofins;

			var testCusProcedure2 = factory.New<RefCusProcedure>();
			testCusProcedure2.ZZ6_ProcedureCode = MessageSubTypeList.Codes._01;
			testCusProcedure2.ZZ6_PreviousProcedureCode = taxRegimeCode;
			testCusProcedure2.ZZ6_Concession = "02";
			testCusProcedure2.ZZ6_ShipmentType = BRJobMessageTypeList.Codes.ImportSiscomex;
			testCusProcedure2.ZZ6_ZZZ_NKDataGrouping = ECC.CountryCodes.Brazil;
			testCusProcedure2.ZZ6_StartDate = ZDateTime.Now.AddDays(-5);
			testCusProcedure2.ZZ6_EndDate = ZDateTime.Now.AddDays(5);
			testCusProcedure2.ZZ6_Description = "Test";
			testCusProcedure2.ZZ6_Category = Constants.ProcedureCategories.PisCofins;

			helper.CreateNewOrGetExistingCusCodeType(ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRPisLegalBaseCode, "BR Pis Legal Base Regime");
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRPisLegalBaseCode, "01", "PIS LIVROS, JORNAIS E PERIODICOS - CF/88, ART.150,VI,D - LEI 8032/90, ART.2,II,A - LEI 8402/92, ART 1,IV", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRPisLegalBaseCode, "02", "PIS PAPEL DEST. A IMPRESSAO D/LIVROS, JORNAIS E PERIODICOS - CF/88,ART.150,VI,D - L. 8032/90,ART.2,II,A - L. 8402/92,ART.1,IV", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));

			factory.Save();
		}

		public static void CreateReferenceDataForICMSLegalBaseList(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRLegalBaseICMS, "ICMS Legal Base");
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRLegalBaseICMS, "01", "ICMS Legal Base 01", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRLegalBaseICMS, "02", "ICMS Legal Base 02", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRLegalBaseICMS, "03", "ICMS Legal Base 03", ZDateTime.Now.AddDays(-5), ZDateTime.Now.AddDays(-1));
			factory.Save();
		}

		public static void CreateReferenceDataForICMSTaxRegimeList(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);

			helper.CreateNewOrGetExistingCusCodeType(ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRTaxRegimeICMS, "ICMS Tax Regime", ECC.CountryCodes.Brazil);
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRTaxRegimeICMS, "1", "Full Collection", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRTaxRegimeICMS, "2", "Immunity", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRTaxRegimeICMS, "3", "Exemption", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRTaxRegimeICMS, "4", "Reduction", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRTaxRegimeICMS, "5", "Suspension", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRTaxRegimeICMS, "6", "Non-incident", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRTaxRegimeICMS, "7", "Deferred", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRTaxRegimeICMS, "8", "Exonerated", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRTaxRegimeICMS, "9", "Non-taxable", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			factory.Save();
		}

		public static void CreateReferenceDataForIPITaxRegimeList(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);

			helper.CreateNewOrGetExistingCusCodeType(ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRTaxRegimeIPI, "IPI Tax Regime", ECC.CountryCodes.Brazil);
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRTaxRegimeIPI, "1", "Exemption", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRTaxRegimeIPI, "2", "Reduction", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRTaxRegimeIPI, "3", "Nontaxable", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRTaxRegimeIPI, "4", "Full Collection", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRTaxRegimeIPI, "5", "Suspension", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			factory.Save();
		}

		public static void CreateReferenceDataForILFFeeTypeList(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);

			helper.CreateRefCusTaxOrFeeType(ECC.Customs.Universal.RefCusTaxOrFee.Types.ImportLicenseFine, "Import License Fine");
			factory.Save();

			var taxOrFee1 = helper.CreateTaxOrFee("F1ND", 0, "BR", 0, 0, Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.ImportLicenseFine, ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5), "Import License fine without discount for goods shipped before the approval date");
			taxOrFee1.ZZF_Value = 30;
			taxOrFee1.ZZF_Minimum = 500;
			taxOrFee1.ZZF_Maximum = 5000;

			var taxOrFee2 = helper.CreateTaxOrFee("F1D5", 0, "BR", 0, 0, Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.ImportLicenseFine, ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5), "Import License fine with 50% discount for goods shipped before the approval date");
			taxOrFee2.ZZF_Value = 15;
			taxOrFee2.ZZF_Minimum = 250;
			taxOrFee2.ZZF_Maximum = 2500;

			factory.Save();
		}

		public static void CreateTaxRevenueTypeList(BusinessObjectFactory factory)
		{
			CreateRefCusMap(factory, RefCusMapTypeList.Codes.RCODE, "Tax Revenue Code", TaxRevenueCodes, "OUT");
		}

		public static List<KeyValuePair<string, string>> TaxRevenueCodes => new List<KeyValuePair<string, string>>
		{
				new KeyValuePair<string, string>("DTY", "0086"),
				new KeyValuePair<string, string>("IPI", "1038"),
				new KeyValuePair<string, string>("F1ND", "5149"),
				new KeyValuePair<string, string>("F1D5", "2185"),
				new KeyValuePair<string, string>("SUF", "7811"),
				new KeyValuePair<string, string>("COF", "5629"),
				new KeyValuePair<string, string>("PIS", "5602"),
				new KeyValuePair<string, string>("ADD", "5529")
		};

		public static void CreateEntryStatusForLicenseAndExportList(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);

			helper.CreateNewOrGetExistingCusCodeType(ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status", ECC.CountryCodes.Brazil);
			var attributeName = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("IUpdateReleaseDate", "Status requires that customs release date is updated", ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus,
				ECC.CountryCodes.Brazil, allowDuplicates: true);
			attributeName.ZXE_ColumnCaption = "Update Release Date";

			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "L01", "For Analysis", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "L02", "Under Analysis", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "L03", "On Demand", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "L04", "Dismissed", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var codeListPk = helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "L05", "Deferred", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue).PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListPk, attributeName.ZXE_Name, "");

			codeListPk = helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "L06", "Judicially Deferred", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue).PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListPk, attributeName.ZXE_Name, "");

			codeListPk = helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "L07", "Deferred Linked to DI", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue).PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListPk, attributeName.ZXE_Name, "");

			codeListPk = helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "L08", "Judicially Deferred Linked to DI", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue).PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListPk, attributeName.ZXE_Name, "");

			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "L09", "Deferred Reserved", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			codeListPk = helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "L10", "Deferred Judicially Reserved", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue).PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListPk, attributeName.ZXE_Name, "");

			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "L15", "Authorized boarding", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "L16", "Expired", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "L17", "Cleared", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "L18", "Canceled", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "E10", "Registered", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "E11", "In Progress", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "I31", "In Conference", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			factory.Save();
		}

		public static void CreateEntryStatusForISWList(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);

			helper.CreateNewOrGetExistingCusCodeType(ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status", ECC.CountryCodes.Brazil);

			var attributeName = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("IUpdateReleaseDate", "Status requires that customs release date is updated", ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus,
				ECC.CountryCodes.Brazil, allowDuplicates: true);
			attributeName.ZXE_ColumnCaption = "Update Release Date";

			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "S01", "Autorização de entrega com ou sem prosseguimento do despacho", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var codeListPk = helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "S02", "Desembaraçada", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue).PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListPk, attributeName.ZXE_Name, "");

			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "S03", "Aguardando desembaraço", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "S04", "Aguardando distribuição", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "S05", "Despacho interrompido", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "S06", "Declaração em análise", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			factory.Save();
		}

		public static void CreateEntryStatusForExport(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);

			helper.CreateNewOrGetExistingCusCodeType(ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status", ECC.CountryCodes.Brazil);
			var attributeName = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("CustomsCode", "Customs Status Received from Brazil Customs", ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus,
				ECC.CountryCodes.Brazil, allowDuplicates: true);

			var codeListPk = helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "E01", "In preparation", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue).PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListPk, attributeName.ZXE_Name, "EM_ELABORACAO");

			codeListPk = helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "E10", "Registered", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue).PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListPk, attributeName.ZXE_Name, "REGISTRADA");

			codeListPk = helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "E70", "Endorsed", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue).PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListPk, attributeName.ZXE_Name, "AVERBADA_SEM_DIVERGENCIA");
		}

		public static void CreateEntryStatusForImport(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);

			helper.CreateNewOrGetExistingCusCodeType(ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status", ECC.CountryCodes.Brazil);
			var attributeName = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("CustomsCode", "Customs Status Received from Brazil Customs", ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus,
				ECC.CountryCodes.Brazil, allowDuplicates: true);

			var codeListPk = helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "I11", "Registered and Awaiting Risk Analysis Result", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue).PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListPk, attributeName.ZXE_Name, "REGISTRADA_AGUARDANDO_CANAL");

			codeListPk = helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "I31", "In Conference", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue).PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListPk, attributeName.ZXE_Name, "EM_CONFERENCIA_SELECIONADA");

			codeListPk = helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "I43", "Cleared and Cargo Delivered", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue).PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListPk, attributeName.ZXE_Name, "DESEMBARACADA_CARGA_ENTREGUE");
		}

		public static void CreateEntryStatusForLPCO(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);

			helper.CreateNewOrGetExistingCusCodeType(ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status", ECC.CountryCodes.Brazil);
			var attributeName = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("CustomsCode", "Customs Status Received from Brazil Customs", ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus,
				ECC.CountryCodes.Brazil, allowDuplicates: true);

			var codeListPk = helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "P01", "Awaiting payment", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue).PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListPk, attributeName.ZXE_Name, "PARA_ANALISE");

			codeListPk = helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "P02", "Annulled/Revoked", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue).PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListPk, attributeName.ZXE_Name, "EM_ANALISE");

			codeListPk = helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "P17", "Overdue", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue).PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListPk, attributeName.ZXE_Name, "RECURSO_DIVERSO");
		}

		public static void CreateRefCusRateCodeAndType(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);

			var rateType = helper.CreateCusRateType(ECC.CountryCodes.Brazil, Constants.RateTypes.PIS, "Social Integration Program");
			helper.CreateCusRateCode(factory, Constants.RateCodes.PIS, rateType.PK, description: "Social Integration Program");
			rateType = helper.CreateCusRateType(ECC.CountryCodes.Brazil, Constants.RateTypes.Cofins, "Contribution for Social Security Financing");
			helper.CreateCusRateCode(factory, Constants.RateCodes.Cofins, rateType.PK, description: "Contribution for Social Security Financing");
			rateType = helper.CreateCusRateType(ECC.CountryCodes.Brazil, Constants.RateTypes.Antidumping, "Anti-Dumping Duty");
			helper.CreateCusRateCode(factory, Constants.RateCodes.Antidumping, rateType.PK, description: "Anti-Dumping Duty");
			rateType = helper.CreateCusRateType(ECC.CountryCodes.Brazil, Constants.RateTypes.IPI, "IPI");
			helper.CreateCusRateCode(factory, Constants.RateCodes.IPI, rateType.PK, description: "IPI");
			rateType = helper.CreateCusRateType(ECC.CountryCodes.Brazil, Constants.RateTypes.ICMSFCP, "ICMS");
			helper.CreateCusRateCode(factory, Constants.RateCodes.ICMSFCP, rateType.PK, description: "ICMS");
			rateType = helper.CreateCusRateType(ECC.CountryCodes.Brazil, Constants.RateTypes.ImportDuty, "ImportDuty");
			helper.CreateCusRateCode(factory, Constants.RateCodes.ImportDuty, rateType.PK, description: "ImportDuty");
			rateType = helper.CreateCusRateType(ECC.CountryCodes.Canada, Constants.RateTypes.Antidumping, "Anti-Dumping Duty");
			helper.CreateCusRateCode(factory, "0089", rateType.PK, description: "Anti-Dumping Duty");

			factory.Save();
		}

		public static void CreateNCMRefCusProfileQuestions(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(ECC.CountryCodes.Brazil, "HSN");
			factory.Save();

			helper.LoadOrCreateNewTariff(ECC.CountryCodes.Brazil, tariffType.PK, "87654321", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.LoadOrCreateNewTariff(ECC.CountryCodes.Brazil, tariffType.PK, "12345678", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var profileType = helper.CreateRefCusProfileType(Constants.Profile.Types.NCM, "HSN", ECC.CountryCodes.Brazil);
			var profileType2 = helper.CreateRefCusProfileType(Constants.Profile.Types.NCMTE, "HSN", ECC.CountryCodes.Brazil);
			
			var productAttr = new[] { (Constants.ProfileQuestion.AttributeNames.Target, Constants.ProfileQuestion.AttributeValues.Product) };
			var duimpAttr = new[] { (Constants.ProfileQuestion.AttributeNames.Target, Constants.ProfileQuestion.AttributeValues.Duimp) };
			var importAttr = new[] { (Constants.Profile.AttributeNames.Modality, "IMP") };
			var exportAttr = new[] { (Constants.Profile.AttributeNames.Modality, "EXP") };

			var profile1 = helper.CreateRefCusProfile(profileType, "87654321", "ATT1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, importAttr);
			var profile2 = helper.CreateRefCusProfile(profileType, "8765432", "ATT_3891", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, exportAttr);
			var profile3 = helper.CreateRefCusProfile(profileType2, "876543", "ATT_2557", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, importAttr);
			var profile4 = helper.CreateRefCusProfile(profileType, "87654", "ATT_3061", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, exportAttr);
			var profile5 = helper.CreateRefCusProfile(profileType2, "8765", "ATT_3887", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, exportAttr);
			var profile6 = helper.CreateRefCusProfile(profileType2, "876", "ATT_3890", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, exportAttr);
			var profile7 = helper.CreateRefCusProfile(profileType2, "87", "ATT_3892", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, importAttr);
			var profile8 = helper.CreateRefCusProfile(profileType, "87", "ATT_1000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, exportAttr);
			var profile9 = helper.CreateRefCusProfile(profileType, "87654321", "ATT_4801", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, importAttr);
			var profile10 = helper.CreateRefCusProfile(profileType, "8765432", "ATT_4802", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, importAttr);
			var profile11 = helper.CreateRefCusProfile(profileType, "876543", "ATT_4803", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, importAttr);
			var profile12 = helper.CreateRefCusProfile(profileType, "87654", "ATT_4804", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, importAttr);
			var profile13 = helper.CreateRefCusProfile(profileType2, "8765", "ATT_4805", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, importAttr);
			var profile14 = helper.CreateRefCusProfile(profileType2, "12345678", "ATT_4806", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, importAttr);
			var profile15 = helper.CreateRefCusProfile(profileType2, "87", "ATT_4807", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, importAttr);
			helper.CreateRefCusProfile(profileType, "12345678", "ATT_3891", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, exportAttr);
			helper.CreateRefCusProfile(profileType2, "12345678", "ATT_3892", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, importAttr);

			helper.CreateRefCusProfileQuestion(profileType, "Test 1", "ATT1", "Text 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, note: "Oriatention01",
				attributes: productAttr);
			helper.CreateRefCusProfileQuestion(profileType, "Test 2", "ATT_3891", "Text 2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Universal.Constants.ProfileQuestion.AnswerDataTypes.List, note: "Oriatention02",
				attributes: productAttr, allowMultipleAnswers: true, answers: new[] { ("1", "desc"), ("2", "desc2"), ("3", "desc3"), ("4", "desc4"), ("5", "desc5") });
			helper.CreateRefCusProfileQuestion(profileType2, "Test 3", "ATT_2557", "Text 3", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Universal.Constants.ProfileQuestion.AnswerDataTypes.String, note: "Oriatention02",
				attributes: productAttr, allowMultipleAnswers: true, answers: new[] { ("1", "desc"), ("2", "desc2") });
			helper.CreateRefCusProfileQuestion(profileType, "Test 4", "ATT_3061", "Text 4", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Universal.Constants.ProfileQuestion.AnswerDataTypes.String, note: "Oriatention02",
				attributes: productAttr);
			helper.CreateRefCusProfileQuestion(profileType2, "Test 5", "ATT_3887", "Text 5", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Universal.Constants.ProfileQuestion.AnswerDataTypes.Number, note: "Oriatention02",
				attributes: productAttr);
			var profile3890 = helper.CreateRefCusProfileQuestion(profileType2, "Test 6", "ATT_3890", "Text 6", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Universal.Constants.ProfileQuestion.AnswerDataTypes.Boolean, note: "Oriatention02",
				attributes: productAttr);
			helper.CreateRefCusProfileQuestion(profileType, "Test 6", "ATT_3890", "Text 6", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Universal.Constants.ProfileQuestion.AnswerDataTypes.Boolean, note: "Oriatention02",
				attributes: productAttr);
			helper.CreateRefCusProfileQuestion(profileType2, "Test 7", "ATT_3892", "Text 7", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Universal.Constants.ProfileQuestion.AnswerDataTypes.Number, note: "Oriatention02", answerDecimalPlaces: 5,
				attributes: productAttr);
			var profile1000 = helper.CreateRefCusProfileQuestion(profileType, "Test 8", "ATT_1000", "Text 8", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Universal.Constants.ProfileQuestion.AnswerDataTypes.Compound, note: "Oriatention02",
				attributes: productAttr);
			var profile10001 = helper.CreateRefCusProfileQuestion(profileType, "Test 9", "ATT_10001", "Text 9", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Universal.Constants.ProfileQuestion.AnswerDataTypes.Compound, note: "Oriatention02",
				attributes: productAttr);
			var profile100011 = helper.CreateRefCusProfileQuestion(profileType, "Test 10", "ATT_100011", "Text 10", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Universal.Constants.ProfileQuestion.AnswerDataTypes.Number, note: "Oriatention02", answerDecimalPlaces: 2,
				attributes: productAttr);
			var profile100012 = helper.CreateRefCusProfileQuestion(profileType, "Test 11", "ATT_100012", "Text 11", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Universal.Constants.ProfileQuestion.AnswerDataTypes.Boolean, note: "Oriatention02",
				attributes: productAttr);
			var profile10002 = helper.CreateRefCusProfileQuestion(profileType, "Test 12", "ATT_10002", "Text 12", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Universal.Constants.ProfileQuestion.AnswerDataTypes.Date, note: "Oriatention02",
				attributes: productAttr);
			var profile38901 = helper.CreateRefCusProfileQuestion(profileType, "Test 13", "ATT_38901", "Text 13", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Universal.Constants.ProfileQuestion.AnswerDataTypes.Number, note: "Oriatention02",
				attributes: productAttr);
			var profile38902 = helper.CreateRefCusProfileQuestion(profileType, "Test 14", "ATT_38902", "Text 14", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Universal.Constants.ProfileQuestion.AnswerDataTypes.String, note: "Oriatention02",
				attributes: productAttr);
			var profile4807 = helper.CreateRefCusProfileQuestion(profileType2, "Test 15", "ATT_4807", "Text 15", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Universal.Constants.ProfileQuestion.AnswerDataTypes.String, note: "Oriatention02",
				allowMultipleAnswers: true, attributes: productAttr);

			helper.CreateRefCusProfileQuestion(profileType, "Test 15", "ATT_4801", "Text 15", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Universal.Constants.ProfileQuestion.AnswerDataTypes.String, note: "Oriatention02",
				attributes: duimpAttr);
			helper.CreateRefCusProfileQuestion(profileType, "Test 16", "ATT_4802", "Text 16", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Universal.Constants.ProfileQuestion.AnswerDataTypes.List, note: "Oriatention02",
				attributes: duimpAttr);
			var profile4803 = helper.CreateRefCusProfileQuestion(profileType, "Test 17", "ATT_4803", "Text 17", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Universal.Constants.ProfileQuestion.AnswerDataTypes.Boolean, note: "Oriatention02",
				attributes: duimpAttr);
			var profile48031 = helper.CreateRefCusProfileQuestion(profileType, "Test 18", "ATT_48031", "Text 18", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Universal.Constants.ProfileQuestion.AnswerDataTypes.String, note: "Oriatention02",
				attributes: duimpAttr);
			var profile48032 = helper.CreateRefCusProfileQuestion(profileType, "Test 19", "ATT_48032", "Text 19", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Universal.Constants.ProfileQuestion.AnswerDataTypes.String, note: "Oriatention02",
				attributes: duimpAttr);
			var profile4804 = helper.CreateRefCusProfileQuestion(profileType, "Test 20", "ATT_4804", "Text 20", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Universal.Constants.ProfileQuestion.AnswerDataTypes.Compound, note: "Oriatention02",
				attributes: duimpAttr);
			var profile48041 = helper.CreateRefCusProfileQuestion(profileType, "Test 21", "ATT_48041", "Text 21", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Universal.Constants.ProfileQuestion.AnswerDataTypes.Number, note: "Oriatention02",
				attributes: duimpAttr);
			var profile48042 = helper.CreateRefCusProfileQuestion(profileType, "Test 22", "ATT_48042", "Text 22", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Universal.Constants.ProfileQuestion.AnswerDataTypes.String, note: "Oriatention02",
				attributes: duimpAttr);
			var profile4805 = helper.CreateRefCusProfileQuestion(profileType2, "Test 23", "ATT_4805", "Text 23", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Universal.Constants.ProfileQuestion.AnswerDataTypes.String, note: "Oriatention02",
				attributes: duimpAttr);
			var profile4806 = helper.CreateRefCusProfileQuestion(profileType2, "Test 24", "ATT_4806", "Text 24", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Universal.Constants.ProfileQuestion.AnswerDataTypes.String, note: "Oriatention02",
				attributes: duimpAttr);

			helper.CreateRefCusProfileQuestionPathway(profile3890, profile38901, "[Anwser] = 'true'", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "[Anwser] = 'true'");
			helper.CreateRefCusProfileQuestionPathway(profile3890, profile38902, "[Anwser] = 'false'", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "[Anwser] = 'false'");

			helper.CreateRefCusProfileQuestionPathway(profile1000, profile10001, "ATT_1000|ATT_10001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "");
			helper.CreateRefCusProfileQuestionPathway(profile1000, profile10002, "ATT_1000|ATT_10002", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "");
			helper.CreateRefCusProfileQuestionPathway(profile10001, profile100011, "ATT_10001|ATT_100011", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "");
			helper.CreateRefCusProfileQuestionPathway(profile10001, profile100012, "ATT_10001|ATT_100012", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "");

			helper.CreateRefCusProfileQuestionPathway(profile4803, profile48031, "ATT_4803|ATT_48031", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "[Anwser] = 'true'");
			helper.CreateRefCusProfileQuestionPathway(profile4803, profile48032, "ATT_4803|ATT_48032", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "[Anwser] = 'false'");

			helper.CreateRefCusProfileQuestionPathway(profile4804, profile48041, "ATT_4804|ATT_48041", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "");
			helper.CreateRefCusProfileQuestionPathway(profile4804, profile48042, "ATT_4804|ATT_48042", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "");

			helper.CreateRefCusProfileQuestion(profileType, "Test 2", "ATT_3899", "Text 8", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Universal.Constants.ProfileQuestion.AnswerDataTypes.List, allowMultipleAnswers: true, note: "Oriatention02",
				attributes: productAttr, answers: new[] { ("1", "desc"), ("2", "desc2"), ("3", "desc3"), ("4", "desc4"), ("5", "desc5") });

			factory.Save();
		}

		public static void CreateRefCusRateType(BusinessObjectFactory factory)
		{
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;

			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping(ECC.CountryCodes.Brazil);
			factory.Save();
			helper.CreateNewOrGetExistingRateType(ECC.CountryCodes.Brazil, Universal.Constants.RateTypes.Duty, "Duty");
			helper.CreateNewOrGetExistingRateType(ECC.CountryCodes.Brazil, Constants.RateTypes.PIS, "PIS");
			helper.CreateNewOrGetExistingRateType(ECC.CountryCodes.Brazil, Constants.RateTypes.Cofins, "COFINS");
			factory.Save();
		}

		public static List<RefCusProfile> CreateTTRefCusProfileAndQuestions(BusinessObjectFactory factory)
		{
			CreateRefCusRateType(factory);
			var helper = new UniversalReferenceTestDataHelper(factory);
			var tariffType1 = helper.CreateNewOrGetExistingTariffType(ECC.CountryCodes.Brazil, "HSN", "NCM");
			factory.Save();

			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;

			helper.LoadOrCreateNewTariff(ECC.CountryCodes.Brazil, tariffType1.PK, "12345678", startDate, ZDateTime.Now.AddDays(1), "dummy Description 0");
			helper.LoadOrCreateNewTariff(ECC.CountryCodes.Brazil, tariffType1.PK, "87654321", startDate, ZDateTime.Now.AddDays(1), "dummy Description 1");
			var profileType1 = helper.CreateRefCusProfileType(tariffType1.PK, ECC.CountryCodes.Brazil, "TTDTY", "Duty");
			var profileType2 = helper.CreateRefCusProfileType(tariffType1.PK, ECC.CountryCodes.Brazil, "TTCOF", "Cofins");
			var profileType3 = helper.CreateRefCusProfileType(tariffType1.PK, ECC.CountryCodes.Brazil, "TTDTYTE", "Duty Tests");
			var profileType4 = helper.CreateRefCusProfileType(tariffType1.PK, ECC.CountryCodes.Brazil, "TTCOFTE", "Cofins Tests");
			factory.Save();

			var productAttr = new[] { (Constants.ProfileQuestion.AttributeNames.Target, Constants.ProfileQuestion.AttributeValues.Product) };
			var attributes1 = new List<(string, string)>()
			{
				(Constants.Profile.AttributeNames.LegalCode, "P01"),
				(Constants.Profile.AttributeNames.Mandatory, "True"),
				(Constants.Profile.AttributeNames.Origin, "ZA"),
				(Constants.Profile.AttributeNames.Regime, "Test 1"),
				(Constants.Profile.AttributeNames.TaxType, "DTY"),
				(Constants.Profile.AttributeNames.Modality, "IMP")
			};
			var attributes2 = new List<(string, string)>()
			{
				(Constants.Profile.AttributeNames.LegalCode, "P02"),
				(Constants.Profile.AttributeNames.Mandatory, "False"),
				(Constants.Profile.AttributeNames.Origin, "ZA"),
				(Constants.Profile.AttributeNames.Regime, "Test 2"),
				(Constants.Profile.AttributeNames.TaxType, "COF"),
				(Constants.Profile.AttributeNames.Modality, "IMP")
			};
			var attributes3 = new List<(string, string)>()
			{
				(Constants.Profile.AttributeNames.LegalCode, "P03"),
				(Constants.Profile.AttributeNames.Mandatory, "False"),
				(Constants.Profile.AttributeNames.Origin, "AO"),
				(Constants.Profile.AttributeNames.Regime, "Test 3"),
				(Constants.Profile.AttributeNames.TaxType, "PIS"),
				(Constants.Profile.AttributeNames.Modality, "IMP")
			};
			var attributes4 = new List<(string, string)>()
			{
				(Constants.Profile.AttributeNames.LegalCode, "P04"),
				(Constants.Profile.AttributeNames.Mandatory, "False"),
				(Constants.Profile.AttributeNames.Origin, "ALL"),
				(Constants.Profile.AttributeNames.Regime, "Test 4"),
				(Constants.Profile.AttributeNames.TaxType, "PIS"),
				(Constants.Profile.AttributeNames.Modality, "IMP")
			};
			var attributes5 = new List<(string, string)>()
			{
				(Constants.Profile.AttributeNames.LegalCode, "P05"),
				(Constants.Profile.AttributeNames.Mandatory, "False"),
				(Constants.Profile.AttributeNames.Origin, "ALL"),
				(Constants.Profile.AttributeNames.Regime, "Test 5"),
				(Constants.Profile.AttributeNames.TaxType, "PIS"),
				(Constants.Profile.AttributeNames.Modality, "IMP")
			};
			var attributes6 = new List<(string, string)>()
			{
				(Constants.Profile.AttributeNames.LegalCode, "P06"),
				(Constants.Profile.AttributeNames.Mandatory, "False"),
				(Constants.Profile.AttributeNames.Origin, "ALL"),
				(Constants.Profile.AttributeNames.Regime, "Test 6"),
				(Constants.Profile.AttributeNames.TaxType, "PIS"),
				(Constants.Profile.AttributeNames.Modality, "IMP")
			};
			var attributes7 = new List<(string, string)>()
			{
				(Constants.Profile.AttributeNames.LegalCode, "P02"),
				(Constants.Profile.AttributeNames.Mandatory, "False"),
				(Constants.Profile.AttributeNames.Origin, "ALL"),
				(Constants.Profile.AttributeNames.Regime, "Test 5"),
				(Constants.Profile.AttributeNames.TaxType, "PIS"),
				(Constants.Profile.AttributeNames.Modality, "IMP")
			};
			var attributes8 = new List<(string, string)>()
			{
				(Constants.Profile.AttributeNames.LegalCode, "P02"),
				(Constants.Profile.AttributeNames.Mandatory, "False"),
				(Constants.Profile.AttributeNames.Origin, "ALL"),
				(Constants.Profile.AttributeNames.Regime, "Test 5"),
				(Constants.Profile.AttributeNames.TaxType, "PIS"),
				(Constants.Profile.AttributeNames.Modality, "IMP")
			};
			var profile1 = helper.CreateRefCusProfile(profileType1, "12345678", "ATT1", startDate, endDate, attributes1);
			var profile2 = helper.CreateRefCusProfile(profileType1, "12345678", "ATT2", startDate, endDate, attributes2);
			var profile3 = helper.CreateRefCusProfile(profileType2, "12345678", "ATT3", startDate, endDate, attributes2);
			var profile4 = helper.CreateRefCusProfile(profileType2, "87654321", "ATT4", startDate, endDate, attributes3);
			var profile5 = helper.CreateRefCusProfile(profileType2, "8765432", "ATT5", startDate, endDate, attributes3);
			var profile6 = helper.CreateRefCusProfile(profileType2, "876543", "ATT6", startDate, endDate, attributes3);
			var profile7 = helper.CreateRefCusProfile(profileType2, "12345678", "ATT7", startDate, endDate, attributes4);
			var profile8 = helper.CreateRefCusProfile(profileType2, "12345678", "ATT2", startDate, endDate, attributes4);
			var profile9 = helper.CreateRefCusProfile(profileType2, "12345678", "", startDate, endDate, attributes5);
			var profile10 = helper.CreateRefCusProfile(profileType3, "12345678", "ATT8", startDate, endDate, attributes1);
			var profile11 = helper.CreateRefCusProfile(profileType4, "12345678", "ATT9", startDate, endDate, attributes2);
			var profile12 = helper.CreateRefCusProfile(profileType2, "12345678", "ATT10", startDate, endDate, attributes6);
			var profile13 = helper.CreateRefCusProfile(profileType2, "12345678", "ATT11", startDate, endDate, attributes6);
			var profile14 = helper.CreateRefCusProfile(profileType2, "12345678", "ATT12", startDate, endDate, attributes7);
			var profile15 = helper.CreateRefCusProfile(profileType2, "12345678", "ATT13", startDate, endDate, attributes8);
			var profile16 = helper.CreateRefCusProfile(profileType2, "12345678", "ATT1", startDate, endDate, attributes2);

			var question1 = helper.CreateRefCusProfileQuestion(profileType1, "Test Q 1", "ATT1", "Test Q 1", startDate, endDate, Universal.Constants.ProfileQuestion.AnswerDataTypes.Boolean, note: "Oriatention01",
				attributes: productAttr);
			var question1_1 = helper.CreateRefCusProfileQuestion(profileType1, "Test Q 1_1", "ATT1_1", "Test Q 1_1", startDate, endDate, Universal.Constants.ProfileQuestion.AnswerDataTypes.Boolean, note: "Oriatention01",
				attributes: productAttr);
			helper.CreateRefCusProfileQuestion(profileType1, "Test Q 2", "ATT2", "Test Q 2", startDate, endDate, Universal.Constants.ProfileQuestion.AnswerDataTypes.Boolean, note: "Oriatention01",
				attributes: productAttr);
			helper.CreateRefCusProfileQuestion(profileType2, "Test Q 3", "ATT3", "Test Q 3", startDate, endDate, Universal.Constants.ProfileQuestion.AnswerDataTypes.List, note: "Oriatention02",
				attributes: productAttr, answers: new[] { ("1", "desc"), ("2", "desc2"), ("3", "desc3"), ("4", "desc4"), ("5", "desc5") });
			helper.CreateRefCusProfileQuestion(profileType2, "Test Q 4", "ATT4", "Test Q 4", startDate, endDate, Universal.Constants.ProfileQuestion.AnswerDataTypes.String, note: "Oriatention03",
				attributes: productAttr, answers: new[] { ("1", "desc"), ("2", "desc2") });
			helper.CreateRefCusProfileQuestion(profileType2, "Test Q 5", "ATT5", "Test Q 5", startDate, endDate, Universal.Constants.ProfileQuestion.AnswerDataTypes.Number, note: "Oriatention04",
				attributes: productAttr);
			helper.CreateRefCusProfileQuestion(profileType2, "Test Q 6", "ATT6", "Test Q 6", startDate, endDate, Universal.Constants.ProfileQuestion.AnswerDataTypes.Number, note: "Oriatention04",
				attributes: productAttr);
			helper.CreateRefCusProfileQuestion(profileType3, "Test Q 8", "ATT8", "Test Q 8", startDate, endDate, Universal.Constants.ProfileQuestion.AnswerDataTypes.Boolean, note: "Oriatention01",
				attributes: productAttr);
			helper.CreateRefCusProfileQuestion(profileType4, "Test Q 9", "ATT9", "Test Q 9", startDate, endDate, Universal.Constants.ProfileQuestion.AnswerDataTypes.List, note: "Oriatention02",
				attributes: productAttr);
			helper.CreateRefCusProfileQuestion(profileType2, "Test Q 10", "ATT1", "Test Q 10", startDate, endDate, Universal.Constants.ProfileQuestion.AnswerDataTypes.Boolean, note: "Oriatention01",
				attributes: productAttr);

			var question10 = helper.CreateRefCusProfileQuestion(profileType2, "Test Q 10", "ATT10", "Test Q 10", startDate, endDate, Universal.Constants.ProfileQuestion.AnswerDataTypes.String, note: "Oriatention04",
				attributes: productAttr);
			var question10_1 = helper.CreateRefCusProfileQuestion(profileType2, "Test Q 10_1", "ATT10_1", "Test Q 10_1", startDate, endDate, Universal.Constants.ProfileQuestion.AnswerDataTypes.Boolean, note: "Oriatention04",
				attributes: productAttr);
			var question10_2 = helper.CreateRefCusProfileQuestion(profileType2, "Test Q 10_2", "ATT10_2", "Test Q 10_2", startDate, endDate, Universal.Constants.ProfileQuestion.AnswerDataTypes.Number, note: "Oriatention04",
				attributes: productAttr);	
			var question11 = helper.CreateRefCusProfileQuestion(profileType2, "Test Q 11", "ATT11", "Test Q 11", startDate, endDate, Universal.Constants.ProfileQuestion.AnswerDataTypes.Boolean, note: "Oriatention04",
				attributes: productAttr);
			var question11_1 = helper.CreateRefCusProfileQuestion(profileType2, "Test Q 11_1", "ATT11_1", "Test Q 11_1", startDate, endDate, Universal.Constants.ProfileQuestion.AnswerDataTypes.Number, note: "Oriatention04",
				attributes: productAttr);
			var question11_11 = helper.CreateRefCusProfileQuestion(profileType2, "Test Q 11_11", "ATT11_11", "Test Q 11_11", startDate, endDate, Universal.Constants.ProfileQuestion.AnswerDataTypes.Number, note: "Oriatention04",
				attributes: productAttr);
			   
			var question12 = helper.CreateRefCusProfileQuestion(profileType2, "Test Q 12", "ATT12", "Test Q 12", startDate, endDate, Universal.Constants.ProfileQuestion.AnswerDataTypes.String, note: "Oriatention04",
				attributes: productAttr);
			var question12_1 = helper.CreateRefCusProfileQuestion(profileType2, "Test Q 12_1", "ATT12_1", "Test Q 12_1", startDate, endDate, Universal.Constants.ProfileQuestion.AnswerDataTypes.Boolean, note: "Oriatention04",
				attributes: productAttr);
			var question13 = helper.CreateRefCusProfileQuestion(profileType2, "Test Q 13", "ATT13", "Test Q 13", startDate, endDate, Universal.Constants.ProfileQuestion.AnswerDataTypes.Boolean, note: "Oriatention04",
				attributes: productAttr);
			var question13_1 = helper.CreateRefCusProfileQuestion(profileType2, "Test Q 13_1", "ATT13_1", "Test Q 13_1", startDate, endDate, Universal.Constants.ProfileQuestion.AnswerDataTypes.Number, note: "Oriatention04",
				attributes: productAttr);

			helper.CreateRefCusProfileQuestionPathway(question1, question1_1, "ATT1|ATT1_1", startDate, endDate, "");
			helper.CreateRefCusProfileQuestionPathway(question10, question10_1, "ATT10|ATT10_1", startDate, endDate, "");
			helper.CreateRefCusProfileQuestionPathway(question10, question10_2, "ATT10|ATT10_2", startDate, endDate, "");
			helper.CreateRefCusProfileQuestionPathway(question11, question11_1, "ATT11|ATT11_1", startDate, endDate, "");
			helper.CreateRefCusProfileQuestionPathway(question11_1, question11_11, "ATT11_1|ATT11_11", startDate, endDate, "");
			helper.CreateRefCusProfileQuestionPathway(question12, question12_1, "ATT12|ATT12_2", startDate, endDate, "");
			helper.CreateRefCusProfileQuestionPathway(question13, question13_1, "ATT13|ATT13_1", startDate, endDate, "");
			factory.Save();

			return new List<RefCusProfile>() { profile1, profile2, profile3, profile4, profile5, profile6, profile7, profile8, profile9, profile10, profile11, profile12, profile13, profile14, profile15, profile16 };
		}

		public static void CreateDuimpLegalBaseCodes(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);

			helper.CreateNewOrGetExistingCusCodeType(ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRDuimpLegalBase, "BR Duimp Legal Base", ECC.CountryCodes.Brazil);
			var attributeName = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("BRDuimpLegalBase", "BR Duimp Legal Base", ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRDuimpLegalBase,
				ECC.CountryCodes.Brazil, allowDuplicates: true);

			var codeListPk = helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRDuimpLegalBase, "P01", "Awaiting payment", new ZDateTime(1900, 01, 01), new ZDateTime(2079, 06, 06)).PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListPk, attributeName.ZXE_Name, "PARA_ANALISE");

			codeListPk = helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRDuimpLegalBase, "P02", "Annulled/Revoked", new ZDateTime(1900, 01, 01), new ZDateTime(2079, 06, 06)).PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListPk, attributeName.ZXE_Name, "EM_ANALISE");

			codeListPk = helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRDuimpLegalBase, "P03", "Dummy 1", new ZDateTime(1900, 01, 01), new ZDateTime(2079, 06, 06)).PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListPk, attributeName.ZXE_Name, "Dummy 1");

			codeListPk = helper.CreateNewOrGetExistingCusCodeList(ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusCodeListTypes.Codes.BRDuimpLegalBase, "P04", "Dummy 2", new ZDateTime(1900, 01, 01), new ZDateTime(2079, 06, 06)).PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(codeListPk, attributeName.ZXE_Name, "Dummy 2");
			factory.Save();
		}

		public static void SetExchangeRate(BusinessObjectFactory factory, string currency, ZDecimal rate, ZDateTime effectiveDate, ExchangeRateType rateType = ExchangeRateType.Customs)
		{
			var refCurrency = factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currency);
			SetExchangeRate(refCurrency, rate, effectiveDate, rateType);
		}

		static void SetExchangeRate(RefCurrency currency, ZDecimal rate, ZDateTime effectiveDate, ExchangeRateType rateType = ExchangeRateType.Customs)
		{
			var rateCode = rateType == ExchangeRateType.Customs ? ECC.ExchangeRateTypes.Code.CustomsRate : ECC.ExchangeRateTypes.Code.CustomsRateSecondary;

			var filter = new ZQuery(RefExchangeRateSchema.RE_RX_NKExCurrency, SQLComparisonOperator.Equal, currency.RX_Code);
			filter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);
			filter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, effectiveDate);
			filter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_ExpiryDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, effectiveDate);
			filter.AddToFilter(JoinCondition.And, RefExchangeRateSchema.RE_ExRateType, SQLComparisonOperator.Equal, rateCode);

			var exchangeRate = currency.Factory.LoadTop1<RefExchangeRate>(filter);
			if (exchangeRate == null)
			{
				exchangeRate = currency.ExchangeRates.AddNew();
				exchangeRate.RE_RX_NKExCurrency = currency.RX_Code;
				exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;
				exchangeRate.RE_StartDate = effectiveDate;
				exchangeRate.RE_ExpiryDate = effectiveDate.AddDays(1);
				exchangeRate.RE_ExRateType = rateCode;
			}
			exchangeRate.RE_SellRate = rate;
		}
	}
}
