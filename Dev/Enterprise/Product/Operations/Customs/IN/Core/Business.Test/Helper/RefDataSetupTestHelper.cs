using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.IN.Business.Testing;

public sealed class RefDataSetupTestHelper
{
	public static void SetupCertificateTokenData(BusinessObjectFactory factory)
	{
		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingCusCodeType("CERAU", "Certificate Authority", "IN");
		helper.CreateNewOrGetExistingCusCodeType("CHPST", "Chipset Manufacturer", "IN");

		helper.CreateNewOrGetExistingCusCodeList("IN", "CERAU", "eMudhra", "eMudhra Limited", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		var chipset = helper.CreateNewOrGetExistingCusCodeList("IN", "CHPST", "WatchData", "WatchData", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		helper.CreateNewOrGetExistingCusCodeListAttribute(chipset.PK, "Chipsetdll", "TRUSTKEYP11_ND_v34.dll");
		factory.Save();
	}

	public static void SetupUNLOCOData(BusinessObjectFactory factory)
	{
		var refUNLOCO = factory.New<RefUNLOCO>();
		refUNLOCO.RL_Code = "INABC";
		refUNLOCO.RL_IATA = "XYZ";

		refUNLOCO = factory.New<RefUNLOCO>();
		refUNLOCO.RL_Code = "INLMN";
		refUNLOCO.RL_IATA = ZString.Empty;

		factory.Save();
	}

	public static void SetupCustomsOfficeData(BusinessObjectFactory factory)
	{
		var today = ZDateTime.Now;
		var startDate = today.AddDays(-10);
		var endDate = today.AddDays(10);
		CreateCustomsOfficeCode(Core.Constants.CountryCodes.India, "ABC123", startDate, endDate, factory, RefTransportModeList.Codes.SEA);
		CreateCustomsOfficeCode(Core.Constants.CountryCodes.India, "DEF123", startDate, endDate, factory, RefTransportModeList.Codes.AIR, RefTransportModeList.Codes.ROA);
		CreateCustomsOfficeCode(Core.Constants.CountryCodes.India, "GHI123", startDate, endDate, factory, RefTransportModeList.Codes.AIR);
		CreateCustomsOfficeCode(Core.Constants.CountryCodes.India, "LMN456", today.AddDays(-20), startDate, factory, RefTransportModeList.Codes.AIR);
		CreateCustomsOfficeCode(Core.Constants.CountryCodes.Eritrea, "XYZ789", startDate, endDate, factory, RefTransportModeList.Codes.AIR);
		factory.Save();
	}

	public static void SetupNFEICategoryCodes(BusinessObjectFactory factory)
	{
		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.NFEICategoryCode, "NFEI Category", Core.Constants.CountryCodes.India);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.India, RefCusCodeListTypes.Codes.NFEICategoryCode, "AA", "Aa", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeUtc);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.India, RefCusCodeListTypes.Codes.NFEICategoryCode, "BB", "Bb", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeUtc);
		factory.Save();
	}

	public static void SetupCustomsUnitOfQuantityCode(BusinessObjectFactory factory)
	{
		const string codeType = RefCusCodeListTypes.Codes.CustomsUQ;
		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingCusCodeType(codeType, "IN Customs D_QTY_CODE List", Core.Constants.CountryCodes.India);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.India, codeType, "KGS", "Kilograms", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.India, codeType, "PCS", "Pieces", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SriLanka, codeType, "LTR", "Liter", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		factory.Save();
	}

	public static void SetupCustomsLocationeData(BusinessObjectFactory factory)
	{
		var today = ZDateTime.Now;
		var startDate = today.AddDays(-10);
		var endDate = today.AddDays(10);
		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.CustomsOffice, "India Customs EDI Location");
		CreateCustomsOfficeCode(Core.Constants.CountryCodes.India, "ABC123", startDate, endDate, factory, RefTransportModeList.Codes.SEA);
		CreateCustomsOfficeCode(Core.Constants.CountryCodes.India, "GHI123", startDate, endDate, factory, RefTransportModeList.Codes.AIR);
		CreateCustomsOfficeCode(Core.Constants.CountryCodes.India, "PQR123", startDate, endDate, factory, RefTransportModeList.Codes.ROA);
		factory.Save();
	}

	public static void SetupControlResultCode(BusinessObjectFactory factory)
	{
		const string codeType = RefCusCodeListTypes.Codes.SingleWindowControl;
		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingCusCodeType(codeType, "Single Window Control", Core.Constants.CountryCodes.India);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.India, codeType, "IN00121", "Indicates no accessory is associated with the item", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.India, codeType, "IN00122", "Accessory is included and it is free of cost", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SriLanka, codeType, "IN00123", "Something", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.India, RefCusCodeListTypes.Codes.ErrorCodeDescription, "IN00124", "Something", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.India);
		factory.Save();
	}

	public static void CreateCustomsOfficeCode(ZString dataGroupingCode, ZString code, ZDateTime startDate, ZDateTime endDate, BusinessObjectFactory factory, params string[] transportModes)
	{
		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.CustomsOffice, "India Customs EDI Location");
		var codeList = helper.CreateNewOrGetExistingCusCodeList(dataGroupingCode, RefCusCodeListTypes.Codes.CustomsOffice, code, code + " DESC", startDate, endDate);
		Array.ForEach(transportModes, x => helper.CreateTransportModeForCusCodeList(codeList.PK, x));
	}

	public static void SetupExportAccessoryStatusCodes(BusinessObjectFactory factory)
	{
		const string codeType = RefCusCodeListTypes.Codes.ExportAccessoryCode;
		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingCusCodeType(codeType, "IN Customs Accessory Status List", Core.Constants.CountryCodes.India);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.India, codeType, "0", "Indicates no accessory is associated with the item", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.India, codeType, "1", "Accessory is included and it is free of cost", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SriLanka, codeType, "0", "Something", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		factory.Save();
	}

	public static void SetupImportAccessoryStatusCodes(BusinessObjectFactory factory)
	{
		const string codeType = RefCusCodeListTypes.Codes.ImportAccessoryCode;
		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingCusCodeType(codeType, "IN Customs Accessory Status List", Core.Constants.CountryCodes.India);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.India, codeType, "1", "Indicates that accessories/spare parts/maintenance or repair implements are imported along with the item. These are compulsorily supplied with the item, and are supplied free of cost with the item (Refer to Rule 2 of Accessories (Condition) Rules, 1963).", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.India, codeType, "2", "Indicates that accessories/spare parts/maintenance or repair implements are imported along with the item. All such accessories /spare parts/maintenance or repair implements have been declared as separate items (and classified under the respective CTHs) in the Bill of Entry", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SriLanka, codeType, "0", "Something", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		factory.Save();
	}

	public static void SetupSupportDocumentTypeData(BusinessObjectFactory factory)
	{
		var today = ZDateTime.Now;
		var startDate = today.AddDays(-10);
		var endDate = today.AddDays(10);

		var helper = new UniversalReferenceTestDataHelper(factory);

		helper.CreateNewOrGetExistingCusCodeType("SPDOC", "Supporting Document", Core.Constants.CountryCodes.India);

		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.India, "SPDOC", "DOC001", "Invoice", startDate, endDate);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.India, "SPDOC", "DOC002", "Packing List", startDate, endDate);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.India, "SPDOC", "DOC003", "Bill of Lading", startDate, endDate);

		factory.Save();
	}

	public static (OrgAddress, OrgAddress) SetupCusCodes(BusinessObjectFactory factory)
	{
		var orgHeader = factory.NewWithValidTestData<OrgHeader>();
		orgHeader.OH_Code = "TTTTTT";
		var addressAU = orgHeader.Addresses.AddNew();
		addressAU.Address1 = "Address1";
		addressAU.Address2 = "Address2";
		addressAU.OA_RN_NKCountryCode = "AU";
		var addressIN = orgHeader.Addresses.AddNew();
		addressIN.Address1 = "Address1";
		addressIN.Address2 = "Address2";
		addressIN.OA_RN_NKCountryCode = "IN";

		var gstCode = orgHeader.CustomsCodes.AddNew("GST", "235345", "IN");
		var iecCode = orgHeader.CustomsCodes.AddNew("IEC", "235346", "IN");
		var pasCode = orgHeader.CustomsCodes.AddNew("PAS", "235347", "IN");
		var licCode = orgHeader.CustomsCodes.AddNew("LIC", "235341", "IN");
		var adhCode = orgHeader.CustomsCodes.AddNew("ADH", "235342", "IN");

		gstCode.OK_OA_PremisesAddress = addressAU.PK;
		iecCode.OK_OA_PremisesAddress = addressIN.PK;
		pasCode.OK_OA_PremisesAddress = ZGuid.Empty;
		licCode.OK_OA_PremisesAddress = addressAU.PK;
		adhCode.OK_OA_PremisesAddress = addressIN.PK;

		return (addressAU, addressIN);
	}

	public static string SetupStandardCurrencyCodes(BusinessObjectFactory factory, string currencyCode = "CNY")
	{
		var helper = new UniversalReferenceTestDataHelper(factory);
		helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.INCustomsStandardCurrency, "Standard Currency", Core.Constants.CountryCodes.India);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.India, RefCusCodeListTypes.Codes.INCustomsStandardCurrency, currencyCode, currencyCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		factory.Save();
		return currencyCode;
	}

	public static void SetupCusPack(BusinessObjectFactory factory, ZString commercialPack, ZString customPack, ZDecimal factor)
	{
		var pack = factory.New<CusRefPacks>();
		pack.RP_CustomsCountry = "IN";
		pack.RP_Type = "CIP";
		pack.RP_CommercialPack = commercialPack;
		pack.RP_CustomsPack = customPack;
		pack.RP_ConversionFactor = factor;
	}

	public static void SetExchangeRates(BusinessObjectFactory factory, ZString currency, ZDecimal exchangeRate)
	{
		GlbCompany.CurrentCompany.GC_IsReciprocal = true;

		var foreignCurrency = RefCurrency.New(factory);
		foreignCurrency.RX_Code = currency;

		var cusRate = foreignCurrency.ExchangeRates.AddNew();
		cusRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary;
		cusRate.RE_RX_NKExCurrency = currency;
		cusRate.RE_StartDate = ZDateTime.Today.AddDays(-9);
		cusRate.RE_ExpiryDate = ZDateTime.Today.AddDays(9);
		cusRate.RE_SellRate = exchangeRate;
	}
}
