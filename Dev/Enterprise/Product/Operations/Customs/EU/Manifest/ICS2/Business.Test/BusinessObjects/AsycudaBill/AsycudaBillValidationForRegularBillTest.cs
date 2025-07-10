using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using static Enterprise.Core.Constants;
using static Enterprise.Core.Constants.Customs.Universal;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class AsycudaBillValidationForRegularBillTest : BusinessObjectValidationTestCase
	{
		public void TestCheckABL_SequenceNumber()
		{
			ValidationTestHelper.AssertErrorIfNotEntered(bill.ABL_SequenceNumberInfo, MandatoryValidation.ValueCannotBeZeroMessage(bill.ABL_SequenceNumberInfo.Description));
		}

		public void TestCheckABL_ManifestUQ()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.PackageTypes, "PKG");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, RefCusCodeListTypes.Codes.PackageTypes, "KG", "KEG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			bill.ABL_ManifestUQ = string.Empty;
			AssertNoMessageErrors(bill.ABL_ManifestUQInfo);

			bill.ABL_ManifestUQ = "KG";
			AssertNoMessageErrors(bill.ABL_ManifestUQInfo);

			bill.ABL_ManifestUQ = "ABC";
			AssertHasMessageError(bill.ABL_ManifestUQInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckABL_ShipmentType()
		{
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Export22;
			AssertHasMessageError(bill.ABL_ShipmentTypeInfo, "ICS2 manifests are Import manifests. Please select Import from the list.");

			bill.ABL_ShipmentType = string.Empty;
			AssertHasMessageError(bill.ABL_ShipmentTypeInfo, "ICS2 manifests are Import manifests. Please select Import from the list.");

			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;
			AssertNoMessageErrors(bill.ABL_ShipmentTypeInfo);
		}

		public void TestABL_ShipperPhone()
		{
			bill.ABL_OA_Shipper = Factory.New<OrgHeader>().MainAddress.PK;
			bill.ABL_ShipperPhone = "123456";
			AssertNoMessageErrors(bill.ABL_ShipperPhoneInfo);

			bill.ABL_OA_Shipper = ZGuid.Empty;
			bill.ABL_ShipperPhone = "123456";
			AssertHasMessageError(bill.ABL_ShipperPhoneInfo, "Phone Number of Shipper is in an invalid format. The phone number is required to start with + and have 6 to 14 characters (digits and spaces) E.g. +14155552671, +1 415 555 2671, +44 7911 123456, +44 7911 123 456, +33 6 12 34 56 78, +33123456789");

			bill.ABL_ShipperPhone = "+14155552671";
			AssertNoMessageErrors(bill.ABL_ShipperPhoneInfo);
		}

		public void TestABL_ConsigneePhone()
		{
			bill.ABL_OA_Consignee = Factory.New<OrgHeader>().MainAddress.PK;
			bill.ABL_ConsigneePhone = "123456";
			AssertNoMessageErrors(bill.ABL_ConsigneePhoneInfo);

			bill.ABL_OA_Consignee = ZGuid.Empty;
			bill.ABL_ConsigneePhone = "123456";
			AssertHasMessageError(bill.ABL_ConsigneePhoneInfo, "Phone Number of Consignee is in an invalid format. The phone number is required to start with + and have 6 to 14 characters (digits and spaces) E.g. +14155552671, +1 415 555 2671, +44 7911 123456, +44 7911 123 456, +33 6 12 34 56 78, +33123456789");

			bill.ABL_ConsigneePhone = "+14155552671";
			AssertNoMessageErrors(bill.ABL_ConsigneePhoneInfo);
		}

		public void TestABL_NotifyPartyPhone()
		{
			bill.ABL_OA_NotifyParty = Factory.New<OrgHeader>().MainAddress.PK;
			bill.ABL_NotifyPartyPhone = "123456";
			AssertNoMessageErrors(bill.ABL_ConsigneePhoneInfo);

			bill.ABL_OA_NotifyParty = ZGuid.Empty;
			bill.ABL_NotifyPartyPhone = "123456";
			AssertHasMessageError(bill.ABL_NotifyPartyPhoneInfo, "Phone Number of Notify Party is in an invalid format. The phone number is required to start with + and have 6 to 14 characters (digits and spaces) E.g. +14155552671, +1 415 555 2671, +44 7911 123456, +44 7911 123 456, +33 6 12 34 56 78, +33123456789");

			bill.ABL_NotifyPartyPhone = "+14155552671";
			AssertNoMessageErrors(bill.ABL_NotifyPartyPhoneInfo);
		}

		public void TestCheckABL_PrepaidCollect()
		{
			var messageError = "You have not entered a Method of Payment";

			var testCases = new[]
			{
				(specificCircumstance: EUICS2SpecificCircumstanceList.Codes.F24, applicationCode: ApplicationCodeTypeList.Codes.Consolidator, hasMessageError: false),
				(specificCircumstance: EUICS2SpecificCircumstanceList.Codes.F26, applicationCode: ApplicationCodeTypeList.Codes.Consolidator, hasMessageError: true),
				(specificCircumstance: EUICS2SpecificCircumstanceList.Codes.F22, applicationCode: ApplicationCodeTypeList.Codes.Consolidator, hasMessageError: true),
				(specificCircumstance: EUICS2SpecificCircumstanceList.Codes.F50, applicationCode: ApplicationCodeTypeList.Codes.ShippingLine, hasMessageError: true),
				(specificCircumstance: EUICS2SpecificCircumstanceList.Codes.F14, applicationCode: ApplicationCodeTypeList.Codes.ShippingLine, hasMessageError: true),
				(specificCircumstance: EUICS2SpecificCircumstanceList.Codes.F15, applicationCode: ApplicationCodeTypeList.Codes.ShippingLine, hasMessageError: true),
			};

			foreach (var (specificCircumstance, applicationCode, hasMessageError) in testCases)
			{
				manifestHeader.AMA_ApplicationCode = applicationCode;
				manifestHeader.SpecificCircumstanceIndicator = specificCircumstance;
				bill.ABL_PrepaidCollect = string.Empty;
				if (hasMessageError)
				{
					AssertHasMessageErrorContaining($"SpecificCircumstanceIndicator = {specificCircumstance}", bill.ABL_PrepaidCollectInfo, messageError);
				}
				else
				{
					AssertNoMessageErrorContaining($"SpecificCircumstanceIndicator = {specificCircumstance}", bill.ABL_PrepaidCollectInfo, messageError);
				}

				bill.ABL_PrepaidCollect = EUICS2PaymentMethodList.Codes.B;
				AssertNoMessageErrorContaining($"SpecificCircumstanceIndicator = {specificCircumstance}", bill.ABL_PrepaidCollectInfo, messageError);
			}

			ValidationTestHelper.AssertInvalidCodeMessageError(bill.ABL_PrepaidCollectInfo, "愛", EUICS2PaymentMethodList.Codes.Y);
		}

		public void TestCheckF50HasAPack_ShowMessageErrorWhenNoPacks()
		{
			manifestHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			manifestHeader.AMA_TransportMode = Constants.TransportModes.Road;

			const string expectedError = "You have not entered any Pack Details.";

			CombineAssertions(() =>
			{
				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F21;
				bill.Validation.ValidateAll();
				AssertNoRowMessageError("No error when F21 and no packs", bill, expectedError);

				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F50;
				bill.Validation.ValidateAll();
				AssertHasRowMessageError("Show error when F50 and no packs", bill, expectedError);

				bill.Packs.AddNew();

				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F50;
				bill.Validation.ValidateAll();
				AssertNoRowMessageError("No error when F50 and has packs", bill, expectedError);
			});
		}

		public void TestCheckBuyerMandatory()
		{
			var messageError = MandatoryValidation.YouHaveNotEnteredMessage(Buyer);

			bill.ABL_RL_NKFinalDestination = "DE123";

			manifestHeader.AMA_TransportMode = TransportTypeList.Codes.Road;
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F50;

			bill.ABL_OA_Buyer = ZGuid.Empty;

			CombineAssertions(() =>
			{
				AssertHasMessageError(bill.ABL_OA_BuyerInfo, messageError);

				bill.ABL_BuyerName = "TEST";
				bill.ABL_OA_Buyer = ZGuid.Empty;

				AssertNoMessageError("No Error, Buyer Name Entered", bill.ABL_OA_BuyerInfo, messageError);

				bill.ABL_BuyerName = ZString.Empty;
				bill.ABL_OA_Buyer = ZGuid.BrettsGuid;

				AssertNoMessageError("No Message Error, Value Entered", bill.ABL_OA_BuyerInfo, messageError);

				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F24;

				bill.ABL_OA_Buyer = ZGuid.Empty;
				AssertNoMessageErrorContaining("SpecificCircumstanceIndicator != F50", bill.ABL_OA_BuyerInfo, messageError);

				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F50;
				manifestHeader.AMA_TransportMode = TransportTypeList.Codes.Sea;
				bill.ABL_OA_Buyer = ZGuid.Empty;
				AssertNoMessageErrorContaining("AMA_TransportMode != ROA", bill.ABL_OA_BuyerInfo, messageError);

				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F50;
				manifestHeader.AMA_TransportMode = TransportTypeList.Codes.Sea;
				bill.ABL_RL_NKFinalDestination = "US123";

				bill.ABL_OA_Buyer = ZGuid.Empty;
				AssertNoMessageErrorContaining("Not Mandatory, Final Destination Not in EU", bill.ABL_OA_BuyerInfo, messageError);
			});
		}

		public void TestCheckSellerMandatory()
		{
			var messageError = MandatoryValidation.YouHaveNotEnteredMessage(Seller);

			bill.ABL_RL_NKFinalDestination = "DE123";

			manifestHeader.AMA_TransportMode = TransportTypeList.Codes.Road;
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F50;

			bill.ABL_OA_Seller = ZGuid.Empty;

			CombineAssertions(() =>
			{
				AssertHasMessageError(bill.ABL_OA_SellerInfo, messageError);

				bill.ABL_SellerName = "TEST";
				bill.ABL_OA_Seller = ZGuid.Empty;

				AssertNoMessageError("No Error, Seller Name Entered", bill.ABL_OA_SellerInfo, messageError);

				bill.ABL_SellerName = ZString.Empty;
				bill.ABL_OA_Seller = ZGuid.BrettsGuid;

				AssertNoMessageError("No Message Error, Value Entered", bill.ABL_OA_SellerInfo, messageError);

				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F24;

				bill.ABL_OA_Seller = ZGuid.Empty;
				AssertNoMessageErrorContaining("SpecificCircumstanceIndicator != F50", bill.ABL_OA_SellerInfo, messageError);

				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F50;
				manifestHeader.AMA_TransportMode = TransportTypeList.Codes.Sea;
				bill.ABL_OA_Seller = ZGuid.Empty;
				AssertNoMessageErrorContaining("AMA_TransportMode != ROA", bill.ABL_OA_SellerInfo, messageError);

				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F50;
				manifestHeader.AMA_TransportMode = TransportTypeList.Codes.Sea;
				bill.ABL_RL_NKFinalDestination = "US123";

				bill.ABL_OA_Seller = ZGuid.Empty;
				AssertNoMessageErrorContaining("Not Mandatory, Final Destination Not in EU", bill.ABL_OA_SellerInfo, messageError);
			});
		}

		public void TestCheckABL_FreightValue()
		{
			var messageError = "You have not entered Postal Charges.";

			manifestHeader.AMA_TransportMode = TransportTypeList.Codes.Rail;
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F43;

			var billAdditionalInfo = bill.AdditionalInfos.AddNew();
			billAdditionalInfo.CSI_Code = EUICS2AdditionalInfoTypes.Codes.CL701_10900;
			billAdditionalInfo.CSI_Description = "TEST Bill AdditionalInfo";

			bill.ABL_FreightValue = 0.00m;

			CombineAssertions(() =>
			{
				AssertHasMessageError("Has Error, when FreightValue is 0, SpecificCircumstanceIndicator is F43 and AdditionalInfos has 10900 code", bill.ABL_FreightValueInfo, messageError);

				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F44;
				bill.ABL_FreightValue = 0.00m;
				AssertNoMessageError("No Error, when FreightValue is 0, SpecificCircumstanceIndicator is not F43 and AdditionalInfos has 10900 code", bill.ABL_FreightValueInfo, messageError);

				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F43;
				bill.ABL_FreightValue = 10.00m;
				AssertNoMessageError("No Error, when FreightValue is > 0, SpecificCircumstanceIndicator is F43 and AdditionalInfos has 10900 code", bill.ABL_FreightValueInfo, messageError);

				bill.AdditionalInfos.RemoveAndDeleteAll();
				bill.ABL_FreightValue = 0.00m;
				AssertNoMessageError("No Error, when FreightValue is 0, SpecificCircumstanceIndicator is F43 and there is not AdditionalInfo with 10900 code", bill.ABL_FreightValueInfo, messageError);
			});
		}

		public void TestCheckReceptacleId()
		{
			const string messageError = "You have not entered a Receptacle Identification Number.";

			manifestHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			manifestHeader.AMA_TransportMode = Constants.TransportModes.Road;

			CombineAssertions(() =>
			{
				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F44;
				bill.ReceptacleId = "357159";
				bill.Validation.ValidateAll();
				AssertNoMessageError("No error when F44, ReceptacleId entered", bill.ReceptacleIdInfo, messageError);

				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F44;
				bill.ReceptacleId = "";
				bill.Validation.ValidateAll();
				AssertHasMessageError("Error when F44, ReceptacleId not entered", bill.ReceptacleIdInfo, messageError);
			});
		}

		public void TestCheckABL_RX_NKFreightValueCurrency()
		{
			var messageError = MandatoryValidation.YouHaveNotEnteredMessage("Currency");

			manifestHeader.AMA_TransportMode = TransportTypeList.Codes.Rail;
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F43;

			bill.ABL_FreightValue = 10.00m;

			CombineAssertions(() =>
			{
				AssertHasMessageError("Has Error, when ABL_FreightValue > 0 and ABL_RX_NKFreightValueCurrency is blank", bill.ABL_RX_NKFreightValueCurrencyInfo, messageError);

				bill.ABL_RX_NKFreightValueCurrency = "EUR";

				AssertNoMessageError("No Error, when ABL_FreightValue > 0 and ABL_RX_NKFreightValueCurrency is not blank", bill.ABL_RX_NKFreightValueCurrencyInfo, messageError);

				bill.ABL_RX_NKFreightValueCurrency = "TNA";

				AssertHasMessageError(bill.ABL_RX_NKFreightValueCurrencyInfo, "Enter a valid Currency.");
			});
		}

		public void TestCheckABL_BuyerName()
		{
			AssertPartyPropertyMandatory(bill.ABL_BuyerNameInfo, Buyer);
		}

		public void TestCheckABL_BuyerStreet1()
		{
			AssertPartyPropertyMandatory(bill.ABL_BuyerStreet1Info, Buyer);
		}

		public void TestCheckABL_BuyerCity()
		{
			AssertPartyPropertyMandatory(bill.ABL_BuyerCityInfo, Buyer);
		}

		public void TestCheckABL_BuyerState()
		{
			AssertPartyPropertyMandatory(bill.ABL_BuyerStateInfo, Buyer);
		}

		public void TestCheckABL_BuyerPostcode()
		{
			AssertPartyPropertyMandatory(bill.ABL_BuyerPostcodeInfo, Buyer);
		}

		public void TestCheckABL_RN_NKBuyerCountry()
		{
			AssertPartyPropertyMandatory(bill.ABL_RN_NKBuyerCountryInfo, Buyer);
		}

		public void TestCheckABL_SellerName()
		{
			AssertPartyPropertyMandatory(bill.ABL_SellerNameInfo, Seller);
		}

		public void TestCheckABL_SellerStreet1()
		{
			AssertPartyPropertyMandatory(bill.ABL_SellerStreet1Info, Seller);
		}

		public void TestCheckABL_SellerCity()
		{
			AssertPartyPropertyMandatory(bill.ABL_SellerCityInfo, Seller);
		}

		public void TestCheckABL_SellerState()
		{
			AssertPartyPropertyMandatory(bill.ABL_SellerStateInfo, Seller);
		}

		public void TestCheckABL_SellerPostcode()
		{
			AssertPartyPropertyMandatory(bill.ABL_SellerPostcodeInfo, Seller);
		}

		public void TestCheckABL_RN_NKSellerCountry()
		{
			AssertPartyPropertyMandatory(bill.ABL_RN_NKSellerCountryInfo, Seller);
		}

		public void TestCheckABL_BuyerRegNo()
		{
			AssertErrorOnMultipleReNo(bill.ABL_BuyerRegNoInfo, bill.ABL_OA_BuyerInfo);
		}

		public void TestCheckABL_ConsigneeRegNo()
		{
			AssertErrorOnMultipleReNo(bill.ABL_ConsigneeRegNoInfo, bill.ABL_OA_ConsigneeInfo);
		}

		public void TestCheckABL_NotifyPartyRegNo()
		{
			AssertErrorOnMultipleReNo(bill.ABL_NotifyPartyRegNoInfo, bill.ABL_OA_NotifyPartyInfo);
		}

		public void TestCheckABL_SellerRegNo()
		{
			AssertErrorOnMultipleReNo(bill.ABL_SellerRegNoInfo, bill.ABL_OA_SellerInfo);
		}

		public void TestCheckABL_ShipperRegNo()
		{
			AssertErrorOnMultipleReNo(bill.ABL_ShipperRegNoInfo, bill.ABL_OA_ShipperInfo);
		}

		public void TestCheckF14HasSupplementaryDeclarant_WithEUCountry()
		{
			manifestHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			manifestHeader.AMA_TransportMode = Constants.TransportModes.Sea;
			bill.ABL_RL_NKFinalDestination = "DE123";

			const string expectedError = "You have not entered a Supplementary Declarant.";

			CombineAssertions(() =>
			{
				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F15;
				bill.Validation.ValidateAll();
				AssertNoRowMessageError("No error when F15, EU Member State and no Supplementary Declarant", bill, expectedError);

				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F14;
				bill.Validation.ValidateAll();
				AssertHasRowMessageError("Show error when F14, EU Member State and no Supplementary Declarant", bill, expectedError);

				bill.SupplementaryDeclarants.AddNew();

				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F14;
				bill.Validation.ValidateAll();
				AssertNoRowMessageError("No error when F14, EU Member State and has Supplementary Declarant", bill, expectedError);
			});
		}

		public void TestCheckF14HasSupplementaryDeclarant_WithNonEUCountry()
		{
			manifestHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			manifestHeader.AMA_TransportMode = Constants.TransportModes.Sea;
			bill.ABL_RL_NKFinalDestination = "US123";

			const string expectedError = "You have not entered a Supplementary Declarant.";

			CombineAssertions(() =>
			{
				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F15;
				bill.Validation.ValidateAll();
				AssertNoRowMessageError("No error when F15, Non EU Member State and no Supplementary Declarant", bill, expectedError);

				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F14;
				bill.Validation.ValidateAll();
				AssertNoRowMessageError("No error when F14, Non EU Member State and no Supplementary Declarant", bill, expectedError);

				bill.SupplementaryDeclarants.AddNew();

				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F14;
				bill.Validation.ValidateAll();
				AssertNoRowMessageError("No error when F14, Non EU Member State and has Supplementary Declarant", bill, expectedError);
			});
		}

		public void TestCheckABL_OA_Shipper()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();

			manifestHeader.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;
			bill.Header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F40;
			bill.ABL_OA_Shipper = ZGuid.Empty;
			bill.Validation.ValidateABL_OA_Shipper();
			AssertHasMessageError(bill.ABL_OA_ShipperInfo, "A Shipper is required.");

			bill.ABL_OA_Shipper = Factory.New<OrgHeader>().MainAddress.PK;
			bill.Validation.ValidateABL_OA_Shipper();
			AssertNoMessageError(bill.ABL_OA_ShipperInfo, "A Shipper is required.");

			bill.Header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F16;
			bill.ABL_OA_Shipper = ZGuid.Empty;
			bill.Validation.ValidateABL_OA_Shipper();
			AssertNoMessageError(bill.ABL_OA_ShipperInfo, "A Shipper is required.");
		}

		public void TestCheckF14F15HasPack()
		{
			CombineAssertions(() =>
			{
				var message = "At least one Pack is required per Bill.";

				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				var bill = header.Bills.AddNew();
				header.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;
				header.AMA_TransportMode = TransportModes.Sea;
				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F14;
				bill.Validation.ValidateAll();
				AssertHasRowMessageError("F14", bill, message);

				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F15;
				bill.Validation.ValidateAll();
				AssertHasRowMessageError("F15", bill, message);

				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F16;
				bill.Validation.ValidateAll();
				AssertNoRowMessageError("F16", bill, message);

				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F14;
				var pack = bill.Packs.AddNew();
				bill.Validation.ValidateAll();
				AssertNoRowMessageError("F14 with packs", bill, message);
			});
		}

		public void TestCheckF14F15SpecificCirumstandIndicatorItinerary()
		{
			var message = "Itinerary must at least contain Origin and Final Destination.";

			var helper = new MasterFilesTestHelper(Factory);
			var germany = RefCountry.LoadFromCountryCode(Factory, "DE");
			helper.CreateUnlocoIfNotExists("DE123", germany);
			helper.CreateUnlocoIfNotExists("DE456", germany);
			var usa = RefCountry.LoadFromCountryCode(Factory, "US");
			helper.CreateUnlocoIfNotExists("US456", usa);
			helper.CreateUnlocoIfNotExists("US789", usa);
			Factory.Save();

			CombineAssertions(() =>
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				var bill = header.Bills.AddNew();
				header.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;
				header.AMA_TransportMode = TransportModes.Sea;
				bill.ABL_RL_NKOrigin = "DE123";
				bill.ABL_RL_NKFinalDestination = "US456";
				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F14;
				bill.Validation.ValidateAll();
				AssertHasRowMessageError("F14", bill, message);

				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F15;
				bill.Validation.ValidateAll();
				AssertHasRowMessageError("F15", bill, message);

				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F16;
				bill.Validation.ValidateAll();
				AssertNoRowMessageError("F16", bill, message);

				var itineraryOrigin = header.Itinerary.AddNew();
				itineraryOrigin.CY_Code = "DE456";
				header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F14;
				bill.Validation.ValidateAll();
				AssertHasRowMessageError("F14, itineraryOrigin entered", bill, message);

				var itineraryFinalDestination = header.Itinerary.AddNew();
				itineraryFinalDestination.CY_Code = "US789";
				bill.Validation.ValidateAll();
				AssertNoRowMessageError("F14, both Itinerary entered", bill, message);

				itineraryOrigin.Delete();

				bill.Validation.ValidateAll();
				AssertHasRowMessageError("F14, itineraryFinalDestination entered", bill, message);
			});
		}

		public void TestCheckABL_ManifestQty_SkipsBaseCheckForForwarderManifestTypes()
		{
			var messageError = MandatoryValidation.YouHaveNotEnteredMessage("Quantity (on Bill)");

			var testCases = new[]
			{
				(EUICS2SpecificCircumstanceList.Codes.F14, false),
				(EUICS2SpecificCircumstanceList.Codes.F15, false),
				(EUICS2SpecificCircumstanceList.Codes.F16, false),
				(EUICS2SpecificCircumstanceList.Codes.F17, false),
				(EUICS2SpecificCircumstanceList.Codes.F44, false),
				(EUICS2SpecificCircumstanceList.Codes.F24, true),
			};

			foreach (var (specificCircumstance, hasMessageError) in testCases)
			{
				manifestHeader.SpecificCircumstanceIndicator = specificCircumstance;

				if (hasMessageError)
				{
					bill.ABL_ManifestQty = ZInt.Zero;
					bill.Validation.ValidateAll();
					AssertHasMessageError($"Base method should run for SpecificCircumstanceIndicator={specificCircumstance}", bill.ABL_ManifestQtyInfo, messageError);
				}
				else
				{
					bill.ABL_ManifestQty = ZInt.Zero;
					bill.Validation.ValidateAll();
					AssertNoMessageError($"Expected no error for SpecificCircumstanceIndicator={specificCircumstance}", bill.ABL_ManifestQtyInfo, messageError);
				}
			}
		}

		public void TestABL_RL_NKOrigin_ShouldSkipValidation()
		{
			var errorMessage = MandatoryValidation.YouHaveNotEnteredMessage("Origin");

			manifestHeader.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;

			bill.ABL_RL_NKOrigin = string.Empty;

			CombineAssertions(() =>
			{
				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F16;
				bill.Validation.ValidateABL_RL_NKOrigin();
				AssertNoMessageError("No error for F16 with ENS", bill.ABL_RL_NKOriginInfo, errorMessage);
				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F17;
				bill.Validation.ValidateABL_RL_NKOrigin();
				AssertNoMessageError("No error for F17 with ENS", bill.ABL_RL_NKOriginInfo, errorMessage);
				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F15;
				bill.Validation.ValidateABL_RL_NKOrigin();
				AssertHasMessageError("Notification for F15 with ENS", bill.ABL_RL_NKOriginInfo, errorMessage);
				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F44;
				bill.Validation.ValidateABL_RL_NKOrigin();
				AssertNoMessageError("Notification for F44 with ENS", bill.ABL_RL_NKOriginInfo, errorMessage);
			});
		}

		public void TestABL_RL_NKFinalDestination_ShouldSkipValidation()
		{
			var errorMessage = "A Final Destination is required";

			manifestHeader.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;
			bill.ABL_RL_NKFinalDestination = string.Empty;

			CombineAssertions(() =>
			{
				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F16;
				bill.Validation.ValidateABL_RL_NKFinalDestination();
				AssertNoMessageError("No error message on Final Destination For F16 with ENS", bill.ABL_RL_NKFinalDestinationInfo, errorMessage);

				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F17;
				bill.Validation.ValidateABL_RL_NKFinalDestination();
				AssertNoMessageError("No error message on Final Destination For F17 with ENS", bill.ABL_RL_NKFinalDestinationInfo, errorMessage);

				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F15;
				bill.Validation.ValidateABL_RL_NKFinalDestination();
				AssertHasMessageError("Notification for F15 with ENS", bill.ABL_RL_NKFinalDestinationInfo, errorMessage);

				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F44;
				bill.Validation.ValidateABL_RL_NKFinalDestination();
				AssertNoMessageError("No error message on Final Destination For F44", bill.ABL_RL_NKFinalDestinationInfo, errorMessage);
			});
		}

		public void TestCheckABL_GrossWeight()
		{
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F44;
			ValidationTestHelper.AssertFieldIsNotMandatory(bill.ABL_GrossWeightInfo);
		}

		public void TestCheckTransportDocumentType()
		{
			manifestHeader.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;

			manifestHeader.AMA_TransportMode = TransportTypeList.Codes.Air;
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F14;
			var validation = bill.Validation as AsycudaBillValidationForRegularBill;

			AssertNotNull(validation);
			validation.ValidateTransportDocumentType();
			AssertNoMessageErrors(bill.TransportDocumentTypeInfo);

			manifestHeader.AMA_TransportMode = TransportTypeList.Codes.Road;
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F44;
			validation.ValidateTransportDocumentType();
			AssertHasMessageErrorContaining(bill.TransportDocumentTypeInfo, MandatoryValidation.YouHaveNotEntered);

			manifestHeader.AMA_TransportMode = TransportTypeList.Codes.Sea;
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F10;
			validation.ValidateTransportDocumentType();
			AssertNoMessageErrors(bill.TransportDocumentTypeInfo);

			foreach (var specificCircumstanceIndicator in new[] { EUICS2SpecificCircumstanceList.Codes.F14, EUICS2SpecificCircumstanceList.Codes.F15, EUICS2SpecificCircumstanceList.Codes.F16, EUICS2SpecificCircumstanceList.Codes.F17 })
			{
				manifestHeader.SpecificCircumstanceIndicator = specificCircumstanceIndicator;

				CombineAssertions($"When SpecificCircumstanceIndicator is {specificCircumstanceIndicator}", () =>
				{
					bill.TransportDocumentType = ZString.Empty;
					validation.ValidateTransportDocumentType();
					AssertHasMessageErrorContaining(bill.TransportDocumentTypeInfo, MandatoryValidation.YouHaveNotEntered);

					bill.TransportDocumentType = "XXXX";
					AssertHasMessageErrorContaining(bill.TransportDocumentTypeInfo, ListValidation.InvalidCodeMessageError);

					bill.TransportDocumentType = "N741";
					AssertNoMessageErrors(bill.TransportDocumentTypeInfo);
				});
			}
		}

		public void TestCheckConsignee()
		{
			const string messageError = "A Consignee is required";

			manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;
			var bill = manifestHeader.Bills.AddNew();
			bill.ABL_OA_Consignee = ZGuid.Empty;

			var testCases = new[]
			{
				(Message: "SpecificCircumstanceIndicator is not F14, F15, F16, or F17", TransportMode: TransportModes.Air, SpecificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F24, HasCL701_10600: false, ExpectedMessageError: true),
				(Message: "SpecificCircumstanceIndicator is F14 with 10600 code in additionalInformation", TransportMode: TransportModes.Sea, SpecificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F14, HasCL701_10600: true, ExpectedMessageError: false),
				(Message: "SpecificCircumstanceIndicator is F14 without 10600 code in addititionalInformation", TransportMode: TransportModes.Sea, SpecificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F14, HasCL701_10600: false, ExpectedMessageError: true),
				(Message: "SpecificCircumstanceIndicator is F15 with 10600 code in additionalInformation", TransportMode: TransportModes.Sea, SpecificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F15, HasCL701_10600: true, ExpectedMessageError: false),
				(Message: "SpecificCircumstanceIndicator is F15 without 10600 code in addititionalInformation", TransportMode: TransportModes.Sea, SpecificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F15, HasCL701_10600: false, ExpectedMessageError: true),
				(Message: "SpecificCircumstanceIndicator is F16", TransportMode: TransportModes.Sea, SpecificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F16, HasCL701_10600: false, ExpectedMessageError: false),
				(Message: "SpecificCircumstanceIndicator is F17", TransportMode: TransportModes.Sea, SpecificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F16, HasCL701_10600: false, ExpectedMessageError: false),
				(Message: "SpecificCircumstanceIndicator is F44", TransportMode: TransportModes.Sea, SpecificCircumstanceIndicator: EUICS2SpecificCircumstanceList.Codes.F44, HasCL701_10600: false, ExpectedMessageError: false),
			};

			foreach (var (message, transportMode, specificCircumstanceIndicator, hasCL701_10600, expectedMessageError) in testCases)
			{
				manifestHeader.AMA_TransportMode = transportMode;
				manifestHeader.SpecificCircumstanceIndicator = specificCircumstanceIndicator;
				if (hasCL701_10600)
				{
					var billAdditionalInfo = bill.AdditionalInfos.AddNew();
					billAdditionalInfo.CSI_Code = EUICS2AdditionalInfoTypes.Codes.CL701_10600;
				}
				else
				{
					bill.AdditionalInfos.RemoveAndDeleteAll();
				}
				bill.Validation.ValidateAll();
				CombineAssertions(message, () =>
				{
					if (expectedMessageError)
					{
						AssertHasMessageError(bill.ABL_OA_ConsigneeInfo, messageError);
						AssertHasMessageError(bill.ABL_ConsigneeNameInfo, messageError);
						AssertHasMessageError(bill.ABL_ConsigneeCityInfo, messageError);
						AssertHasMessageError(bill.ABL_ConsigneeStreet1Info, messageError);
						AssertHasMessageError(bill.ABL_RN_NKConsigneeCountryInfo, messageError);
						AssertHasMessageError(bill.ABL_ConsigneePostcodeInfo, messageError);
					}
					else
					{
						AssertNoMessageError(bill.ABL_OA_ConsigneeInfo, messageError);
						AssertNoMessageError(bill.ABL_ConsigneeNameInfo, messageError);
						AssertNoMessageError(bill.ABL_ConsigneeCityInfo, messageError);
						AssertNoMessageError(bill.ABL_ConsigneeStreet1Info, messageError);
						AssertNoMessageError(bill.ABL_RN_NKConsigneeCountryInfo, messageError);
						AssertNoMessageError(bill.ABL_ConsigneePostcodeInfo, messageError);
					}
				});
			}
		}

		public void TestCheckABL_ShipperPostcode()
		{
			const string messageError = "A Shipper is required.";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("CL733", "CL733");
			helper.CreateCusCodeList("EUN", "CL733", "PA", ZDateTime.Today.AddDays(-1), ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;
			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F14;
			var bill = header.Bills.AddNew();

			bill.ABL_RN_NKShipperCountry = "IT";
			bill.Validation.ValidateABL_ShipperPostcode();
			AssertHasMessageError(bill.ABL_ShipperPostcodeInfo, messageError);

			bill.ABL_RN_NKShipperCountry = ZString.Empty;
			bill.Validation.ValidateABL_ShipperPostcode();
			AssertNoMessageError(bill.ABL_ShipperPostcodeInfo, messageError);

			bill.ABL_RN_NKShipperCountry = "PA";
			bill.Validation.ValidateABL_ShipperPostcode();
			AssertNoMessageError(bill.ABL_ShipperPostcodeInfo, messageError);
		}

		public void TestCheckABL_ShipperName()
		{
			const string messageError = "A Shipper is required.";

			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			header.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;
			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F14;

			bill.Validation.ValidateABL_ShipperName();
			AssertHasMessageError(bill.ABL_ShipperNameInfo, messageError);

			bill.ABL_ShipperName = "Shipper";
			bill.Validation.ValidateABL_ShipperName();
			AssertNoMessageError(bill.ABL_ShipperNameInfo, messageError);
			bill.ABL_ShipperName = ZString.Empty;

			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F16;
			bill.Validation.ValidateABL_ShipperName();
			AssertNoMessageError(bill.ABL_ShipperNameInfo, messageError);
		}

		public void TestCheckABL_ShipperCity()
		{
			const string messageError = "A Shipper is required.";

			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			header.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;
			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F14;

			bill.Validation.ValidateABL_ShipperCity();
			AssertHasMessageError(bill.ABL_ShipperCityInfo, messageError);

			bill.ABL_ShipperCity = "Shipper City";
			bill.Validation.ValidateABL_ShipperCity();
			AssertNoMessageError(bill.ABL_ShipperCityInfo, messageError);
			bill.ABL_ShipperCity = ZString.Empty;

			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F16;
			bill.Validation.ValidateABL_ShipperCity();
			AssertNoMessageError(bill.ABL_ShipperCityInfo, messageError);
		}

		public void TestCheckABL_RN_NKShipperCountry()
		{
			const string messageError = "A Shipper is required.";

			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			header.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;
			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F14;

			bill.Validation.ValidateABL_RN_NKShipperCountry();
			AssertHasMessageError(bill.ABL_RN_NKShipperCountryInfo, messageError);

			bill.ABL_RN_NKShipperCountry = "IT";
			bill.Validation.ValidateABL_RN_NKShipperCountry();
			AssertNoMessageError(bill.ABL_RN_NKShipperCountryInfo, messageError);
			bill.ABL_RN_NKShipperCountry = ZString.Empty;

			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F16;
			bill.Validation.ValidateABL_RN_NKShipperCountry();
			AssertNoMessageError(bill.ABL_RN_NKShipperCountryInfo, messageError);
		}

		public void TestCheckShipperPersonType()
		{
			const string messageError = "A Shipper is required.";

			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			header.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;
			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F14;

			(bill.Validation as AsycudaBillValidationForRegularBill)?.ValidateShipperPersonType();
			AssertHasMessageError(bill.ShipperPersonTypeInfo, messageError);

			bill.ShipperPersonType = "2";
			(bill.Validation as AsycudaBillValidationForRegularBill)?.ValidateShipperPersonType();
			AssertNoMessageError(bill.ShipperPersonTypeInfo, messageError);
			bill.ShipperPersonType = ZString.Empty;

			header.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F16;
			(bill.Validation as AsycudaBillValidationForRegularBill)?.ValidateShipperPersonType();
			AssertNoMessageError(bill.ShipperPersonTypeInfo, messageError);
		}

		public void TestCheckShipper()
		{
			const string messageError = "A Shipper is required.";
			manifestHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			manifestHeader.AMA_TransportMode = TransportTypeList.Codes.Sea;
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F11;

			AssertRequiredFieldsHaveMessageErrors();

			manifestHeader.AMA_TransportMode = TransportTypeList.Codes.Road;
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F11;
			AssertNoFieldsHaveMessageError("Manifest is not SEA/IWT Carrier with F11/F12 specific circumstance indicator");

			manifestHeader.AMA_TransportMode = TransportTypeList.Codes.Sea;
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F11;
			AssertRequiredFieldsHaveMessageErrors();

			bill.ABL_OA_Shipper = Factory.New<OrgHeader>().MainAddress.PK;
			bill.ABL_ShipperName = "Shipper_Name";
			bill.ABL_ShipperCity = "Shipper_City";
			bill.ABL_RN_NKShipperCountry = Core.Constants.CountryCodes.Germany;
			bill.ABL_ShipperPostcode = "123456";
			AssertNoFieldsHaveMessageError("Shipper details filled");

			bill.ABL_ShipperPostcode = string.Empty;
			AssertHasMessageError("Shipper Post code when country filled", bill.ABL_ShipperPostcodeInfo, messageError);

			void AssertRequiredFieldsHaveMessageErrors()
			{
				bill.Validation.ValidateAll();
				CombineAssertions("SEA/IWT Carrier Manifest with F11 or F12 specific circumstance indicator", () =>
				{
					AssertHasMessageError("Shipper name", bill.ABL_ShipperNameInfo, messageError);
					AssertHasMessageError("Shipper City", bill.ABL_ShipperCityInfo, messageError);
					AssertHasMessageError("Shipper Country", bill.ABL_RN_NKShipperCountryInfo, messageError);
					AssertNoMessageError("Shipper Street1", bill.ABL_ShipperStreet1Info, messageError);
					AssertNoMessageError("Shipper Post code", bill.ABL_ShipperPostcodeInfo, messageError);
					AssertHasMessageError("Shipper Person type", bill.ShipperPersonTypeInfo, messageError);
					AssertHasMessageError("Shipper Org", bill.ABL_OA_ShipperInfo, messageError);
				});
			}
			void AssertNoFieldsHaveMessageError(string message)
			{
				bill.Validation.ValidateAll();
				CombineAssertions(message, () =>
				{
					AssertNoMessageError("Shipper name", bill.ABL_ShipperNameInfo, messageError);
					AssertNoMessageError("Shipper City", bill.ABL_ShipperCityInfo, messageError);
					AssertNoMessageError("Shipper Country", bill.ABL_RN_NKShipperCountryInfo, messageError);
					AssertNoMessageError("Shipper Street1", bill.ABL_ShipperStreet1Info, messageError);
					AssertNoMessageError("Shipper Post code", bill.ABL_ShipperPostcodeInfo, messageError);
					AssertNoMessageError("Shipper Person type", bill.ShipperPersonTypeInfo, messageError);
					AssertNoMessageError("Shipper Org", bill.ABL_OA_ShipperInfo, messageError);
				});
			}
		}

		public void TestCheckShipperCountryInList()
		{
			manifestHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			manifestHeader.AMA_TransportMode = TransportTypeList.Codes.Sea;
			manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F11;
			ValidationTestHelper.AssertInvalidCodeMessageError(bill.ABL_RN_NKShipperCountryInfo, "??", CountryCodes.Sweden);
		}

		protected override void SetUp()
		{
			base.SetUp();

			CreateUNLOCOInEU();
			manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			bill = manifestHeader.Bills.AddNew();
		}

		ZString Seller => "Seller";

		ZString Buyer => "Buyer";

		void AssertPartyPropertyMandatory(ZPropertyInfo propertyInfo, ZString propertyDescriptor)
		{
			CombineAssertions(() =>
			{
				bill.ABL_RL_NKFinalDestination = "DE123";

				ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyHasValue(propertyInfo, manifestHeader.SpecificCircumstanceIndicatorInfo,
					[
						(ZString)EUICS2SpecificCircumstanceList.Codes.F15,
						(ZString)EUICS2SpecificCircumstanceList.Codes.F16,
						(ZString)EUICS2SpecificCircumstanceList.Codes.F50,
						(ZString)EUICS2SpecificCircumstanceList.Codes.F51,
					], false);

				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F24;

				ValidationTestHelper.AssertFieldIsNotMandatory(propertyInfo);

				bill.ABL_RL_NKFinalDestination = "US123";

				ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyHasValue(propertyInfo, manifestHeader.SpecificCircumstanceIndicatorInfo, (ZString)EUICS2SpecificCircumstanceList.Codes.F17, false);

				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F50;
				ValidationTestHelper.AssertFieldIsNotMandatory(propertyInfo);

				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F51;
				ValidationTestHelper.AssertFieldIsNotMandatory(propertyInfo);

				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F16;
				ValidationTestHelper.AssertFieldIsNotMandatory(propertyInfo);

				manifestHeader.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F15;
				ValidationTestHelper.AssertFieldIsNotMandatory(propertyInfo);
			});
		}

		void AssertErrorOnMultipleReNo(ZPropertyInfo targetPropertyInfo, ZPropertyInfo addressPropertyInfo)
		{
			const string message =
				"Multiple EU/XI EORI found for XYZ, which is not supported. Update XYZ to have only one EORI. Then clear and set party again.";
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.Header.OH_Code = "XYZ";
			addressPropertyInfo.Value = address.PK;
			targetPropertyInfo.Value = (ZString)"* multiple found";
			AssertHasError(targetPropertyInfo, message);
			targetPropertyInfo.ClearValue();
			AssertNoMessageError(targetPropertyInfo, message);
			addressPropertyInfo.Value = ZGuid.Empty;
			targetPropertyInfo.Value = (ZString)"* multiple found";
			AssertNoMessageError(targetPropertyInfo, message);
		}

		void CreateUNLOCOInEU()
		{
			var helper = new MasterFilesTestHelper(Factory);
			var germany = RefCountry.LoadFromCountryCode(Factory, "DE");
			helper.CreateUnlocoIfNotExists("DE123", germany);
			Factory.Save();
		}

		AsycudaManifestHeader manifestHeader;
		AsycudaBill bill;
	}
}
