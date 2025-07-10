using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusSCAHouseValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCusSCAHouse()
		{
			CusSCAHouse parent = Factory.New<CusSCAHouse>();
			AssertEquals(parent.Validation.House, parent);
		}

		public void TestValidateMaxCountOfPacking()
		{
			for (int i = 0; i <= 998; i++)
			{
				house.PackLines.AddNew();
			}
			house.Validation.ValidateMaxCountOfPacking();
			AssertHasRowMessageErrorContaining(house, @"The maximum number of packing rows allowed in ACI is 998.");
		}

		public void TestCarrierCodeValidation()
		{
			ValidationTestHelper.RemoveCarrierCodeFromCurrentCompany(Factory);
			house.Validation.ValidateCarrierCode();
			AssertHasMessageErrorContaining(house.CarrierCodeInfo, @"Carrier Code. The carrier code is entered on the Proxy Organization assigned on the Canadian Company.
(i.e. Maintain -> User Admin -> Companies -> Organization -> Detail -> Config -> Standard Carrier Alpha Code)");
			ValidationTestHelper.AddCarrierCodeToCurrentCompany(Factory, "8080");
			Factory.ClearCachedValue<ZString>("CA.Business.CandianCarrierCode");
			house.Validation.ValidateCarrierCode();
			AssertNoMessageErrors(house.CarrierCodeInfo);
		}

		public void TestSRNValidation()
		{
			house.SupplementaryReferenceNumber = ZString.Empty;
			house.Validation.ValidateSupplementaryReferenceNumber();
			AssertHasMessageErrorContaining(house.SupplementaryReferenceNumberInfo, "This number is allocated automatically to the shipment");
			house.SupplementaryReferenceNumber = "XXX";
			house.Validation.ValidateSupplementaryReferenceNumber();
			AssertNoMessageErrors(house.SupplementaryReferenceNumberInfo);
		}

		public void TestOriginalCCNValidation()
		{
			house.OriginalCCN = ZString.Empty;
			house.Validation.ValidateOriginalCCN();
			AssertHasMessageErrorContaining(house.OriginalCCNInfo, "The CCN is defaulted from the consol");
			house.OriginalCCN = "081-";
			house.Validation.ValidateOriginalCCN();
			AssertHasMessageErrorContaining(house.OriginalCCNInfo, "The CCN is defaulted from the consol");
			house.OriginalCCN = "081-9990";
			house.Validation.ValidateOriginalCCN();
			AssertNoMessageErrors(house.OriginalCCNInfo);
		}

		public void TestCA_PortOfDestinationValidation()
		{
			house.CA_RL_NK_PortOfDestination = ZString.Empty;
			house.Validation.ValidateCA_RL_NK_PortOfDestination();
			AssertHasMessageErrorContaining(house.CA_RL_NK_PortOfDestinationInfo, MandatoryValidation.YouHaveNotEntered);
			house.CA_RL_NK_PortOfDestination = "XXQW?";
			AssertHasMessageErrorContaining(house.CA_RL_NK_PortOfDestinationInfo, ListValidation.InvalidCodeMessageError);
			house.CA_RL_NK_PortOfDestination = "CATOR";
			AssertNoMessageErrors(house.CA_RL_NK_PortOfDestinationInfo);
		}

		public void TestCA_FROBTransitImportCodeValidation()
		{
			house.CA_FROBTransitImportCode = ZString.Empty;
			house.Validation.ValidateCA_FROBTransitImportCode();
			AssertHasMessageErrorContaining(house.CA_FROBTransitImportCodeInfo, MandatoryValidation.YouHaveNotEntered);
			house.CA_FROBTransitImportCode = "XX?";
			AssertHasMessageErrorContaining(house.CA_FROBTransitImportCodeInfo, ListValidation.InvalidCodeMessageError);
			house.CA_FROBTransitImportCode = InTransitCodeList.Codes.InTransit;
			AssertNoMessageErrors(house.CA_FROBTransitImportCodeInfo);
			house.CA_FROBTransitImportCode = InTransitCodeList.Codes.FROB;
			AssertNoMessageErrors(house.CA_FROBTransitImportCodeInfo);
		}

		public void TestCA_ConsigneeNameValidation()
		{
			house.CA_ConsigneeName = ZString.Empty;
			house.Validation.ValidateCA_ConsigneeName();
			AssertHasMessageErrorContaining(house.CA_ConsigneeNameInfo, MandatoryValidation.YouHaveNotEntered);
			house.CA_ConsigneeName = "XXXX";
			AssertNoMessageErrors(house.CA_ConsigneeNameInfo);
		}

		public void TestCA_ConsigneeAddress1Validation()
		{
			house.CA_ConsigneeAddress1 = ZString.Empty;
			house.Validation.ValidateCA_ConsigneeAddress1();
			AssertHasMessageErrorContaining(house.CA_ConsigneeAddress1Info, MandatoryValidation.YouHaveNotEntered);
			house.CA_ConsigneeAddress1 = "123456789012345678901234567890123456";
			AssertHasWarningContaining(house.CA_ConsigneeAddress1Info, "exceeds the maximum allowed");
			house.CA_ConsigneeAddress1 = "12345678901234567890123456789012345";
			AssertNoMessageErrors(house.CA_ConsigneeAddress1Info);
		}

		public void TestCA_ConsigneeAddress2Validation()
		{
			house.CA_ConsigneeAddress2 = ZString.Empty;
			house.Validation.ValidateCA_ConsigneeAddress2();
			AssertNoMessageErrors(house.CA_ConsigneeAddress2Info);
			house.CA_ConsigneeAddress2 = "123456789012345678901234567890123456";
			AssertHasWarningContaining(house.CA_ConsigneeAddress2Info, "exceeds the maximum allowed");
			house.CA_ConsigneeAddress2 = "12345678901234567890123456789012345";
			AssertNoMessageErrors(house.CA_ConsigneeAddress2Info);
		}

		public void TestCA_ConsigneeStateValidation()
		{
			house.CA_RN_NKConsigneeCountryCode = Core.Constants.CountryCodes.Canada;
			house.CA_ConsigneeState = "1234567890";
			AssertHasWarningContaining(house.CA_ConsigneeStateInfo, "exceeds the maximum allowed");
			house.CA_ConsigneeState = "123456789";
			AssertNoMessageErrorContaining(house.CA_ConsigneeStateInfo, "exceeds the maximum allowed");
			AssertHasMessageErrorContaining(house.CA_ConsigneeStateInfo, "Invalid Canadian province code");
			house.CA_ConsigneeState = "YT";
			AssertNoMessageErrors(house.CA_ConsigneeStateInfo);
		}

		public void TestCA_ConsigneePostcodeValidation()
		{
			house.CA_RN_NKConsigneeCountryCode = Core.Constants.CountryCodes.Canada;
			house.CA_ConsigneePostcode = ZString.Empty;
			house.Validation.ValidateCA_ConsigneePostcode();
			AssertHasMessageErrorContaining(house.CA_ConsigneePostcodeInfo, "A postal/zip code is required when the country/region is Canada or United States.");
			house.CA_ConsigneePostcode = "1234567890";
			AssertHasWarningContaining(house.CA_ConsigneePostcodeInfo, "exceeds the maximum allowed");
			house.CA_ConsigneePostcode = "123456789";
			AssertNoMessageErrorContaining(house.CA_ConsigneePostcodeInfo, "exceeds the maximum allowed");
			AssertHasMessageErrorContaining(house.CA_ConsigneePostcodeInfo, "A Canadian postal code should be in the following format: A9A9A9");
			house.CA_ConsigneePostcode = "B1C2D3";
			AssertNoMessageErrors(house.CA_ConsigneePostcodeInfo);
		}

		public void TestCA_ConsigneeSuburbValidation()
		{
			house.CA_ConsigneeSuburb = ZString.Empty;
			house.Validation.ValidateCA_ConsigneeSuburb();
			AssertHasMessageErrorContaining(house.CA_ConsigneeSuburbInfo, MandatoryValidation.YouHaveNotEntered);
			house.CA_ConsigneeSuburb = "123456789012345678901234567890123456";
			AssertHasWarningContaining(house.CA_ConsigneeSuburbInfo, "exceeds the maximum allowed");
			house.CA_ConsigneeSuburb = "12345678901234567890123456789012345";
			AssertNoMessageErrors(house.CA_ConsigneeSuburbInfo);
		}

		public void TestCA_RN_NKConsigneeCountryCodeValidation()
		{
			house.CA_RN_NKConsigneeCountryCode = ZString.Empty;
			house.Validation.ValidateCA_RN_NKConsigneeCountryCode();
			AssertHasMessageErrorContaining(house.CA_RN_NKConsigneeCountryCodeInfo, MandatoryValidation.YouHaveNotEntered);
			house.CA_RN_NKConsigneeCountryCode = "??";
			AssertHasMessageErrorContaining(house.CA_RN_NKConsigneeCountryCodeInfo, ListValidation.InvalidCodeMessageError);
			house.CA_RN_NKConsigneeCountryCode = "CA";
			AssertNoMessageErrors(house.CA_RN_NKConsigneeCountryCodeInfo);
		}

		public void TestCA_ConsignorNameValidation()
		{
			house.CA_ConsignorName = ZString.Empty;
			house.Validation.ValidateCA_ConsignorName();
			AssertHasMessageErrorContaining(house.CA_ConsignorNameInfo, MandatoryValidation.YouHaveNotEntered);
			house.CA_ConsignorName = "XXXX";
			AssertNoMessageErrors(house.CA_ConsignorNameInfo);
		}

		public void TestCA_ConsignorAddress1Validation()
		{
			house.CA_ConsignorAddress1 = ZString.Empty;
			house.Validation.ValidateCA_ConsignorAddress1();
			AssertHasMessageErrorContaining(house.CA_ConsignorAddress1Info, MandatoryValidation.YouHaveNotEntered);
			house.CA_ConsignorAddress1 = "123456789012345678901234567890123456";
			AssertHasWarningContaining(house.CA_ConsignorAddress1Info, "exceeds the maximum allowed");
			house.CA_ConsignorAddress1 = "12345678901234567890123456789012345";
			AssertNoMessageErrors(house.CA_ConsignorAddress1Info);
		}

		public void TestCA_ConsignorAddress2Validation()
		{
			house.CA_ConsignorAddress2 = ZString.Empty;
			house.Validation.ValidateCA_ConsignorAddress2();
			AssertNoMessageErrors(house.CA_ConsignorAddress2Info);
			house.CA_ConsignorAddress2 = "123456789012345678901234567890123456";
			AssertHasWarningContaining(house.CA_ConsignorAddress2Info, "exceeds the maximum allowed");
			house.CA_ConsignorAddress2 = "12345678901234567890123456789012345";
			AssertNoMessageErrors(house.CA_ConsignorAddress2Info);
		}

		public void TestCA_ConsignorStateValidation()
		{
			house.CA_RN_NKConsignorCountryCode = Core.Constants.CountryCodes.UnitedStates;
			house.CA_ConsignorState = "1234567890";
			AssertHasWarningContaining(house.CA_ConsignorStateInfo, "exceeds the maximum allowed");
			house.CA_ConsignorState = "123456789";
			AssertNoMessageErrorContaining(house.CA_ConsignorStateInfo, "exceeds the maximum allowed");
			AssertHasMessageErrorContaining(house.CA_ConsignorStateInfo, "Invalid US state code");
			house.CA_ConsignorState = "CA";
			AssertNoMessageErrors(house.CA_ConsignorStateInfo);
		}

		public void TestCA_ConsignorPostcodeValidation()
		{
			house.CA_RN_NKConsignorCountryCode = Core.Constants.CountryCodes.UnitedStates;
			house.CA_ConsignorPostcode = ZString.Empty;
			house.Validation.ValidateCA_ConsignorPostcode();
			AssertHasMessageErrorContaining(house.CA_ConsignorPostcodeInfo, "A postal/zip code is required when the country/region is Canada or United States.");
			house.CA_ConsignorPostcode = "1234567890";
			AssertHasWarningContaining(house.CA_ConsignorPostcodeInfo, "exceeds the maximum allowed");
			house.CA_ConsignorPostcode = "NY";
			AssertHasMessageErrorContaining(house.CA_ConsignorPostcodeInfo, "This US zip code is invalid");
			house.CA_ConsignorPostcode = "12345";
			AssertNoMessageErrors(house.CA_ConsignorPostcodeInfo);
			AssertNoWarnings(house.CA_ConsignorPostcodeInfo);
		}

		public void TestCA_ConsignorSuburbValidation()
		{
			house.CA_ConsignorSuburb = ZString.Empty;
			house.Validation.ValidateCA_ConsignorSuburb();
			AssertHasMessageErrorContaining(house.CA_ConsignorSuburbInfo, MandatoryValidation.YouHaveNotEntered);
			house.CA_ConsignorSuburb = "123456789012345678901234567890123456";
			AssertHasWarningContaining(house.CA_ConsignorSuburbInfo, "exceeds the maximum allowed");
			house.CA_ConsignorSuburb = "12345678901234567890123456789012345";
			AssertNoMessageErrors(house.CA_ConsignorSuburbInfo);
		}

		public void TestCA_RN_NKConsignorCountryCodeValidation()
		{
			house.CA_RN_NKConsignorCountryCode = ZString.Empty;
			house.Validation.ValidateCA_RN_NKConsignorCountryCode();
			AssertHasMessageErrorContaining(house.CA_RN_NKConsignorCountryCodeInfo, MandatoryValidation.YouHaveNotEntered);
			house.CA_RN_NKConsignorCountryCode = "??";
			AssertHasMessageErrorContaining(house.CA_RN_NKConsignorCountryCodeInfo, ListValidation.InvalidCodeMessageError);
			house.CA_RN_NKConsignorCountryCode = "CA";
			AssertNoMessageErrors(house.CA_RN_NKConsignorCountryCodeInfo);
		}

		public void TestCA_DeliveryAddress1Validation()
		{
			house.CA_DeliveryAddress1 = ZString.Empty;
			house.Validation.ValidateCA_DeliveryAddress1();
			AssertNoMessageErrors(house.CA_DeliveryAddress1Info);
			house.CA_DeliveryName = "XXXX";
			house.Validation.ValidateCA_DeliveryAddress1();
			AssertHasMessageErrorContaining(house.CA_DeliveryAddress1Info, MandatoryValidation.YouHaveNotEntered);
			house.CA_DeliveryAddress1 = "123456789012345678901234567890123456";
			AssertHasWarningContaining(house.CA_DeliveryAddress1Info, "exceeds the maximum allowed");
			house.CA_DeliveryAddress1 = "12345678901234567890123456789012345";
			AssertNoMessageErrors(house.CA_DeliveryAddress1Info);
		}

		public void TestCA_DeliveryAddress2Validation()
		{
			house.CA_DeliveryAddress2 = ZString.Empty;
			house.Validation.ValidateCA_DeliveryAddress2();
			AssertNoMessageErrors(house.CA_DeliveryAddress2Info);
			house.CA_DeliveryAddress2 = "123456789012345678901234567890123456";
			AssertHasWarningContaining(house.CA_DeliveryAddress2Info, "exceeds the maximum allowed");
			house.CA_DeliveryAddress2 = "12345678901234567890123456789012345";
			AssertNoMessageErrors(house.CA_DeliveryAddress2Info);
		}

		public void TestCA_DeliveryStateValidation()
		{
			house.CA_DeliveryState = ZString.Empty;
			house.Validation.ValidateCA_DeliveryState();
			AssertNoMessageErrors(house.CA_DeliveryStateInfo);
			house.CA_DeliveryState = "1234567890";
			AssertHasWarningContaining(house.CA_DeliveryStateInfo, "exceeds the maximum allowed");
			house.CA_DeliveryState = "123456789";
			AssertNoMessageErrors(house.CA_DeliveryStateInfo);
		}

		public void TestCA_DeliveryPostcodeValidation()
		{
			house.CA_DeliveryPostcode = ZString.Empty;
			house.Validation.ValidateCA_DeliveryPostcode();
			AssertNoMessageErrors(house.CA_DeliveryPostcodeInfo);
			house.CA_DeliveryPostcode = "1234567890";
			AssertHasWarningContaining(house.CA_DeliveryPostcodeInfo, "exceeds the maximum allowed");
			house.CA_DeliveryPostcode = "123456789";
			AssertNoMessageErrors(house.CA_DeliveryPostcodeInfo);
		}

		public void TestCA_DeliverySuburbValidation()
		{
			house.CA_DeliverySuburb = ZString.Empty;
			house.Validation.ValidateCA_DeliverySuburb();
			AssertNoMessageErrors(house.CA_DeliverySuburbInfo);
			house.CA_DeliveryName = "XXXX";
			house.Validation.ValidateCA_DeliverySuburb();
			AssertHasMessageErrorContaining(house.CA_DeliverySuburbInfo, MandatoryValidation.YouHaveNotEntered);
			house.CA_DeliverySuburb = "123456789012345678901234567890123456";
			AssertHasWarningContaining(house.CA_DeliverySuburbInfo, "exceeds the maximum allowed");
			house.CA_DeliverySuburb = "12345678901234567890123456789012345";
			AssertNoMessageErrors(house.CA_DeliverySuburbInfo);
		}

		public void TestCA_RN_NKDeliveryCountryCodeValidation()
		{
			house.CA_RN_NKDeliveryCountryCode = ZString.Empty;
			house.Validation.ValidateCA_RN_NKDeliveryCountryCode();
			AssertNoMessageErrors(house.CA_RN_NKDeliveryCountryCodeInfo);
			house.CA_DeliveryName = "XXXX";
			house.Validation.ValidateCA_RN_NKDeliveryCountryCode();
			AssertHasMessageErrorContaining(house.CA_RN_NKDeliveryCountryCodeInfo, MandatoryValidation.YouHaveNotEntered);
			house.CA_RN_NKDeliveryCountryCode = "??";
			AssertHasMessageErrorContaining(house.CA_RN_NKDeliveryCountryCodeInfo, ListValidation.InvalidCodeMessageError);
			house.CA_RN_NKDeliveryCountryCode = "CA";
			AssertNoMessageErrors(house.CA_RN_NKDeliveryCountryCodeInfo);
		}

		public void TestCA_DeliveryContactNameValidation()
		{
			house.CA_ConsigneeName = "JANE";
			house.CA_DeliveryContactName = ZString.Empty;
			house.Validation.ValidateCA_DeliveryContactName();
			AssertNoMessageErrorContaining(house.CA_DeliveryContactNameInfo, MandatoryValidation.YouHaveNotEntered);
			house.CA_DeliveryName = "JANE";
			house.Validation.ValidateCA_DeliveryContactName();
			AssertNoMessageErrorContaining(house.CA_DeliveryContactNameInfo, MandatoryValidation.YouHaveNotEntered);
			house.CA_DeliveryName = "JIM";
			house.Validation.ValidateCA_DeliveryContactName();
			AssertHasMessageErrorContaining(house.CA_DeliveryContactNameInfo, MandatoryValidation.YouHaveNotEntered);
			house.CA_DeliveryContactName = "FREDDO";
			AssertNoMessageErrors(house.CA_DeliveryContactNameInfo);
		}

		public void TestCA_NotifyAddress1Validation()
		{
			house.CA_NotifyAddress1 = ZString.Empty;
			house.Validation.ValidateCA_NotifyAddress1();
			AssertNoMessageErrors(house.CA_NotifyAddress1Info);
			house.CA_NotifyName = "XXXX";
			house.Validation.ValidateCA_NotifyAddress1();
			AssertHasMessageErrorContaining(house.CA_NotifyAddress1Info, MandatoryValidation.YouHaveNotEntered);
			house.CA_NotifyAddress1 = "123456789012345678901234567890123456";
			AssertHasWarningContaining(house.CA_NotifyAddress1Info, "exceeds the maximum allowed");
			house.CA_NotifyAddress1 = "12345678901234567890123456789012345";
			AssertNoMessageErrors(house.CA_NotifyAddress1Info);
		}

		public void TestCA_NotifyAddress2Validation()
		{
			house.CA_NotifyAddress2 = ZString.Empty;
			house.Validation.ValidateCA_NotifyAddress2();
			AssertNoMessageErrors(house.CA_NotifyAddress2Info);
			house.CA_NotifyAddress2 = "123456789012345678901234567890123456";
			AssertHasWarningContaining(house.CA_NotifyAddress2Info, "exceeds the maximum allowed");
			house.CA_NotifyAddress2 = "12345678901234567890123456789012345";
			AssertNoMessageErrors(house.CA_NotifyAddress2Info);
		}

		public void TestCA_NotifyStateValidation()
		{
			house.CA_NotifyState = ZString.Empty;
			house.Validation.ValidateCA_NotifyState();
			AssertNoMessageErrors(house.CA_NotifyStateInfo);
			house.CA_NotifyState = "1234567890";
			AssertHasWarningContaining(house.CA_NotifyStateInfo, "exceeds the maximum allowed");
			house.CA_NotifyState = "123456789";
			AssertNoMessageErrors(house.CA_NotifyStateInfo);
		}

		public void TestCA_NotifyPostcodeValidation()
		{
			house.CA_NotifyPostcode = ZString.Empty;
			house.Validation.ValidateCA_NotifyPostcode();
			AssertNoMessageErrors(house.CA_NotifyPostcodeInfo);
			house.CA_NotifyPostcode = "1234567890";
			AssertHasWarningContaining(house.CA_NotifyPostcodeInfo, "exceeds the maximum allowed");
			house.CA_NotifyPostcode = "123456789";
			AssertNoMessageErrors(house.CA_NotifyPostcodeInfo);
		}

		public void TestCA_NotifySuburbValidation()
		{
			house.CA_NotifySuburb = ZString.Empty;
			house.Validation.ValidateCA_NotifySuburb();
			AssertNoMessageErrors(house.CA_NotifySuburbInfo);
			house.CA_NotifyName = "XXXX";
			house.Validation.ValidateCA_NotifySuburb();
			AssertHasMessageErrorContaining(house.CA_NotifySuburbInfo, MandatoryValidation.YouHaveNotEntered);
			house.CA_NotifySuburb = "123456789012345678901234567890123456";
			AssertHasWarningContaining(house.CA_NotifySuburbInfo, "exceeds the maximum allowed");
			house.CA_NotifySuburb = "12345678901234567890123456789012345";
			AssertNoMessageErrors(house.CA_NotifySuburbInfo);
		}

		public void TestCA_RN_NKNotifyCountryCodeValidation()
		{
			house.CA_RN_NKNotifyCountryCode = ZString.Empty;
			house.Validation.ValidateCA_RN_NKNotifyCountryCode();
			AssertNoMessageErrors(house.CA_RN_NKNotifyCountryCodeInfo);
			house.CA_NotifyName = "XXXX";
			house.Validation.ValidateCA_RN_NKNotifyCountryCode();
			AssertHasMessageErrorContaining(house.CA_RN_NKNotifyCountryCodeInfo, MandatoryValidation.YouHaveNotEntered);
			house.CA_RN_NKNotifyCountryCode = "??";
			AssertHasMessageErrorContaining(house.CA_RN_NKNotifyCountryCodeInfo, ListValidation.InvalidCodeMessageError);
			house.CA_RN_NKNotifyCountryCode = "CA";
			AssertNoMessageErrors(house.CA_RN_NKNotifyCountryCodeInfo);
		}

		public void TestValidateEnglishStrictCharacters()
		{
			var engHeader = Factory.New<OrgHeader>();
			engHeader.OH_Code = "ENGORG";
			engHeader.OH_FullName = "TEST ENG COMPANY";
			engHeader.MainAddress.OA_Address1 = "TEST ENG ADDRESS 1";
			engHeader.MainAddress.OA_Address2 = "TEST ENG ADDRESS 2";
			engHeader.MainAddress.OA_City = "TEST CITY";
			engHeader.MainAddress.OA_PostCode = "12345A";
			engHeader.MainAddress.OA_RN_NKCountryCode = "";
			engHeader.OH_Language = Core.SharedConstants.Languages.EnglishAmerican;

			var frnHeader = Factory.New<OrgHeader>();
			frnHeader.OH_Code = "FREORG";
			frnHeader.OH_FullName = "TEST FRÉ COMPÄNY";
			frnHeader.MainAddress.OA_Address1 = "TêST FRÈ àDDRESS 1";
			frnHeader.MainAddress.OA_Address2 = "TëST FRE ÀDDRESS 2";
			frnHeader.MainAddress.OA_City = "TEST CÏTY";
			frnHeader.MainAddress.OA_PostCode = "12345Ä";
			frnHeader.MainAddress.OA_RN_NKCountryCode = "";
			frnHeader.OH_Language = Core.SharedConstants.Languages.French;

			var chsHeader = Factory.New<OrgHeader>();
			chsHeader.OH_Code = "测试company";
			chsHeader.OH_FullName = "测试 chinese company";
			chsHeader.MainAddress.OA_Address1 = "测试 chinese address 1";
			chsHeader.MainAddress.OA_Address2 = "测试 chinese address 2";
			chsHeader.MainAddress.OA_PostCode = "测试 12345";
			chsHeader.MainAddress.OA_City = "测试 city";
			chsHeader.MainAddress.OA_RN_NKCountryCode = "";
			chsHeader.OH_Language = Core.SharedConstants.Languages.ChineseSimplified;

			house.CA_OH_Consignee = engHeader.PK;
			AssertNoMessageErrors(house.CA_ConsigneeNameInfo);
			AssertNoMessageErrors(house.CA_ConsigneeAddress1Info);
			AssertNoMessageErrors(house.CA_ConsigneeAddress2Info);
			AssertNoMessageErrors(house.CA_ConsigneeSuburbInfo);
			AssertNoMessageErrors(house.CA_ConsigneePostcodeInfo);

			house.CA_OH_Consignor = engHeader.PK;
			AssertNoMessageErrors(house.CA_ConsignorNameInfo);
			AssertNoMessageErrors(house.CA_ConsignorAddress1Info);
			AssertNoMessageErrors(house.CA_ConsignorAddress2Info);
			AssertNoMessageErrors(house.CA_ConsignorSuburbInfo);
			AssertNoMessageErrors(house.CA_ConsignorPostcodeInfo);

			house.CA_OH_Notify = engHeader.PK;
			AssertNoMessageErrors(house.CA_NotifyNameInfo);
			AssertNoMessageErrors(house.CA_NotifyAddress1Info);
			AssertNoMessageErrors(house.CA_NotifyAddress2Info);
			AssertNoMessageErrors(house.CA_NotifySuburbInfo);
			AssertNoMessageErrors(house.CA_NotifyPostcodeInfo);

			house.CA_OA_DeliveryAddress = engHeader.MainAddress.PK;
			AssertNoMessageErrors(house.CA_DeliveryNameInfo);
			AssertNoMessageErrors(house.CA_DeliveryAddress1Info);
			AssertNoMessageErrors(house.CA_DeliveryAddress2Info);
			AssertNoMessageErrors(house.CA_DeliverySuburbInfo);
			AssertNoMessageErrors(house.CA_DeliveryPostcodeInfo);

			house.CA_OH_Consignee = frnHeader.PK;
			AssertNoMessageErrors(house.CA_ConsigneeNameInfo);
			AssertNoMessageErrors(house.CA_ConsigneeAddress1Info);
			AssertNoMessageErrors(house.CA_ConsigneeAddress2Info);
			AssertNoMessageErrors(house.CA_ConsigneeSuburbInfo);
			AssertNoMessageErrors(house.CA_ConsigneePostcodeInfo);

			house.CA_OH_Consignor = frnHeader.PK;
			AssertNoMessageErrors(house.CA_ConsignorNameInfo);
			AssertNoMessageErrors(house.CA_ConsignorAddress1Info);
			AssertNoMessageErrors(house.CA_ConsignorAddress2Info);
			AssertNoMessageErrors(house.CA_ConsigneeSuburbInfo);
			AssertNoMessageErrors(house.CA_ConsigneePostcodeInfo);

			house.CA_OH_Notify = frnHeader.PK;
			AssertNoMessageErrors(house.CA_NotifyNameInfo);
			AssertNoMessageErrors(house.CA_NotifyAddress1Info);
			AssertNoMessageErrors(house.CA_NotifyAddress2Info);
			AssertNoMessageErrors(house.CA_NotifySuburbInfo);
			AssertNoMessageErrors(house.CA_NotifyPostcodeInfo);

			house.CA_OA_DeliveryAddress = frnHeader.MainAddress.PK;
			AssertNoMessageErrors(house.CA_DeliveryNameInfo);
			AssertNoMessageErrors(house.CA_DeliveryAddress1Info);
			AssertNoMessageErrors(house.CA_DeliveryAddress2Info);
			AssertNoMessageErrors(house.CA_DeliverySuburbInfo);
			AssertNoMessageErrors(house.CA_DeliveryPostcodeInfo);

			house.CA_OH_Consignee = chsHeader.PK;
			AssertHasMessageErrorContaining(house.CA_ConsigneeNameInfo, EnglishAddressCharactersValidation.GetNotificationMessage(house.CA_ConsigneeNameInfo));
			AssertHasMessageErrorContaining(house.CA_ConsigneeAddress1Info, EnglishAddressCharactersValidation.GetNotificationMessage(house.CA_ConsigneeAddress1Info));
			AssertHasMessageErrorContaining(house.CA_ConsigneeAddress2Info, EnglishAddressCharactersValidation.GetNotificationMessage(house.CA_ConsigneeAddress2Info));
			AssertHasMessageErrorContaining(house.CA_ConsigneeSuburbInfo, EnglishAddressCharactersValidation.GetNotificationMessage(house.CA_ConsigneeSuburbInfo));
			AssertHasMessageErrorContaining(house.CA_ConsigneePostcodeInfo, EnglishAddressCharactersValidation.GetNotificationMessage(house.CA_ConsigneePostcodeInfo));

			house.CA_OH_Consignor = chsHeader.PK;
			AssertHasMessageErrorContaining(house.CA_ConsignorNameInfo, EnglishAddressCharactersValidation.GetNotificationMessage(house.CA_ConsignorNameInfo));
			AssertHasMessageErrorContaining(house.CA_ConsignorAddress1Info, EnglishAddressCharactersValidation.GetNotificationMessage(house.CA_ConsignorAddress1Info));
			AssertHasMessageErrorContaining(house.CA_ConsignorAddress2Info, EnglishAddressCharactersValidation.GetNotificationMessage(house.CA_ConsignorAddress2Info));
			AssertHasMessageErrorContaining(house.CA_ConsigneeSuburbInfo, EnglishAddressCharactersValidation.GetNotificationMessage(house.CA_ConsigneeSuburbInfo));
			AssertHasMessageErrorContaining(house.CA_ConsigneePostcodeInfo, EnglishAddressCharactersValidation.GetNotificationMessage(house.CA_ConsigneePostcodeInfo));

			house.CA_OH_Notify = chsHeader.PK;
			AssertHasMessageErrorContaining(house.CA_NotifyNameInfo, EnglishAddressCharactersValidation.GetNotificationMessage(house.CA_NotifyNameInfo));
			AssertHasMessageErrorContaining(house.CA_NotifyAddress1Info, EnglishAddressCharactersValidation.GetNotificationMessage(house.CA_NotifyAddress1Info));
			AssertHasMessageErrorContaining(house.CA_NotifyAddress2Info, EnglishAddressCharactersValidation.GetNotificationMessage(house.CA_NotifyAddress2Info));
			AssertHasMessageErrorContaining(house.CA_NotifySuburbInfo, EnglishAddressCharactersValidation.GetNotificationMessage(house.CA_NotifySuburbInfo));
			AssertHasMessageErrorContaining(house.CA_NotifyPostcodeInfo, EnglishAddressCharactersValidation.GetNotificationMessage(house.CA_NotifyPostcodeInfo));

			house.CA_OA_DeliveryAddress = chsHeader.MainAddress.PK;
			AssertHasMessageErrorContaining(house.CA_DeliveryNameInfo, EnglishAddressCharactersValidation.GetNotificationMessage(house.CA_DeliveryNameInfo));
			AssertHasMessageErrorContaining(house.CA_DeliveryAddress1Info, EnglishAddressCharactersValidation.GetNotificationMessage(house.CA_DeliveryAddress1Info));
			AssertHasMessageErrorContaining(house.CA_DeliveryAddress2Info, EnglishAddressCharactersValidation.GetNotificationMessage(house.CA_DeliveryAddress2Info));
			AssertHasMessageErrorContaining(house.CA_DeliverySuburbInfo, EnglishAddressCharactersValidation.GetNotificationMessage(house.CA_DeliverySuburbInfo));
			AssertHasMessageErrorContaining(house.CA_DeliveryPostcodeInfo, EnglishAddressCharactersValidation.GetNotificationMessage(house.CA_DeliveryPostcodeInfo));
		}

		CusSCAHouse house;
		protected override void SetUp()
		{
			base.SetUp();
			CusSCAOceanBill oceanBill = Factory.New<CusSCAOceanBill>();
			house = oceanBill.HouseBills.AddNew();
		}
	}
}
