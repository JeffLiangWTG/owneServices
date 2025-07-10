using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing;

[TestedType(typeof(TemporaryStorageBillValidation))]
sealed class TemporaryStorageBillValidationTest : BusinessObjectValidationTestCase
{
	public void TestTypeOfBillDocument()
	{
		var messageError = "You have not entered a Transport Document Type.";

		var tempHeader = Factory.New<TemporaryStorageHeader>();
		var tempBill = tempHeader.Bills.AddNew();
		var item1 = tempBill.PackedItems.AddNew();
		var item2 = tempBill.PackedItems.AddNew();
		var additionalInfos1 = item1.AdditionalInfos.AddNew();
		var additionalInfos2 = item2.AdditionalInfos.AddNew();
		additionalInfos2.CSI_SubType = AdditionalDocList.Codes.AdditionalInformation;

		CombineAssertions(() =>
		{
			tempBill.TypeOfBillDocument = "JPB";
			additionalInfos1.CSI_SubType = AdditionalDocList.Codes.TransportDocuments;
			tempBill.Validation.ValidateTypeOfBillDocument();
			AssertHasWarning("Transport Document Type should have warning message if it isn't empty and kind = TRA", tempBill.TypeOfBillDocumentInfo, "Transport documents must be declared either in Header or in Items, but not in both.");

			tempBill.TypeOfBillDocument = "";
			AssertHasMessageError("Transport Document Type have error if it is empty and have one INF item and one TRA item", tempBill.TypeOfBillDocumentInfo, messageError);

			additionalInfos2.CSI_SubType = AdditionalDocList.Codes.TransportDocuments;
			tempBill.Validation.ValidateTypeOfBillDocument();
			AssertNoMessageError("Transport Document Type have not error if it is empty and have all items are TRA", tempBill.TypeOfBillDocumentInfo, messageError);

			additionalInfos1.CSI_SubType = AdditionalDocList.Codes.AdditionalInformation;
			additionalInfos2.CSI_SubType = AdditionalDocList.Codes.AdditionalInformation;
			tempBill.Validation.ValidateTypeOfBillDocument();
			AssertHasMessageError("Transport Document Type have error if it is empty and have all items are INF", tempBill.TypeOfBillDocumentInfo, messageError);
		});
	}

	public void TestABL_BillNumber()
	{
		var messageError = "You have not entered a Transport Document.";

		var tempHeader = Factory.New<TemporaryStorageHeader>();
		var tempBill = tempHeader.Bills.AddNew();
		var item1 = tempBill.PackedItems.AddNew();
		var item2 = tempBill.PackedItems.AddNew();
		var additionalInfos1 = item1.AdditionalInfos.AddNew();
		var additionalInfos2 = item2.AdditionalInfos.AddNew();

		CombineAssertions(() =>
		{
			additionalInfos1.CSI_SubType = AdditionalDocList.Codes.TransportDocuments;
			additionalInfos2.CSI_SubType = AdditionalDocList.Codes.TransportDocuments;
			tempBill.Validation.ValidateABL_BillNumber();
			AssertNoMessageError("Transport Document have not error if it is empty and have all items are TRA", tempBill.ABL_BillNumberInfo, messageError);

			additionalInfos1.CSI_SubType = AdditionalDocList.Codes.AdditionalInformation;
			additionalInfos2.CSI_SubType = AdditionalDocList.Codes.AdditionalInformation;
			tempBill.Validation.ValidateABL_BillNumber();
			AssertHasMessageError("Transport Document have error if it is empty and have all items are INF", tempBill.ABL_BillNumberInfo, messageError);

			tempBill.ABL_BillNumber = "JPB";
			additionalInfos2.CSI_SubType = AdditionalDocList.Codes.TransportDocuments;
			tempBill.Validation.ValidateABL_BillNumber();
			AssertNoMessageError("Transport Document have not error if it isn't empty and have one INF item and one TRA item", tempBill.ABL_BillNumberInfo, messageError);
		});
	}

	public void TestValidateConsignorOrgPK_ShowsWarningForLAM()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		var tempBill = header.Bills.AddNew();

