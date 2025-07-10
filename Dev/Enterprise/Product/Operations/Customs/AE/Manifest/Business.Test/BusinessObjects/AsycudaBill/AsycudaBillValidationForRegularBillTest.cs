using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AE.Business.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

sealed class AsycudaBillValidationForRegularBillTest : BusinessObjectValidationTestCase
{
	public void TestValidateAll()
	{
		Bill.Validation.ValidateAll();
		AssertHasMessageError("Validation for Negotiable", Bill.NegotiableInfo, "Please indicate whether the bill is Negotiable or Non-Negotiable");
		Bill.Negotiable = NegotiableList.Codes.Yes;
		Bill.Validation.ValidateAll();
		AssertHasMessageError("Validation for Payer", Bill.PayerInfo, "Payer is required for Non-Negotiable bills");
	}

	public void TestCheckABL_SpecialCargoCode()
	{
		var helper = new UAEUniversalReferenceTestHelper(Factory);
		helper.SetupServiceRequirements(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UAEManifest);
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(Bill.ABL_SpecialCargoCodeInfo, new ZString[] { "X", "XX" }, new ZString[] { "ABC" });
	}

	public void TestCheckABL_OA_Shipper() => CombineAssertions(() =>
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Bill.ABL_OA_ShipperInfo);
		AssertCheckOrgContactInfo(Bill.ABL_OA_ShipperInfo, Bill.Validation.ValidateABL_OA_Shipper);
	});

	public void TestCheckABL_CargoType()
	{
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(Bill.ABL_CargoTypeInfo, new ZString[] { "X", "XX" }, Bill.Lookups.AECargoTypeList.GetAllCodesZString());
	}

	public void TestCheckABL_FreightValue()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Bill.ABL_FreightValueInfo);
	}

	public void TestCheckABL_OA_Consignee() => CombineAssertions(() =>
	{
		AssertCheckOrgContactInfo(Bill.ABL_OA_ConsigneeInfo, Bill.Validation.ValidateABL_OA_Consignee);
	});

	public void TestCheckABL_OA_Consignee_CustomsCodes() => CombineAssertions(() =>
	{
		var errorMsg = "Consignee Requires a 'Government Tax File Code (GTX)' or 'Id Number (IDO)' or 'Passport Number (PAS)' or 'Government Corporation Code (GCR)' to be set in the Organization > Details > Config > Registration Numbers / Codes.";
		Bill.Validation.ValidateABL_OA_Consignee();
		AssertNoMessageError("Consignee is null", Bill.ABL_OA_ConsigneeInfo, errorMsg);

		var orgHeader = Factory.New<OrgHeader>();
		Bill.ABL_OA_Consignee = orgHeader.Addresses.AddNew().PK;
		Bill.ABL_RL_NKFinalDestination = "ZAXXX";
		Bill.Validation.ValidateABL_OA_Consignee();
		AssertNoMessageError("Final Destination is not AE", Bill.ABL_OA_ConsigneeInfo, errorMsg);

		Bill.ABL_RL_NKFinalDestination = "AEXXX";
		Bill.Validation.ValidateABL_OA_Consignee();
		AssertHasMessageError("Final Destination is AE, consignee has no customs code", Bill.ABL_OA_ConsigneeInfo, errorMsg);

		var code = orgHeader.CustomsCodes.AddNew();
		code.OK_RN_NKCodeCountry = "AE";
		code.OK_CustomsRegNo = "12345";

		code.OK_CodeType = "PAS";
		Bill.Validation.ValidateABL_OA_Consignee();
		AssertNoMessageError("Consignee has customs code PAS", Bill.ABL_OA_ConsigneeInfo, errorMsg);

		code.OK_CodeType = "IDO";
		Bill.Validation.ValidateABL_OA_Consignee();
		AssertNoMessageError("Consignee has customs code IDO", Bill.ABL_OA_ConsigneeInfo, errorMsg);

		code.OK_RN_NKCodeCountry = "ZA";
		Bill.Validation.ValidateABL_OA_Consignee();
		AssertHasMessageError("Consignee has customs code IDO, but it is from ZA", Bill.ABL_OA_ConsigneeInfo, errorMsg);

		code.OK_RN_NKCodeCountry = "AE";
		code.OK_CodeType = "GTX";
		Bill.Validation.ValidateABL_OA_Consignee();
		AssertNoMessageError("Consignee has customs code GTX", Bill.ABL_OA_ConsigneeInfo, errorMsg);

		code.OK_CodeType = "GCR";
		Bill.Validation.ValidateABL_OA_Consignee();
		AssertNoMessageError("Consignee has customs code GCR", Bill.ABL_OA_ConsigneeInfo, errorMsg);

		code.OK_CodeType = "XXX";
		Bill.Validation.ValidateABL_OA_Consignee();
		AssertHasMessageError("Consignee has none of PAS, IDO, GTX, GCR", Bill.ABL_OA_ConsigneeInfo, errorMsg);
	});

	public void TestCheckABL_OA_NotifyParty() => CombineAssertions(() =>
	{
		AssertCheckOrgContactInfo(Bill.ABL_OA_NotifyPartyInfo, Bill.Validation.ValidateABL_OA_NotifyParty);
	});

	public void TestCheckABL_OA_ContainerAgent() => CombineAssertions(() =>
	{
		AssertCheckOrgContactInfo(Bill.ABL_OA_ContainerAgentInfo, Bill.Validation.ValidateABL_OA_ContainerAgent);
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Bill.ABL_OA_ContainerAgentInfo);
	});

	public void TestCheckABL_OA_DeliveryAgent() => CombineAssertions(() =>
	{
		AssertCheckOrgContactInfo(Bill.ABL_OA_DeliveryAgentInfo, Bill.Validation.ValidateABL_OA_DeliveryAgent);
	});

	public void TestCheckABL_SplitBillNumber()
	{
		const string messageError = "Split Bill Number must be populated for a Split Bill of Lading.";

		ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(Bill.ABL_SplitBillNumberInfo, Bill.ABL_SplitBillInfo, true, messageError);
	}

	public void TestValidateNegotiable() => CombineAssertions(() =>
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Bill.NegotiableInfo,
			"Please indicate whether the bill is Negotiable or Non-Negotiable");
		ValidationTestHelper.AssertInvalidCodeMessageError(Bill.NegotiableInfo, new ZString[] { "A", "B" },
			Bill.Lookups.NegotiableList.GetAllCodesZString());
	});

	public void TestValidatePayer()
	{
		ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyHasValue(Bill.PayerInfo, Bill.NegotiableInfo,
			(ZString)NegotiableList.Codes.Yes, true, "Payer is required for Non-Negotiable bills");
	}

	public void TestCheckABL_OA_Forwarder_HasContact() => CombineAssertions(() =>
	{
		var org = Factory.New<OrgHeader>();
		var orgAddress = org.Addresses.AddNew();
		Bill.ABL_OA_Forwarder = orgAddress.PK;
		AssertNoMessageErrors(Bill.ABL_OA_ForwarderInfo);

		Bill.ABL_BolType = Core.Constants.ShipmentTypes.CoLoadMaster;
		AssertCheckOrgContactInfo(Bill.ABL_OA_ForwarderInfo, Bill.Validation.ValidateABL_OA_Forwarder);
	});

	public void TestCheckABL_OA_Forwarder_HasMPCI() => CombineAssertions(() =>
	{
		var errorMessage = "Freight Forwarder Requires a 'MPCI Party Id (MPC)' Registration Code to be Configured";

		var orgHeader = Factory.New<OrgHeader>();
		var orgAddress = orgHeader.Addresses.AddNew();
		Bill.ABL_OA_Forwarder = orgAddress.PK;
		AssertNoMessageErrors(Bill.ABL_OA_ForwarderInfo);

		Bill.ABL_BolType = Core.Constants.ShipmentTypes.CoLoadMaster;
		Bill.Validation.ValidateABL_OA_Forwarder();
		AssertHasMessageErrorContaining(Bill.ABL_OA_ForwarderInfo, errorMessage);

		var code = orgHeader.CustomsCodes.AddNew();
		code.OK_CodeType = OrgCusCode.UnitedArabEmiratesCodeTypes.MPCINumber;
		code.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedArabEmirates;
		code.OK_CustomsRegNo = "123";
		Bill.Validation.ValidateABL_OA_Forwarder();
		AssertNoMessageError(Bill.ABL_OA_ForwarderInfo, errorMessage);
	});

	public void TestCheckABL_OA_Forwarder_Mandatory()
	{
		var bill1 = Header.Bills.AddNew();
		bill1.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
		bill1.Validation.ValidateABL_OA_Forwarder();
		AssertNoMessageErrors(bill1.ABL_OA_ForwarderInfo);

		var bill2 = Header.Bills.AddNew();
		ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyHasValue(bill2.ABL_OA_ForwarderInfo, bill2.ABL_BolTypeInfo, (ZString)Core.Constants.ShipmentTypes.CoLoadMaster);
	}

	void AssertCheckOrgContactInfo(ZPropertyInfo targetInfo, Action invokeValidation)
	{
		var org = Factory.New<OrgHeader>();
		var orgAddress = org.Addresses.AddNew();
		AEManifestValidationHelper.AssertOrgHasContactInfo(orgAddress, targetInfo, invokeValidation);
	}

	AsycudaBill Bill => bill ??= Header.Bills.AddNew();
	AsycudaBill bill;

	AsycudaManifestHeader Header => header ??= Factory.New<AsycudaManifestHeader>();
	AsycudaManifestHeader header;
}
