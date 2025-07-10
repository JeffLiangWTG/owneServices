using System;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.Testing;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	sealed class AsycudaBillValidationForRegularBillTest : AsycudaBillValidationAbstractTest
	{
		public void TestABL_ConsigneeRegNoAddsError_WhenNameStreetPostcodeCityCountryIsBlank()
		{
			SetUpTestData();
			bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertHasMessageError(bill.ABL_ConsigneeRegNoInfo, c0617RuleImporterIdentificationNumberMessage);

			bill.ABL_ConsigneeRegNo = "AAA";
			bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertNoMessageErrors("No message errors as consignee reg no is populated", bill.ABL_ConsigneeRegNoInfo);

			bill.ABL_ConsigneeRegNo = ZString.Empty;
			bill.ABL_ConsigneeName = "B";
			bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertHasMessageError(bill.ABL_ConsigneeRegNoInfo, c0617RuleImporterIdentificationNumberMessage);

			bill.ABL_ConsigneePostcode = "C";
			bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertHasMessageError(bill.ABL_ConsigneeRegNoInfo, c0617RuleImporterIdentificationNumberMessage);

			bill.ABL_ConsigneeCity = "D";
			bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertHasMessageError(bill.ABL_ConsigneeRegNoInfo, c0617RuleImporterIdentificationNumberMessage);

			bill.ABL_RN_NKConsigneeCountry = "E";
			bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertHasMessageError(bill.ABL_ConsigneeRegNoInfo, c0617RuleImporterIdentificationNumberMessage);

			bill.ABL_ConsigneeStreet1 = "F";
			bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertNoMessageErrors("No message errors as all the required fields are filled (street requirement fulfilled)", bill.ABL_ConsigneeRegNoInfo);

			bill.ABL_ConsigneeStreet2 = "G";
			bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertNoMessageErrors("No message errors as all the required fields are filled (street requirement still fulfilled)", bill.ABL_ConsigneeRegNoInfo);

			bill.ABL_ConsigneeStreet1 = ZString.Empty;
			bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertNoMessageErrors("No message errors as all the required fields are filled (street requirement still fulfilled)", bill.ABL_ConsigneeRegNoInfo);

			bill.ABL_ConsigneeStreet2 = ZString.Empty;
			bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertHasMessageError(bill.ABL_ConsigneeRegNoInfo, c0617RuleImporterIdentificationNumberMessage);
		}

		public void TestABL_ConsigneeRegNo_ValidatesAccordingToRegNoTypes()
		{
			SetUpTestData();
			bill.ABL_ConsigneeRegNoType = ImporterIdentificationTypes.Codes.PYE;
			bill.ABL_ConsigneeRegNo = "ABCDE1234";
			bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertNoMessageErrors("No message errors for PYE with 9 Length alphanumeric RegNo", bill.ABL_ConsigneeRegNoInfo);

			bill.ABL_ConsigneeRegNoType = ImporterIdentificationTypes.Codes.PYE;
			bill.ABL_ConsigneeRegNo = "ABCDE12345";
			bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertHasMessageError(bill.ABL_ConsigneeRegNoInfo, br3181RulePYEFormatMessage);

			bill.ABL_ConsigneeRegNoType = ImporterIdentificationTypes.Codes.ITX;
			bill.ABL_ConsigneeRegNo = "@BCD 12345";
			bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertHasMessageError(bill.ABL_ConsigneeRegNoInfo, br3181RuleITXFormatMessage);

			bill.ABL_ConsigneeRegNoType = ImporterIdentificationTypes.Codes.CGT;
			bill.ABL_ConsigneeRegNo = "@t3stst1nrgn!";
			bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertHasMessageError(bill.ABL_ConsigneeRegNoInfo, br3181RuleCGTFormatMessage);
		}

		public void TestABL_RN_NKConsigneeCountry()
		{
			SetUpTestData();
			AddAustraliaToIrelandCusCodeList();

			bill.ABL_RN_NKConsigneeCountry = "AU";
			bill.Validation.ValidateABL_RN_NKConsigneeCountry();
			AssertNoMessageErrors("No message errors as AU is in the list", bill.ABL_RN_NKConsigneeCountryInfo);

			bill.ABL_RN_NKConsigneeCountry = "12";
			bill.Validation.ValidateABL_RN_NKConsigneeCountry();
			AssertHasMessageError(bill.ABL_RN_NKConsigneeCountryInfo, invalidCodeMessageError);
		}

		public void TestABL_ShipmentType()
		{
			SetUpTestData();
			bill.ABL_ShipmentType = "ZZZ";
			AssertListValidationInvalidCodeError(bill.ABL_ShipmentTypeInfo, isExpectingError: true);
		}

		public void TestCheckABL_RN_NKShipperCountry()
		{
			SetUpTestData();
			AddAustraliaToIrelandCusCodeList();

			bill.Validation.ValidateABL_RN_NKShipperCountry();
			AssertNoMessageErrors("No message errors as it is not mandatory for IE AIS", bill.ABL_RN_NKShipperCountryInfo);

			bill.ABL_RN_NKShipperCountry = "AU";
			bill.Validation.ValidateABL_RN_NKShipperCountry();
			AssertNoMessageErrors("No message errors as AU is in the list", bill.ABL_RN_NKShipperCountryInfo);

			bill.ABL_RN_NKShipperCountry = "12";
			bill.Validation.ValidateABL_RN_NKShipperCountry();
			AssertHasMessageError(bill.ABL_RN_NKShipperCountryInfo, invalidCodeMessageError);
		}

		public void TestCheckABL_RN_NKShipperCountry_AddsErrorMessage()
		{
			AddAustraliaToIrelandCusCodeList();
			SetUpTestData();

			bill.ABL_ShipperRegNo = "123456";
			bill.ABL_RN_NKShipperCountry = "";
			bill.Validation.ValidateABL_RN_NKShipperCountry();
			AssertHasMessageError("Should have error message if empty and exporter id no is added ", bill.ABL_RN_NKShipperCountryInfo, c0617RuleExporterCountryMessage);

			bill.ABL_RN_NKShipperCountry = "AU";
			bill.Validation.ValidateABL_RN_NKShipperCountry();
			AssertNoMessageErrors("Have no error messages if entered", bill.ABL_RN_NKShipperCountryInfo);

			bill.ABL_ShipperRegNo = "";
			bill.ABL_ShipperName = "Exporter Name";
			bill.ABL_RN_NKShipperCountry = "";
			bill.Validation.ValidateABL_RN_NKShipperCountry();
			AssertHasMessageError("Should have error message if empty and exporter name is added ", bill.ABL_RN_NKShipperCountryInfo, c0617RuleExporterCountryMessage);

			bill.ABL_RN_NKShipperCountry = "AU";
			bill.Validation.ValidateABL_RN_NKShipperCountry();
			AssertNoMessageErrors("Have no error messages if entered ", bill.ABL_RN_NKShipperCountryInfo);
		}

		public void TestCheckABL_ShipperRegNo()
		{
			SetUpTestData(headerApplicationCode: "LV2");

			bill.Validation.ValidateABL_ShipperRegNo();
			AssertHasMessageError(bill.ABL_ShipperRegNoInfo, br3006RuleMessage);

			bill.ABL_ShipperRegNo = "AAA";
			bill.Validation.ValidateABL_ShipperRegNo();
			AssertNoMessageErrors("No message errors as shipper reg no is populated", bill.ABL_ShipperRegNoInfo);

			bill.ABL_ShipperRegNo = ZString.Empty;
			bill.ABL_ShipperName = "B";
			bill.Validation.ValidateABL_ShipperRegNo();
			AssertHasMessageError(bill.ABL_ShipperRegNoInfo, br3006RuleMessage);

			bill.ABL_ShipperPostcode = "C";
			bill.Validation.ValidateABL_ShipperRegNo();
			AssertHasMessageError(bill.ABL_ShipperRegNoInfo, br3006RuleMessage);

			bill.ABL_ShipperCity = "D";
			bill.Validation.ValidateABL_ShipperRegNo();
			AssertHasMessageError(bill.ABL_ShipperRegNoInfo, br3006RuleMessage);

			bill.ABL_RN_NKShipperCountry = "E";
			bill.Validation.ValidateABL_ShipperRegNo();
			AssertHasMessageError(bill.ABL_ShipperRegNoInfo, br3006RuleMessage);

			bill.ABL_ShipperStreet1 = "F";
			bill.Validation.ValidateABL_ShipperRegNo();
			AssertNoMessageErrors("No message errors as all the required fields are filled (street requirement fulfilled)", bill.ABL_ShipperRegNoInfo);

			bill.ABL_ShipperStreet2 = "G";
			bill.Validation.ValidateABL_ShipperRegNo();
			AssertNoMessageErrors("No message errors as all the required fields are filled (street requirement still fulfilled)", bill.ABL_ShipperRegNoInfo);

			bill.ABL_ShipperStreet1 = ZString.Empty;
			bill.Validation.ValidateABL_ShipperRegNo();
			AssertNoMessageErrors("No message errors as all the required fields are filled (street requirement still fulfilled)", bill.ABL_ShipperRegNoInfo);

			bill.ABL_ShipperStreet2 = ZString.Empty;
			bill.Validation.ValidateABL_ShipperRegNo();
			AssertHasMessageError(bill.ABL_ShipperRegNoInfo, br3006RuleMessage);
		}

		public void TestCheckABL_ShipperNameValidatesABL_ShipperRegNo_WhenApplicationCodeLv2()
		{
			SetUpTestData(headerApplicationCode: "LV2");

			bill.ABL_ShipperPostcode = "B";
			bill.ABL_ShipperCity = "C";
			bill.ABL_RN_NKShipperCountry = "D";
			bill.ABL_ShipperStreet1 = "E";

			bill.Validation.ValidateABL_ShipperName();
			AssertHasMessageError(bill.ABL_ShipperRegNoInfo, br3006RuleMessage);

			bill.ABL_ShipperName = "A";
			bill.Validation.ValidateABL_ShipperName();
			AssertNoMessageErrors("No message errors as all the required fields are filled", bill.ABL_ShipperRegNoInfo);
		}

		public void TestCheckABL_ShipperPostcodeValidatesABL_ShipperRegNo_WhenApplicationCodeLv2()
		{
			SetUpTestData(headerApplicationCode: "LV2");

			bill.ABL_ShipperName = "A";
			bill.ABL_ShipperCity = "C";
			bill.ABL_RN_NKShipperCountry = "D";
			bill.ABL_ShipperStreet1 = "E";

			bill.Validation.ValidateABL_ShipperPostcode();
			AssertHasMessageError(bill.ABL_ShipperRegNoInfo, br3006RuleMessage);

			bill.ABL_ShipperPostcode = "B";
			bill.Validation.ValidateABL_ShipperPostcode();
			AssertNoMessageErrors("No message errors as all the required fields are filled", bill.ABL_ShipperRegNoInfo);
		}

		public void TestCheckABL_ShipperCityValidatesABL_ShipperRegNo_WhenApplicationCodeLv2()
		{
			SetUpTestData(headerApplicationCode: "LV2");

			bill.ABL_ShipperName = "A";
			bill.ABL_ShipperPostcode = "B";
			bill.ABL_RN_NKShipperCountry = "D";
			bill.ABL_ShipperStreet1 = "E";

			bill.Validation.ValidateABL_ShipperCity();
			AssertHasMessageError(bill.ABL_ShipperRegNoInfo, br3006RuleMessage);

			bill.ABL_ShipperCity = "C";
			bill.Validation.ValidateABL_ShipperCity();
			AssertNoMessageErrors("No message errors as all the required fields are filled", bill.ABL_ShipperRegNoInfo);
		}

		public void TestCheckABL_RN_NKShipperCountryValidatesABL_ShipperRegNo_WhenApplicationCodeLv2()
		{
			SetUpTestData(headerApplicationCode: "LV2");

			bill.ABL_ShipperName = "A";
			bill.ABL_ShipperPostcode = "B";
			bill.ABL_ShipperCity = "C";
			bill.ABL_ShipperStreet1 = "E";

			bill.Validation.ValidateABL_RN_NKShipperCountry();
			AssertHasMessageError(bill.ABL_ShipperRegNoInfo, br3006RuleMessage);

			bill.ABL_RN_NKShipperCountry = "D";
			bill.Validation.ValidateABL_RN_NKShipperCountry();
			AssertNoMessageErrors("No message errors as all the required fields are filled", bill.ABL_ShipperRegNoInfo);
		}

		public void TestCheckABL_ShipperStreet1ValidatesABL_ShipperRegNo_WhenApplicationCodeLv2()
		{
			SetUpTestData(headerApplicationCode: "LV2");

			bill.ABL_ShipperName = "A";
			bill.ABL_ShipperPostcode = "B";
			bill.ABL_ShipperCity = "C";
			bill.ABL_RN_NKShipperCountry = "D";

			bill.Validation.ValidateABL_ShipperStreet1();
			AssertHasMessageError(bill.ABL_ShipperRegNoInfo, br3006RuleMessage);

			bill.ABL_ShipperStreet1 = "E";
			bill.Validation.ValidateABL_ShipperStreet1();
			AssertNoMessageErrors("No message errors as all the required fields are filled", bill.ABL_ShipperRegNoInfo);
		}

		public void TestCheckABL_ShipperStreet2ValidatesABL_ShipperRegNo()
		{
			SetUpTestData(headerApplicationCode: "LV2");

			bill.ABL_ShipperName = "A";
			bill.ABL_ShipperPostcode = "B";
			bill.ABL_ShipperCity = "C";
			bill.ABL_RN_NKShipperCountry = "D";

			bill.Validation.ValidateABL_ShipperStreet2();
			AssertHasMessageError(bill.ABL_ShipperRegNoInfo, br3006RuleMessage);

			bill.ABL_ShipperStreet2 = "F";
			bill.Validation.ValidateABL_ShipperStreet2();
			AssertNoMessageErrors("No message errors as all the required fields are filled", bill.ABL_ShipperRegNoInfo);
		}

		public void TestCheckABL_ShipperStreets_AddsErrorMessage()
		{
			SetUpTestData();

			Factory.Save();

			bill.ABL_ShipperName = "ASD";
			bill.ABL_ShipperStreet1 = "";
			bill.ABL_ShipperStreet2 = "";
			bill.Validation.ValidateABL_ShipperStreet1();
			AssertHasMessageError(bill.ABL_ShipperStreet1Info, c0617RuleExporterStreetMessage);
			bill.Validation.ValidateABL_ShipperStreet2();
			AssertHasMessageError(bill.ABL_ShipperStreet2Info, c0617RuleExporterStreetMessage);
		}

		public void TestCheckABL_RX_NKTransportValueCurrency()
		{
			SetUpTestData();
			bill.ABL_RX_NKTransportValueCurrency = "XXX";

			bill.Validation.ValidateABL_RX_NKTransportValueCurrency();
			AssertHasError(bill.ABL_RX_NKTransportValueCurrencyInfo, invalidCodeMessageError);

			bill.ABL_RX_NKTransportValueCurrency = string.Empty;
			bill.ABL_TransportValue = 10;
			bill.Validation.ValidateABL_RX_NKTransportValueCurrency();
			AssertHasError(bill.ABL_RX_NKTransportValueCurrencyInfo, "Please enter a Transport Value Currency.");

			bill.ABL_RX_NKTransportValueCurrency = "USD";
			bill.Validation.ValidateABL_RX_NKTransportValueCurrency();
			AssertNoError(bill.ABL_RX_NKTransportValueCurrencyInfo, "Please enter a Transport Value Currency.");
		}

		public void TestCheckABL_RX_NKInsuranceValueCurrency()
		{
			SetUpTestData();
			bill.ABL_RX_NKInsuranceValueCurrency = "XXX";

			bill.Validation.ValidateABL_RX_NKInsuranceValueCurrency();
			AssertHasError(bill.ABL_RX_NKInsuranceValueCurrencyInfo, invalidCodeMessageError);

			bill.ABL_RX_NKInsuranceValueCurrency = string.Empty;
			bill.ABL_InsuranceValue = 10;
			bill.Validation.ValidateABL_RX_NKInsuranceValueCurrency();
			AssertHasError(bill.ABL_RX_NKInsuranceValueCurrencyInfo, "Please enter an Insurance Value Currency.");

			bill.ABL_RX_NKInsuranceValueCurrency = "USD";
			bill.Validation.ValidateABL_RX_NKInsuranceValueCurrency();
			AssertNoError(bill.ABL_RX_NKInsuranceValueCurrencyInfo, "Please enter an Insurance Value Currency.");
		}

		public void TestABL_RX_NKInsuranceValueCurrencyWarning()
		{
			SetUpTestData();
			bill.ABL_RX_NKTransportValueCurrency = "AUD";
			bill.ABL_RX_NKInsuranceValueCurrency = "USD";

			bill.Validation.ValidateABL_RX_NKInsuranceValueCurrency();
			AssertHasWarning(bill.ABL_RX_NKInsuranceValueCurrencyInfo,
				"Insurance Value Currency is different from Transport Value Currency which will be converted upon save.");

			bill.ABL_RX_NKInsuranceValueCurrency = "AUD";
			bill.Validation.ValidateABL_RX_NKInsuranceValueCurrency();
			AssertNoWarning(bill.ABL_RX_NKInsuranceValueCurrencyInfo,
				"Insurance Value Currency is different from Transport Value Currency which will be converted upon save.");
		}

		public void TestValidateHasAtLeastOnePack()
		{
			SetUpTestData();
			bill.RunPreSaveValidation();
			AssertHasRowError(bill, "At least one Pack is required.");

			bill.Packs.AddNew();
			bill.RunPreSaveValidation();
			AssertNoRowError(bill, "At least one Pack is required.");
		}

		public void TestCheckABL_Procedure()
		{
			SetUpTestData();
			bill.ABL_Procedure = "XXX";
			bill.Validation.ValidateABL_Procedure();
			AssertHasMessageError(bill.ABL_ProcedureInfo, invalidCodeMessageError);

			bill.ABL_Procedure = "C07";
			bill.Validation.ValidateABL_Procedure();
			AssertNoMessageError(bill.ABL_ProcedureInfo, invalidCodeMessageError);

			bill.ABL_Procedure = string.Empty;
			bill.Validation.ValidateABL_Procedure();
			AssertHasMessageError(bill.ABL_ProcedureInfo, br600002RuleMessage);

			bill.ABL_Procedure = "C07";
			var requestedDocument = bill.AdditionalDocuments.AddNew();
			requestedDocument.CSI_Type = "OTH";
			requestedDocument.CSI_SubType = "REF";
			requestedDocument.CSI_Code = "1A06";
			requestedDocument.CSI_ParentID = bill.PK;
			bill.Validation.ValidateABL_Procedure();
			AssertHasMessageError(bill.ABL_ProcedureInfo, br600014RuleMessage);

			bill.ABL_Procedure = "C07+F49";
			bill.Validation.ValidateABL_Procedure();
			AssertNoMessageError(bill.ABL_ProcedureInfo, br600014RuleMessage);

			bill.ABL_Procedure = "C07";
			requestedDocument.CSI_Type = "ZZZ";
			requestedDocument.CSI_SubType = "REF";
			requestedDocument.CSI_Code = "1A06";
			bill.Validation.ValidateABL_Procedure();
			AssertNoMessageError(bill.ABL_ProcedureInfo, br600014RuleMessage);

			bill.ABL_Procedure = "C07";
			requestedDocument.CSI_Type = "OTH";
			requestedDocument.CSI_SubType = "ZZZ";
			requestedDocument.CSI_Code = "1A06";
			bill.Validation.ValidateABL_Procedure();
			AssertNoMessageError(bill.ABL_ProcedureInfo, br600014RuleMessage);

			bill.ABL_Procedure = "C07";
			requestedDocument.CSI_Type = "OTH";
			requestedDocument.CSI_SubType = "REF";
			requestedDocument.CSI_Code = "ZZZ";
			bill.Validation.ValidateABL_Procedure();
			AssertNoMessageError(bill.ABL_ProcedureInfo, br600014RuleMessage);

			bill = Factory.New<AsycudaBill>();
			bill.ABL_Procedure = "C07";
			bill.ABL_SellerRegNo = "123";
			bill.Validation.ValidateABL_Procedure();
			AssertHasMessageError(bill.ABL_ProcedureInfo, br600009RuleMessage);

			bill.ABL_Procedure = "C07+F48";
			bill.Validation.ValidateABL_Procedure();
			AssertNoMessageError(bill.ABL_ProcedureInfo, br600009RuleMessage);
		}

		public void TestCheckABL_TransportValue()
		{
			SetUpTestData();
			bill.ABL_RX_NKTransportValueCurrency = "EUR";
			bill.ABL_RX_NKInsuranceValueCurrency = "EUR";
			var packedItem = bill.PackedItems.AddNew();
			packedItem.API_GoodsValue = 0;
			packedItem.API_RX_NKGoodsValueCurrency = "EUR";

			bill.ABL_Procedure = "C08";
			bill.ABL_TransportValue = 45;
			bill.ABL_InsuranceValue = 0;
			bill.Validation.ValidateABL_TransportValue();
			AssertNoMessageError(bill.ABL_TransportValueInfo, br600005RuleMessage);

			bill.ABL_TransportValue = 50;
			bill.Validation.ValidateABL_TransportValue();
			AssertHasMessageError(bill.ABL_TransportValueInfo, br600005RuleMessage);

			packedItem.API_GoodsValue = 5;
			bill.ABL_TransportValue = 45;
			bill.Validation.ValidateABL_TransportValue();
			AssertHasMessageError(bill.ABL_TransportValueInfo, br600005RuleMessage);

			packedItem.API_GoodsValue = 0;
			bill.ABL_TransportValue = 45;
			bill.ABL_InsuranceValue = 5;
			bill.Validation.ValidateABL_TransportValue();
			AssertHasMessageError(bill.ABL_TransportValueInfo, br600005RuleMessage);

			bill.ABL_Procedure = "C07";
			bill.Validation.ValidateABL_TransportValue();
			AssertNoMessageError(bill.ABL_TransportValueInfo, br600005RuleMessage);
		}

		public void TestCheckABL_InsuranceValue()
		{
			SetUpTestData();
			bill.ABL_RX_NKTransportValueCurrency = "EUR";
			bill.ABL_RX_NKInsuranceValueCurrency = "EUR";
			var packedItem = bill.PackedItems.AddNew();
			packedItem.API_GoodsValue = 0;
			packedItem.API_RX_NKGoodsValueCurrency = "EUR";

			bill.ABL_Procedure = "C08";
			bill.ABL_TransportValue = 0;
			bill.ABL_InsuranceValue = 45;
			bill.Validation.ValidateABL_InsuranceValue();
			AssertNoMessageError(bill.ABL_InsuranceValueInfo, br600005RuleMessage);

			bill.ABL_InsuranceValue = 50;
			bill.Validation.ValidateABL_InsuranceValue();
			AssertHasMessageError(bill.ABL_InsuranceValueInfo, br600005RuleMessage);

			packedItem.API_GoodsValue = 5;
			bill.ABL_InsuranceValue = 45;
			bill.Validation.ValidateABL_InsuranceValue();
			AssertHasMessageError(bill.ABL_InsuranceValueInfo, br600005RuleMessage);

			packedItem.API_GoodsValue = 0;
			bill.ABL_TransportValue = 5;
			bill.ABL_InsuranceValue = 45;
			bill.Validation.ValidateABL_InsuranceValue();
			AssertHasMessageError(bill.ABL_InsuranceValueInfo, br600005RuleMessage);

			bill.ABL_Procedure = "C07";
			bill.Validation.ValidateABL_InsuranceValue();
			AssertNoMessageError(bill.ABL_InsuranceValueInfo, br600005RuleMessage);
		}

		public void TestCheckAdditionalDocuments_RequireTransportDocumentWithValidCode_V2()
		{
			var errorMessage = "[BR1106] A Transport Document Reference must be entered either at Bill or Item level with at least one of the following Full Type codes: 'N235', 'N271', 'N703', 'N704', 'N705', 'N710', 'N714', 'N720', 'N722', 'N730', 'N740', 'N741', 'N750', 'N760', 'N785', 'N787', 'N952', 'N955'.";
			CheckAdditionalDocuments(errorMessage, "REF", "TRA",
				(bill) =>
				{
					var item = bill.PackedItems.AddNew();
					var itemDocument = item.AdditionalDocuments.AddNew();
					itemDocument.CSI_SubType = "REF";
					bill.Validation.ValidateAll();
					AssertHasRowMessageError("there is no transport document on item level", bill, errorMessage);

					itemDocument.CSI_SubType = "TRA";
					itemDocument.CSI_Code = "123";
					bill.Validation.ValidateAll();
					AssertHasRowMessageError("item's transport document does not have the valid code", bill, errorMessage);

					itemDocument.CSI_Code = "N235";
					bill.Validation.ValidateAll();
					AssertNoRowMessageError("there is a valid transport document on item level", bill, errorMessage);

					item.AdditionalDocuments.Delete();
					bill.PackedItems.Delete();
				});
		}

		public void TestCheckAdditionalDocuments_RequireAdditionalReferenceWithCode1D24_V2()
		{
			CheckAdditionalDocuments("[BR20319] Additional Reference Type '1D24' is required.", "TRA", "REF");
		}

		public void TestCheckAdditionalDocuments_1A06RequiredForProcedureF49()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			AssertApplyToBothV1AndV2(header, () =>
			{
				var bill = header.Bills.AddNew();
				bill.ABL_Procedure = EU.H7.Business.EUH7AdditionalProcedureCodeList.Codes.C07F49;

				bill.Validation.ValidateAll();
				AssertHasRowMessageError("add row message error when no additional documents", bill, "[BR600008] If Additional Procedure contains the value 'F49', Additional Reference Type must be '1A06'.");

				bill.ABL_Procedure = "C07";
				bill.Validation.ValidateAll();
				AssertNoRowMessageError("validation is only for F49 procedure", bill, "[BR600008] If Additional Procedure contains the value 'F49', Additional Reference Type must be '1A06'.");

				bill.ABL_Procedure = EU.H7.Business.EUH7AdditionalProcedureCodeList.Codes.C07F49;
				var document = bill.AdditionalDocuments.AddNew();
				document.CSI_SubType = "TRA";
				bill.Validation.ValidateAll();
				AssertHasRowMessageError("add row message error when no REF documents", bill, "[BR600008] If Additional Procedure contains the value 'F49', Additional Reference Type must be '1A06'.");
				document.CSI_SubType = "REF";
				document.CSI_Code = "1234";
				bill.Validation.ValidateAll();
				AssertNoRowMessageError("do not need to validate additional documents as it will be validated by document itself", bill, "[BR600008] If Additional Procedure contains the value 'F49', Additional Reference Type must be '1A06'.");
				AssertHasMessageErrorContaining("it is validated by document.Validation.CheckCSI_Code()", document.CSI_CodeInfo, "[BR600008] If Additional Procedure contains the value 'F49', Additional Reference Type must be '1A06'.");
			});
		}

		public void TestCheckPreviousDocuments_AtLeastOneForDeclarationTypeA_V1()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = "LV1";
			var bill = header.Bills.AddNew();
			bill.ABL_ShipmentType = "A";

			bill.Validation.ValidateAll();
			AssertHasRowMessageError("add row message error when no previous documents", bill, "[BR2011] At least one Previous Document is required when Additional Declaration Type is 'A'.");

			AssertApplyOnlyToV1(header, bill, "[BR2011] At least one Previous Document is required when Additional Declaration Type is 'A'.");

			bill.ABL_ShipmentType = "B";
			bill.Validation.ValidateAll();
			AssertNoRowMessageError("validation is only for shipment type A", bill, "[BR2011] At least one Previous Document is required when Additional Declaration Type is 'A'.");

			bill.ABL_ShipmentType = "A";
			bill.PreviousDocuments.AddNew();
			bill.Validation.ValidateAll();
			AssertNoRowMessageError("there is one previous document", bill, "[BR2011] At least one Previous Document is required when Additional Declaration Type is 'A'.");
		}

		public void TestCheckPreviousDocuments_AtLeastOnePreviousDocument_V2()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = "LV2";
			var bill = header.Bills.AddNew();

			bill.Validation.ValidateAll();
			AssertHasRowMessageError("add row message error when no previous documents", bill, "[C0614] At least one Previous Document Reference is required.");

			AssertApplyOnlyToV2(header, bill, "[C0614] At least one Previous Document Reference is required.");

			bill.PreviousDocuments.AddNew();
			bill.Validation.ValidateAll();
			AssertNoRowMessageError("there is one previous document", bill, "[C0614] At least one Previous Document Reference is required.");
		}

		public void TestCheckSupportingDocuments_AtLeasetOneSupportingDocumentForNon08()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			AssertApplyToBothV1AndV2(header, () =>
			{
				var bill = header.Bills.AddNew();
				bill.ABL_Procedure = EU.H7.Business.EUH7AdditionalProcedureCodeList.Codes.C07F49;

				bill.Validation.ValidateAll();
				AssertHasRowMessageError("add row message error when no previous documents", bill, "[BR2037] One of the following codes must be declared: D005, D008, N325, N380, N864, N935 when Additional Procedure is not C08.");

				bill.ABL_Procedure = "C08";
				bill.Validation.ValidateAll();
				AssertNoRowMessageError("validation is only for non C08 procedure", bill, "[BR2037] One of the following codes must be declared: D005, D008, N325, N380, N864, N935 when Additional Procedure is not C08.");

				bill.ABL_Procedure = EU.H7.Business.EUH7AdditionalProcedureCodeList.Codes.C07F49;
				var document = bill.SupportingDocuments.AddNew();
				document.CSI_Code = "1234";
				bill.Validation.ValidateAll();
				AssertNoRowMessageError("do not need to validate supporting documents as it will be validated by document itself", bill, "[BR2037] One of the following codes must be declared: D005, D008, N325, N380, N864, N935 when Additional Procedure is not C08.");
				AssertHasMessageErrorContaining("it is validated by document.Validation.CheckCSI_Code()", document.CSI_CodeInfo, "[BR2037] One of the following codes must be declared: D005, D008, N325, N380, N864, N935 when Additional Procedure is not C08.");
			});
		}

		public void TestCheckSupportingDocuments_Require1D24_V1()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = "LV1";
			var bill = header.Bills.AddNew();

			bill.Validation.ValidateAll();
			AssertHasRowMessageError("add row message error when no supporting documents", bill, "[BR20319] Supporting Document Type '1D24' is required.");

			AssertApplyOnlyToV1(header, bill, "[BR20319] Supporting Document Type '1D24' is required.");

			var document = bill.SupportingDocuments.AddNew();
			document.CSI_Code = "1234";
			bill.Validation.ValidateAll();
			AssertNoRowMessageError("do not need to validate supporting documents as it will be validated by document itself", bill, "[BR20319] Supporting Document Type '1D24' is required.");
			AssertHasMessageErrorContaining("it is validated by document.Validation.CheckCSI_Code()", document.CSI_CodeInfo, "[BR20319] Supporting Document Type '1D24' is required.");
		}

		void CheckAdditionalDocuments(string errorMessage, string incorrectType, string correctType, Action<AsycudaBill> additionalCheck = null)
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = "LV2";
			var bill = header.Bills.AddNew();

			bill.Validation.ValidateAll();
			AssertHasRowMessageError("add row message error when no additional documents", bill, errorMessage);

			AssertApplyOnlyToV2(header, bill, errorMessage);
			if (additionalCheck != null)
			{
				additionalCheck(bill);
			}

			var document = bill.AdditionalDocuments.AddNew();
			document.CSI_SubType = incorrectType;
			bill.Validation.ValidateAll();
			AssertHasRowMessageError("add row message error when the expected type of additional documents does not exist", bill, errorMessage);
			document.CSI_SubType = correctType;
			document.CSI_Code = "1234";
			bill.Validation.ValidateAll();
			AssertNoRowMessageError("do not need to validate supporting documents as it will be validated by document itself", bill, errorMessage);
			AssertHasMessageErrorContaining("it is validated by document.Validation.CheckCSI_Code()", document.CSI_CodeInfo, errorMessage);
		}

		void AssertApplyOnlyToV2(AsycudaManifestHeader header, AsycudaBill bill, string errorMessage)
		{
			header.AMA_ApplicationCode = "LV1";
			bill.Validation.ValidateAll();
			AssertNoRowMessageError("apply only to LV2", bill, errorMessage);

			header.AMA_ApplicationCode = "LV2";
		}

		void AssertApplyOnlyToV1(AsycudaManifestHeader header, AsycudaBill bill, string errorMessage)
		{
			header.AMA_ApplicationCode = "LV2";
			bill.Validation.ValidateAll();
			AssertNoRowMessageError("apply only to LV1", bill, errorMessage);

			header.AMA_ApplicationCode = "LV1";
		}

		void AssertApplyToBothV1AndV2(AsycudaManifestHeader header, Action validateErrorCase)
		{
			header.AMA_ApplicationCode = "LV1";
			validateErrorCase();

			header.AMA_ApplicationCode = "LV2";
			validateErrorCase();
		}

		void AddAustraliaToIrelandCusCodeList()
		{
			var today = ZDateTime.Now;
			var yesterday = today.AddDays(-1);
			var tomorrow = today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "AI008", "AU", "Australia", yesterday, tomorrow);

			Factory.Save();
		}

		void SetUpTestData(string headerApplicationCode = "LV1")
		{
			header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = headerApplicationCode;
			bill = header.Bills.AddNew();
		}

		AsycudaManifestHeader header;
		AsycudaBill bill;

		const string invalidCodeMessageError = "[BR0020] The code you have selected is not in the list.";
		const string br3006RuleMessage = "[BR3006] Identification number is mandatory if any of the following fields are empty: Exporter name, address, postcode, city, country.";
		const string br3181RulePYEFormatMessage = "[BR3181] Invalid PYE format. Should be maximum 9 characters long.";
		const string br3181RuleITXFormatMessage = "[BR3181] Invalid ITX format. Should be maximum 9 characters long.";
		const string br3181RuleCGTFormatMessage = "[BR3181] Invalid CGT format. Should be maximum 9 characters long.";
		const string br600002RuleMessage = "[BR600002] Additional Procedure' is required and has to contain either of these codes: 'C07' or 'C08'.";
		const string br600005RuleMessage = "[BR600005] Sum of Intrinsic Value (Items), Transport Value and Insurance Value must not exceed EUR 45 when Add. Procedure(s) is C08.";
		const string br600009RuleMessage = "[BR600009] If ‘Seller IOSS Number’ has been provided then ‘Additional Procedure' must contain the value 'F48'.";
		const string br600014RuleMessage = "[BR600014] If Additional Reference Type is '1A06' then Additional Procedure must contain the value 'F49'.";
		const string c0617RuleImporterIdentificationNumberMessage = "[C0617] Identification number is mandatory if any of the following fields are empty: Importer name, address, postcode, city, country.";
		const string c0617RuleExporterStreetMessage = "[C0617] Please enter an Exporter Street Address.";
		const string c0617RuleExporterCountryMessage = "[C0617] Please enter an Exporter Country/Region.";
	}
}