		CombineAssertions(() =>
		{
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			tempBill.Validation.ValidateConsignorOrgPK();
			AssertNoWarning(tempBill.ConsignorOrgPKInfo, "Consignor should be filled to set its EORI to the Owner ID of the lines entered in the Temporary Storage Register.");

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			tempBill.ABL_OA_Shipper = ZGuid.Empty;
			tempBill.Validation.ValidateConsignorOrgPK();
			AssertHasWarning(tempBill.ConsignorOrgPKInfo, "Consignor should be filled to set its EORI to the Owner ID of the lines entered in the Temporary Storage Register.");

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			tempBill.ABL_OA_Shipper = ZGuid.NewZGuid();
			tempBill.Validation.ValidateConsignorOrgPK();
			AssertNoWarning(tempBill.ConsignorOrgPKInfo, "Consignor should be filled to set its EORI to the Owner ID of the lines entered in the Temporary Storage Register.");
		});
	}

	public void TestValidateABL_ShipperName_NotMandatoryForTSMLAM()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		var tempBill = header.Bills.AddNew();

		CombineAssertions(() =>
		{
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
			tempBill.ABL_ShipperName = ZString.Empty;
			tempBill.Validation.ValidateABL_ShipperName();
			AssertHasMessageError("ShipperName should be mandatory for non-TSM/LAM", tempBill.ABL_ShipperNameInfo, "The ‘Name’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			tempBill.ABL_ShipperName = ZString.Empty;
			tempBill.Validation.ValidateABL_ShipperName();
			AssertHasWarning("ShipperName should not be mandatory for LAM", tempBill.ABL_ShipperNameInfo, "Consignor should be filled to set its EORI to the Owner ID of the lines entered in the Temporary Storage Register.");

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			tempBill.ABL_ShipperName = ZString.Empty;
			tempBill.Validation.ValidateABL_ShipperName();
			AssertNoWarning(tempBill.ABL_ShipperNameInfo, "Consignor should be filled to set its EORI to the Owner ID of the lines entered in the Temporary Storage Register.");
			AssertNoMessageError(tempBill.ABL_ShipperNameInfo, "The ‘Name’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
		});
	}

	public void TestValidateABL_RN_NKShipperCountry_NotMandatoryForTSMLAM()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		var tempBill = header.Bills.AddNew();
		CombineAssertions(() =>
		{
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
			tempBill.ABL_RN_NKShipperCountry = ZString.Empty;
			tempBill.Validation.ValidateABL_RN_NKShipperCountry();
			AssertHasMessageError("ShipperCountry should be mandatory for non-TSM/LAM", tempBill.ABL_RN_NKShipperCountryInfo, "The ‘Country’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			tempBill.ABL_RN_NKShipperCountry = ZString.Empty;
			tempBill.Validation.ValidateABL_RN_NKShipperCountry();
			AssertHasWarning("ShipperCountry should show warning for LAM", tempBill.ABL_RN_NKShipperCountryInfo, "Consignor should be filled to set its EORI to the Owner ID of the lines entered in the Temporary Storage Register.");

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			tempBill.ABL_RN_NKShipperCountry = ZString.Empty;
			tempBill.Validation.ValidateABL_RN_NKShipperCountry();
			AssertNoWarning(tempBill.ABL_RN_NKShipperCountryInfo, "Consignor should be filled to set its EORI to the Owner ID of the lines entered in the Temporary Storage Register.");
			AssertNoMessageError(tempBill.ABL_RN_NKShipperCountryInfo, "The ‘Country’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
		});
	}

	public void TestValidateABL_ShipperPostcode_NotMandatoryForTSMLAM()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		var tempBill = header.Bills.AddNew();

		CombineAssertions(() =>
		{
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
			tempBill.ABL_ShipperPostcode = ZString.Empty;
			tempBill.Validation.ValidateABL_ShipperPostcode();
			AssertHasMessageError("ShipperPostcode should be mandatory for non-TSM/LAM", tempBill.ABL_ShipperPostcodeInfo, "The ‘Postcode’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			tempBill.ABL_ShipperPostcode = ZString.Empty;
			tempBill.Validation.ValidateABL_ShipperPostcode();
			AssertHasWarning("ShipperPostcode should show warning for LAM", tempBill.ABL_ShipperPostcodeInfo, "Consignor should be filled to set its EORI to the Owner ID of the lines entered in the Temporary Storage Register.");

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			tempBill.ABL_ShipperPostcode = ZString.Empty;
			tempBill.Validation.ValidateABL_ShipperPostcode();
			AssertNoWarning(tempBill.ABL_ShipperPostcodeInfo, "Consignor should be filled to set its EORI to the Owner ID of the lines entered in the Temporary Storage Register.");
			AssertNoMessageError(tempBill.ABL_ShipperPostcodeInfo, "The ‘Postcode’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
		});
	}

	public void TestValidateABL_ShipperRegNoType_NotMandatoryForTSMLAM()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		var tempBill = header.Bills.AddNew();
		CombineAssertions(() =>
		{
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
			tempBill.ABL_ShipperRegNoType = ZString.Empty;
			tempBill.Validation.ValidateABL_ShipperRegNoType();
			AssertHasMessageError("ShipperRegNoType should be mandatory for non-TSM/LAM", tempBill.ABL_ShipperRegNoTypeInfo, "The ‘Type of Person’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			tempBill.ABL_ShipperRegNoType = ZString.Empty;
			tempBill.Validation.ValidateABL_ShipperRegNoType();
			AssertHasWarning("ShipperRegNoType should show warning for LAM", tempBill.ABL_ShipperRegNoTypeInfo, "Consignor should be filled to set its EORI to the Owner ID of the lines entered in the Temporary Storage Register.");

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			tempBill.ABL_ShipperRegNoType = ZString.Empty;
			tempBill.Validation.ValidateABL_ShipperRegNoType();
			AssertNoWarning(tempBill.ABL_ShipperRegNoTypeInfo, "Consignor should be filled to set its EORI to the Owner ID of the lines entered in the Temporary Storage Register.");
			AssertNoMessageError(tempBill.ABL_ShipperRegNoTypeInfo, "The ‘Type of Person’ of the Consignor must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
		});
	}

	public void TestValidateConsigneeOrgPK_ShowsWarningForTSM()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		var tempBill = header.Bills.AddNew();

		CombineAssertions(() =>
		{
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			tempBill.Validation.ValidateConsigneeOrgPK();
			AssertNoWarning(tempBill.ConsigneeOrgPKInfo, "Consignee should be filled to set its EORI to the Owner ID of the lines entered in the Temporary Storage Register.");

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			tempBill.ABL_OA_Consignee = ZGuid.Empty;
			tempBill.Validation.ValidateConsigneeOrgPK();
			AssertHasWarning(tempBill.ConsigneeOrgPKInfo, "Consignee should be filled to set its EORI to the Owner ID of the lines entered in the Temporary Storage Register.");

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			tempBill.ABL_OA_Consignee = ZGuid.NewZGuid();
			tempBill.Validation.ValidateConsigneeOrgPK();
			AssertNoWarning(tempBill.ConsigneeOrgPKInfo, "Consignee should be filled to set its EORI to the Owner ID of the lines entered in the Temporary Storage Register.");
		});
	}

	public void TestValidateABL_ConsigneeName_NotMandatoryForTSMLAM()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		var tempBill = header.Bills.AddNew();
		CombineAssertions(() =>
		{
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
			tempBill.ABL_ConsigneeName = ZString.Empty;
			tempBill.Validation.ValidateABL_ConsigneeName();
			AssertHasMessageError("ConsigneeName should be mandatory for non-TSM/LAM", tempBill.ABL_ConsigneeNameInfo, "The ‘Name’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			tempBill.ABL_ConsigneeName = ZString.Empty;
			tempBill.Validation.ValidateABL_ConsigneeName();
			AssertHasWarning("ConsigneeName should show warning for TSM", tempBill.ABL_ConsigneeNameInfo, "Consignee should be filled to set its EORI to the Owner ID of the lines entered in the Temporary Storage Register.");

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			tempBill.ABL_ConsigneeName = ZString.Empty;
			tempBill.Validation.ValidateABL_ConsigneeName();
			AssertNoWarning(tempBill.ABL_ConsigneeNameInfo, "Consignee should be filled to set its EORI to the Owner ID of the lines entered in the Temporary Storage Register.");
			AssertNoMessageError(tempBill.ABL_ConsigneeNameInfo, "The ‘Name’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
		});
	}

	public void TestValidateABL_RN_NKConsigneeCountry_NotMandatoryForTSMLAM()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		var tempBill = header.Bills.AddNew();
		CombineAssertions(() =>
		{
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
			tempBill.ABL_RN_NKConsigneeCountry = ZString.Empty;
			tempBill.Validation.ValidateABL_RN_NKConsigneeCountry();
			AssertHasMessageError("ConsigneeCountry should be mandatory for non-TSM/LAM", tempBill.ABL_RN_NKConsigneeCountryInfo, "The ‘Country’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			tempBill.ABL_RN_NKConsigneeCountry = ZString.Empty;
			tempBill.Validation.ValidateABL_RN_NKConsigneeCountry();
			AssertHasWarning("ConsigneeCountry should not be mandatory for TSM", tempBill.ABL_RN_NKConsigneeCountryInfo, "Consignee should be filled to set its EORI to the Owner ID of the lines entered in the Temporary Storage Register.");

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			tempBill.ABL_RN_NKConsigneeCountry = ZString.Empty;
			tempBill.Validation.ValidateABL_RN_NKConsigneeCountry();
			AssertNoWarning(tempBill.ABL_RN_NKConsigneeCountryInfo, "Consignee should be filled to set its EORI to the Owner ID of the lines entered in the Temporary Storage Register.");
			AssertNoMessageError(tempBill.ABL_RN_NKConsigneeCountryInfo, "The ‘Country’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
		});
	}

	public void TestValidateABL_ConsigneePostcode_NotMandatoryForTSMLAM()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		var tempBill = header.Bills.AddNew();
		CombineAssertions(() =>
		{
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
			tempBill.ABL_ConsigneePostcode = ZString.Empty;
			tempBill.Validation.ValidateABL_ConsigneePostcode();
			AssertHasMessageError("ConsigneePostcode should be mandatory for non-TSM/LAM", tempBill.ABL_ConsigneePostcodeInfo, "The ‘Postcode’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			tempBill.ABL_ConsigneePostcode = ZString.Empty;
			tempBill.Validation.ValidateABL_ConsigneePostcode();
			AssertHasWarning("ConsigneePostcode should show warning for TSM", tempBill.ABL_ConsigneePostcodeInfo, "Consignee should be filled to set its EORI to the Owner ID of the lines entered in the Temporary Storage Register.");

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			tempBill.ABL_ConsigneePostcode = ZString.Empty;
			tempBill.Validation.ValidateABL_ConsigneePostcode();
			AssertNoWarning(tempBill.ABL_ConsigneePostcodeInfo, "Consignee should be filled to set its EORI to the Owner ID of the lines entered in the Temporary Storage Register.");
			AssertNoMessageError(tempBill.ABL_ConsigneePostcodeInfo, "The ‘Postcode’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
		});
	}

	public void TestValidateABL_ConsigneeRegNoType_NotMandatoryForTSMLAM()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		var tempBill = header.Bills.AddNew();

		CombineAssertions(() =>
		{
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Expedition;
			tempBill.ABL_ConsigneeRegNoType = ZString.Empty;
			tempBill.Validation.ValidateABL_ConsigneeRegNoType();
			AssertHasMessageError("ConsigneeRegNoType should be mandatory for non-TSM/LAM", tempBill.ABL_ConsigneeRegNoTypeInfo, "The ‘Type of Person’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			tempBill.ABL_ConsigneeRegNoType = ZString.Empty;
			tempBill.Validation.ValidateABL_ConsigneeRegNoType();
			AssertHasWarning("ConsigneeRegNoType should show warning for TSM", tempBill.ABL_ConsigneeRegNoTypeInfo, "Consignee should be filled to set its EORI to the Owner ID of the lines entered in the Temporary Storage Register.");

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			tempBill.ABL_ConsigneeRegNoType = ZString.Empty;
			tempBill.Validation.ValidateABL_ConsigneeRegNoType();
			AssertNoWarning(tempBill.ABL_ConsigneeRegNoTypeInfo, "Consignee should be filled to set its EORI to the Owner ID of the lines entered in the Temporary Storage Register.");
			AssertNoMessageError(tempBill.ABL_ConsigneeRegNoTypeInfo, "The ‘Type of Person’ of the Consignee must be filled in the tab ‘Bill Parties’ or an organization must be selected.");
		});
	}
}
