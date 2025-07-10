using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using ManifestValidationRuleCodes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeList.ManifestValidationRuleCodes;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;
using universalAlias = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	public class AsycudaManifestHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public virtual void TestCheckSpecificCircumstanceIndicator()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			AssertNoMessageErrorContaining(header.SpecificCircumstanceIndicatorInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(header.SpecificCircumstanceIndicatorInfo, "cannot be used with transport mode");

			header.SpecificCircumstanceIndicator = "X";
			AssertHasMessageErrorContaining(header.SpecificCircumstanceIndicatorInfo, ListValidation.InvalidCodeMessageError);
			header.SpecificCircumstanceIndicator = SpecificCircumstanceList.Codes.A;
			AssertNoMessageErrorContaining(header.SpecificCircumstanceIndicatorInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(header.SpecificCircumstanceIndicatorInfo, "cannot be used with transport mode");

			header.AMA_TransportMode = "SEA";
			header.SpecificCircumstanceIndicator = SpecificCircumstanceList.Codes.A;
			AssertNoMessageErrorContaining(header.SpecificCircumstanceIndicatorInfo, "cannot be used with transport mode");
			header.SpecificCircumstanceIndicator = SpecificCircumstanceList.Codes.C;
			AssertHasMessageErrorContaining(header.SpecificCircumstanceIndicatorInfo, "cannot be used with transport mode");
			AssertHasMessageErrorContaining(header.SpecificCircumstanceIndicatorInfo, "Specific Circumstance Indicator C cannot be used with transport mode SEA");
			header.SpecificCircumstanceIndicator = SpecificCircumstanceList.Codes.D;
			AssertHasMessageErrorContaining(header.SpecificCircumstanceIndicatorInfo, "cannot be used with transport mode");
			AssertHasMessageErrorContaining(header.SpecificCircumstanceIndicatorInfo, "Specific Circumstance Indicator D cannot be used with transport mode SEA");
			header.SpecificCircumstanceIndicator = SpecificCircumstanceList.Codes.E;
			AssertNoMessageErrorContaining(header.SpecificCircumstanceIndicatorInfo, "cannot be used with transport mode");
		}

		public void TestCheckMethodOfPayment()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(universalAlias.RefCusCodeListTypes.Codes.ICSMethodOfPayment, "ICS Method of Payment");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, universalAlias.RefCusCodeListTypes.Codes.ICSMethodOfPayment, "A", "Payment in cash", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			AssertNoMessageErrorContaining(header.MethodOfPaymentInfo, ListValidation.InvalidCodeMessageError);

			header.MethodOfPayment = "X";
			AssertHasMessageErrorContaining(header.MethodOfPaymentInfo, ListValidation.InvalidCodeMessageError);
			header.MethodOfPayment = "A";
			AssertNoMessageErrorContaining(header.MethodOfPaymentInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckSpecialMentions()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(universalAlias.RefCusCodeListTypes.Codes.ICSSpecialMentions, "ICS Special Mentions");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, universalAlias.RefCusCodeListTypes.Codes.ICSSpecialMentions, "10600", "Negotiable Bill of lading 'to order blank endorsed'", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			AssertNoMessageErrorContaining(header.SpecialMentionsInfo, ListValidation.InvalidCodeMessageError);

			header.SpecialMentions = "00000";
			AssertHasMessageErrorContaining(header.SpecialMentionsInfo, ListValidation.InvalidCodeMessageError);
			header.SpecialMentions = "10600";
			AssertNoMessageErrorContaining(header.SpecialMentionsInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckAMA_RN_NKCountry()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeList(universalAlias.RefDataGrouping.Codes.CommonDataGrouping, universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.SouthAfrica, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(universalAlias.RefDataGrouping.Codes.CommonDataGrouping, universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Singapore, Core.Constants.CountryCodes.Singapore, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(universalAlias.RefDataGrouping.Codes.CommonDataGrouping, universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Eritrea, Core.Constants.CountryCodes.Eritrea, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeType(universalAlias.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var zaCustomsOfffice = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, universalAlias.RefCusCodeListTypes.Codes.CustomsOffice, "JHB", "JOHANNESBURG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(zaCustomsOfffice.PK, "Code", "10");

			helper.CreateNewOrGetExistingCusCodeType(universalAlias.RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");
			var sgValidationRule = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Nigeria, universalAlias.RefCusCodeListTypes.Codes.ManifestValidationRule, "OrgProxy", "OrgProxy", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sgValidationRule.PK, "MANDATORY", "");
			helper.CreateNewOrGetExistingCusCodeListAttribute(sgValidationRule.PK, "MANDATORYCUSCODE", "UEN");

			var wcoDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Nigeria, parent: wcoDataGrouping);
			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "~~";
			AssertHasMessageErrorContaining(header.AMA_RN_NKCountryInfo, "list");
			header.AMA_RN_NKCountry = "";
			AssertHasMessageErrorContaining(header.AMA_RN_NKCountryInfo, "enter");
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			AssertNoMessageErrorContaining(header.AMA_RN_NKCountryInfo, "enter");
			AssertNoMessageErrorContaining(header.AMA_RN_NKCountryInfo, "list");

			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.RemoveAndDeleteAll();
			var headerSG = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Nigeria, "MGI");
			AssertHasMessageErrorContaining(headerSG.AMA_RN_NKCountryInfo, "'UEN'");
			GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew("UEN", "11111111", "NG");
			headerSG.Validation.ValidateAMA_RN_NKCountry();
			AssertNoMessageErrorContaining(headerSG.AMA_RN_NKCountryInfo, "'UEN'");
		}

		public void TestCheckAMA_RL_NKPortOfFirstArrival()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeList(universalAlias.RefDataGrouping.Codes.CommonDataGrouping, universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.UnitedStates, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeType(universalAlias.RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");
			var messageError1 = "Port Of First Arrival is required";
			var za1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, universalAlias.RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.PortOfFirstArrival, messageError1, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTransportModeForCusCodeList(za1.PK, Core.Constants.TransportModes.Air);

			messageError1 += " for US.";

			var messageError2 = "IATA is required for Port Of First Arrival";
			var za2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, universalAlias.RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.IATAPortOfFirstArrival, messageError2, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(ManifestValidationRuleCodes.Mandatory, "Desc.", RefCusCodeListTypes.Codes.ManifestValidationRule, Core.Constants.CountryCodes.UnitedStates, RefCusCodeListTypes.Codes.ManifestValidationRule);
			za2.Attributes.AddNew(ManifestValidationRuleCodes.Mandatory, ZString.Empty);
			messageError2 += " for US.";
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "US!2#";
			unloco.RL_HasAirport = true;
			unloco.RL_IATA = ZString.Empty;
			Factory.Save();

			var headerUS = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedStates, "IAM");
			headerUS.FillWithValidTestData();
			headerUS.AMA_TransportMode = Core.Constants.TransportModes.Air;
			headerUS.AMA_RL_NKPortOfFirstArrival = ZString.Empty;
			AssertHasMessageError(headerUS.AMA_RL_NKPortOfFirstArrivalInfo, messageError1);
			AssertNoMessageErrorContaining(headerUS.AMA_RL_NKPortOfFirstArrivalInfo, ListValidation.InvalidCodeMessageError);
			headerUS.AMA_RL_NKPortOfFirstArrival = "Z#@";
			AssertNoMessageError(headerUS.AMA_RL_NKPortOfFirstArrivalInfo, messageError1);
			AssertHasMessageErrorContaining(headerUS.AMA_RL_NKPortOfFirstArrivalInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageError(headerUS.AMA_RL_NKPortOfFirstArrivalInfo, messageError2);
			headerUS.AMA_RL_NKPortOfFirstArrival = "US!2#";
			AssertNoMessageError(headerUS.AMA_RL_NKPortOfFirstArrivalInfo, messageError1);
			AssertNoMessageErrorContaining(headerUS.AMA_RL_NKPortOfFirstArrivalInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageError(headerUS.AMA_RL_NKPortOfFirstArrivalInfo, messageError2);
			headerUS.AMA_RL_NKPortOfFirstArrival = "USLAX";
			AssertNoMessageError(headerUS.AMA_RL_NKPortOfFirstArrivalInfo, messageError1);
			AssertNoMessageErrorContaining(headerUS.AMA_RL_NKPortOfFirstArrivalInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(headerUS.AMA_RL_NKPortOfFirstArrivalInfo, messageError2);

			headerUS.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			headerUS.AMA_RL_NKPortOfFirstArrival = ZString.Empty;
			AssertNoMessageError(headerUS.AMA_RL_NKPortOfFirstArrivalInfo, messageError1);
			AssertNoMessageErrorContaining(headerUS.AMA_RL_NKPortOfFirstArrivalInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(headerUS.AMA_RL_NKPortOfFirstArrivalInfo, messageError2);
			headerUS.AMA_RL_NKPortOfFirstArrival = "US!2#";
			AssertNoMessageError(headerUS.AMA_RL_NKPortOfFirstArrivalInfo, messageError1);
			AssertNoMessageErrorContaining(headerUS.AMA_RL_NKPortOfFirstArrivalInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(headerUS.AMA_RL_NKPortOfFirstArrivalInfo, messageError2);

			var headerAU = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Australia, "ASY");
			headerAU.FillWithValidTestData();
			headerAU.AMA_TransportMode = Core.Constants.TransportModes.Air;
			headerAU.AMA_RL_NKPortOfFirstArrival = ZString.Empty;
			AssertNoMessageError(headerAU.AMA_RL_NKPortOfFirstArrivalInfo, messageError1);
			AssertNoMessageErrorContaining(headerAU.AMA_RL_NKPortOfFirstArrivalInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(headerAU.AMA_RL_NKPortOfFirstArrivalInfo, messageError2);
			headerAU.AMA_RL_NKPortOfFirstArrival = "US!2#";
			AssertNoMessageError(headerAU.AMA_RL_NKPortOfFirstArrivalInfo, messageError1);
			AssertNoMessageErrorContaining(headerAU.AMA_RL_NKPortOfFirstArrivalInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(headerAU.AMA_RL_NKPortOfFirstArrivalInfo, messageError2);
		}

		public void TestCheckAMA_CarrierCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeList(universalAlias.RefDataGrouping.Codes.CommonDataGrouping, universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.UnitedStates, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeType(universalAlias.RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");
			var messageError1 = "Carrier code is required";
			var za1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, universalAlias.RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.CarrierCode, messageError1, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			messageError1 += " for US.";
			helper.CreateTransportModeForCusCodeList(za1.PK, Core.Constants.TransportModes.Air);
			Factory.Save();

			var headerUS = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedStates, "IAM");
			headerUS.FillWithValidTestData();
			headerUS.AMA_TransportMode = Core.Constants.TransportModes.Air;
			headerUS.AMA_CarrierCode = ZString.Empty;
			AssertHasMessageError(headerUS.AMA_CarrierCodeInfo, messageError1);
			headerUS.AMA_CarrierCode = "Z#@";
			AssertNoMessageError(headerUS.AMA_CarrierCodeInfo, messageError1);

			headerUS.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			headerUS.AMA_CarrierCode = ZString.Empty;
			AssertNoMessageErrors(headerUS.AMA_CarrierCodeInfo);

			var headerAU = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Australia, "ASY");
			headerAU.FillWithValidTestData();
			headerAU.AMA_TransportMode = Core.Constants.TransportModes.Air;
			headerAU.AMA_CarrierCode = ZString.Empty;
			AssertNoMessageErrors(headerAU.AMA_CarrierCodeInfo);
		}

		public void TestCheckAMA_DateAtCustomsOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(universalAlias.RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");
			var zaValidationRule = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Nigeria, universalAlias.RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.DateAtCustomsOffice, "Date at Customs Office is required", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTransportModeForCusCodeList(zaValidationRule.PK, Core.Constants.TransportModes.Road);

			var wcoDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Nigeria, parent: wcoDataGrouping);
			Factory.Save();

			var headerZA = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Nigeria, "HAB");

			headerZA.AMA_TransportMode = Core.Constants.TransportModes.Road;
			headerZA.AMA_DateAtCustomsOffice = ZDate.Empty;
			AssertHasMessageErrorContaining(headerZA.AMA_DateAtCustomsOfficeInfo, "Date at Customs Office is required");

			headerZA.AMA_DateAtCustomsOffice = new ZDate(1990, 1, 1);
			AssertNoMessageErrorContaining(headerZA.AMA_DateAtCustomsOfficeInfo, "Date at Customs Office is required");

			headerZA.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			headerZA.AMA_DateAtCustomsOffice = ZDate.Empty;
			AssertNoMessageErrorContaining(headerZA.AMA_DateAtCustomsOfficeInfo, "Date at Customs Office is required");

			headerZA.AMA_TransportMode = Core.Constants.TransportModes.Air;
			headerZA.Validation.ValidateAMA_DateAtCustomsOffice();
			AssertNoMessageErrorContaining(headerZA.AMA_DateAtCustomsOfficeInfo, "Date at Customs Office is required");

			var headerVU = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Vanuatu, "ASY");
			headerVU.AMA_TransportMode = Core.Constants.TransportModes.Road;
			headerVU.AMA_DateAtCustomsOffice = ZDate.Empty;
			AssertNoMessageErrorContaining(headerVU.AMA_DateAtCustomsOfficeInfo, "Date at Customs Office is required");
		}

		void PrepareCheckAMA_CustomsOfficeData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeList(universalAlias.RefDataGrouping.Codes.CommonDataGrouping, universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Vanuatu, Core.Constants.CountryCodes.Vanuatu, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(universalAlias.RefDataGrouping.Codes.CommonDataGrouping, universalAlias.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Nigeria, Core.Constants.CountryCodes.Nigeria, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeType(universalAlias.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var vuVAIR = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Vanuatu, universalAlias.RefCusCodeListTypes.Codes.CustomsOffice, "VAIR", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(vuVAIR.PK, "AIR", "VUVLI");
			var za1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Nigeria, universalAlias.RefCusCodeListTypes.Codes.CustomsOffice, "JHB", "JOHANNESBURG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(za1.PK, "Code", "10");

			helper.CreateNewOrGetExistingCusCodeType(universalAlias.RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");
			var zzValidationRule = helper.CreateNewOrGetExistingCusCodeList(universalAlias.RefDataGrouping.Codes.CommonDataGrouping, universalAlias.RefCusCodeListTypes.Codes.ManifestValidationRule, "OfficeCode", "A Customs Office Code is required", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(zzValidationRule.PK, "MANDATORY", "");
			var zaValidationRule = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Nigeria, universalAlias.RefCusCodeListTypes.Codes.ManifestValidationRule, "OfficeCode", "A Customs Office is required", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(zaValidationRule.PK, "OPTIONAL", "");
			helper.CreateTransportModeForCusCodeList(zaValidationRule.PK, Core.Constants.TransportModes.Road);
			Factory.Save();
		}

		public void TestCheckAMA_CustomsOfficeIfNonAsycuda()
		{
			PrepareCheckAMA_CustomsOfficeData();

			var headerVU = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Vanuatu, "ASY");
			headerVU.AMA_CustomsOffice = "~~";
			AssertHasMessageErrorContaining(headerVU.AMA_CustomsOfficeInfo, "list");
			headerVU.AMA_CustomsOffice = "";
			AssertNoNotifications(headerVU.AMA_CustomsOfficeInfo);
			headerVU.AMA_CustomsOffice = ((CodeDescriptionPairList)headerVU.Lookups.CustomsOffices)[0].Code;
			AssertNoNotifications(headerVU.AMA_CustomsOfficeInfo);

			var headerZA = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Nigeria, "HAB");
			headerZA.Validation.ValidateAMA_CustomsOffice();
			AssertNoNotifications(headerZA.AMA_CustomsOfficeInfo);

			headerZA.AMA_TransportMode = "SEA";
			headerZA.AMA_CustomsOffice = "";
			headerZA.Validation.ValidateAMA_CustomsOffice();
			AssertNoNotifications(headerZA.AMA_CustomsOfficeInfo);

			headerZA.AMA_TransportMode = "ROA";
			headerZA.AMA_CustomsOffice = "";
			headerZA.Validation.ValidateAMA_CustomsOffice();
			AssertNoNotifications(headerZA.AMA_CustomsOfficeInfo);
		}

		public void TestCheckAMA_CustomsOfficeIfTrueAsycuda()
		{
			PrepareCheckAMA_CustomsOfficeData();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var wcoDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Vanuatu, parent: wcoDataGrouping);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Nigeria, parent: wcoDataGrouping);
			Factory.Save();

			var headerVU = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Vanuatu, "ASY");
			headerVU.AMA_CustomsOffice = "~~";
			AssertHasMessageErrorContaining(headerVU.AMA_CustomsOfficeInfo, "list");
			headerVU.AMA_CustomsOffice = "";
			AssertHasMessageErrorContaining(headerVU.AMA_CustomsOfficeInfo, "required");
			headerVU.AMA_CustomsOffice = ((CodeDescriptionPairList)headerVU.Lookups.CustomsOffices)[0].Code;
			AssertNoMessageErrorContaining(headerVU.AMA_CustomsOfficeInfo, "required");
			AssertNoMessageErrorContaining(headerVU.AMA_CustomsOfficeInfo, "list");

			var headerZA = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Nigeria, "HAB");
			headerZA.Validation.ValidateAMA_CustomsOffice();
			AssertNoMessageErrorContaining(headerZA.AMA_CustomsOfficeInfo, "required");

			headerZA.AMA_TransportMode = "SEA";
			headerZA.AMA_CustomsOffice = "";
			headerZA.Validation.ValidateAMA_CustomsOffice();
			AssertNoMessageErrorContaining(headerZA.AMA_CustomsOfficeInfo, "A Customs Office is required for NG");

			headerZA.AMA_TransportMode = "ROA";
			headerZA.AMA_CustomsOffice = "";
			headerZA.Validation.ValidateAMA_CustomsOffice();
			AssertHasMessageErrorContaining(headerZA.AMA_CustomsOfficeInfo, "A Customs Office is required for NG");
		}

		public void TestCheckAMA_NatureIfNonAsycuda()
		{
			PrepareDataForCheckAMA_Nature();

			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Vanuatu, "ASY");
			header.AMA_Nature = "~~~";
			AssertHasMessageErrorContaining(header.AMA_NatureInfo, ListValidation.InvalidCodeMessageError);
			header.AMA_Nature = ZString.Empty;
			AssertNoNotifications(header.AMA_NatureInfo);
			header.AMA_Nature = header.Lookups.Natures[0].Code;
			AssertNoNotifications(header.AMA_NatureInfo);
			header.AMA_RN_NKCountry = "!";
			header.AMA_Nature = ZString.Empty;
			AssertNoMessageErrors(header.AMA_NatureInfo);
		}

		public void TestCheckAMA_NatureIfTrueAsycuda()
		{
			PrepareDataForCheckAMA_Nature();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var wcoDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Vanuatu, parent: wcoDataGrouping);
			Factory.Save();

			var messageError = "A Nature is required for VU.";

			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Vanuatu, "ASY");
			header.AMA_Nature = "~~~";
			AssertHasMessageErrorContaining(header.AMA_NatureInfo, ListValidation.InvalidCodeMessageError);
			header.AMA_Nature = ZString.Empty;
			AssertHasMessageError(header.AMA_NatureInfo, messageError);
			header.AMA_Nature = header.Lookups.Natures[0].Code;
			AssertNoMessageError(header.AMA_NatureInfo, messageError);
			AssertNoMessageErrorContaining(header.AMA_NatureInfo, ListValidation.InvalidCodeMessageError);
			header.AMA_RN_NKCountry = "!";
			header.AMA_Nature = ZString.Empty;
			AssertNoMessageErrors(header.AMA_NatureInfo);
		}

		public void TestCheckAMA_ManifestType()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_ManifestType = nameof(ManifestDocumentType.COH);
			var billRequiredMessage = "At least one bill (shipment) is required.";
			AssertHasMessageError(header.AMA_ManifestTypeInfo, billRequiredMessage);
			var bill = header.Bills.AddNew();
			header.Validation.ValidateAMA_ManifestType();
			AssertNoMessageError(header.AMA_ManifestTypeInfo, billRequiredMessage);

			var errorMessage = "At least one container must be captured for Manifest Type ";
			var messageError = errorMessage + header.AMA_ManifestType;
			AssertHasMessageError(header.AMA_ManifestTypeInfo, messageError);
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertNoMessageError(header.AMA_ManifestTypeInfo, messageError);

			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_ManifestType = nameof(ManifestDocumentType.COM);
			messageError = errorMessage + header.AMA_ManifestType;
			AssertHasMessageError(header.AMA_ManifestTypeInfo, messageError);
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertNoMessageError(header.AMA_ManifestTypeInfo, messageError);

			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_ManifestType = nameof(ManifestDocumentType.ECL);
			messageError = errorMessage + header.AMA_ManifestType;
			AssertHasMessageError(header.AMA_ManifestTypeInfo, messageError);
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertNoMessageError(header.AMA_ManifestTypeInfo, messageError);

			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_ManifestType = nameof(ManifestDocumentType.ALM);
			messageError = errorMessage + header.AMA_ManifestType;
			AssertHasMessageError(header.AMA_ManifestTypeInfo, messageError);
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertNoMessageError(header.AMA_ManifestTypeInfo, messageError);

			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_ManifestType = nameof(ManifestDocumentType.ALH);
			messageError = errorMessage + header.AMA_ManifestType;
			AssertHasMessageError(header.AMA_ManifestTypeInfo, messageError);
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertNoMessageError(header.AMA_ManifestTypeInfo, messageError);

			header.AMA_ManifestType = "ZZZ";
			AssertHasError(header.AMA_ManifestTypeInfo, "Enter a valid Manifest Type.");
		}

		public void TestValidateShippingAgentComesFromZzRules()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(universalAlias.RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");
			var rule = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ethiopia, universalAlias.RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.ShippingAgent, "Yo mama's hot", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(ManifestValidationRuleCodes.Mandatory, "Desc.", universalAlias.RefCusCodeListTypes.Codes.ManifestValidationRule, Core.Constants.CountryCodes.Ethiopia, universalAlias.RefCusCodeListTypes.Codes.ManifestValidationRule);
			rule.Attributes.AddNew(ManifestValidationRuleCodes.Mandatory, ZString.Empty);
			var wcoDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ethiopia, parent: wcoDataGrouping);
			Factory.Save();

			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Ethiopia, "ASY");
			header.Validation.ValidateAMA_OA_ShippingAgent();
			AssertHasMessageErrorContaining(header.AMA_OA_ShippingAgentInfo, "Yo mama's hot for ET.");
			header.AMA_RN_NKCountry = "ER";
			header.Validation.ValidateAMA_OA_ShippingAgent();
			AssertNoMessageErrorContaining(header.AMA_OA_ShippingAgentInfo, "Yo mama's hot for ET.");
			header.AMA_RN_NKCountry = "ET";
			header.AMA_OA_ShippingAgent = ZGuid.NewZGuid();
			AssertNoMessageErrorContaining(header.AMA_OA_ShippingAgentInfo, "Yo mama's hot for ET.");
		}

		public void TestCheckAMA_MasterBill()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_MasterBill = ZString.Empty;
			AssertHasMessageError(header.AMA_MasterBillInfo, ValidationConstants.ManifestNumberIsRequired(header.MasterBillLabel.Caption));
			header.AMA_MasterBill = "MB1";
			AssertNoMessageErrors(header.AMA_MasterBillInfo);
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			header.AMA_MasterBill = ZString.Empty;
			AssertNoMessageErrors(header.AMA_MasterBillInfo);

			header.AMA_TransportMode = "AIR";
			header.AMA_MasterBill = "123-12345629";
			AssertHasWarningContaining(header.AMA_MasterBillInfo, "Invalid check digit. The last digit should be '0'");
			header.AMA_MasterBill = "123-12345620";
			AssertNoWarningContaining(header.AMA_MasterBillInfo, "Invalid check digit. The last digit should be '0'");
			header.AMA_TransportMode = "SEA";
			header.AMA_MasterBill = "123-12345629";
			AssertNoWarningContaining(header.AMA_MasterBillInfo, "Invalid check digit. The last digit should be '0'");
			header.AMA_TransportMode = "AIR";
			header.Validation.ValidateAMA_MasterBill();
			AssertHasWarningContaining(header.AMA_MasterBillInfo, "Invalid check digit. The last digit should be '0'");
		}

		public void TestCarrierAndRadioSignFromZzDatabase()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");
			var validationRule = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.ZZVESSELANDCARRIER, "Lame", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			var attributeCarrier = helper.CreateCusCodeListAttribute(validationRule.PK, ManifestValidationRuleCodes.MANDATORYZZQUALITY, ManifestValidationRuleCodes.Carrier);
			helper.CreateTransportModeForCusCodeAttribute(attributeCarrier.PK, Core.Constants.TransportModes.Sea);
			var attributeRadio = helper.CreateCusCodeListAttribute(validationRule.PK, ManifestValidationRuleCodes.MANDATORYZZQUALITY, ManifestValidationRuleCodes.RadioCallSign);

			var globalVessel = Factory.New<RefVessel>();
			globalVessel.RV_Code = "HMS Liana";
			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "XX";

			RunCarrierAndRadioValidation(header, header.AMA_VesselNameInfo, false, false, false);
			RunCarrierAndRadioValidation(header, header.AMA_OA_CarrierInfo, false, false, false);

			header.AMA_RN_NKCountry = validationRule.ZZD_ZZZ_NKDataGrouping;
			RunCarrierAndRadioValidation(header, header.AMA_VesselNameInfo, false, false, false);
			RunCarrierAndRadioValidation(header, header.AMA_OA_CarrierInfo, false, false, false);

			header.AMA_TransportMode = "SEA";
			RunCarrierAndRadioValidation(header, header.AMA_VesselNameInfo, false, false, false);
			RunCarrierAndRadioValidation(header, header.AMA_OA_CarrierInfo, false, false, false);

			header.AMA_VesselName = globalVessel.RV_Code;
			RunCarrierAndRadioValidation(header, header.AMA_VesselNameInfo, true, true, false);
			RunCarrierAndRadioValidation(header, header.AMA_OA_CarrierInfo, false, true, false);

			globalVessel.RV_RadioCallSign = "SOS";
			RunCarrierAndRadioValidation(header, header.AMA_VesselNameInfo, false, true, false);
			RunCarrierAndRadioValidation(header, header.AMA_OA_CarrierInfo, false, true, false);

			header.AMA_OA_Carrier = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
			RunCarrierAndRadioValidation(header, header.AMA_VesselNameInfo, false, true, false);
			RunCarrierAndRadioValidation(header, header.AMA_OA_CarrierInfo, false, true, false);

			header.Carrier.Header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "ABC123", "XX");
			RunCarrierAndRadioValidation(header, header.AMA_VesselNameInfo, false, true, false);
			RunCarrierAndRadioValidation(header, header.AMA_OA_CarrierInfo, false, true, false);

			header.Carrier.Header.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "ABC123", validationRule.ZZD_ZZZ_NKDataGrouping);
			RunCarrierAndRadioValidation(header, header.AMA_VesselNameInfo, false, false, false);
			RunCarrierAndRadioValidation(header, header.AMA_OA_CarrierInfo, false, false, false);

			header.Carrier.Header.CustomsCodes.RemoveAndDeleteAll();
			globalVessel.RV_RadioCallSign = "";
			var zzVessel = helper.CreateVesselZZ(globalVessel.RV_Code, globalVessel.RV_RadioCallSign, globalVessel.RV_VesselType, validationRule.ZZD_ZZZ_NKDataGrouping);
			Factory.Save();

			header = new BusinessObjectFactory().Load<AsycudaManifestHeader>(header.PK); // releases factory cache of ZZ tables issue.
			RunCarrierAndRadioValidation(header, header.AMA_VesselNameInfo, true, false, false);
			RunCarrierAndRadioValidation(header, header.AMA_OA_CarrierInfo, false, false, false);

			zzVessel.ZZO_RadioCallSign = "SOS";
			zzVessel.Factory.Save();
			header = new BusinessObjectFactory().Load<AsycudaManifestHeader>(header.PK);
			RunCarrierAndRadioValidation(header, header.AMA_VesselNameInfo, true, false, false);
			RunCarrierAndRadioValidation(header, header.AMA_OA_CarrierInfo, false, false, false);

			var zzCarrier = helper.CreateCarrierCode("MSC", "Lame", "DE");
			var zzPivot = helper.CreateCarrierVesselPivot(zzCarrier.PK, zzVessel.PK);
			Factory.Save();
			header = new BusinessObjectFactory().Load<AsycudaManifestHeader>(header.PK);
			// We have no ZZ carrier, but there are no ZZ carriers at all for this coutnry, so show no carrier validation
			RunCarrierAndRadioValidation(header, header.AMA_VesselNameInfo, true, false, false);
			RunCarrierAndRadioValidation(header, header.AMA_OA_CarrierInfo, false, false, false);

			// Add some ZZ carrier data for this country (but unrelated to our manifest) - show carrier validation
			var globalVesselUnrelated = Factory.New<RefVessel>();
			globalVesselUnrelated.RV_Code = "RMS John Locke";
			var zzVesselUnrelated = Factory.New<RefVesselZZ>();
			zzVesselUnrelated.ZZO_Code = globalVesselUnrelated.RV_Code;
			zzVesselUnrelated.ZZO_ZZZ_NKDataGrouping = validationRule.ZZD_ZZZ_NKDataGrouping;
			var zzCarrierUnrelated = Factory.New<RefCarrierCode>();
			var zzPivotUnreleated = Factory.New<RefCarrierVesselPivot>();
			zzPivotUnreleated.ZZQ_ZZO = zzVesselUnrelated.PK;
			zzPivotUnreleated.ZZQ_ZZ4 = zzCarrierUnrelated.PK;
			zzCarrierUnrelated.ZZ4_Code = "JL";
			zzCarrierUnrelated.ZZ4_Description = "Lame 2";
			zzCarrierUnrelated.ZZ4_ZZZ_NKDataGrouping = validationRule.ZZD_ZZZ_NKDataGrouping;
			Factory.Save();
			header = new BusinessObjectFactory().Load<AsycudaManifestHeader>(header.PK);
			RunCarrierAndRadioValidation(header, header.AMA_VesselNameInfo, true, false, true);
			RunCarrierAndRadioValidation(header, header.AMA_OA_CarrierInfo, false, false, true);

			// Now change the ZZ carrier to be in the right country, and we'll not have any validation
			zzCarrier.ZZ4_ZZZ_NKDataGrouping = validationRule.ZZD_ZZZ_NKDataGrouping;
			Factory.Save();
			header = new BusinessObjectFactory().Load<AsycudaManifestHeader>(header.PK);
			RunCarrierAndRadioValidation(header, header.AMA_VesselNameInfo, true, false, false);
			RunCarrierAndRadioValidation(header, header.AMA_OA_CarrierInfo, false, false, false);

			// No carrier data, but no carrier rule --> no validation
			zzPivot.Delete();
			zzCarrier.Delete();
			zzPivotUnreleated.Delete();
			zzCarrierUnrelated.Delete();
			attributeCarrier.Delete();
			Factory.Save();
			header.AMA_OA_Carrier = ZGuid.Empty;
			header.Factory.Save();
			header = new BusinessObjectFactory().Load<AsycudaManifestHeader>(header.PK);
			RunCarrierAndRadioValidation(header, header.AMA_VesselNameInfo, true, false, false);
			RunCarrierAndRadioValidation(header, header.AMA_OA_CarrierInfo, false, false, false);

			// No vessel data, but no vessel rule --> no validation
			zzVessel.Delete();
			zzVesselUnrelated.Delete();
			Factory.Save();
			globalVessel.RV_RadioCallSign = "";
			attributeRadio.Delete();
			Factory.Save();
			header = new BusinessObjectFactory().Load<AsycudaManifestHeader>(header.PK);
			RunCarrierAndRadioValidation(header, header.AMA_VesselNameInfo, false, false, false);
			RunCarrierAndRadioValidation(header, header.AMA_OA_CarrierInfo, false, false, false);
		}

		public void TestCheckAMA_OA_Carrier()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_OA_Carrier = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
			AssertNoWarningContaining(header.AMA_OA_CarrierInfo, "not entered");
			header.AMA_OA_Carrier = ZGuid.Empty;
			AssertHasWarningContaining(header.AMA_OA_CarrierInfo, "not entered");
		}

		public virtual void TestCheckAMA_RN_NKConveyanceNationality()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = "SEA";
			header.AMA_RN_NKConveyanceNationality = "XX";
			AssertHasMessageErrorContaining(header.AMA_RN_NKConveyanceNationalityInfo, "list");
			header.AMA_RN_NKConveyanceNationality = "";
			AssertHasWarningContaining(header.AMA_RN_NKConveyanceNationalityInfo, "not entered");
			header.AMA_RN_NKConveyanceNationality = "GB";
			AssertNoMessageErrorContaining(header.AMA_RN_NKConveyanceNationalityInfo, "list");
			AssertNoWarningContaining(header.AMA_RN_NKConveyanceNationalityInfo, "not entered");
			header.AMA_TransportMode = "AIR";
			header.AMA_RN_NKConveyanceNationality = "";
			AssertNoWarningContaining(header.AMA_RN_NKConveyanceNationalityInfo, "not entered");
		}

		public virtual void TestCheckAMA_RN_NKConveyanceNationalityForAsycudaManifests()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Turkey, "ASY");
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_RN_NKConveyanceNationality = "XX";
			AssertHasMessageErrorContaining(header.AMA_RN_NKConveyanceNationalityInfo, "list");

			header.AMA_RN_NKConveyanceNationality = "";
			AssertHasWarning(header.AMA_RN_NKConveyanceNationalityInfo, "You have not entered a Conveyance Country/Region.");
			header.AMA_RN_NKConveyanceNationality = "TW";
			AssertNoNotifications(header.AMA_RN_NKConveyanceNationalityInfo);

			var headerNZ = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.NewZealand, "OCR");
			headerNZ.AMA_TransportMode = Core.Constants.TransportModes.Air;
			headerNZ.AMA_RN_NKConveyanceNationality = "";
			AssertNoNotifications("Validation should not be applied to other non Asycuda manifest countries", headerNZ.AMA_RN_NKConveyanceNationalityInfo);
		}

		public void TestCheckAMA_Voyage()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_Voyage = "X";
			var message = MandatoryValidation.YouHaveNotEnteredMessage(header.VoyageFlightNoLabel.Caption);
			AssertNoMessageErrors(header.AMA_VoyageInfo);
			header.AMA_Voyage = "";
			AssertHasMessageError(header.AMA_VoyageInfo, message);
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_Voyage = "X";
			AssertHasMessageError(header.AMA_VoyageInfo, ValidationConstants.FlightNumberDoesNotStartWithAValidIATAAirCode);
			header.AMA_Voyage = "QF";
			AssertNoMessageErrors(header.AMA_VoyageInfo);
		}

		public void TestVoyageTransportModeIsROA()
		{
			var manifest = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "HAB");
			var bill = manifest.Bills.AddNew();
			manifest.AMA_TransportMode = "ROA";
			manifest.AMA_Voyage = ZString.Empty;
			manifest.AMA_ManifestType = nameof(ManifestDocumentType.COH);

			var errorCollector = manifest.ValidateEverythingAndSummariseProblems();
			AssertNotContains("Voyage/Flight", errorCollector);
		}

		public virtual void TestCheckAMA_VesselName()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_VesselName = "X";
			AssertHasWarning(header.AMA_VesselNameInfo, "The Vessel Name entered does not exist in the reference file.");

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "Vessel";
			header.AMA_VesselName = vessel.RV_Code;
			AssertNoWarning(header.AMA_VesselNameInfo, "The Vessel Name entered does not exist in the reference file.");
			// For more functionality, see method TestCarrierAndRadioSignFromZzDatabase()
		}

		public void TestCheckAMA_JobReference()
		{
			GlbStaff.CurrentUser.GS_EmailAddress = "foo@bar.com";
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.Validation.ValidateAMA_JobReference();
			AssertNoErrorContaining(header.AMA_JobReferenceInfo, "enter");
			header.AMA_JobReference = "X";
			AssertNoErrorContaining(header.AMA_JobReferenceInfo, "enter");
			Factory.Save();
			header.AMA_JobReference = "";
			AssertHasErrorContaining(header.AMA_JobReferenceInfo, "enter");
			header.AMA_JobReference = "X";
			AssertNoErrorContaining(header.AMA_JobReferenceInfo, "enter");
		}

		public void TestTransportModePortValidation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.SouthAfrica, "South Africa", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Fiji, "Fiji", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Vanuatu, "Vanuatu", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.SolomonIslands, "Solomon Islands", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var air = "ZAR#@";
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = air;
			unloco.RL_PortName = air + " NAME";
			unloco.RL_HasAirport = true;
			var sea = "FJE%#";
			unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = sea;
			unloco.RL_PortName = sea + " NAME";
			unloco.RL_HasSeaport = true;
			var road = "VUL$#";
			unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = road;
			unloco.RL_PortName = road + " NAME";
			unloco.RL_HasRoad = true;
			var mail = "SBH$#";
			unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = mail;
			unloco.RL_PortName = mail + " NAME";
			unloco.RL_HasPost = true;
			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_RL_NKPortOfLoading = sea;
			header.AMA_RL_NKPortOfDischarge = road;
			header.Validation.ValidateAMA_TransportMode();
			AssertHasWarningContaining(header.AMA_RL_NKPortOfLoadingInfo, "cannot be used for");
			AssertHasWarningContaining(header.AMA_RL_NKPortOfDischargeInfo, "cannot be used for");

			header.AMA_RL_NKPortOfLoading = air;
			AssertNoWarning(header.AMA_RL_NKPortOfLoadingInfo, "cannot be used for");

			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_RL_NKPortOfLoading = air;
			header.AMA_RL_NKPortOfDischarge = road;
			header.Validation.ValidateAMA_TransportMode();
			AssertHasWarningContaining(header.AMA_RL_NKPortOfLoadingInfo, "cannot be used for");
			AssertHasWarningContaining(header.AMA_RL_NKPortOfDischargeInfo, "cannot be used for");

			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			header.AMA_RL_NKPortOfLoading = air;
			header.AMA_RL_NKPortOfDischarge = mail;
			header.Validation.ValidateAMA_TransportMode();
			AssertHasWarningContaining(header.AMA_RL_NKPortOfLoadingInfo, "cannot be used for");
			AssertHasWarningContaining(header.AMA_RL_NKPortOfDischargeInfo, "cannot be used for");

			header.AMA_TransportMode = Core.Constants.TransportModes.Mail;
			header.AMA_RL_NKPortOfLoading = air;
			header.AMA_RL_NKPortOfDischarge = road;
			header.Validation.ValidateAMA_TransportMode();
			AssertHasWarningContaining(header.AMA_RL_NKPortOfLoadingInfo, "cannot be used for");
			AssertHasWarningContaining(header.AMA_RL_NKPortOfDischargeInfo, "cannot be used for");
		}

		public void TestCheckAMA_VehicleRegistration()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			header.AMA_VehicleRegistration = "";
			AssertHasMessageErrorContaining(header.AMA_VehicleRegistrationInfo, "Vehicle Registration Number is mandatory for Transport Mode Road");

			header.AMA_VehicleRegistration = "ABC123";
			AssertNoMessageErrorContaining(header.AMA_VehicleRegistrationInfo, "Vehicle Registration Number is mandatory for Transport Mode Road");
		}

		public void MandatoryFields()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RL_NKPortOfLoading = "";
			header.AMA_RL_NKPortOfDischarge = "";
			header.AMA_E_ARV = ZDateTime.Empty;

			AssertHasMessageErrorContaining(header.AMA_RL_NKPortOfLoadingInfo, "mandatory");
			AssertHasMessageErrorContaining(header.AMA_RL_NKPortOfDischargeInfo, "mandatory");
			AssertHasMessageErrorContaining(header.AMA_E_ARVInfo, "mandatory");

			header.AMA_RL_NKPortOfLoading = "USLAX";
			header.AMA_RL_NKPortOfDischarge = "AUSYD";
			header.AMA_E_ARV = ZDateTime.Now;

			AssertNoMessageErrorContaining(header.AMA_RL_NKPortOfLoadingInfo, "mandatory");
			AssertNoMessageErrorContaining(header.AMA_RL_NKPortOfDischargeInfo, "mandatory");
			AssertNoMessageErrorContaining(header.AMA_E_ARVInfo, "mandatory");
		}

		void PrepareDataForCheckAMA_Nature()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(universalAlias.RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");
			var rule = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Vanuatu, universalAlias.RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.Nature, "A Nature is required ", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(rule.PK, ManifestValidationRuleCodes.Mandatory, "");
			Factory.Save();
		}

		void RunCarrierAndRadioValidation(AsycudaManifestHeader header, ZPropertyInfo zpi, bool expectRadioErrorForMissingZzVessel, bool expectCccErrorForMissingZzVessel, bool expectCarrierErrorForExtantZzVessel)
		{
			header.Validation.ValidateAMA_VesselName();
			header.Validation.ValidateAMA_OA_Carrier();

			if (expectRadioErrorForMissingZzVessel)
			{
				AssertHasMessageError(zpi, "The vessel has no radio call sign to default to this manifest. Please enter the radio call sign here for this manifest. (And update the vessel reference details if desired).");
			}
			else
			{
				AssertNoMessageError(zpi, "The vessel has no radio call sign to default to this manifest. Please enter the radio call sign here for this manifest. (And update the vessel reference details if desired).");
			}

			if (expectCccErrorForMissingZzVessel)
			{
				AssertHasMessageErrorContaining(zpi, "No carrier code can be determined. There is no related");
			}
			else
			{
				AssertNoMessageErrorContaining(zpi, "No carrier code can be determined. There is no related");
			}

			if (expectCarrierErrorForExtantZzVessel)
			{
				AssertHasMessageErrorContaining(zpi, "No carrier code can be determined. The related");
			}
			else
			{
				AssertNoMessageErrorContaining(zpi, "No carrier code can be determined. The related");
			}
		}
	}
}
