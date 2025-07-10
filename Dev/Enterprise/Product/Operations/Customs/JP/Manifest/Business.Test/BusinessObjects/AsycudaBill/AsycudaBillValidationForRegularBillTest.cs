using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.JP.Common.Testing;
using Enterprise.Customs.JP.Manifest.Business.Test;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeList;

namespace Enterprise.Customs.JP.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaBillValidationForRegularBill))]
	sealed class AsycudaBillValidationForRegularBillTest : AsycudaBillValidationTest
	{
		public void TestABL_ManifestUQNotMappedMessageErrorIsNotRequired()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			var jpCountry = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.Japan, Core.Constants.CountryCodes.Japan, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(jpCountry.PK, Universal.RefCusCodeListAttributeTypes.Codes.NVC, ZString.Empty);
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_ManifestUQ = Core.Constants.PkgUnit.Bag;
			AssertEquals("Should not ShowMessageErrorIfUQUnknownForCountry", false, bill.ABL_ManifestUQInfo.Notifications.GetMessageErrors().ContainsNotificationContaining("Package type BAG does not map to a Customs package type for country JP. Please add a mapping via Maintain > Customs > Customs Files > Packs Conversion."));
		}

		public void TestCheckABL_OA_Consignee()
		{
			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.ManifestValidationRule, "ManifestValidationRule");
			var expectedMessage = "Consignee is required";
			var validationRule = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, RefCusCodeListTypes.Codes.ManifestValidationRule, ManifestValidationRuleCodes.Consignee, expectedMessage, ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(validationRule.PK, ManifestValidationRuleCodes.Mandatory, ZString.Empty);

			Factory.Save();

			var validation = Bill.RegularBillValidation;
			validation.ValidateABL_OA_Consignee();
			AssertHasMessageError(Bill.ABL_OA_ConsigneeInfo, expectedMessage);

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			validation.ValidateABL_OA_Consignee();
			AssertNoMessageError(Bill.ABL_OA_ConsigneeInfo, expectedMessage);

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
			validation.ValidateABL_OA_Consignee();
			AssertNoMessageError(Bill.ABL_OA_ConsigneeInfo, expectedMessage);
		}

		public void TestCheckABL_RL_NKOrigin()
		{
			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			var validation = Bill.RegularBillValidation;
			var info = Bill.ABL_RL_NKOriginInfo;
			validation.ValidateABL_RL_NKOrigin();
			AssertNoMessageErrors(info);

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			validation.ValidateABL_RL_NKOrigin();
			AssertNoMessageErrors(info);

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
			validation.ValidateABL_RL_NKOrigin();
			AssertNoMessageErrors(info);
		}

		public void TestCheckABL_CountryOfOrigin()
		{
			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			var expectedMessage = ListValidation.InvalidCodeMessageError;
			var validation = Bill.RegularBillValidation;
			var info = Bill.ABL_CountryOfOriginInfo;
			Bill.ABL_CountryOfOrigin = "JP";
			AssertNoMessageError(info, expectedMessage);

			Bill.ABL_CountryOfOrigin = "12";
			AssertHasMessageError(info, expectedMessage);

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			validation.ValidateABL_CountryOfOrigin();
			AssertNoMessageError(info, expectedMessage);

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
			validation.ValidateABL_CountryOfOrigin();
			AssertNoMessageError(info, expectedMessage);
		}

		public void TestCheckABL_RL_NKPortOfDischarge()
		{
			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			var targetInfo = Bill.ABL_RL_NKPortOfDischargeInfo;
			ValidationTestHelper.AssertInvalidCodeMessageError(targetInfo, "JPXXX", "JPTKX");

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			Bill.ABL_RL_NKPortOfDischarge = "JPXXX";
			AssertNoMessageError(targetInfo, ListValidation.InvalidCodeMessageError);

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
			Bill.Validation.ValidateABL_RL_NKPortOfDischarge();
			AssertNoMessageError(targetInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckABL_BillNumber()
		{
			var expectedMessage = "The entered HAWB number exceeds the maximum allowable length set by the customs. Only the first 16 characters will be sent to the customs.";
			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;

			Bill.ABL_BillNumber = "1234567890123456";
			AssertNoWarning(Bill.ABL_BillNumberInfo, expectedMessage);

			Bill.ABL_BillNumber = "12345678901234567";
			AssertHasWarning(Bill.ABL_BillNumberInfo, expectedMessage);

			expectedMessage = "[NVC01] House B/L must have five or more digits.";
			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;

			Bill.Validation.ValidateABL_BillNumber();
			AssertNoMessageError(Bill.ABL_BillNumberInfo, expectedMessage);

			Bill.ABL_BillNumber = "1234";
			AssertHasMessageError(Bill.ABL_BillNumberInfo, expectedMessage);

			expectedMessage = "[NVC01] House B/L cannot contain the comma (\",\") symbol.";
			Bill.Validation.ValidateABL_BillNumber();
			AssertNoMessageError(Bill.ABL_BillNumberInfo, expectedMessage);

			Bill.ABL_BillNumber = "1234,";
			AssertHasMessageError(Bill.ABL_BillNumberInfo, expectedMessage);
		}

		public void TestCheckABL_GrossWeight()
		{
			var targetInfo = Bill.ABL_GrossWeightInfo;
			Bill.ABL_GrossWeightUQ = Core.Constants.Weight.Pounds;
			Bill.ABL_GrossWeight = 12345.678m;
			AssertHasMessageError(targetInfo, "The 'Weight' entered surpasses the maximum allowed by customs. Only 8 characters, including the decimal point, can be transmitted.");

			Bill.ABL_GrossWeightUQ = Core.Constants.Weight.Grams;
			AssertNoMessageError(targetInfo, "The 'Weight' entered surpasses the maximum allowed by customs. Only 8 characters, including the decimal point, can be transmitted.");

			Bill.ABL_GrossWeightUQ = Core.Constants.Weight.Pounds;
			Bill.ABL_GrossWeight = 12345.67m;
			AssertNoMessageError(targetInfo, "The 'Weight' entered surpasses the maximum allowed by customs. Only 8 characters, including the decimal point, can be transmitted.");

			Bill.ABL_GrossWeightUQ = ZString.Empty;
			Bill.ABL_GrossWeight = 0m;
			AssertNoMessageErrors(targetInfo);

			Bill.ABL_GrossWeightUQ = Core.Constants.Weight.Pounds;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);
		}

		public void TestCheckABL_GrossWeightUQ()
		{
			var targetInfo = Bill.ABL_GrossWeightUQInfo;
			Bill.ABL_GrossWeight = 0m;
			Bill.ABL_GrossWeightUQ = ZString.Empty;
			AssertNoMessageErrors(targetInfo);

			Bill.ABL_GrossWeight = 1m;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);
		}

		public void TestCheckCustomsWeight()
		{
			var expectedMessageError = "Value exceeds the upper limit.";
			Bill.ABL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			Bill.ABL_GrossWeight = 1000000m;
			AssertHasMessageError(Bill.CustomsWeightInfo, expectedMessageError);

			Bill.ABL_GrossWeight = 999999m;
			AssertNoMessageError(Bill.CustomsWeightInfo, expectedMessageError);
		}

		public void TestCheckCustomsNetWeight()
		{
			var expectedMessageError = "Value exceeds the upper limit.";
			Bill.ABL_NetWeightUQ = Core.Constants.Weight.Kilograms;
			Bill.ABL_NetWeight = 1000000m;
			AssertHasMessageError(Bill.CustomsNetWeightInfo, expectedMessageError);

			Bill.ABL_NetWeight = 999999m;
			AssertNoMessageError(Bill.CustomsNetWeightInfo, expectedMessageError);
		}

		public void TestCheckCustomsVolume()
		{
			var expectedMessageError = "Value exceeds the upper limit.";
			Bill.ABL_VolumeUQ = Core.Constants.Volume.CubicMetres;
			Bill.ABL_Volume = 1000000m;
			AssertHasMessageError(Bill.CustomsVolumeInfo, expectedMessageError);

			Bill.ABL_Volume = 999999m;
			AssertNoMessageError(Bill.CustomsVolumeInfo, expectedMessageError);
		}

		public void TestCheckABL_ShipperStreet1()
		{
			SetForHCH01Address();
			var bill = Bill;
			bill.ABL_ShipperStreet1 = new string('X', 30);
			bill.ABL_ShipperStreet2 = new string('Y', 30);
			bill.ABL_ShipperCity = new string('Z', 30);
			bill.ABL_ShipperPostcode = "6789?";
			bill.ABL_ShipperState = "12345";
			var truncatedAddress = bill.ShipperAddress.Substring(0, 105);
			var message = string.Format("Only the initial 105 bytes will be sent to customs as the address. The designated value sent to customs is \"{0}\". If this isn't desired, please clear the 'Party' field to override the values.", truncatedAddress);
			bill.Validation.ValidateABL_ShipperStreet1();
			Assert(!bill.ABL_ShipperStreet1Info.HasWarning(message));

			bill.ABL_RN_NKShipperCountry = "JP";
			truncatedAddress = bill.ShipperAddress.Substring(0, 105);
			message = string.Format("Only the initial 105 bytes will be sent to customs as the address. The designated value sent to customs is \"{0}\". If this isn't desired, please clear the 'Party' field to override the values.", truncatedAddress);
			bill.Validation.ValidateABL_ShipperStreet1();
			Assert(bill.ABL_ShipperStreet1Info.HasWarning(message));

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			bill.Validation.ValidateABL_ShipperStreet1();
			Assert(!bill.ABL_ShipperStreet1Info.HasWarning(message));

			CheckMessageErrorIfNotWesternEuropean(bill, x => x.ABL_ShipperStreet1Info);
		}

		public override void TestCheckABL_ShipperStreet2()
		{
			base.TestCheckABL_ShipperStreet2();
			CheckMessageErrorIfNotWesternEuropean(Bill, x => x.ABL_ShipperStreet2Info);
		}

		public void TestCheckABL_ShipperCity()
		{
			CheckMessageErrorIfNotWesternEuropean(Bill, x => x.ABL_ShipperCityInfo);
		}

		public void TestCheckABL_ConsigneeStreet2()
		{
			CheckMessageErrorIfNotWesternEuropean(Bill, x => x.ABL_ConsigneeStreet2Info);
		}

		public void TestCheckABL_ConsigneeCity()
		{
			CheckMessageErrorIfNotWesternEuropean(Bill, x => x.ABL_ConsigneeCityInfo);
		}

		public void TestCheckABL_NotifyPartyStreet2()
		{
			CheckMessageErrorIfNotWesternEuropean(Bill, x => x.ABL_NotifyPartyStreet2Info);
		}

		public void TestCheckABL_NotifyPartyCity()
		{
			CheckMessageErrorIfNotWesternEuropean(Bill, x => x.ABL_NotifyPartyCityInfo);
		}

		public void TestCheckABL_NotifyPartyStreet1()
		{
			CheckMessageErrorIfNotWesternEuropean(Bill, x => x.ABL_NotifyPartyStreet1Info);
		}

		public void TestCheckABL_ShipperState()
		{
			CheckMessageErrorIfNotWesternEuropean(Bill, x => x.ABL_ShipperStateInfo);
		}

		public void TestCheckABL_ConsigneeState()
		{
			CheckMessageErrorIfNotWesternEuropean(Bill, x => x.ABL_ConsigneeStateInfo);
		}

		public void TestCheckABL_NotifyPartyState()
		{
			CheckMessageErrorIfNotWesternEuropean(Bill, x => x.ABL_NotifyPartyStateInfo);
		}

		public void TestCheckABL_ConsigneeStreet1()
		{
			SetForHCH01Address();
			var bill = Bill;
			bill.ABL_ConsigneePostcode = new string('X', 10);
			bill.ABL_ConsigneeState = new string('Y', 20);
			bill.ABL_ConsigneeCity = new string('Z', 30);
			bill.ABL_ConsigneeStreet1 = new string('W', 30);
			bill.ABL_ConsigneeStreet2 = "1234567890?";

			var address = bill.ConsigneeAddress;
			var truncatedAddress = address.Substring(0, 105);
			var message = string.Format("Only the initial 105 bytes will be sent to customs as the address. The designated value sent to customs is \"{0}\". If this isn't desired, please clear the 'Party' field to override the values.", truncatedAddress);
			bill.Validation.ValidateABL_ConsigneeStreet1();
			AssertNoWarning(bill.ABL_ConsigneeStreet1Info, message);

			bill.ABL_ConsigneeStreet2 = "1234567890?1";
			address = bill.ConsigneeAddress;
			truncatedAddress = address.Substring(0, 105);
			message = string.Format("Only the initial 105 bytes will be sent to customs as the address. The designated value sent to customs is \"{0}\". If this isn't desired, please clear the 'Party' field to override the values.", truncatedAddress);
			bill.Validation.ValidateABL_ConsigneeStreet1();
			AssertHasWarning(bill.ABL_ConsigneeStreet1Info, message);

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			bill.Validation.ValidateABL_ConsigneeStreet1();
			AssertNoWarning(bill.ABL_ConsigneeStreet1Info, message);

			CheckMessageErrorIfNotWesternEuropean(bill, x => x.ABL_ConsigneeStreet1Info);
		}

		public void TestCheckABL_GoodsLocation()
		{
			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			var targetInfo = Bill.ABL_GoodsLocationInfo;
			var code = TestDataHelper.CreateJapanBondedAreaCode(Factory);

			Bill.ABL_GoodsLocation = "QQ";
			AssertHasMessageError(targetInfo, ListValidation.InvalidCodeMessageError);

			Bill.ABL_GoodsLocation = code;
			AssertNoMessageError(targetInfo, ListValidation.InvalidCodeMessageError);

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			Bill.ABL_GoodsLocation = "QQ";
			AssertNoMessageError(targetInfo, ListValidation.InvalidCodeMessageError);

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;

			Bill.TemporaryLandingStartDate = new ZDate(2024, 7, 26);
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			Bill.TemporaryLandingStartDate = ZDate.Empty;
			Bill.TemporaryLandingEndDate = new ZDate(2024, 7, 26);
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			Bill.TemporaryLandingEndDate = ZDate.Empty;
			Bill.TemporaryLandingBondedTransportCode = BondedTransportCodeList.Codes.Truck;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			Bill.TemporaryLandingBondedTransportCode = ZString.Empty;
			Bill.OtherLawsandRegulations.AddNew().CFR_Reference = "T";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			Bill.Validation.ValidateABL_GoodsLocation();
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheck_PartyRegNoType()
		{
			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;

			var consigneeInfo = Bill.ABL_ConsigneeRegNoTypeInfo;
			var shipperInfo = Bill.ABL_ShipperRegNoTypeInfo;
			var notifyPartyInfo = Bill.ABL_NotifyPartyRegNoTypeInfo;

			Bill.ABL_ConsigneeRegNo = "1234567890123";
			Bill.ABL_ShipperRegNo = "1234567890123";
			Bill.ABL_NotifyPartyRegNo = "1234567890123";
			AssertNoMessageErrorContaining(consigneeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(shipperInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(notifyPartyInfo, MandatoryValidation.YouHaveNotEntered);

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			Bill.ABL_ConsigneeRegNoType = ZString.Empty;
			Bill.ABL_ShipperRegNoType = ZString.Empty;
			Bill.ABL_NotifyPartyRegNoType = ZString.Empty;
			AssertNoMessageErrorContaining(consigneeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(shipperInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(notifyPartyInfo, MandatoryValidation.YouHaveNotEntered);

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
			Bill.Validation.ValidateABL_ConsigneeRegNoType();
			AssertHasMessageErrorContaining(consigneeInfo, MandatoryValidation.YouHaveNotEntered);
			Bill.Validation.ValidateABL_ShipperRegNoType();
			AssertHasMessageErrorContaining(shipperInfo, MandatoryValidation.YouHaveNotEntered);
			Bill.Validation.ValidateABL_NotifyPartyRegNoType();
			AssertNoMessageErrorContaining(notifyPartyInfo, MandatoryValidation.YouHaveNotEntered);

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			Bill.Validation.ValidateABL_ConsigneeRegNoType();
			AssertHasMessageErrorContaining(consigneeInfo, MandatoryValidation.YouHaveNotEntered);
			Bill.Validation.ValidateABL_ShipperRegNoType();
			AssertHasMessageErrorContaining(shipperInfo, MandatoryValidation.YouHaveNotEntered);
			Bill.Validation.ValidateABL_NotifyPartyRegNoType();
			AssertHasMessageErrorContaining(notifyPartyInfo, MandatoryValidation.YouHaveNotEntered);

			Bill.ABL_ConsigneeRegNoType = "FSB";
			Bill.Validation.ValidateABL_ConsigneeRegNoType();
			AssertHasMessageErrorContaining(consigneeInfo, CustomsRegNumTypeValidation.InvalidMessage.ToString());
			Bill.ABL_ConsigneeRegNoType = "LPC";
			Bill.Validation.ValidateABL_ConsigneeRegNoType();
			AssertNoMessageErrorContaining(consigneeInfo, CustomsRegNumTypeValidation.InvalidMessage.ToString());

			Bill.ABL_ShipperRegNoType = "CIE";
			Bill.Validation.ValidateABL_ShipperRegNoType();
			AssertHasMessageErrorContaining(shipperInfo, CustomsRegNumTypeValidation.InvalidMessage.ToString());
			Bill.ABL_ShipperRegNoType = "LPC";
			Bill.Validation.ValidateABL_ShipperRegNoType();
			AssertNoMessageErrorContaining(shipperInfo, CustomsRegNumTypeValidation.InvalidMessage.ToString());

			Bill.ABL_NotifyPartyRegNoType = "FSB";
			Bill.Validation.ValidateABL_NotifyPartyRegNoType();
			AssertHasMessageErrorContaining(notifyPartyInfo, CustomsRegNumTypeValidation.InvalidMessage.ToString());
			Bill.ABL_NotifyPartyRegNoType = "LPC";
			Bill.Validation.ValidateABL_NotifyPartyRegNoType();
			AssertNoMessageErrorContaining(notifyPartyInfo, CustomsRegNumTypeValidation.InvalidMessage.ToString());
		}

		public void TestCheck_PartyRegNo_Mandatory()
		{
			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;

			var consigneeInfo = Bill.ABL_ConsigneeRegNoInfo;
			var shipperInfo = Bill.ABL_ShipperRegNoInfo;
			var notifyPartyInfo = Bill.ABL_NotifyPartyRegNoInfo;

			Bill.ABL_ConsigneeRegNo = "1234567890123";
			Bill.ABL_ConsigneeRegNoType = "LPC";
			AssertNoMessageErrorContaining(consigneeInfo, MandatoryValidation.YouHaveNotEntered);
			Bill.ABL_ShipperRegNo = "1234567890123";
			Bill.ABL_ShipperRegNoType = "LPC";
			AssertNoMessageErrorContaining(shipperInfo, MandatoryValidation.YouHaveNotEntered);
			Bill.ABL_NotifyPartyRegNo = "1234567890123";
			Bill.ABL_NotifyPartyRegNoType = "LPC";
			AssertNoMessageErrorContaining(notifyPartyInfo, MandatoryValidation.YouHaveNotEntered);

			Bill.ABL_ConsigneeRegNo = ZString.Empty;
			Bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertHasMessageErrorContaining(consigneeInfo, MandatoryValidation.YouHaveNotEntered);
			Bill.ABL_ShipperRegNo = ZString.Empty;
			Bill.Validation.ValidateABL_ShipperRegNo();
			AssertHasMessageErrorContaining(shipperInfo, MandatoryValidation.YouHaveNotEntered);
			Bill.ABL_NotifyPartyRegNo = ZString.Empty;
			Bill.Validation.ValidateABL_NotifyPartyRegNo();
			AssertHasMessageErrorContaining(notifyPartyInfo, MandatoryValidation.YouHaveNotEntered);

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
			Bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertHasMessageErrorContaining(consigneeInfo, MandatoryValidation.YouHaveNotEntered);
			Bill.Validation.ValidateABL_ShipperRegNo();
			AssertHasMessageErrorContaining(shipperInfo, MandatoryValidation.YouHaveNotEntered);
			Bill.Validation.ValidateABL_NotifyPartyRegNo();
			AssertNoMessageErrorContaining(notifyPartyInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckOK_CustomsRegNo_JAS()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
			var bill = header.Bills.AddNew();
			var targetInfo = bill.ABL_ConsigneeRegNoInfo;

			bill.ABL_ConsigneeRegNoType = OrgCusCode.JapanCodeTypes.JAS;
			var errorMessage = "Please enter exactly 12 characters consisting solely of digits and capital letters [A-Z].";

			bill.ABL_ConsigneeRegNo = "12345678901";
			AssertHasMessageErrorContaining(targetInfo, errorMessage);

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertNoMessageErrorContaining(targetInfo, errorMessage);

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
			bill.ABL_ConsigneeRegNo = "123456789012";
			AssertNoMessageErrorContaining(targetInfo, errorMessage);

			bill.ABL_ConsigneeRegNo = "12345678901-";
			AssertHasMessageErrorContaining(targetInfo, errorMessage);

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertNoMessageErrorContaining(targetInfo, errorMessage);

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
			bill.ABL_ConsigneeRegNo = "12345678901L";
			AssertNoMessageErrorContaining(targetInfo, errorMessage);

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			bill.ABL_ConsigneeRegNo = "12345678901";
			AssertHasMessageErrorContaining(targetInfo, errorMessage);

			bill.ABL_ConsigneeRegNo = "12345678901L";
			AssertNoMessageErrorContaining(targetInfo, errorMessage);
		}

		public void TestCheckOK_CustomsRegNo_LPC()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
			var bill = header.Bills.AddNew();
			var targetInfo = bill.ABL_ConsigneeRegNoInfo;

			bill.ABL_ConsigneeRegNoType = OrgCusCode.JapanCodeTypes.LPC;
			var errorMessage = "Please enter 13 digits, or 17 digits.";

			bill.ABL_ConsigneeRegNo = "123456789012";
			AssertHasMessageErrorContaining(targetInfo, errorMessage);

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertNoMessageErrorContaining(targetInfo, errorMessage);

			bill.ABL_ConsigneeRegNo = "123456789012L";
			AssertNoMessageErrorContaining(targetInfo, errorMessage);

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
			bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertHasMessageErrorContaining(targetInfo, errorMessage);

			bill.ABL_ConsigneeRegNo = "1234567890123";
			AssertNoMessageErrorContaining(targetInfo, errorMessage);

			bill.ABL_ConsigneeRegNo = "1234567890123456";
			AssertHasMessageErrorContaining(targetInfo, errorMessage);

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertNoMessageErrorContaining(targetInfo, errorMessage);

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
			bill.ABL_ConsigneeRegNo = "12345678901234567";
			AssertNoMessageErrorContaining(targetInfo, errorMessage);

			bill.ABL_ConsigneeRegNo = ZString.Empty;
			AssertNoMessageErrorContaining(targetInfo, errorMessage);

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			bill.ABL_ConsigneeRegNo = "123456789012";
			AssertHasMessageErrorContaining(targetInfo, errorMessage);

			bill.ABL_ConsigneeRegNo = "12345678901234567";
			AssertNoMessageErrorContaining(targetInfo, errorMessage);
		}

		public void TestCheckOK_CustomsRegNo_CIE()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
			var bill = header.Bills.AddNew();
			var targetInfo = bill.ABL_ConsigneeRegNoInfo;

			bill.ABL_ConsigneeRegNoType = OrgCusCode.JapanCodeTypes.CIE;
			var expectedErrorMessage = "Please enter C0000 followed by 8 or 12 digits, or 1 followed by 7 or 11 digits.";

			bill.ABL_ConsigneeRegNo = "1123456";
			AssertHasMessageError(targetInfo, expectedErrorMessage);

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertNoMessageErrorContaining(targetInfo, expectedErrorMessage);

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
			bill.ABL_ConsigneeRegNo = "11234567";
			AssertNoMessageError(targetInfo, expectedErrorMessage);

			bill.ABL_ConsigneeRegNo = "112345678901";
			AssertNoMessageError(targetInfo, expectedErrorMessage);

			bill.ABL_ConsigneeRegNo = "C00001234567";
			AssertHasMessageError(targetInfo, expectedErrorMessage);

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			bill.Validation.ValidateABL_ConsigneeRegNo();
			AssertNoMessageErrorContaining(targetInfo, expectedErrorMessage);

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
			bill.ABL_ConsigneeRegNo = "C000012345678";
			AssertNoMessageError(targetInfo, expectedErrorMessage);

			bill.ABL_ConsigneeRegNo = "C0000123456789012";
			AssertNoMessageError(targetInfo, expectedErrorMessage);

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			bill.ABL_ConsigneeRegNo = "1123456";
			AssertHasMessageError(targetInfo, expectedErrorMessage);

			bill.ABL_ConsigneeRegNo = "C0000123456789012";
			AssertNoMessageError(targetInfo, expectedErrorMessage);
		}

		public void TestCheckOK_ShipperRegNo_LPC()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			var bill = header.Bills.AddNew();
			var targetInfo = bill.ABL_ShipperRegNoInfo;

			bill.ABL_ShipperRegNoType = OrgCusCode.JapanCodeTypes.LPC;
			var errorMessage = "Please enter 13 digits, or 17 digits.";

			bill.ABL_ShipperRegNo = "123456789012";
			AssertHasMessageErrorContaining(targetInfo, errorMessage);

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			bill.Validation.ValidateABL_ShipperRegNo();
			AssertNoMessageErrorContaining(targetInfo, errorMessage);

			bill.ABL_ShipperRegNo = "123456789012L";
			AssertNoMessageErrorContaining(targetInfo, errorMessage);

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			bill.Validation.ValidateABL_ShipperRegNo();
			AssertHasMessageErrorContaining(targetInfo, errorMessage);

			bill.ABL_ShipperRegNo = "1234567890123";
			AssertNoMessageErrorContaining(targetInfo, errorMessage);

			bill.ABL_ShipperRegNo = "1234567890123456";
			AssertHasMessageErrorContaining(targetInfo, errorMessage);

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			bill.Validation.ValidateABL_ShipperRegNo();
			AssertNoMessageErrorContaining(targetInfo, errorMessage);

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			bill.ABL_ShipperRegNo = "12345678901234567";
			AssertNoMessageErrorContaining(targetInfo, errorMessage);

			bill.ABL_ShipperRegNo = ZString.Empty;
			AssertNoMessageErrorContaining(targetInfo, errorMessage);
		}

		public void TestCheckOK_ShipperRegNo_FSB()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			var bill = header.Bills.AddNew();
			var targetInfo = bill.ABL_ShipperRegNoInfo;

			bill.ABL_ShipperRegNoType = OrgCusCode.JapanCodeTypes.FSB;
			var expectedErrorMessage = "Please enter exactly 12 characters consisting solely of digits and capital letters [A-Z]. The first character must be F.";

			bill.ABL_ShipperRegNo = "1123456";
			bill.Validation.ValidateABL_ShipperRegNo();
			AssertNoMessageErrorContaining(targetInfo, expectedErrorMessage);

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			bill.Validation.ValidateABL_ShipperRegNo();
			AssertHasMessageError(targetInfo, expectedErrorMessage);

			bill.ABL_ShipperRegNo = "11234567";
			AssertHasMessageError(targetInfo, expectedErrorMessage);

			bill.ABL_ShipperRegNo = "F0000011111A";
			AssertNoMessageErrorContaining(targetInfo, expectedErrorMessage);

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			bill.Validation.ValidateABL_ShipperRegNo();
			AssertNoMessageErrorContaining(targetInfo, expectedErrorMessage);
		}

		public void TestCheckOK_NotifyPartyRegNo_JAS()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			var bill = header.Bills.AddNew();
			var targetInfo = bill.ABL_NotifyPartyRegNoInfo;

			bill.ABL_NotifyPartyRegNoType = OrgCusCode.JapanCodeTypes.JAS;
			var errorMessage = "Please enter exactly 12 characters consisting solely of digits and capital letters [A-Z].";

			bill.ABL_NotifyPartyRegNo = "12345678901";
			AssertHasMessageErrorContaining(targetInfo, errorMessage);

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			bill.Validation.ValidateABL_NotifyPartyRegNo();
			AssertNoMessageErrorContaining(targetInfo, errorMessage);

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			bill.ABL_NotifyPartyRegNo = "123456789012";
			AssertNoMessageErrorContaining(targetInfo, errorMessage);

			bill.ABL_NotifyPartyRegNo = "12345678901-";
			AssertHasMessageErrorContaining(targetInfo, errorMessage);

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			bill.Validation.ValidateABL_NotifyPartyRegNo();
			AssertNoMessageErrorContaining(targetInfo, errorMessage);

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			bill.ABL_NotifyPartyRegNo = "12345678901L";
			AssertNoMessageErrorContaining(targetInfo, errorMessage);
		}

		public void TestCheckOK_NotifyPartyRegNo_LPC()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			var bill = header.Bills.AddNew();
			var targetInfo = bill.ABL_NotifyPartyRegNoInfo;

			bill.ABL_NotifyPartyRegNoType = OrgCusCode.JapanCodeTypes.LPC;
			var errorMessage = "Please enter 13 digits, or 17 digits.";

			bill.ABL_NotifyPartyRegNo = "123456789012";
			AssertHasMessageErrorContaining(targetInfo, errorMessage);

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			bill.Validation.ValidateABL_NotifyPartyRegNo();
			AssertNoMessageErrorContaining(targetInfo, errorMessage);

			bill.ABL_NotifyPartyRegNo = "123456789012L";
			AssertNoMessageErrorContaining(targetInfo, errorMessage);

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			bill.Validation.ValidateABL_NotifyPartyRegNo();
			AssertHasMessageErrorContaining(targetInfo, errorMessage);

			bill.ABL_NotifyPartyRegNo = "1234567890123";
			AssertNoMessageErrorContaining(targetInfo, errorMessage);

			bill.ABL_NotifyPartyRegNo = "1234567890123456";
			AssertHasMessageErrorContaining(targetInfo, errorMessage);

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			bill.Validation.ValidateABL_NotifyPartyRegNo();
			AssertNoMessageErrorContaining(targetInfo, errorMessage);

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			bill.ABL_NotifyPartyRegNo = "12345678901234567";
			AssertNoMessageErrorContaining(targetInfo, errorMessage);

			bill.ABL_NotifyPartyRegNo = ZString.Empty;
			AssertNoMessageErrorContaining(targetInfo, errorMessage);
		}

		public void TestCheckOK_NotifyPartyRegNo_CIE()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			var bill = header.Bills.AddNew();
			var targetInfo = bill.ABL_NotifyPartyRegNoInfo;

			bill.ABL_NotifyPartyRegNoType = OrgCusCode.JapanCodeTypes.CIE;
			var expectedErrorMessage = "Please enter C0000 followed by 8 or 12 digits, or 1 followed by 7 or 11 digits.";

			bill.ABL_NotifyPartyRegNo = "1123456";
			AssertHasMessageError(targetInfo, expectedErrorMessage);

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			bill.Validation.ValidateABL_NotifyPartyRegNo();
			AssertNoMessageErrorContaining(targetInfo, expectedErrorMessage);

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			bill.ABL_NotifyPartyRegNo = "11234567";
			AssertNoMessageError(targetInfo, expectedErrorMessage);

			bill.ABL_NotifyPartyRegNo = "112345678901";
			AssertNoMessageError(targetInfo, expectedErrorMessage);

			bill.ABL_NotifyPartyRegNo = "C00001234567";
			AssertHasMessageError(targetInfo, expectedErrorMessage);

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			bill.Validation.ValidateABL_NotifyPartyRegNo();
			AssertNoMessageErrorContaining(targetInfo, expectedErrorMessage);

			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			bill.ABL_NotifyPartyRegNo = "C000012345678";
			AssertNoMessageError(targetInfo, expectedErrorMessage);

			bill.ABL_NotifyPartyRegNo = "C0000123456789012";
			AssertNoMessageError(targetInfo, expectedErrorMessage);
		}

		void SetForHCH01Address()
		{
			Header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Japan;
			Header.AMA_TransportMode = TransportTypeList.Codes.Air;
			Header.AMA_Nature = JPJobMessageTypeList.Codes.Import;
		}

		public void TestCheckABL_ShipperName()
		{
			var bill = Bill;
			AssertNameLength(bill.ABL_ShipperNameInfo);
			CheckMessageErrorIfNotWesternEuropean(bill, x => x.ABL_ShipperNameInfo);
		}

		public void TestCheckABL_ConsigneeName()
		{
			var bill = Bill;
			AssertNameLength(bill.ABL_ConsigneeNameInfo);
			CheckMessageErrorIfNotWesternEuropean(bill, x => x.ABL_ConsigneeNameInfo);
		}

		public void TestCheckABL_NotifyPartyName()
		{
			var bill = Bill;
			AssertNameLength(bill.ABL_NotifyPartyNameInfo);
			CheckMessageErrorIfNotWesternEuropean(bill, x => x.ABL_NotifyPartyNameInfo);
		}

		public void TestCheckABL_ManifestQtyAndUQ()
		{
			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			Bill.ABL_ManifestQty = 0;
			Bill.ABL_ManifestUQ = ZString.Empty;

			var validation = Bill.RegularBillValidation;
			var manifestQtyInfo = Bill.ABL_ManifestQtyInfo;
			var manifestUQInfo = Bill.ABL_ManifestUQInfo;

			validation.ValidateABL_ManifestQty();
			AssertHasMessageErrorContaining(manifestQtyInfo, MandatoryValidation.YouHaveNotEntered);
			validation.ValidateABL_ManifestUQ();
			AssertHasMessageErrorContaining(manifestUQInfo, MandatoryValidation.YouHaveNotEntered);

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
			validation.ValidateABL_ManifestQty();
			AssertNoMessageError(manifestQtyInfo, MandatoryValidation.YouHaveNotEntered);
			validation.ValidateABL_ManifestUQ();
			AssertNoMessageError(manifestUQInfo, MandatoryValidation.YouHaveNotEntered);

			Bill.ABL_ManifestQty = 1;
			validation.ValidateABL_ManifestUQ();
			AssertHasMessageErrorContaining(manifestUQInfo, MandatoryValidation.YouHaveNotEntered);

			Bill.ABL_ManifestQty = 0;
			Bill.ABL_ManifestUQ = "KG";
			validation.ValidateABL_ManifestQty();
			AssertHasMessageErrorContaining(manifestQtyInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckABL_NetWeightUQ()
		{
			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			Bill.ABL_NetWeight = 1;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(Bill.ABL_NetWeightUQInfo, "XX", "DT");

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			Bill.ABL_NetWeightUQ = "XX";
			AssertNoMessageError(Bill.ABL_NetWeightUQInfo, ListValidation.InvalidCodeMessageError);

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
			Bill.Validation.ValidateABL_NetWeightUQ();
			AssertNoMessageError(Bill.ABL_NetWeightUQInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckABL_NetWeightAndUQ()
		{
			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			Bill.ABL_NetWeight = 1;
			Bill.ABL_NetWeightUQ = ZString.Empty;

			var validation = Bill.RegularBillValidation;
			var netWeightInfo = Bill.ABL_NetWeightInfo;
			var netWeightUQInfo = Bill.ABL_NetWeightUQInfo;

			validation.ValidateABL_NetWeightUQ();
			AssertHasMessageErrorContaining(netWeightUQInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(netWeightInfo, MandatoryValidation.YouHaveNotEntered);

			Bill.ABL_NetWeight = 0;
			Bill.ABL_NetWeightUQ = "KG";
			validation.ValidateABL_NetWeight();
			AssertHasMessageErrorContaining(netWeightInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(netWeightUQInfo, MandatoryValidation.YouHaveNotEntered);

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
			validation.ValidateABL_NetWeight();
			validation.ValidateABL_NetWeightUQ();
			AssertNoMessageError(netWeightInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(netWeightUQInfo, MandatoryValidation.YouHaveNotEntered);

			Bill.ABL_NetWeight = 1;
			Bill.ABL_NetWeightUQ = ZString.Empty;
			validation.ValidateABL_NetWeightUQ();
			AssertNoMessageError(netWeightInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(netWeightUQInfo, MandatoryValidation.YouHaveNotEntered);
		}

		void AssertNameLength(ZPropertyInfo info)
		{
			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			var validValue1 = new ZString('X', 70);
			var validValue2 = new ZString('春', 35);

			var warning1 = $"Only the initial 70 bytes will be sent to customs as the {info.HumanReadableName}. The designated character string sent to customs is \"{validValue1}\".";
			var warning2 = $"Only the initial 70 bytes will be sent to customs as the {info.HumanReadableName}. The designated character string sent to customs is \"{validValue2}\".";

			Bill.SetPropertyValue(info.Name, validValue1);
			AssertNoWarning(info, warning1);
			Bill.SetPropertyValue(info.Name, new ZString(validValue1 + "1"));
			AssertHasWarning(info, warning1);

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			Bill.SetPropertyValue(info.Name, new ZString(validValue1 + "2"));
			AssertNoWarning(info, warning1);

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			Bill.SetPropertyValue(info.Name, validValue2);
			AssertNoWarning(info, warning2);
			Bill.SetPropertyValue(info.Name, new ZString(validValue2 + "1"));
			AssertHasWarning(info, warning2);

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			Bill.SetPropertyValue(info.Name, new ZString(validValue2 + "2"));
			AssertNoWarning(info, warning1);
		}

		public void TestCheckABL_Tariff()
		{
			Factory.CreateTariffData(Universal.Constants.TariffTypes.Export, "010", "0101", "010121", "0101210003");
			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			Header.AMA_Nature = JPJobMessageTypeList.Codes.Export;
			var targetInfo = Bill.ABL_TariffInfo;
			var invalidCodeMessage = ListValidation.InvalidCodeMessage.ToString();
			var invalidLengthMessage = "Statistical/HS Code must be at least 4 characters long.";
			var validation = Bill.RegularBillValidation;
			CombineAssertions(() =>
			{
				Bill.ABL_Tariff = "0101";
				AssertNoMessageError("0101", targetInfo, invalidCodeMessage);
				AssertNoMessageError("0101", targetInfo, invalidLengthMessage);

				Bill.ABL_Tariff = "010121";
				AssertNoMessageErrors("010121", targetInfo);

				Bill.ABL_Tariff = "0101210003";
				AssertNoMessageErrors("0101210003", targetInfo);

				Bill.ABL_Tariff = "010122";
				AssertHasMessageError("010122", targetInfo, invalidCodeMessage);

				Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
				validation.ValidateABL_Tariff();
				AssertNoMessageError("No message error should show when HDF01.", targetInfo, invalidCodeMessage);

				Bill.ABL_Tariff = "0101234";
				AssertNoMessageError("No message error should show when HDF01.", targetInfo, invalidCodeMessage);

				Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
				validation.ValidateABL_Tariff();
				AssertNoMessageError("No message error should show when HCH01.", targetInfo, invalidCodeMessage);

				Bill.ABL_Tariff = "0101234";
				AssertNoMessageError("No message error should show when HCH01.", targetInfo, invalidCodeMessage);

				Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
				validation.ValidateABL_Tariff();
				AssertHasMessageError("0101234", targetInfo, invalidCodeMessage);

				Bill.ABL_Tariff = "010";
				AssertNoMessageError("010", targetInfo, invalidCodeMessage);
				AssertHasMessageError("010", targetInfo, invalidLengthMessage);

				Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
				validation.ValidateABL_Tariff();
				AssertNoMessageError("No message error should show when HDF01.", targetInfo, invalidLengthMessage);

				Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
				validation.ValidateABL_Tariff();
				AssertNoMessageError("No message error should show when HCH01.", targetInfo, invalidLengthMessage);
			});
		}

		public void TestCheckABL_MarksAndNumbers()
		{
			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Bill.ABL_MarksAndNumbersInfo);

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			Bill.ABL_MarksAndNumbers = ZString.Empty;
			AssertNoMessageErrorContaining(Bill.ABL_MarksAndNumbersInfo, MandatoryValidation.YouHaveNotEntered);

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
			Bill.Validation.ValidateABL_MarksAndNumbers();
			AssertNoMessageErrorContaining(Bill.ABL_MarksAndNumbersInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckABL_ShipmentType()
		{
			var targetInfo = Bill.ABL_ShipmentTypeInfo;
			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			Bill.ABL_ShipmentType = "XXX";
			AssertNoMessageErrors(targetInfo);

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			Bill.Validation.ValidateABL_ShipmentType();
			AssertNoMessageErrors(targetInfo);

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
			Bill.Validation.ValidateABL_ShipmentType();
			AssertNoMessageErrors(targetInfo);
		}

		void CheckMessageErrorIfNotWesternEuropean(AsycudaBill bill, Func<AsycudaBill, ZPropertyInfo> propertyInfSelector)
		{
			var targetInfo = propertyInfSelector.Invoke(bill);
			var humanReadableName = targetInfo.HumanReadableName;
			var message = $"{humanReadableName} only accepts Western European languages characters.";
			targetInfo.Value = (ZString)"ゐゐゐ";
			AssertHasMessageError($"{humanReadableName} has message error.", targetInfo, message);

			targetInfo.Value = (ZString)"A11";
			AssertNoMessageError($"{humanReadableName} has no message error.", targetInfo, message);
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

			var validation = Bill.RegularBillValidation;
			var targetInfo = Bill.ABL_RL_NKFinalDestinationInfo;

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;

			Bill.ABL_RL_NKFinalDestination = ZString.Empty;
			validation.ValidateABL_RL_NKFinalDestination();
			AssertHasMessageError(targetInfo, messageError);
			Bill.ABL_RL_NKFinalDestination = "USLAX";
			validation.ValidateABL_RL_NKFinalDestination();
			AssertNoMessageError(targetInfo, messageError);

			ValidationTestHelper.AssertInvalidCodeMessageError(targetInfo, "JPXXX", "JPTKX");
			Bill.ABL_RL_NKFinalDestination = "JPXXX";
			AssertHasMessageError(targetInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckFinalDestinationIATACode()
		{
			Factory.CreateUNLOCOData();
			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			var validation = Bill.RegularBillValidation;

			var targetInfo = Bill.FinalDestinationIATACodeInfo;
			var expectedErrorMessage = "You have not entered";
			validation.ValidateFinalDestinationIATACode();
			AssertHasMessageErrorContaining(targetInfo, expectedErrorMessage);

			Bill.ABL_RL_NKFinalDestination = "JP001";
			validation.ValidateFinalDestinationIATACode();
			AssertHasMessageErrorContaining(targetInfo, expectedErrorMessage);

			Bill.ABL_RL_NKFinalDestination = "JP002";
			Bill.FinalDestinationIATACode = "TKU";
			AssertNoMessageErrorContaining(targetInfo, expectedErrorMessage);
			expectedErrorMessage = string.Format("The entered {0} IATA Code is not recognized and will be cleared upon reloading the screen. To save this value, please either create a new {0} with the specified IATA Code or add it to an existing {0}.", Bill.ABL_RL_NKFinalDestinationInfo.HumanReadableName);
			AssertNoMessageError(targetInfo, expectedErrorMessage);

			Bill.ABL_RL_NKFinalDestination = ZString.Empty;
			Bill.FinalDestinationIATACode = "123";
			AssertHasMessageError(targetInfo, expectedErrorMessage);
		}

		public void TestCheckABL_RX_NKFreightValueCurrency()
		{
			AssertCurrencyCodeCannotBeEmpty(Bill.ABL_RX_NKFreightValueCurrencyInfo, Bill.ABL_FreightValueInfo);
		}

		public void TestCheckABL_RX_NKTransportValueCurrency()
		{
			AssertCurrencyCodeCannotBeEmpty(Bill.ABL_RX_NKTransportValueCurrencyInfo, Bill.ABL_TransportValueInfo);
		}

		public void TestCheckABL_RX_NKInsuranceValueCurrency()
		{
			AssertCurrencyCodeCanBeEmpty(Bill.ABL_RX_NKInsuranceValueCurrencyInfo, Bill.ABL_InsuranceValueInfo);
		}

		public void TestCheckDiscountValueCurrency()
		{
			AssertCurrencyCodeCanBeEmpty(Bill.DiscountValueCurrencyInfo, Bill.DiscountValueInfo);
		}

		public void TestCheckOtherChargesValueCurrency()
		{
			AssertCurrencyCodeCanBeEmpty(Bill.OtherChargesValueCurrencyInfo, Bill.OtherChargesValueInfo);
		}

		public void TestCheckABL_RX_NKCustomsValueCurrency()
		{
			AssertCurrencyCodeCanBeEmpty(Bill.ABL_RX_NKCustomsValueCurrencyInfo, Bill.ABL_CustomsValueInfo);
		}

		void AssertCurrencyCodeCannotBeEmpty(ZPropertyInfo targetInfo, ZPropertyInfo valueInfo)
		{
			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			Bill.SetPropertyValue(valueInfo.Name, new ZDecimal(10m));
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			Bill.SetPropertyValue(valueInfo.Name, new ZDecimal(0m));
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			Bill.SetPropertyValue(valueInfo.Name, new ZDecimal(10m));
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
			Bill.SetPropertyValue(valueInfo.Name, new ZDecimal(9m));
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		void AssertCurrencyCodeCanBeEmpty(ZPropertyInfo targetInfo, ZPropertyInfo valueInfo)
		{
			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
			Bill.SetPropertyValue(valueInfo.Name, new ZDecimal(10m));
			AssertNoMessageErrors(targetInfo);

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			Bill.SetPropertyValue(valueInfo.Name, new ZDecimal(9m));
			AssertNoMessageErrors(targetInfo);

			Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
			Bill.SetPropertyValue(valueInfo.Name, new ZDecimal(10m));
			AssertNoMessageErrors(targetInfo);
		}
	}
}
