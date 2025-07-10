using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.Asycuda;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeList;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	public sealed class AsycudaBillForRegularBillValidationTest : AsycudaBillValidationAbstractTest
	{
		public void TestCycleNumber()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var country = (Integration.Customs.ASYCUDA.SGAccess.IAsycudaBill)bill;
			country.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;

			country.CycleNumber = ZString.Empty;
			var info = ((BusinessObject)country).ZPropertyInfoHash.GetPropertySafe("CycleNumber");
			AssertNoNotifications(info);

			country.CycleNumber = "XXX";
			AssertHasMessageError(info, ListValidation.InvalidCodeMessageError);

			country.CycleNumber = "9999";
			AssertHasMessageError(info, ListValidation.InvalidCodeMessageError);

			country.CycleNumber = "1";
			AssertNoNotifications(info);
		}

		public void TestCycleDate()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var bill = header.Bills.AddNew();
			var billCountry = (Integration.Customs.ASYCUDA.SGAccess.IAsycudaBill)bill;
			var info = ((BusinessObject)billCountry).ZPropertyInfoHash.GetPropertySafe("CycleDate");
			billCountry.CycleDate = ZDateTime.Empty;
			AssertNoNotifications(info);

			billCountry.CycleDate = new ZDateTime(2080, 2, 1);
			AssertHasErrorContaining(info, "is later than '06-Jun-2079'");

			billCountry.CycleDate = new ZDateTime(1899, 2, 1);
			AssertNoErrorContaining(info, "is later than '06-Jun-2079'");
			AssertHasErrorContaining(info, "is earlier than '01-Jan-1900'");

			billCountry.CycleDate = ZDateTime.Today.AddYears(6);
			AssertNoErrorContaining(info, "is earlier than '01-Jan-1900'");
			AssertHasErrorContaining(info, "is more than 5 years from now");

			billCountry.CycleDate = ZDateTime.Today.AddYears(-11);
			AssertNoErrorContaining(info, "is more than 5 years from now");
			AssertHasErrorContaining(info, "is more than 10 years old");

			billCountry.CycleDate = ZDateTime.Today.AddYears(2);
			AssertNoErrorContaining(info, "is more than 10 years old");
			AssertHasWarningContaining(info, "is more than 1 year from now");

			billCountry.CycleDate = ZDateTime.Today.AddYears(-2);
			AssertNoWarningContaining(info, "is more than 1 year from now");
			AssertHasWarningContaining(info, "is more than 1 year old");

			billCountry.CycleDate = new ZDateTime(1);
			AssertNoWarningContaining(info, "is more than 1 year old");
			AssertHasErrorContaining(info, "Enter a valid Cycle Date");

			billCountry.CycleDate = ZDateTime.Today;
			AssertNoNotifications(info);
		}

		public void TestCheckABL_GoodsLocation()
		{
			AsycudaManifestHeaderTestHelper.EnsureOrCreateManifestTypesDataInZZ(Core.Constants.CountryCodes.SouthAfrica, Factory);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");

			var validationRule = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.GoodsLocation, "GoodsLocation", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(ManifestValidationRuleCodes.MANDATORYFORMESSAGETYPE, "Desc.", RefCusCodeListTypes.Codes.ManifestValidationRule, Core.Constants.CountryCodes.SouthAfrica, RefCusCodeListTypes.Codes.ManifestValidationRule);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(ManifestValidationRuleCodes.MANDATORYFORNATURE, "Desc.", RefCusCodeListTypes.Codes.ManifestValidationRule, Core.Constants.CountryCodes.SouthAfrica, RefCusCodeListTypes.Codes.ManifestValidationRule);
			validationRule.Attributes.AddNew(ManifestValidationRuleCodes.MANDATORYFORMESSAGETYPE, nameof(ManifestDocumentType.COH));
			validationRule.Attributes.AddNew(ManifestValidationRuleCodes.MANDATORYFORMESSAGETYPE, nameof(ManifestDocumentType.ALH));
			validationRule.Attributes.AddNew(ManifestValidationRuleCodes.MANDATORYFORMESSAGETYPE, nameof(ManifestDocumentType.HAB));
			validationRule.Attributes.AddNew(ManifestValidationRuleCodes.MANDATORYFORNATURE, ShipmentTypeList.Codes.Import23);

			Factory.Save();

			CombineAssertions(() =>
			{
				var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, nameof(ManifestDocumentType.COH));
				header.AMA_Nature = ShipmentTypeList.Codes.Import23;
				var bill = header.Bills.AddNew();
				bill.ABL_GoodsLocation = ZString.Empty;

				AssertHasMessageError(bill.ABL_GoodsLocationInfo, "Location of Goods is compulsory when Manifest Nature is IMP and Manifest Type is COH for ZA (South Africa).");
				header.AMA_ManifestType = nameof(ManifestDocumentType.ALH);
				bill.ABL_GoodsLocation = ZString.Empty;
				AssertHasMessageError(bill.ABL_GoodsLocationInfo, "Location of Goods is compulsory when Manifest Nature is IMP and Manifest Type is ALH for ZA (South Africa).");

				header.AMA_ManifestType = ZString.Empty;
				header.AMA_Nature = ShipmentTypeList.Codes.Import23;
				bill.ABL_GoodsLocation = ZString.Empty;
				AssertNoMessageError(bill.ABL_GoodsLocationInfo, "Location of Goods is compulsory when Manifest Nature is IMP and Manifest Type is ALH for ZA (South Africa).");

				header.AMA_ManifestType = nameof(ManifestDocumentType.HAB);
				bill.ABL_GoodsLocation = ZString.Empty;
				AssertHasMessageError(bill.ABL_GoodsLocationInfo, "Location of Goods is compulsory when Manifest Nature is IMP and Manifest Type is HAB for ZA (South Africa).");
			});
		}

		public void TestCheckABL_BillIssuer()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);

			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "Manifest Validation");
			var validationRule = helper.CreateCusCodeList("GB", RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.BillIssuer, "Lame", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(validationRule.PK, ManifestValidationRuleCodes.Mandatory, ZString.Empty);

			var validationRuleZa = helper.CreateCusCodeList("ZA", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestValidationRule, Core.Constants.Customs.Universal.RefCusCodeList.ManifestValidationRuleCodes.BillIssuer, "A Bill issuer is required", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTransportModeForCusCodeList(validationRuleZa.PK, "AIR");
			helper.CreateCusCodeListAttribute(validationRuleZa.PK, "MANDATORYFORMESSAGETYPE", "FWB");
			helper.CreateCusCodeListAttribute(validationRuleZa.PK, "MANDATORYFORMANIFESTTYPE", "COH");

			var carrier = Factory.New<ZZRefCarrierCombined>();
			carrier.ZZ4_Code = "DJC";
			carrier.ZZ4_CountryOrGrouping = "GB";
			carrier.ZZ4_Description = "Daniel";
			Factory.Save();

			var headerGB = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedKingdom, "ICS");
			headerGB.FillWithValidTestData();
			headerGB.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedKingdom;
			var billGB = headerGB.Bills.AddNew();
			billGB.ABL_BillIssuer = "X";
			AssertNoMessageErrorContaining(billGB.ABL_BillIssuerInfo, "Lame");
			AssertHasMessageErrorContaining(billGB.ABL_BillIssuerInfo, "list");
			billGB.ABL_BillIssuer = "";
			AssertHasMessageErrorContaining(billGB.ABL_BillIssuerInfo, "Lame");
			AssertNoMessageErrorContaining(billGB.ABL_BillIssuerInfo, "list");

			var headerXX = AsycudaManifestHeaderHelper.CreateNew(Factory, "XX", "XX'");
			headerXX.FillWithValidTestData();
			headerXX.AMA_RN_NKCountry = "XX";
			var billXX = headerXX.Bills.AddNew();
			billXX.Validation.ValidateABL_BillIssuer();
			AssertNoMessageErrorContaining(billXX.ABL_BillIssuerInfo, "Lame");
			AssertNoMessageErrorContaining(billXX.ABL_BillIssuerInfo, "list");
			billXX.ABL_BillIssuer = "Y";
			AssertNoMessageErrorContaining(billXX.ABL_BillIssuerInfo, "list");

			var headerUS = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedStates, "IAM");
			headerUS.FillWithValidTestData();
			var billUS = headerUS.Bills.AddNew();
			billUS.ABL_BillIssuer = "";
			AssertNoMessageErrorContaining(billUS.ABL_BillIssuerInfo, "Lame");
			AssertNoMessageErrorContaining(billUS.ABL_BillIssuerInfo, "list");

			var headerZA = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "ALH");
			headerZA.FillWithValidTestData();
			headerZA.AMA_TransportMode = "AIR";
			var billZA = headerZA.Bills.AddNew();
			billZA.ABL_BillIssuer = "";
			AssertNoMessageErrorContaining(billZA.ABL_BillIssuerInfo, "A Bill issuer is required for ZA");

			headerZA.AMA_RN_NKCountry = "ZA";
			headerZA.AMA_TransportMode = "ROA";
			billZA.ABL_BillIssuer = "";
			AssertNoMessageErrorContaining(billZA.ABL_BillIssuerInfo, "A Bill issuer is required for ZA");

			headerZA.AMA_TransportMode = "SEA";
			headerZA.AMA_ManifestType = "COH";
			billZA.ABL_BillIssuer = "";
			AssertHasMessageErrorContaining(billZA.ABL_BillIssuerInfo, "A Bill issuer is required for ZA");
			billZA.ABL_BillIssuer = "Y";
			AssertNoMessageErrorContaining(billZA.ABL_BillIssuerInfo, "A Bill issuer is required for ZA");

			headerZA.AMA_ManifestType = "COM";
			billZA.ABL_BillIssuer = "";
			AssertNoMessageErrorContaining(billZA.ABL_BillIssuerInfo, "A Bill issuer is required for ZA");
			headerZA.AMA_ManifestType = "BBB";
			billZA.ABL_BillIssuer = "Y";
			billZA.ABL_BillIssuer = "";
			AssertNoMessageErrorContaining(billZA.ABL_BillIssuerInfo, "A Bill issuer is required for ZA");
			headerZA.AMA_ManifestType = "ECL";
			billZA.ABL_BillIssuer = "Y";
			billZA.ABL_BillIssuer = "";
			AssertNoMessageErrorContaining(billZA.ABL_BillIssuerInfo, "A Bill issuer is required for ZA");

			headerZA.AMA_ManifestType = "COM";
			billZA.ABL_BillIssuer = "Y";
			AssertNoMessageErrorContaining(billZA.ABL_BillIssuerInfo, "A Bill issuer is required for ZA");
			headerZA.AMA_ManifestType = "BBB";
			billZA.ABL_BillIssuer = "Y";
			billZA.ABL_BillIssuer = "";
			AssertNoMessageErrorContaining(billZA.ABL_BillIssuerInfo, "A Bill issuer is required for ZA");
			headerZA.AMA_ManifestType = "ECL";
			billZA.ABL_BillIssuer = "Y";
			billZA.ABL_BillIssuer = "";
			AssertNoMessageErrorContaining(billZA.ABL_BillIssuerInfo, "A Bill issuer is required for ZA");

			headerZA.AMA_TransportMode = "SEA";
			headerZA.AMA_ManifestType = nameof(ManifestDocumentType.FWB);
			billZA.ABL_BillIssuer = "";
			AssertHasMessageErrorContaining(billZA.ABL_BillIssuerInfo, "A Bill issuer is required for ZA");
		}

		public void TestCheckABL_ShipmentType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "Manifest Validation");
			var messageError = "A Shipment Type is required";
			var rule = helper.CreateNewOrGetExistingCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.ShipmentType, messageError, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(rule.PK, ManifestValidationRuleCodes.Mandatory, ZString.Empty);
			messageError += $" for {GlbCompany.CurrentCompany.GC_RN_NKCountryCode}.";
			Factory.Save();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var bill = header.Bills.AddNew();
			bill.ABL_ShipmentType = "~~~";
			AssertHasMessageErrorContaining(bill.ABL_ShipmentTypeInfo, ListValidation.InvalidCodeMessageError);
			bill.ABL_ShipmentType = "";
			AssertHasMessageError(bill.ABL_ShipmentTypeInfo, messageError);
			bill.ABL_ShipmentType = bill.Lookups.ShipmentTypes[0].Code;
			AssertNoMessageError(bill.ABL_ShipmentTypeInfo, messageError);
			AssertNoMessageErrorContaining(bill.ABL_ShipmentTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckSG_PartyStatus()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var bill = header.Bills.AddNew();
			var billCountry = (Integration.Customs.ASYCUDA.SGAccess.IAsycudaBill)bill;
			var info = ((BusinessObject)billCountry).ZPropertyInfoHash.GetPropertySafe("SG_PartyStatus");
			billCountry.SG_PartyStatus = "";
			AssertNoMessageErrors(info);
			billCountry.SG_PartyStatus = "~";
			AssertHasMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);
			billCountry.SG_PartyStatus = "A";
			AssertNoMessageErrors(info);
		}

		public void TestCheckSG_PayeeIndicator()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var bill = header.Bills.AddNew();
			var billCountry = (Integration.Customs.ASYCUDA.SGAccess.IAsycudaBill)bill;
			var info = ((BusinessObject)billCountry).ZPropertyInfoHash.GetPropertySafe("SG_PayeeIndicator");
			billCountry.SG_PayeeIndicator = "~";
			AssertHasMessageErrorContaining(info, ListValidation.InvalidCodeMessageError);
			billCountry.SG_PayeeIndicator = SGPayeeIndicatorList.Codes.Q;
			AssertNoMessageErrors(info);
		}

		public void TestCheckABL_BolType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");

			var validationRule = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Eritrea, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.BillType, "Lame", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(ManifestValidationRuleCodes.Mandatory, "Desc.", RefCusCodeListTypes.Codes.ManifestValidationRule, Core.Constants.CountryCodes.Eritrea, RefCusCodeListTypes.Codes.ManifestValidationRule);
			validationRule.Attributes.AddNew(ManifestValidationRuleCodes.Mandatory, ZString.Empty);
			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			header.AMA_RN_NKCountry = "XX";
			bill.ABL_BolType = "X";
			AssertHasMessageErrorContaining(bill.ABL_BolTypeInfo, "list");
			bill.ABL_BolType = "STD";
			AssertNoMessageErrorContaining(bill.ABL_BolTypeInfo, "list");
			bill.ABL_BolType = "";
			AssertNoMessageErrorContaining(bill.ABL_BolTypeInfo, "list");
			AssertNoMessageErrorContaining(bill.ABL_BolTypeInfo, "Lame");

			bill = header.Bills.AddNew();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			bill.ABL_BolType = "x";
			AssertNoMessageErrorContaining(bill.ABL_BolTypeInfo, "Lame");
			bill.ABL_BolType = "";
			AssertHasMessageErrorContaining(bill.ABL_BolTypeInfo, "Lame");
		}

		public void TestCheckShipper()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");
			var messageError = "Shipper is required";
			var validationRule = helper.CreateNewOrGetExistingCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.Consignor, messageError, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(validationRule.PK, ManifestValidationRuleCodes.Mandatory, ZString.Empty);
			helper.CreateCusCodeListAttribute(validationRule.PK, ManifestValidationRuleCodes.MandatoryOrgCusCode, OrgCusCode.CodeTypes.AgentCode);
			var agentCodeMessageError = ValidationConstants.RequiresCustomsCode(OrgCusCode.CodeTypes.AgentCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var currentCountryMessageError = $"Shipper is required for {GlbCompany.CurrentCompany.GC_RN_NKCountryCode}.";

			validationRule = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Zimbabwe, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.Consignor, messageError, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(validationRule.PK, ManifestValidationRuleCodes.Mandatory, ZString.Empty);
			helper.CreateCusCodeListAttribute(validationRule.PK, ManifestValidationRuleCodes.MandatoryOrgCusCode, OrgCusCode.CodeTypes.BondHolderCode);
			var bondHolderCodeMessageError = ValidationConstants.RequiresCustomsCode(OrgCusCode.CodeTypes.BondHolderCode, Core.Constants.CountryCodes.Zimbabwe);
			var zwMessageError = "Shipper is required for ZW.";

			var messageErrorState = "Shipper's state is required";
			validationRule = helper.CreateNewOrGetExistingCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.ConsignorState, messageErrorState, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(validationRule.PK, ManifestValidationRuleCodes.Mandatory, ZString.Empty);
			messageErrorState += $" for {GlbCompany.CurrentCompany.GC_RN_NKCountryCode}.";
			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_RN_NKCountry = "$#";
			var bill = header.Bills.AddNew();
			bill.ABL_OA_Shipper = ZGuid.Empty;
			AssertNoMessageErrors(bill.ABL_OA_ShipperInfo);
			bill.ABL_ShipperName = ZString.Empty;
			AssertNoMessageErrors(bill.ABL_ShipperNameInfo);
			bill.ABL_ShipperStreet1 = ZString.Empty;
			AssertNoMessageErrors(bill.ABL_ShipperStreet1Info);
			bill.ABL_ShipperStreet2 = ZString.Empty;
			AssertNoMessageErrors(bill.ABL_ShipperStreet2Info);
			bill.ABL_ShipperCity = ZString.Empty;
			AssertNoMessageErrors(bill.ABL_ShipperCityInfo);
			bill.ABL_ShipperState = ZString.Empty;
			AssertNoMessageErrors(bill.ABL_ShipperStateInfo);
			bill.ABL_ShipperPostcode = ZString.Empty;
			AssertNoMessageErrors(bill.ABL_ShipperPostcodeInfo);
			bill.ABL_RN_NKShipperCountry = ZString.Empty;
			AssertNoMessageErrors(bill.ABL_RN_NKShipperCountryInfo);

			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Zimbabwe;
			bill = header.Bills.AddNew();
			var validation = bill.Validation;
			validation.ValidateABL_OA_Shipper();
			AssertHasMessageErrorContaining(bill.ABL_OA_ShipperInfo, bondHolderCodeMessageError);
			AssertNoMessageErrorContaining(bill.ABL_OA_ShipperInfo, agentCodeMessageError);
			AssertNoMessageErrorContaining(bill.ABL_OA_ShipperInfo, currentCountryMessageError);
			AssertHasMessageErrorContaining(bill.ABL_OA_ShipperInfo, zwMessageError);
			validation.ValidateABL_ShipperName();
			AssertNoMessageErrorContaining(bill.ABL_ShipperNameInfo, currentCountryMessageError);
			AssertHasMessageErrorContaining(bill.ABL_ShipperNameInfo, zwMessageError);
			validation.ValidateABL_ShipperStreet1();
			AssertNoMessageErrorContaining(bill.ABL_ShipperStreet1Info, currentCountryMessageError);
			AssertHasMessageErrorContaining(bill.ABL_ShipperStreet1Info, zwMessageError);
			validation.ValidateABL_ShipperStreet2();
			AssertNoMessageErrors(bill.ABL_ShipperStreet2Info);
			validation.ValidateABL_ShipperCity();
			AssertNoMessageErrorContaining(bill.ABL_ShipperCityInfo, currentCountryMessageError);
			AssertHasMessageErrorContaining(bill.ABL_ShipperCityInfo, zwMessageError);
			validation.ValidateABL_ShipperState();
			AssertNoMessageError(bill.ABL_ShipperStateInfo, messageErrorState);
			validation.ValidateABL_ShipperPostcode();
			AssertNoMessageErrorContaining(bill.ABL_ShipperPostcodeInfo, currentCountryMessageError);
			AssertHasMessageErrorContaining(bill.ABL_ShipperPostcodeInfo, zwMessageError);
			validation.ValidateABL_RN_NKShipperCountry();
			AssertNoMessageErrorContaining(bill.ABL_RN_NKShipperCountryInfo, currentCountryMessageError);
			AssertHasMessageErrorContaining(bill.ABL_RN_NKShipperCountryInfo, zwMessageError);

			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			bill = header.Bills.AddNew();
			validation = bill.Validation;
			validation.ValidateABL_OA_Shipper();
			AssertNoMessageErrorContaining(bill.ABL_OA_ShipperInfo, bondHolderCodeMessageError);
			AssertHasMessageErrorContaining(bill.ABL_OA_ShipperInfo, agentCodeMessageError);
			AssertHasMessageErrorContaining(bill.ABL_OA_ShipperInfo, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_OA_ShipperInfo, zwMessageError);

			bill.ABL_ShipperName = "A";
			AssertNoMessageErrorContaining(bill.ABL_ShipperNameInfo, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_ShipperNameInfo, zwMessageError);
			validation.ValidateABL_OA_Shipper();
			AssertNoMessageErrorContaining(bill.ABL_OA_ShipperInfo, bondHolderCodeMessageError);
			AssertHasMessageErrorContaining(bill.ABL_OA_ShipperInfo, agentCodeMessageError);
			AssertNoMessageErrorContaining(bill.ABL_OA_ShipperInfo, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_OA_ShipperInfo, zwMessageError);
			bill.ABL_ShipperName = ZString.Empty;
			validation.ValidateABL_OA_Shipper();
			AssertHasMessageErrorContaining(bill.ABL_ShipperNameInfo, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_ShipperNameInfo, zwMessageError);
			AssertNoMessageErrorContaining(bill.ABL_OA_ShipperInfo, bondHolderCodeMessageError);
			AssertHasMessageErrorContaining(bill.ABL_OA_ShipperInfo, agentCodeMessageError);

			bill.ABL_ShipperStreet1 = "A";
			AssertNoMessageErrorContaining(bill.ABL_ShipperStreet1Info, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_ShipperStreet1Info, zwMessageError);
			bill.ABL_ShipperStreet1 = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_ShipperStreet1Info, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_ShipperStreet1Info, zwMessageError);

			bill.ABL_ShipperStreet2 = ZString.Empty;
			AssertNoMessageErrors(bill.ABL_ShipperStreet2Info);

			bill.ABL_ShipperCity = "A";
			AssertNoMessageErrorContaining(bill.ABL_ShipperCityInfo, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_ShipperCityInfo, zwMessageError);
			bill.ABL_ShipperCity = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_ShipperCityInfo, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_ShipperCityInfo, zwMessageError);

			bill.ABL_ShipperState = "A";
			AssertNoMessageError(bill.ABL_ShipperStateInfo, messageErrorState);
			bill.ABL_ShipperState = ZString.Empty;
			AssertHasMessageError(bill.ABL_ShipperStateInfo, messageErrorState);

			bill.ABL_ShipperPostcode = "A";
			AssertNoMessageErrorContaining(bill.ABL_ShipperPostcodeInfo, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_ShipperPostcodeInfo, zwMessageError);
			bill.ABL_ShipperPostcode = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_ShipperPostcodeInfo, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_ShipperPostcodeInfo, zwMessageError);

			bill.ABL_RN_NKShipperCountry = "A";
			AssertNoMessageErrorContaining(bill.ABL_RN_NKShipperCountryInfo, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_RN_NKShipperCountryInfo, zwMessageError);
			bill.ABL_RN_NKShipperCountry = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_RN_NKShipperCountryInfo, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_RN_NKShipperCountryInfo, zwMessageError);

			var org = Factory.New<OrgHeader>();
			bill.ABL_OA_Shipper = org.MainAddress.PK;
			AssertNoMessageErrorContaining(bill.ABL_OA_ShipperInfo, bondHolderCodeMessageError);
			AssertHasMessageErrorContaining(bill.ABL_OA_ShipperInfo, agentCodeMessageError);
			AssertNoMessageErrorContaining(bill.ABL_OA_ShipperInfo, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_OA_ShipperInfo, zwMessageError);

			validation.ValidateABL_ShipperName();
			AssertNoMessageErrors(bill.ABL_ShipperNameInfo);

			validation.ValidateABL_ShipperStreet1();
			AssertNoMessageErrors(bill.ABL_ShipperStreet1Info);

			validation.ValidateABL_ShipperStreet2();
			AssertNoMessageErrors(bill.ABL_ShipperStreet2Info);

			validation.ValidateABL_ShipperCity();
			AssertNoMessageErrors(bill.ABL_ShipperCityInfo);

			validation.ValidateABL_ShipperState();
			AssertNoMessageErrors(bill.ABL_ShipperStateInfo);

			validation.ValidateABL_ShipperPostcode();
			AssertNoMessageErrors(bill.ABL_ShipperPostcodeInfo);

			validation.ValidateABL_RN_NKShipperCountry();
			AssertNoMessageErrors(bill.ABL_RN_NKShipperCountryInfo);

			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "AS", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			validation.ValidateABL_OA_Shipper();
			AssertNoMessageErrorContaining(bill.ABL_OA_ShipperInfo, bondHolderCodeMessageError);
			AssertNoMessageErrorContaining(bill.ABL_OA_ShipperInfo, agentCodeMessageError);
			AssertNoMessageErrorContaining(bill.ABL_OA_ShipperInfo, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_OA_ShipperInfo, zwMessageError);
		}

		public void TestCheckConsignee()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");
			var messageError = "Consignee is required";
			var validationRule = helper.CreateNewOrGetExistingCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.Consignee, messageError, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(validationRule.PK, ManifestValidationRuleCodes.Mandatory, ZString.Empty);
			helper.CreateCusCodeListAttribute(validationRule.PK, ManifestValidationRuleCodes.MandatoryOrgCusCode, OrgCusCode.CodeTypes.AgentCode);
			var agentCodeMessageError = ValidationConstants.RequiresCustomsCode(OrgCusCode.CodeTypes.AgentCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var currentCountryMessageError = $"Consignee is required for {GlbCompany.CurrentCompany.GC_RN_NKCountryCode}.";

			validationRule = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Zimbabwe, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.Consignee, messageError, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(validationRule.PK, ManifestValidationRuleCodes.Mandatory, ZString.Empty);
			helper.CreateCusCodeListAttribute(validationRule.PK, ManifestValidationRuleCodes.MandatoryOrgCusCode, OrgCusCode.CodeTypes.BondHolderCode);
			var bondHolderCodeMessageError = ValidationConstants.RequiresCustomsCode(OrgCusCode.CodeTypes.BondHolderCode, Core.Constants.CountryCodes.Zimbabwe);
			var zwMessageError = "Consignee is required for ZW.";

			var messageErrorState = "Consignee's state is required";
			validationRule = helper.CreateNewOrGetExistingCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.ConsigneeState, messageErrorState, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(validationRule.PK, ManifestValidationRuleCodes.Mandatory, ZString.Empty);
			messageErrorState += $" for {GlbCompany.CurrentCompany.GC_RN_NKCountryCode}.";

			var messageErrorPhone = "Consignee's phone is required";
			validationRule = helper.CreateNewOrGetExistingCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.ConsigneePhone, messageErrorPhone, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(validationRule.PK, ManifestValidationRuleCodes.Mandatory, ZString.Empty);
			messageErrorPhone += $" for {GlbCompany.CurrentCompany.GC_RN_NKCountryCode}.";

			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_RN_NKCountry = "$#";
			var bill = header.Bills.AddNew();
			bill.ABL_OA_Consignee = ZGuid.Empty;
			AssertNoMessageErrors(bill.ABL_OA_ConsigneeInfo);
			bill.ABL_ConsigneeName = ZString.Empty;
			AssertNoMessageErrors(bill.ABL_ConsigneeNameInfo);
			bill.ABL_ConsigneeStreet1 = ZString.Empty;
			AssertNoMessageErrors(bill.ABL_ConsigneeStreet1Info);
			bill.ABL_ConsigneeStreet2 = ZString.Empty;
			AssertNoMessageErrors(bill.ABL_ConsigneeStreet2Info);
			bill.ABL_ConsigneeCity = ZString.Empty;
			AssertNoMessageErrors(bill.ABL_ConsigneeCityInfo);
			bill.ABL_ConsigneeState = ZString.Empty;
			AssertNoMessageErrors(bill.ABL_ConsigneeStateInfo);
			bill.ABL_ConsigneePostcode = ZString.Empty;
			AssertNoMessageErrors(bill.ABL_ConsigneePostcodeInfo);
			bill.ABL_RN_NKConsigneeCountry = ZString.Empty;
			AssertNoMessageErrors(bill.ABL_RN_NKConsigneeCountryInfo);
			bill.ABL_ConsigneePhone = ZString.Empty;
			AssertNoMessageErrors(bill.ABL_ConsigneePhoneInfo);

			header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Zimbabwe;
			bill = header.Bills.AddNew();
			var validation = bill.Validation;
			validation.ValidateABL_OA_Consignee();
			AssertHasMessageErrorContaining(bill.ABL_OA_ConsigneeInfo, bondHolderCodeMessageError);
			AssertNoMessageErrorContaining(bill.ABL_OA_ConsigneeInfo, agentCodeMessageError);
			AssertNoMessageErrorContaining(bill.ABL_OA_ConsigneeInfo, currentCountryMessageError);
			AssertHasMessageErrorContaining(bill.ABL_OA_ConsigneeInfo, zwMessageError);
			validation.ValidateABL_ConsigneeName();
			AssertNoMessageErrorContaining(bill.ABL_ConsigneeNameInfo, currentCountryMessageError);
			AssertHasMessageErrorContaining(bill.ABL_ConsigneeNameInfo, zwMessageError);
			validation.ValidateABL_ConsigneeStreet1();
			AssertNoMessageErrorContaining(bill.ABL_ConsigneeStreet1Info, currentCountryMessageError);
			AssertHasMessageErrorContaining(bill.ABL_ConsigneeStreet1Info, zwMessageError);
			validation.ValidateABL_ConsigneeStreet2();
			AssertNoMessageErrors(bill.ABL_ConsigneeStreet2Info);
			validation.ValidateABL_ConsigneeCity();
			AssertNoMessageErrorContaining(bill.ABL_ConsigneeCityInfo, currentCountryMessageError);
			AssertHasMessageErrorContaining(bill.ABL_ConsigneeCityInfo, zwMessageError);
			validation.ValidateABL_ConsigneeState();
			AssertNoMessageError(bill.ABL_ConsigneeStateInfo, messageErrorState);
			validation.ValidateABL_ConsigneePostcode();
			AssertNoMessageErrorContaining(bill.ABL_ConsigneePostcodeInfo, currentCountryMessageError);
			AssertHasMessageErrorContaining(bill.ABL_ConsigneePostcodeInfo, zwMessageError);
			validation.ValidateABL_RN_NKConsigneeCountry();
			AssertNoMessageErrorContaining(bill.ABL_RN_NKConsigneeCountryInfo, currentCountryMessageError);
			AssertHasMessageErrorContaining(bill.ABL_RN_NKConsigneeCountryInfo, zwMessageError);
			validation.ValidateABL_ConsigneePhone();
			AssertNoMessageError(bill.ABL_ConsigneePhoneInfo, messageErrorPhone);

			header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			bill = header.Bills.AddNew();
			validation = bill.Validation;
			validation.ValidateABL_OA_Consignee();
			AssertNoMessageErrorContaining(bill.ABL_OA_ConsigneeInfo, bondHolderCodeMessageError);
			AssertHasMessageErrorContaining(bill.ABL_OA_ConsigneeInfo, agentCodeMessageError);
			AssertHasMessageErrorContaining(bill.ABL_OA_ConsigneeInfo, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_OA_ConsigneeInfo, zwMessageError);

			bill.ABL_ConsigneeName = "A";
			AssertNoMessageErrorContaining(bill.ABL_ConsigneeNameInfo, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_ConsigneeNameInfo, zwMessageError);
			validation.ValidateABL_OA_Consignee();
			AssertNoMessageErrorContaining(bill.ABL_OA_ConsigneeInfo, bondHolderCodeMessageError);
			AssertHasMessageErrorContaining(bill.ABL_OA_ConsigneeInfo, agentCodeMessageError);
			AssertNoMessageErrorContaining(bill.ABL_OA_ConsigneeInfo, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_OA_ConsigneeInfo, zwMessageError);
			bill.ABL_ConsigneeName = ZString.Empty;
			validation.ValidateABL_OA_Consignee();
			AssertHasMessageErrorContaining(bill.ABL_ConsigneeNameInfo, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_ConsigneeNameInfo, zwMessageError);
			AssertNoMessageErrorContaining(bill.ABL_OA_ConsigneeInfo, bondHolderCodeMessageError);
			AssertHasMessageErrorContaining(bill.ABL_OA_ConsigneeInfo, agentCodeMessageError);

			bill.ABL_ConsigneeStreet1 = "A";
			AssertNoMessageErrorContaining(bill.ABL_ConsigneeStreet1Info, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_ConsigneeStreet1Info, zwMessageError);
			bill.ABL_ConsigneeStreet1 = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_ConsigneeStreet1Info, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_ConsigneeStreet1Info, zwMessageError);

			bill.ABL_ConsigneeStreet2 = ZString.Empty;
			AssertNoMessageErrors(bill.ABL_ConsigneeStreet2Info);

			bill.ABL_ConsigneeCity = "A";
			AssertNoMessageErrorContaining(bill.ABL_ConsigneeCityInfo, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_ConsigneeCityInfo, zwMessageError);
			bill.ABL_ConsigneeCity = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_ConsigneeCityInfo, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_ConsigneeCityInfo, zwMessageError);

			bill.ABL_ConsigneeState = "A";
			AssertNoMessageError(bill.ABL_ConsigneeStateInfo, messageErrorState);
			bill.ABL_ConsigneeState = ZString.Empty;
			AssertHasMessageError(bill.ABL_ConsigneeStateInfo, messageErrorState);

			bill.ABL_ConsigneePostcode = "A";
			AssertNoMessageErrorContaining(bill.ABL_ConsigneePostcodeInfo, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_ConsigneePostcodeInfo, zwMessageError);
			bill.ABL_ConsigneePostcode = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_ConsigneePostcodeInfo, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_ConsigneePostcodeInfo, zwMessageError);

			bill.ABL_RN_NKConsigneeCountry = "A";
			AssertNoMessageErrorContaining(bill.ABL_RN_NKConsigneeCountryInfo, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_RN_NKConsigneeCountryInfo, zwMessageError);
			bill.ABL_RN_NKConsigneeCountry = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_RN_NKConsigneeCountryInfo, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_RN_NKConsigneeCountryInfo, zwMessageError);

			bill.ABL_ConsigneePhone = "A";
			AssertNoMessageError(bill.ABL_ConsigneePhoneInfo, messageErrorPhone);
			bill.ABL_ConsigneePhone = ZString.Empty;
			AssertHasMessageError(bill.ABL_ConsigneePhoneInfo, messageErrorPhone);

			var org = Factory.New<OrgHeader>();
			bill.ABL_OA_Consignee = org.MainAddress.PK;
			AssertNoMessageErrorContaining(bill.ABL_OA_ConsigneeInfo, bondHolderCodeMessageError);
			AssertHasMessageErrorContaining(bill.ABL_OA_ConsigneeInfo, agentCodeMessageError);
			AssertNoMessageErrorContaining(bill.ABL_OA_ConsigneeInfo, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_OA_ConsigneeInfo, zwMessageError);

			validation.ValidateABL_ConsigneeName();
			AssertNoMessageErrors(bill.ABL_ConsigneeNameInfo);

			validation.ValidateABL_ConsigneeStreet1();
			AssertNoMessageErrors(bill.ABL_ConsigneeStreet1Info);

			validation.ValidateABL_ConsigneeStreet2();
			AssertNoMessageErrors(bill.ABL_ConsigneeStreet2Info);

			validation.ValidateABL_ConsigneeCity();
			AssertNoMessageErrors(bill.ABL_ConsigneeCityInfo);

			validation.ValidateABL_ConsigneeState();
			AssertNoMessageErrors(bill.ABL_ConsigneeStateInfo);

			validation.ValidateABL_ConsigneePostcode();
			AssertNoMessageErrors(bill.ABL_ConsigneePostcodeInfo);

			validation.ValidateABL_RN_NKConsigneeCountry();
			AssertNoMessageErrors(bill.ABL_RN_NKConsigneeCountryInfo);

			validation.ValidateABL_ConsigneePhone();
			AssertNoMessageErrors(bill.ABL_ConsigneePhoneInfo);

			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "AS", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			validation.ValidateABL_OA_Consignee();
			AssertNoMessageErrorContaining(bill.ABL_OA_ConsigneeInfo, bondHolderCodeMessageError);
			AssertNoMessageErrorContaining(bill.ABL_OA_ConsigneeInfo, agentCodeMessageError);
			AssertNoMessageErrorContaining(bill.ABL_OA_ConsigneeInfo, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_OA_ConsigneeInfo, zwMessageError);
		}

		public void TestCheckNotifyParty()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");
			var messageError = "NotifyParty is required";
			var validationRule = helper.CreateNewOrGetExistingCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.Notify, messageError, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(validationRule.PK, ManifestValidationRuleCodes.Mandatory, ZString.Empty);
			helper.CreateCusCodeListAttribute(validationRule.PK, ManifestValidationRuleCodes.MandatoryOrgCusCode, OrgCusCode.CodeTypes.AgentCode);
			var agentCodeMessageError = ValidationConstants.RequiresCustomsCode(OrgCusCode.CodeTypes.AgentCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var currentCountryMessageError = $"NotifyParty is required for {GlbCompany.CurrentCompany.GC_RN_NKCountryCode}.";

			validationRule = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Zimbabwe, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.Notify, messageError, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(validationRule.PK, ManifestValidationRuleCodes.Mandatory, ZString.Empty);
			helper.CreateCusCodeListAttribute(validationRule.PK, ManifestValidationRuleCodes.MandatoryOrgCusCode, OrgCusCode.CodeTypes.BondHolderCode);
			var bondHolderCodeMessageError = ValidationConstants.RequiresCustomsCode(OrgCusCode.CodeTypes.BondHolderCode, Core.Constants.CountryCodes.Zimbabwe);
			var zwMessageError = "NotifyParty is required for ZW.";

			var messageErrorState = "NotifyParty's state is required";
			validationRule = helper.CreateNewOrGetExistingCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.NotifyState, messageErrorState, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(validationRule.PK, ManifestValidationRuleCodes.Mandatory, ZString.Empty);
			messageErrorState += $" for {GlbCompany.CurrentCompany.GC_RN_NKCountryCode}.";

			var messageErrorPhone = "NotifyParty's phone is required";
			validationRule = helper.CreateNewOrGetExistingCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.NotifyPhone, messageErrorPhone, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(validationRule.PK, ManifestValidationRuleCodes.Mandatory, ZString.Empty);
			messageErrorPhone += $" for {GlbCompany.CurrentCompany.GC_RN_NKCountryCode}.";

			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_RN_NKCountry = "$#";
			var bill = header.Bills.AddNew();
			bill.ABL_OA_NotifyParty = ZGuid.Empty;
			AssertNoMessageErrors(bill.ABL_OA_NotifyPartyInfo);
			bill.ABL_NotifyPartyName = ZString.Empty;
			AssertNoMessageErrors(bill.ABL_NotifyPartyNameInfo);
			bill.ABL_NotifyPartyStreet1 = ZString.Empty;
			AssertNoMessageErrors(bill.ABL_NotifyPartyStreet1Info);
			bill.ABL_NotifyPartyStreet2 = ZString.Empty;
			AssertNoMessageErrors(bill.ABL_NotifyPartyStreet2Info);
			bill.ABL_NotifyPartyCity = ZString.Empty;
			AssertNoMessageErrors(bill.ABL_NotifyPartyCityInfo);
			bill.ABL_NotifyPartyState = ZString.Empty;
			AssertNoMessageErrors(bill.ABL_NotifyPartyStateInfo);
			bill.ABL_NotifyPartyPostcode = ZString.Empty;
			AssertNoMessageErrors(bill.ABL_NotifyPartyPostcodeInfo);
			bill.ABL_RN_NKNotifyPartyCountry = ZString.Empty;
			AssertNoMessageErrors(bill.ABL_RN_NKNotifyPartyCountryInfo);
			bill.ABL_NotifyPartyPhone = ZString.Empty;
			AssertNoMessageErrors(bill.ABL_NotifyPartyPhoneInfo);

			header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Zimbabwe;
			bill = header.Bills.AddNew();
			var validation = bill.Validation;
			validation.ValidateABL_OA_NotifyParty();
			AssertHasMessageErrorContaining(bill.ABL_OA_NotifyPartyInfo, bondHolderCodeMessageError);
			AssertNoMessageErrorContaining(bill.ABL_OA_NotifyPartyInfo, agentCodeMessageError);
			AssertNoMessageErrorContaining(bill.ABL_OA_NotifyPartyInfo, currentCountryMessageError);
			AssertHasMessageErrorContaining(bill.ABL_OA_NotifyPartyInfo, zwMessageError);
			validation.ValidateABL_NotifyPartyName();
			AssertNoMessageErrorContaining(bill.ABL_NotifyPartyNameInfo, currentCountryMessageError);
			AssertHasMessageErrorContaining(bill.ABL_NotifyPartyNameInfo, zwMessageError);
			validation.ValidateABL_NotifyPartyStreet1();
			AssertNoMessageErrorContaining(bill.ABL_NotifyPartyStreet1Info, currentCountryMessageError);
			AssertHasMessageErrorContaining(bill.ABL_NotifyPartyStreet1Info, zwMessageError);
			validation.ValidateABL_NotifyPartyStreet2();
			AssertNoMessageErrors(bill.ABL_NotifyPartyStreet2Info);
			validation.ValidateABL_NotifyPartyCity();
			AssertNoMessageErrorContaining(bill.ABL_NotifyPartyCityInfo, currentCountryMessageError);
			AssertHasMessageErrorContaining(bill.ABL_NotifyPartyCityInfo, zwMessageError);
			validation.ValidateABL_NotifyPartyState();
			AssertNoMessageError(bill.ABL_NotifyPartyStateInfo, messageErrorState);
			validation.ValidateABL_NotifyPartyPostcode();
			AssertNoMessageErrorContaining(bill.ABL_NotifyPartyPostcodeInfo, currentCountryMessageError);
			AssertHasMessageErrorContaining(bill.ABL_NotifyPartyPostcodeInfo, zwMessageError);
			validation.ValidateABL_RN_NKNotifyPartyCountry();
			AssertNoMessageErrorContaining(bill.ABL_RN_NKNotifyPartyCountryInfo, currentCountryMessageError);
			AssertHasMessageErrorContaining(bill.ABL_RN_NKNotifyPartyCountryInfo, zwMessageError);
			validation.ValidateABL_NotifyPartyPhone();
			AssertNoMessageError(bill.ABL_NotifyPartyPhoneInfo, messageErrorPhone);

			header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			bill = header.Bills.AddNew();
			validation = bill.Validation;
			validation.ValidateABL_OA_NotifyParty();
			AssertNoMessageErrorContaining(bill.ABL_OA_NotifyPartyInfo, bondHolderCodeMessageError);
			AssertHasMessageErrorContaining(bill.ABL_OA_NotifyPartyInfo, agentCodeMessageError);
			AssertHasMessageErrorContaining(bill.ABL_OA_NotifyPartyInfo, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_OA_NotifyPartyInfo, zwMessageError);

			bill.ABL_NotifyPartyName = "A";
			AssertNoMessageErrorContaining(bill.ABL_NotifyPartyNameInfo, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_NotifyPartyNameInfo, zwMessageError);
			validation.ValidateABL_OA_NotifyParty();
			AssertNoMessageErrorContaining(bill.ABL_OA_NotifyPartyInfo, bondHolderCodeMessageError);
			AssertHasMessageErrorContaining(bill.ABL_OA_NotifyPartyInfo, agentCodeMessageError);
			AssertNoMessageErrorContaining(bill.ABL_OA_NotifyPartyInfo, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_OA_NotifyPartyInfo, zwMessageError);
			bill.ABL_NotifyPartyName = ZString.Empty;
			validation.ValidateABL_OA_NotifyParty();
			AssertHasMessageErrorContaining(bill.ABL_NotifyPartyNameInfo, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_NotifyPartyNameInfo, zwMessageError);
			AssertNoMessageErrorContaining(bill.ABL_OA_NotifyPartyInfo, bondHolderCodeMessageError);
			AssertHasMessageErrorContaining(bill.ABL_OA_NotifyPartyInfo, agentCodeMessageError);

			bill.ABL_NotifyPartyStreet1 = "A";
			AssertNoMessageErrorContaining(bill.ABL_NotifyPartyStreet1Info, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_NotifyPartyStreet1Info, zwMessageError);
			bill.ABL_NotifyPartyStreet1 = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_NotifyPartyStreet1Info, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_NotifyPartyStreet1Info, zwMessageError);

			bill.ABL_NotifyPartyStreet2 = ZString.Empty;
			AssertNoMessageErrors(bill.ABL_NotifyPartyStreet2Info);

			bill.ABL_NotifyPartyCity = "A";
			AssertNoMessageErrorContaining(bill.ABL_NotifyPartyCityInfo, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_NotifyPartyCityInfo, zwMessageError);
			bill.ABL_NotifyPartyCity = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_NotifyPartyCityInfo, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_NotifyPartyCityInfo, zwMessageError);

			bill.ABL_NotifyPartyState = "A";
			AssertNoMessageError(bill.ABL_NotifyPartyStateInfo, messageErrorState);
			bill.ABL_NotifyPartyState = ZString.Empty;
			AssertHasMessageError(bill.ABL_NotifyPartyStateInfo, messageErrorState);

			bill.ABL_NotifyPartyPostcode = "A";
			AssertNoMessageErrorContaining(bill.ABL_NotifyPartyPostcodeInfo, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_NotifyPartyPostcodeInfo, zwMessageError);
			bill.ABL_NotifyPartyPostcode = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_NotifyPartyPostcodeInfo, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_NotifyPartyPostcodeInfo, zwMessageError);

			bill.ABL_RN_NKNotifyPartyCountry = "A";
			AssertNoMessageErrorContaining(bill.ABL_RN_NKNotifyPartyCountryInfo, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_RN_NKNotifyPartyCountryInfo, zwMessageError);
			bill.ABL_RN_NKNotifyPartyCountry = ZString.Empty;
			AssertHasMessageErrorContaining(bill.ABL_RN_NKNotifyPartyCountryInfo, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_RN_NKNotifyPartyCountryInfo, zwMessageError);

			bill.ABL_NotifyPartyPhone = "A";
			AssertNoMessageError(bill.ABL_NotifyPartyPhoneInfo, messageErrorPhone);
			bill.ABL_NotifyPartyPhone = ZString.Empty;
			AssertHasMessageError(bill.ABL_NotifyPartyPhoneInfo, messageErrorPhone);

			var org = Factory.New<OrgHeader>();
			bill.ABL_OA_NotifyParty = org.MainAddress.PK;
			AssertNoMessageErrorContaining(bill.ABL_OA_NotifyPartyInfo, bondHolderCodeMessageError);
			AssertHasMessageErrorContaining(bill.ABL_OA_NotifyPartyInfo, agentCodeMessageError);
			AssertNoMessageErrorContaining(bill.ABL_OA_NotifyPartyInfo, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_OA_NotifyPartyInfo, zwMessageError);

			validation.ValidateABL_NotifyPartyName();
			AssertNoMessageErrors(bill.ABL_NotifyPartyNameInfo);

			validation.ValidateABL_NotifyPartyStreet1();
			AssertNoMessageErrors(bill.ABL_NotifyPartyStreet1Info);

			validation.ValidateABL_NotifyPartyStreet2();
			AssertNoMessageErrors(bill.ABL_NotifyPartyStreet2Info);

			validation.ValidateABL_NotifyPartyCity();
			AssertNoMessageErrors(bill.ABL_NotifyPartyCityInfo);

			validation.ValidateABL_NotifyPartyState();
			AssertNoMessageErrors(bill.ABL_NotifyPartyStateInfo);

			validation.ValidateABL_NotifyPartyPostcode();
			AssertNoMessageErrors(bill.ABL_NotifyPartyPostcodeInfo);

			validation.ValidateABL_RN_NKNotifyPartyCountry();
			AssertNoMessageErrors(bill.ABL_RN_NKNotifyPartyCountryInfo);

			validation.ValidateABL_NotifyPartyPhone();
			AssertNoMessageErrors(bill.ABL_NotifyPartyPhoneInfo);

			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "AS", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			validation.ValidateABL_OA_NotifyParty();
			AssertNoMessageErrorContaining(bill.ABL_OA_NotifyPartyInfo, bondHolderCodeMessageError);
			AssertNoMessageErrorContaining(bill.ABL_OA_NotifyPartyInfo, agentCodeMessageError);
			AssertNoMessageErrorContaining(bill.ABL_OA_NotifyPartyInfo, currentCountryMessageError);
			AssertNoMessageErrorContaining(bill.ABL_OA_NotifyPartyInfo, zwMessageError);
		}

		public void TestCheckAtLeastOnePackWhereRelevant()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.Validation.ValidateABL_BillNumber();
			AssertNoMessageErrorContaining(bill.ABL_BillNumberInfo, "pack");
			var cont = header.Containers.AddNew();
			AssertNoMessageErrorContaining(bill.ABL_BillNumberInfo, "pack");
			bill.Validation.ValidateABL_BillNumber();
			AssertHasWarningContaining(bill.ABL_BillNumberInfo, "pack");
			var pack = bill.Packs.AddNew();
			bill.Validation.ValidateABL_BillNumber();
			AssertNoMessageErrorContaining(bill.ABL_BillNumberInfo, "pack");
		}

		public void TestCheckABL_BillNumber_NoPackLine()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.Containers.AddNew();

			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "ABCDEF";
			AssertHasWarning(bill.ABL_BillNumberInfo, "Containers exist on this manifest.  If this bill is packed into a container please enter at least one pack line.");

			bill.Packs.AddNew();
			bill.Validation.ValidateABL_BillNumber();

			AssertNoWarning(bill.ABL_BillNumberInfo, "Containers exist on this manifest.  If this bill is packed into a container please enter at least one pack line.");
		}

		public void TestManifestUQ()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry", Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice", Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.PackageTypes, "PackageTypes", Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping);

			var sb = helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, "SB", "Solomons", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sb.PK, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NVC, "0.0.0.0");
			var sbHIRS = helper.CreateNewOrGetExistingCusCodeList("SB", RefCusCodeListTypes.Codes.CustomsOffice, "HIRS", "Honiara", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sbHIRS.PK, "SEA", "SBHIR");
			var consol = Factory.New<ForwardingConsol>();
			Factory.Save();

			var factoryThatWillNotSeeStandingData = new BusinessObjectFactory();

			var headerOne = AsycudaManifestHeaderHelper.CreateNew(factoryThatWillNotSeeStandingData, Core.Constants.CountryCodes.SolomonIslands, "ASY");
			headerOne.FillWithValidTestData();
			headerOne.SetParent(consol);
			headerOne.AMA_RN_NKCountry = Core.Constants.CountryCodes.SolomonIslands;
			headerOne.AMA_RL_NKPortOfLoading = "SBHIR";
			headerOne.AMA_RL_NKPortOfDischarge = "FJAQS";

			var billOne = headerOne.Bills.AddNew();
			billOne.ABL_RL_NKOrigin = "SBHIR";
			billOne.ABL_RL_NKFinalDestination = "FJAQS";

			billOne.ABL_ManifestUQ = Core.Constants.PkgUnit.Bag;
			AssertHasMessageErrorContaining(billOne.ABL_ManifestUQInfo, $"Package type {Core.Constants.PkgUnit.Bag} does not map to a Customs package type for country SB.");

			billOne.ABL_ManifestUQ = Core.Constants.PkgUnit.Drum;
			AssertHasMessageErrorContaining(billOne.ABL_ManifestUQInfo, $"Package type {Core.Constants.PkgUnit.Drum} does not map to a Customs package type for country SB.");

			var factoryForSetup = new BusinessObjectFactory();
			FindOrMakeRefPackAndCusCodeList(Core.Constants.PkgUnit.Bag, "XXX", "SB", factoryForSetup);
			factoryForSetup.Save();

			var headerTwo = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SolomonIslands, "ASY");
			headerTwo.FillWithValidTestData();
			headerTwo.SetParent(consol);
			headerTwo.AMA_RN_NKCountry = Core.Constants.CountryCodes.SolomonIslands;
			headerTwo.AMA_RL_NKPortOfLoading = "SBHIR";
			headerTwo.AMA_RL_NKPortOfDischarge = "FJAQS";

			var billTwo = headerTwo.Bills.AddNew();
			billTwo.ABL_RL_NKOrigin = "SBHIR";
			billTwo.ABL_RL_NKFinalDestination = "FJAQS";

			billTwo.ABL_ManifestUQ = Core.Constants.PkgUnit.Envelope;
			AssertHasMessageErrorContaining(billTwo.ABL_ManifestUQInfo, $"Package type {Core.Constants.PkgUnit.Envelope} does not map to a Customs package type for country SB.");

			billTwo.ABL_ManifestUQ = Core.Constants.PkgUnit.Bag;
			AssertNoMessageErrorContaining(billTwo.ABL_ManifestUQInfo, $"Package type {Core.Constants.PkgUnit.Bag} does not map to a Customs package type for country SB.");

			billTwo.ABL_ManifestUQ = Core.Constants.PkgUnit.Drum;
			AssertHasMessageErrorContaining(billTwo.ABL_ManifestUQInfo, $"Package type {Core.Constants.PkgUnit.Drum} does not map to a Customs package type for country SB.");

			var headerThree = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "HAB");
			headerThree.SetParent(consol);
			var billThree = headerThree.Bills.AddNew();
			billThree.ABL_ManifestQty = 1;
			billThree.ABL_ManifestUQ = "~!";
			AssertEquals("One notification about it not being in the list, but no other notification about there being no mapping", 1, billThree.ABL_ManifestUQInfo.Notifications.Count());
		}

		public void TestShipperIncludingDynamicValidation()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule", header.AMA_RN_NKCountry);
			helper.CreateNewOrGetExistingDataGrouping("ER");
			var address = Factory.LoadTop1<OrgAddress>(new ZQuery());
			var zzd = Factory.New<RefCusCodeList>();
			zzd.ZZD_Code = "CONSIGNOR";
			zzd.ZZD_Description = "Anything you like guv'nor";
			zzd.ZZD_ZZK_NKCodeType = RefCusCodeListTypes.Codes.ManifestValidationRule;
			zzd.ZZD_ZZZ_NKDataGrouping = header.AMA_RN_NKCountry;
			zzd.ZZD_StartDate = ZDateTime.MinSmallDateTimeValue; // lame
			zzd.ZZD_EndDate = ZDateTime.MaxSmallDateTimeValue;
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("MANDATORY", "Desc.", RefCusCodeListTypes.Codes.ManifestValidationRule, header.AMA_RN_NKCountry, RefCusCodeListTypes.Codes.ManifestValidationRule);
			var zze = zzd.Attributes.AddNew("MANDATORY", ZString.Empty);
			Factory.Save();

			var bill = header.Bills.AddNew();
			bill.Validation.ValidateAll();
			AssertHasMessageErrorContaining(bill.ABL_OA_ShipperInfo, zzd.ZZD_Description);
			bill.ABL_OA_Shipper = address.PK;
			bill.Validation.ValidateAll();
			AssertNoMessageErrorContaining(bill.ABL_OA_ShipperInfo, zzd.ZZD_Description);
		}

		public void TestConsignee()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");

			var zzCon = helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestValidationRule, "Consignee", "A Consignee is required", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(zzCon.PK, "MANDATORY", "");
			Factory.Save();

			var address = Factory.LoadTop1<OrgAddress>(new ZQuery());
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.Validation.ValidateAll();
			AssertHasMessageErrorContaining(bill.ABL_OA_ConsigneeInfo, "Consignee");
			bill.ABL_OA_Consignee = address.PK;
			bill.Validation.ValidateAll();
			AssertNoMessageErrorContaining(bill.ABL_OA_ConsigneeInfo, "Consignee");
		}

		public void TestNotifyPartySriLanka()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");

			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, "LK", "Sri Lanka", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var lkMan = helper.CreateNewOrGetExistingCusCodeList("LK", RefCusCodeListTypes.Codes.ManifestValidationRule, "Notify", "A Notify Party is required", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(lkMan.PK, "MANDATORY", "");
			Factory.Save();

			NotifyPartyFieldRequiredRunner("LKCMB");
		}

		public void TestNotifyPartyBangladesh()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");

			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, "BD", "Bangladesh", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var bdMan = helper.CreateNewOrGetExistingCusCodeList("BD", RefCusCodeListTypes.Codes.ManifestValidationRule, "Notify", "A Notify Party is required", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(bdMan.PK, "MANDATORY", "");
			helper.CreateNewOrGetExistingCusCodeListAttribute(bdMan.PK, "MANDATORYCUSCODE", "CCD");
			Factory.Save();

			NotifyPartyFieldRequiredRunner("BDDAC");
		}

		public void TestNotifyPartyOrgCusCodeCodeSriLankaAndBangladesh()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");

			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, "LK", "Sri Lanka", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, "BD", "Bangladesh", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var lkMan = helper.CreateNewOrGetExistingCusCodeList("LK", RefCusCodeListTypes.Codes.ManifestValidationRule, "Notify", "A Notify Party is required", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(lkMan.PK, "MANDATORY", "");
			var bdMan = helper.CreateNewOrGetExistingCusCodeList("BD", RefCusCodeListTypes.Codes.ManifestValidationRule, "Notify", "A Notify Party is required", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(bdMan.PK, "MANDATORY", "");
			helper.CreateNewOrGetExistingCusCodeListAttribute(bdMan.PK, "MANDATORYCUSCODE", "CCD");
			Factory.Save();

			NotifyPartyCusCodeRequiredRunner("LKCMB", false);
			NotifyPartyCusCodeRequiredRunner("BDDAC", true);
		}

		public void TestCheckABL_BillNumber()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "HAB");
			header.FillWithValidTestData();
			var bill = header.Bills.AddNew();
			bill.CustomsEntryNumberType = "LRN";
			bill.CustomsEntryNumber = "LRN111";
			bill.ABL_BillNumber = "X";
			AssertNoMessageErrorContaining(bill.ABL_BillNumberInfo, "You have not entered a Bill Number");
			bill.ABL_BillNumber = "";
			AssertHasMessageErrorContaining(bill.ABL_BillNumberInfo, "You have not entered a Bill Number");
			bill.ABL_BillNumber = "X";
			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "X";
			AssertHasWarningContaining(bill2.ABL_BillNumberInfo, "Bill number must be unique");
			bill2.ABL_BillNumber = "Y";
			AssertNoWarningContaining(bill2.ABL_BillNumberInfo, "Bill number must be unique");
		}

		public void TestCheckABL_GrossWeight()
		{
			const string errorMessage = "cannot be negative.";
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_GrossWeight = 10m;
			bill.ABL_GrossWeightUQ = bill.Lookups.WeightUQList[0].Code;
			AssertNoErrorContaining("Positive", bill.ABL_GrossWeightInfo, errorMessage);
			AssertNoMessageErrorContaining(bill.ABL_GrossWeightInfo, "enter");
			AssertNoMessageErrorContaining(bill.ABL_GrossWeightUQInfo, "enter");
			AssertNoMessageErrorContaining(bill.ABL_GrossWeightUQInfo, "list");
			bill.ABL_GrossWeight = 0m;
			bill.ABL_GrossWeightUQ = "";
			AssertNoErrorContaining("Positive", bill.ABL_GrossWeightInfo, errorMessage);
			AssertHasMessageErrorContaining(bill.ABL_GrossWeightInfo, "enter");
			AssertHasMessageErrorContaining(bill.ABL_GrossWeightUQInfo, "enter");
			bill.ABL_GrossWeightUQ = "X";
			AssertHasMessageErrorContaining(bill.ABL_GrossWeightUQInfo, "list");
			bill.ABL_GrossWeight = -1m;
			AssertHasErrorContaining("Negative", bill.ABL_GrossWeightInfo, errorMessage);
		}

		public void TestCheckABL_NetWeightUQ()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_NetWeightUQ = "X";
			AssertHasMessageErrorContaining(bill.ABL_NetWeightUQInfo, ListValidation.InvalidCodeMessageError);
			bill.ABL_NetWeightUQ = bill.Lookups.WeightUQList[0].Code;
			AssertNoMessageErrors(bill.ABL_NetWeightUQInfo);
		}

		public void TestCheckABL_Volume()
		{
			const string errorMessage = "cannot be negative.";
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_Volume = 10m;
			bill.ABL_VolumeUQ = bill.Lookups.VolumeUQList[0].Code;
			AssertNoErrorContaining("Positive", bill.ABL_VolumeInfo, errorMessage);
			AssertNoMessageErrorContaining(bill.ABL_VolumeInfo, "enter");
			AssertNoMessageErrorContaining(bill.ABL_VolumeUQInfo, "enter");
			AssertNoMessageErrorContaining(bill.ABL_VolumeUQInfo, "list");
			bill.ABL_Volume = 0m;
			bill.ABL_VolumeUQ = "";
			AssertNoErrorContaining("Positive", bill.ABL_VolumeInfo, errorMessage);
			AssertNoMessageErrorContaining(bill.ABL_VolumeInfo, "enter");
			AssertNoMessageErrorContaining(bill.ABL_VolumeUQInfo, "enter");
			bill.ABL_Volume = 10m;
			AssertHasMessageErrorContaining(bill.ABL_VolumeUQInfo, "enter");  // volume demands unit
			bill.ABL_VolumeUQ = "X";
			AssertHasMessageErrorContaining(bill.ABL_VolumeUQInfo, "list");
			AssertNoMessageErrorContaining(bill.ABL_VolumeUQInfo, "enter");
			bill.ABL_Volume = -1m;
			AssertHasErrorContaining("Negative", bill.ABL_VolumeInfo, errorMessage);
		}

		public void TestCheckABL_RL_NKFinalDestination()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");
			var messageError = "A Final Destination is required";
			var rule = helper.CreateNewOrGetExistingCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.FinalDestination, messageError, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(rule.PK, ManifestValidationRuleCodes.Mandatory, ZString.Empty);
			messageError += $" for {GlbCompany.CurrentCompany.GC_RN_NKCountryCode}.";
			Factory.Save();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var bill = header.Bills.AddNew();
			bill.ABL_RL_NKFinalDestination = "USLAX";
			AssertNoMessageError(bill.ABL_RL_NKFinalDestinationInfo, messageError);
			AssertNoMessageErrorContaining(bill.ABL_RL_NKFinalDestinationInfo, ListValidation.InvalidCodeMessageError);
			bill.ABL_RL_NKFinalDestination = ZString.Empty;
			AssertHasMessageError(bill.ABL_RL_NKFinalDestinationInfo, messageError);
			bill.ABL_RL_NKFinalDestination = "!£XXX";
			AssertHasMessageErrorContaining(bill.ABL_RL_NKFinalDestinationInfo, ListValidation.InvalidCodeMessageError);

			header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;

			header.AMA_ManifestType = "MGE";
			bill = header.Bills.AddNew();
			bill.ABL_RL_NKOrigin = "SGSIN";
			bill.ABL_RL_NKFinalDestination = "SGSIN";
			AssertHasMessageError(bill.ABL_RL_NKFinalDestinationInfo, "Destination cannot be a Singapore port for an Export job.");

			bill.ABL_RL_NKFinalDestination = "ZACPT";
			AssertNoMessageError(bill.ABL_RL_NKFinalDestinationInfo, "Destination cannot be a Singapore port for an Export job.");

			header.AMA_ManifestType = "MGI";
			bill.ABL_RL_NKFinalDestination = "ZACPT";
			bill.Validation.ValidateABL_RL_NKFinalDestination();
			AssertHasMessageError(bill.ABL_RL_NKFinalDestinationInfo, "Destination must be a Singapore port for an Import job.");

			bill.ABL_RL_NKFinalDestination = "SGSIN";
			AssertNoMessageError(bill.ABL_RL_NKFinalDestinationInfo, "Destination must be a Singapore port for an Import job.");
		}

		public void TestCheckABL_RL_NKOrigin()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");

			var validationRule = helper.CreateNewOrGetExistingCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.IATAPortOfOrigin, "IATA Code is required for Port of Origin", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(ManifestValidationRuleCodes.Mandatory, "Desc.", RefCusCodeListTypes.Codes.ManifestValidationRule, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, RefCusCodeListTypes.Codes.ManifestValidationRule);
			validationRule.Attributes.AddNew(ManifestValidationRuleCodes.Mandatory, ZString.Empty);
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "US!2#";
			unloco.RL_HasAirport = true;
			unloco.RL_IATA = ZString.Empty;

			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.SouthAfrica, "South Africa", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Fiji, "Fiji", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Vanuatu, "Vanuatu", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.SolomonIslands, "Solomon Islands", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Singapore, "Singapore", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var bill = header.Bills.AddNew();
			bill.ABL_RL_NKOrigin = "";
			bill.Validation.ValidateABL_RL_NKOrigin();
			AssertHasMessageErrorContaining(bill.ABL_RL_NKOriginInfo, "enter");
			bill.ABL_RL_NKOrigin = "!£XXX";
			AssertHasMessageErrorContaining(bill.ABL_RL_NKOriginInfo, "list");
			bill.ABL_RL_NKOrigin = "USLAX";
			AssertNoMessageErrorContaining(bill.ABL_RL_NKOriginInfo, "enter");
			AssertNoMessageErrorContaining(bill.ABL_RL_NKOriginInfo, "list");
			AssertNoMessageErrorContaining(bill.ABL_RL_NKOriginInfo, "IATA Code is required for Port of Origin");
			bill.ABL_RL_NKOrigin = "US!2#";
			AssertNoMessageErrorContaining(bill.ABL_RL_NKOriginInfo, "list");
			AssertNoMessageErrorContaining(bill.ABL_RL_NKOriginInfo, "IATA Code is required for Port of Origin");
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			bill.ABL_RL_NKOrigin = "US!2#";
			bill.Validation.ValidateABL_RL_NKOrigin();
			AssertHasMessageErrorContaining(bill.ABL_RL_NKOriginInfo, "IATA Code is required for Port of Origin");

			header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;

			header.AMA_ManifestType = "MGE";
			bill = header.Bills.AddNew();
			bill.ABL_RL_NKFinalDestination = "SGSIN";
			bill.ABL_RL_NKOrigin = "ZACPT";
			AssertHasMessageError(bill.ABL_RL_NKOriginInfo, "Origin must be a Singapore port for an Export job.");

			bill.ABL_RL_NKOrigin = "SGSIN";
			AssertNoMessageError(bill.ABL_RL_NKOriginInfo, "Origin must be a Singapore port for an Export job.");

			header.AMA_ManifestType = "MGI";
			bill.ABL_RL_NKOrigin = "SGSIN";
			bill.Validation.ValidateABL_RL_NKOrigin();
			AssertHasMessageError(bill.ABL_RL_NKOriginInfo, "Origin cannot be a Singapore port for an Import job.");

			bill.ABL_RL_NKOrigin = "ZACPT";
			AssertNoMessageError(bill.ABL_RL_NKOriginInfo, "Origin cannot be a Singapore port for an Import job.");
		}

		public void TestCheckABL_ManifestQty()
		{
			const string errorMessage = "cannot be negative.";
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_ManifestQty = 10;
			bill.ABL_ManifestUQ = "PKG";
			AssertNoErrorContaining("Positive", bill.ABL_ManifestQtyInfo, errorMessage);
			AssertNoMessageErrorContaining(bill.ABL_ManifestQtyInfo, "enter");
			AssertNoMessageErrorContaining(bill.ABL_ManifestUQInfo, "enter");
			AssertNoMessageErrorContaining(bill.ABL_ManifestUQInfo, "list");
			bill.ABL_ManifestQty = 0;
			bill.ABL_ManifestUQ = "";
			AssertNoErrorContaining("Positive", bill.ABL_ManifestQtyInfo, errorMessage);
			AssertHasMessageErrorContaining(bill.ABL_ManifestQtyInfo, "enter");
			AssertHasMessageErrorContaining(bill.ABL_ManifestUQInfo, "enter");
			bill.ABL_ManifestUQ = "X";
			AssertHasMessageErrorContaining(bill.ABL_ManifestUQInfo, "list");
			bill.ABL_ManifestQty = 10;
			AssertNoWarningContaining(bill.ABL_ManifestUQInfo, "Sum");
			var pack1 = bill.Packs.AddNew();
			pack1.APA_PackQty = 6;
			var pack2 = bill.Packs.AddNew();
			pack2.APA_PackQty = 6;
			bill.Validation.ValidateABL_ManifestQty();
			AssertHasWarningContaining(bill.ABL_ManifestQtyInfo, "Sum");
			AssertHasWarningContaining(bill.ABL_ManifestQtyInfo, "(12)");
			pack2.APA_PackQty = 4;
			bill.Validation.ValidateABL_ManifestQty();
			AssertNoWarningContaining(bill.ABL_ManifestQtyInfo, "Sum");
			AssertNoWarningContaining(bill.ABL_ManifestQtyInfo, "(12)");
			bill.ABL_ManifestQty = -1;
			AssertHasErrorContaining("Negative", bill.ABL_ManifestQtyInfo, errorMessage);
		}

		public void TestCheckDiscountValue()
		{
			var bill = Factory.New<AsycudaBill>();
			AssertValueAndCurrency(bill.DiscountValueInfo, bill.DiscountValueCurrencyInfo);
		}

		public void TestCheckOtherChargesValue()
		{
			var bill = Factory.New<AsycudaBill>();
			AssertValueAndCurrency(bill.OtherChargesValueInfo, bill.OtherChargesValueCurrencyInfo);
		}

		public void TestCheckABL_MarksAndNumbers()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");
			var messageError = "Marks and Numbers is required";
			var validationRule = helper.CreateNewOrGetExistingCusCodeList(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.MarksAndNumbers, messageError, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(validationRule.PK, ManifestValidationRuleCodes.Mandatory, ZString.Empty);
			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var bill = header.Bills.AddNew();
			bill.ABL_MarksAndNumbers = "";
			AssertHasMessageErrorContaining(bill.ABL_MarksAndNumbersInfo, messageError);
			bill.ABL_MarksAndNumbers = "test";
			AssertNoMessageErrorContaining(bill.ABL_MarksAndNumbersInfo, messageError);
		}

		public void TestCheckABL_GoodsDescription()
		{
			var country = Core.Constants.CountryCodes.SouthAfrica;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");
			var messageError = "Goods Description is required";
			var validationRule = helper.CreateNewOrGetExistingCusCodeList(country, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.GoodsDescription, messageError, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(validationRule.PK, ManifestValidationRuleCodes.Mandatory, ZString.Empty);
			Factory.Save();

			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, country, "HAB");
			header.FillWithValidTestData();
			var bill = header.Bills.AddNew();
			bill.ABL_GoodsDescription = "";
			AssertHasMessageErrorContaining(bill.ABL_GoodsDescriptionInfo, messageError);
			bill.ABL_GoodsDescription = "some goods";
			AssertNoMessageErrorContaining(bill.ABL_GoodsDescriptionInfo, messageError);
		}

		public void TestCheckTransportValue()
		{
			var bill = Factory.New<AsycudaBill>();
			AssertValueAndCurrency(bill.ABL_TransportValueInfo, bill.ABL_RX_NKTransportValueCurrencyInfo);
		}

		public void TestCheckFreightValue()
		{
			var bill = Factory.New<AsycudaBill>();
			AssertValueAndCurrency(bill.ABL_FreightValueInfo, bill.ABL_RX_NKFreightValueCurrencyInfo);
		}

		public void TestCheckFreightValue_CheckTotalPackLinePriceValue()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			bill.ABL_FreightValue = 90m;
			bill.ABL_RX_NKFreightValueCurrency = Core.Constants.CurrencyCodes.Singapore;

			var pack1 = bill.Packs.AddNew();
			pack1.LinePrice = 80.51m;
			pack1.LinePriceCurrency = Core.Constants.CurrencyCodes.UnitedStates;

			bill.Validation.ValidateABL_FreightValue();
			var goodsValueCurrencyNotSameWithLinePriceCurrencyMessage = "Cannot verify that the sum of the Pack Lines Price matches the Goods Value as there are in different currency. This may cause a valuation error for Singapore.";
			AssertHasWarning(bill.ABL_FreightValueInfo, goodsValueCurrencyNotSameWithLinePriceCurrencyMessage);
			var goodsValueNotMatchTotalLinePriceMessage = "The Goods Value does not equal the sum of the Pack Lines Price. This may cause a valuation error for Singapore.";
			AssertNoMessageError(bill.ABL_FreightValueInfo, goodsValueNotMatchTotalLinePriceMessage);

			var pack2 = bill.Packs.AddNew();
			pack2.LinePrice = 100.49m;
			pack2.LinePriceCurrency = Core.Constants.CurrencyCodes.UnitedStates;

			bill.Validation.ValidateABL_FreightValue();
			AssertHasWarning(bill.ABL_FreightValueInfo, goodsValueCurrencyNotSameWithLinePriceCurrencyMessage);
			AssertNoMessageError(bill.ABL_FreightValueInfo, goodsValueNotMatchTotalLinePriceMessage);

			bill.ABL_RX_NKFreightValueCurrency = ZString.Empty;
			bill.Validation.ValidateABL_FreightValue();
			AssertNoWarning(bill.ABL_FreightValueInfo, goodsValueCurrencyNotSameWithLinePriceCurrencyMessage);
			AssertNoMessageError(bill.ABL_FreightValueInfo, goodsValueNotMatchTotalLinePriceMessage);

			bill.ABL_RX_NKFreightValueCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			bill.Validation.ValidateABL_FreightValue();
			AssertNoWarning(bill.ABL_FreightValueInfo, goodsValueCurrencyNotSameWithLinePriceCurrencyMessage);
			AssertHasMessageError(bill.ABL_FreightValueInfo, goodsValueNotMatchTotalLinePriceMessage);

			bill.ABL_FreightValue = 181m;
			AssertNoWarning(bill.ABL_FreightValueInfo, goodsValueCurrencyNotSameWithLinePriceCurrencyMessage);
			AssertNoMessageError(bill.ABL_FreightValueInfo, goodsValueNotMatchTotalLinePriceMessage);
		}

		public void TestCheckInsuranceValue()
		{
			var bill = Factory.New<AsycudaBill>();
			AssertValueAndCurrency(bill.ABL_InsuranceValueInfo, bill.ABL_RX_NKInsuranceValueCurrencyInfo);
		}

		public void TestABL_GrossWeightMatchSumOfPacks()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();

			var pack1 = bill.Packs.AddNew();
			pack1.APA_Weight = 4;
			pack1.APA_WeightUQ = Core.Constants.Weight.Kilograms;

			bill.ABL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			bill.ABL_GrossWeight = 5;

			AssertNoMessageErrors(bill.ABL_GrossWeightInfo);
		}

		public void TestABL_VolumeMatchSumOfPacks()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();

			var pack1 = bill.Packs.AddNew();
			pack1.APA_Volume = 4;
			pack1.APA_VolumeUQ = Core.Constants.Volume.Litre;

			bill.ABL_VolumeUQ = Core.Constants.Volume.Litre;
			bill.ABL_Volume = 5;

			AssertNoMessageErrors(bill.ABL_VolumeInfo);
		}

		public void TestValidateAll()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var valueCurrencyInfos = new List<(ZPropertyInfo valueInfo, ZPropertyInfo currencyInfo)>
			{
				(bill.DiscountValueInfo, bill.DiscountValueCurrencyInfo),
				(bill.OtherChargesValueInfo, bill.OtherChargesValueCurrencyInfo)
			};

			CombineAssertions(() =>
			{
				foreach (var (valueInfo, currencyInfo) in valueCurrencyInfos)
				{
					var currencyPropertyName = currencyInfo.Name;
					using (bill.GetValidationSuspender())
					{
						valueInfo.SetValueFromString("1");
					}
					AssertNoNotifications($"No notification on {currencyPropertyName}", currencyInfo);
					bill.Validation.ValidateAll();
					AssertHasMessageErrorContaining($"{currencyPropertyName} has message error about not being entered", currencyInfo, MandatoryValidation.YouHaveNotEntered);
				}
			});
		}

		public static void FindOrMakeRefPackAndCusCodeList(string commerical, string customs, string country, BusinessObjectFactory factory, bool alsoMakeZzRecordToo = true)
		{
			if (alsoMakeZzRecordToo)
			{
				var helper = new UniversalReferenceTestDataHelper(factory);
				helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.PackageTypes, "PackageTypes");
				helper.CreateNewOrGetExistingCusCodeList(country, RefCusCodeListTypes.Codes.PackageTypes, customs, "This is a lame requirement", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			}

			var queryForRefPack = new ZQuery(RefPacksSchema.RP_CustomsCountry, country);
			queryForRefPack.AddToFilter(RefPacksSchema.RP_CustomsPack, customs);
			queryForRefPack.AddToFilter(RefPacksSchema.RP_CommercialPack, commerical);
			var refPack = factory.LoadTop1<CusRefPacks>(queryForRefPack);
			if (refPack == null)
			{
				refPack = factory.New<CusRefPacks>();
				refPack.RP_CommercialPack = commerical;
				refPack.RP_CustomsPack = customs;
				refPack.RP_CustomsCountry = country;
			}
		}

		static void AssertValueAndCurrency(ZPropertyInfo valueInfo, ZPropertyInfo currencyInfo)
		{
			CombineAssertions(() =>
			{
				AssertNoMessageErrorContaining("Value has no not entered message error", valueInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining("Currency has no not entered message error", currencyInfo, MandatoryValidation.YouHaveNotEntered);
				valueInfo.SetValueFromString("10");
				AssertNoMessageErrorContaining("Value is entered so no message error", valueInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining("Currency is required as value is entered", currencyInfo, MandatoryValidation.YouHaveNotEntered);
				currencyInfo.SetValueFromString("GBP");
				AssertNoMessageErrorContaining("Currency is entered no Message error", currencyInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining("Valid Currency no message error", currencyInfo, ListValidation.InvalidCodeError);
				AssertNoErrorContaining("Valid Currency no error", currencyInfo, ListValidation.InvalidCodeError);
				currencyInfo.SetValueFromString("XXX");
				AssertNoMessageErrorContaining("Invalid Currency no message error", currencyInfo, ListValidation.InvalidCodeError);
				AssertHasErrorContaining(currencyInfo, ListValidation.InvalidCodeError);
			});
		}

		void NotifyPartyFieldRequiredRunner(string port)
		{
			var address = Factory.LoadTop1<OrgAddress>(new ZQuery());
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "ER";

			var consol = Factory.New<ForwardingConsol>();
			header.SetParent(consol);

			var bill = header.Bills.AddNew();
			bill.Validation.ValidateAll();
			AssertNoMessageErrorContaining(bill.ABL_OA_NotifyPartyInfo, "Notify Party");

			header.AMA_RL_NKPortOfDischarge = port;
			header.AMA_RN_NKCountry = port.Substring(0, 2);

			bill.Validation.ValidateAll();
			AssertHasMessageErrorContaining(bill.ABL_OA_NotifyPartyInfo, "Notify Party");

			bill.ABL_OA_NotifyParty = address.PK;
			bill.Validation.ValidateAll();
			AssertNoMessageErrorContaining(bill.ABL_OA_NotifyPartyInfo, "Notify Party");
		}

		void NotifyPartyCusCodeRequiredRunner(ZString port, bool expectRequiredWhenCodeIsMissing)
		{
			var address = Factory.LoadTop1<OrgAddress>(new ZQuery());
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "ER";

			var consol = Factory.New<ForwardingConsol>();
			header.SetParent(consol);

			header.AMA_RL_NKPortOfDischarge = port;
			header.AMA_RN_NKCountry = port.Substring(0, 2);

			var bill = header.Bills.AddNew();
			bill.ABL_OA_NotifyParty = address.PK;
			bill.Validation.ValidateAll();

			if (expectRequiredWhenCodeIsMissing)
			{
				AssertHasMessageErrorContaining(bill.ABL_OA_NotifyPartyInfo, "CCD");
			}
			else
			{
				AssertNoMessageErrorContaining(bill.ABL_OA_NotifyPartyInfo, "CCD");
			}
			var ok = address.Header.CustomsCodes.AddNew("CCD", "abc", port.Left(2));
			bill.Validation.ValidateAll();
			AssertNoMessageErrorContaining(bill.ABL_OA_NotifyPartyInfo, "CCD");
		}
	}
}
