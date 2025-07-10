using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.JP.Common.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.Business.Testing;

[TestedType(typeof(AsycudaManifestHeaderValidation))]
sealed class AsycudaManifestHeaderValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckAMA_CustomsAgentCredentialPK()
	{
		var expectedMessageError = "You have not entered a NACCS Credential.";
		var (_, glbExternalPasswordPK2, _) = new DefaultBrokerAndCredentialTestHelper().CreateBrokerStaffAndReturnPK();
		Header.AMA_CustomsAgentCredentialPK = ZGuid.Empty;
		AssertHasMessageError(Header.AMA_CustomsAgentCredentialPKInfo, expectedMessageError);

		Header.AMA_GS_NKCustomsAgent = "AN";
		header.AMA_CustomsAgentCredentialPK = glbExternalPasswordPK2;
		AssertNoMessageError(Header.AMA_CustomsAgentCredentialPKInfo, expectedMessageError);

		expectedMessageError = "The entered value is not in the list.";
		AssertNoMessageError(Header.AMA_CustomsAgentCredentialPKInfo, expectedMessageError);

		header.AMA_CustomsAgentCredentialPK = ZGuid.NewZGuid();
		AssertHasMessageError(Header.AMA_CustomsAgentCredentialPKInfo, expectedMessageError);
	}

	public void TestCheckAMA_GS_NKCustomsAgent()
	{
		Header.Validation.ValidateAll();
		AssertHasMessageError(Header.AMA_GS_NKCustomsAgentInfo, "You have not entered a Customs Agent.");

		Header.AMA_GS_NKCustomsAgent = "LX";
		AssertNoMessageError(Header.AMA_GS_NKCustomsAgentInfo, "You have not entered a Customs Agent.");
	}

	public void TestCheckAMA_CarrierCodeForSea()
	{
		var message = "The entered Carrier Code is different from the one configured in the organization.";
		var targetInfo = Header.AMA_CarrierCodeInfo;
		var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
		shippingLine.RSL_StandardCarrierAlphaCode = "1234";

		var carrier = Factory.NewWithValidTestData<OrgHeader>();
		carrier.OH_Code = "BBG";
		carrier.OH_RSL_ShippingLine = shippingLine.PK;

		var header = Factory.NewWithValidTestData<OrgHeader>();
		header.OH_Code = "CCA";
		Factory.Save();

		Header.AMA_ManifestType = ZString.Empty;
		Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
		Header.AMA_OA_Carrier = carrier.MainAddress.PK;
		Header.AMA_CarrierCode = "1234";
		AssertNoWarning(targetInfo, message);
		Header.AMA_CarrierCode = "4321";
		AssertHasWarning(targetInfo, message);
		Header.AMA_CarrierCode = ZString.Empty;
		AssertHasWarning(targetInfo, message);

		Header.AMA_OA_Carrier = header.MainAddress.PK;
		Header.AMA_CarrierCode = "4321";
		AssertNoWarning(targetInfo, message);
	}

	public void TestCheckAMA_CustomsOffice()
	{
		var validCode = "11";
		Factory.CreateRefDataForTest(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, validCode);

		var header = Factory.New<AsycudaManifestHeader>();
		header.AMA_TransportMode = TransportTypeList.Codes.Air;
		header.AMA_Nature = JPJobMessageTypeList.Codes.Import;

		header.AMA_CustomsOffice = "xx";
		AssertHasMessageError(header.AMA_CustomsOfficeInfo, ListValidation.InvalidCodeMessageError);

		header.AMA_CustomsOffice = "11";
		AssertNoMessageError(header.AMA_CustomsOfficeInfo, ListValidation.InvalidCodeMessageError);

		header.AMA_Nature = JPJobMessageTypeList.Codes.Export;
		header.AMA_CustomsOffice = "xx";
		AssertHasMessageError(header.AMA_CustomsOfficeInfo, ListValidation.InvalidCodeMessageError);
	}

	public void TestCheckAMA_MasterBill_IsSendNVC01BondedLocationAmendment()
	{
		Header.AMA_TransportMode = TransportTypeList.Codes.Sea;

		var info = Header.AMA_MasterBillInfo;
		Header.Validation.ValidateAMA_MasterBill();
		AssertNoMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);

		var context = new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.NVC01, Action = JPMessageActionList.Codes.Five };
		using (Header.SetCurrentMessageSendingContext(context))
		{
			Header.Validation.ValidateAMA_MasterBill();
			AssertHasMessageErrorContaining(info, MandatoryValidation.YouHaveNotEntered);
		}
	}

	public void TestCheckAMA_MasterBill()
	{
		Header.AMA_TransportMode = TransportTypeList.Codes.Air;
		Header.AMA_Nature = JPJobMessageTypeList.Codes.Import;
		Header.IsSubConsolidation = true;
		AssertHasMessageErrorContaining(Header.AMA_MasterBillInfo, MandatoryValidation.YouHaveNotEntered);
		Header.AMA_MasterBill = "123456789123456789";
		AssertNoMessageErrorContaining(Header.AMA_MasterBillInfo, MandatoryValidation.YouHaveNotEntered);
		AssertHasMessageError(Header.AMA_MasterBillInfo, "The MAWB must be limited to 16 characters or fewer.");
		Header.AMA_MasterBill = "123456";
		AssertNoMessageErrors(Header.AMA_MasterBillInfo);

		Header.AMA_Nature = JPJobMessageTypeList.Codes.Export;
		AssertHasMessageError(Header.AMA_MasterBillInfo, "The MAWB should contain 11 digits.");	
		Header.AMA_MasterBill = "96312345678";
		AssertNoMessageError(Header.AMA_MasterBillInfo, "The MAWB should contain 11 digits.");
		AssertHasMessageError(Header.AMA_MasterBillInfo, "Invalid check digit. The last digit should be '5'");
		Header.AMA_MasterBill = "96312345675";
		AssertNoMessageError(Header.AMA_MasterBillInfo, "Invalid check digit. The last digit should be '5'");
		AssertHasMessageError(Header.AMA_MasterBillInfo, "The first three characters should be a valid airline IATA code.");
		Header.AMA_MasterBill = "96612345675";
		AssertNoMessageErrors(Header.AMA_MasterBillInfo);
	}

	public void TestCheckAMA_BookingNumber()
	{
		Header.AMA_TransportMode = "SEA";
		Header.AMA_Nature = "EXP";
		var warningMessage = "ZZZZ will be populated as the booking number if the booking number is not provided.";
		var targetInfo = Header.AMA_BookingNumberInfo;
		Header.AMA_BookingNumber = "123";
		AssertNoWarning(targetInfo, warningMessage);

		Header.AMA_BookingNumber = ZString.Empty;
		AssertHasWarning(targetInfo, warningMessage);
	}

	public void TestCheckViaLocationCode()
	{
		var messageError = "Via location must be empty if at least one seal number is entered.";
		var header = Factory.New<AsycudaManifestHeader>();
		header.AMA_TransportMode = TransportTypeList.Codes.Sea;
		var container = header.Containers.AddNew();
		var code = TestDataHelper.CreateJapanBondedAreaCode(Factory);

		ValidationTestHelper.AssertInvalidCodeMessageError(header.ViaLocationCodeInfo, "XXXXX", code, "The entered Move-In Destination is invalid. Please select a value from the list.");
		header.ViaLocationCode = code;
		container.VanningLocationCode = code;
		header.Validation.ValidateViaLocationCode();
		AssertHasMessageError(header.ViaLocationCodeInfo, "The same bonded area code is entered in the via location code and the vanning location code.");
		container.VanningLocationCode = "3QRA9";
		header.Validation.ValidateViaLocationCode();
		AssertNoMessageError(header.ViaLocationCodeInfo, "The same bonded area code is entered in the via location code and the vanning location code.");

		header.ViaLocationCode = "XXX";
		var targetInfo = header.ViaLocationCodeInfo;
		AssertNoMessageError(targetInfo, messageError);

		container.ACN_Seal1 = "123";
		AssertCheckViaLocationCode();

		container.ACN_Seal1 = ZString.Empty;
		container.ACN_Seal2 = "123";
		AssertCheckViaLocationCode();

		container.ACN_Seal2 = ZString.Empty;
		container.ACN_Seal3 = "123";
		AssertCheckViaLocationCode();

		container.ACN_Seal3 = ZString.Empty;
		container.AdditionalSeals.AddNew().BK_SealNumber = "123";
		AssertCheckViaLocationCode();

		header.AMA_TransportMode = TransportTypeList.Codes.Air;
		header.ViaLocationCode = "YYY";
		AssertNoMessageError(targetInfo, messageError);

		void AssertCheckViaLocationCode()
		{
			header.ViaLocationCode = ZString.Empty;
			AssertNoMessageError(targetInfo, messageError);
			header.ViaLocationCode = "XXX";
			AssertHasMessageError(targetInfo, messageError);
		}
	}

	public void TestCheckAMA_RL_NKPortOfFinalDeparture()
	{
		Header.AMA_ManifestType = ZString.Empty;
		Header.AMA_TransportMode = TransportTypeList.Codes.Sea;

		var code = TestDataHelper.CreateJapanBondedAreaCode(Factory);
		ValidationTestHelper.AssertInvalidCodeMessageError(Header.AMA_RL_NKPortOfFinalDepartureInfo, "XXXXX", code, "The entered Move-In Destination is invalid. Please select a value from the list.");

		var messageErrorText = "Please enter either Move-In Destination or Vessel Call Sign.";
		var warningText = "If Move-In Destination is left blank, your vessel call sign will be sent as the Move-In Destination instead.";

		Header.AMA_RL_NKPortOfFinalDeparture = ZString.Empty;
		Header.Validation.ValidateAMA_RL_NKPortOfFinalDeparture();
		AssertHasMessageError(Header.AMA_RL_NKPortOfFinalDepartureInfo, messageErrorText);
		AssertNoWarning(Header.AMA_RL_NKPortOfFinalDepartureInfo, warningText);

		Header.AMA_RadioCallSign = "1234";
		Header.Validation.ValidateAMA_RL_NKPortOfFinalDeparture();
		AssertNoMessageError(Header.AMA_RL_NKPortOfFinalDepartureInfo, messageErrorText);
		AssertHasWarning(Header.AMA_RL_NKPortOfFinalDepartureInfo, warningText);

		Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
		Header.AMA_RadioCallSign = ZString.Empty;
		Header.Validation.ValidateAMA_RL_NKPortOfFinalDeparture();
		AssertNoMessageError(Header.AMA_RL_NKPortOfFinalDepartureInfo, messageErrorText);
		AssertNoWarning(Header.AMA_RL_NKPortOfFinalDepartureInfo, warningText);

		Header.AMA_RadioCallSign = "1234";
		Header.Validation.ValidateAMA_RL_NKPortOfFinalDeparture();
		AssertNoMessageError(Header.AMA_RL_NKPortOfFinalDepartureInfo, messageErrorText);
		AssertNoWarning(Header.AMA_RL_NKPortOfFinalDepartureInfo, warningText);
	}

	public void TestCheckAMA_Voyage()
	{
		var targetInfo = Header.AMA_VoyageInfo;
		Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
		Header.Validation.ValidateAMA_Voyage();
		AssertNoMessageErrors(targetInfo);

		Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
		Header.Validation.ValidateAMA_Voyage();
		AssertHasMessageErrors(targetInfo);

		Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
		Header.Validation.ValidateAMA_Voyage();
		AssertNoMessageErrors(targetInfo);
	}

	public void TestCheckAMA_OA_Carrier()
	{
		var targetInfo = Header.AMA_OA_CarrierInfo;
		Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
		Header.Validation.ValidateAMA_OA_Carrier();
		AssertNoWarnings(targetInfo);

		Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
		Header.Validation.ValidateAMA_OA_Carrier();
		AssertNoWarnings(targetInfo);

		Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
		Header.Validation.ValidateAMA_OA_Carrier();
		AssertNoWarnings(targetInfo);
	}

	public void TestCheckAMA_RN_NKConveyanceNationalityMandatory()
	{
		var targetInfo = Header.AMA_RN_NKConveyanceNationalityInfo;
		Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
		Header.Validation.ValidateAMA_RN_NKConveyanceNationality();
		AssertNoWarnings(targetInfo);
	}

	public void TesAMA_VoyageMandatory()
	{
		Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.NVC;
		var expectedMessage = "You have not entered";
		Header.Validation.ValidateAMA_Voyage();
		AssertHasWarningContaining(Header.AMA_VoyageInfo, expectedMessage);

		Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
		Header.Validation.ValidateAMA_Voyage();
		AssertNoWarningContaining(Header.AMA_VoyageInfo, expectedMessage);

		Header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
		Header.Validation.ValidateAMA_Voyage();
		AssertHasWarningContaining(Header.AMA_VoyageInfo, expectedMessage);
	}

	public void TestCheckPortOfLoadingIATACode()
	{
		Factory.CreateUNLOCOData();
		Header.AMA_TransportMode = TransportTypeList.Codes.Air;
		var targetInfo = Header.PortOfLoadingIATACodeInfo;
		var expectedErrorMessage = "You have not entered";
		Header.Validation.ValidatePortOfLoadingIATACode();
		AssertHasMessageErrorContaining(targetInfo, expectedErrorMessage);

		Header.AMA_RL_NKPortOfLoading = "JP001";
		Header.Validation.ValidatePortOfLoadingIATACode();
		AssertHasMessageErrorContaining(targetInfo, expectedErrorMessage);

		Header.AMA_RL_NKPortOfLoading = "JP002";
		Header.PortOfLoadingIATACode = "TKU";
		AssertNoMessageErrorContaining(targetInfo, expectedErrorMessage);
		expectedErrorMessage = string.Format("The entered {0} IATA Code is not recognized and will be cleared upon reloading the screen. To save this value, please either create a new {0} with the specified IATA Code or add it to an existing {0}.", Header.AMA_RL_NKPortOfLoadingInfo.HumanReadableName);
		AssertNoMessageError(targetInfo, expectedErrorMessage);

		Header.AMA_RL_NKPortOfLoading = ZString.Empty;
		Header.PortOfLoadingIATACode = "123";
		AssertHasMessageError(targetInfo, expectedErrorMessage);
	}

	public void TestCheckPortOfDischargeIATACode()
	{
		Factory.CreateUNLOCOData();
		Header.AMA_TransportMode = TransportTypeList.Codes.Air;
		var targetInfo = Header.PortOfDischargeIATACodeInfo;
		var expectedErrorMessage = "You have not entered";
		Header.Validation.ValidatePortOfDischargeIATACode();
		AssertHasMessageErrorContaining(targetInfo, expectedErrorMessage);

		Header.AMA_RL_NKPortOfDischarge = "JP001";
		Header.Validation.ValidatePortOfDischargeIATACode();
		AssertHasMessageErrorContaining(targetInfo, expectedErrorMessage);

		Header.AMA_RL_NKPortOfDischarge = "JP002";
		Header.PortOfDischargeIATACode = "TKU";
		AssertNoMessageErrorContaining(targetInfo, expectedErrorMessage);
		expectedErrorMessage = string.Format("The entered {0} IATA Code is not recognized and will be cleared upon reloading the screen. To save this value, please either create a new {0} with the specified IATA Code or add it to an existing {0}.", Header.AMA_RL_NKPortOfDischargeInfo.HumanReadableName);
		AssertNoMessageError(targetInfo, expectedErrorMessage);

		Header.AMA_RL_NKPortOfDischarge = ZString.Empty;
		Header.PortOfDischargeIATACode = "123";
		AssertHasMessageError(targetInfo, expectedErrorMessage);
	}

	public void TestCheckAMA_OA_Consolidator()
	{
		var expectedMessage = "The selected Consolidator address does not have a NACCS User Code. To add one, press F3 to visit the organization, go to Details > Config > Registration Number / Codes and add a new row where Country/Region of Issue is JP, Type is NUC, and Premises Address is the selected Consolidator address.";
		var invalidAddress = Factory.NewWithValidTestData<OrgAddress>();
		var validAddress = Factory.NewWithValidTestData<OrgAddress>();
		validAddress.CustomsCodes.AddNew(OrgCusCode.JapanCodeTypes.NUC, "12345");

		Header.AMA_OA_Consolidator = invalidAddress.PK;
		AssertHasMessageError(Header.AMA_OA_ConsolidatorInfo, expectedMessage);

		Header.AMA_OA_Consolidator = validAddress.PK;
		AssertNoMessageError(Header.AMA_OA_ConsolidatorInfo, expectedMessage);
	}

	AsycudaManifestHeader Header => header ??= Factory.New<AsycudaManifestHeader>();
	AsycudaManifestHeader header;
}
