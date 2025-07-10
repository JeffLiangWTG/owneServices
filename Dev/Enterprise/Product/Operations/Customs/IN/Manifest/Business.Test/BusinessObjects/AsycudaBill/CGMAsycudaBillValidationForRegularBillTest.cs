using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

[TestedType(typeof(CGMAsycudaBillValidationForRegularBill))]
sealed class CGMAsycudaBillValidationForRegularBillTest : BusinessObjectValidationTestCase
{
	public void TestCheckABL_CarrierReference()
	{
		var targetPropertyInfo = (ZPropertyInfoString)Bill.ABL_CarrierReferenceInfo;
		var expectedMessage = ValidationMessages.Shared.GetFieldIsNotWithinRangeMessage(targetPropertyInfo.HumanReadableName, 1, 9999);
		CombineAssertions(() =>
		{
			Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			Bill.ABL_CarrierReference = ZString.Empty;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetPropertyInfo);
			Bill.ABL_CarrierReference = "3241";
			AssertNoMessageErrors(targetPropertyInfo);

			Header.AMA_TransportMode = TransportTypeList.Codes.Air;
			Bill.ABL_CarrierReference = ZString.Empty;
			AssertNoMessageErrors(targetPropertyInfo);

			Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			Bill.ABL_CarrierReference = "-";
			AssertHasMessageErrorContaining("Message error when AMA_CarrierReference is _", targetPropertyInfo, expectedMessage);

			Header.AMA_TransportMode = TransportTypeList.Codes.Air;
			Bill.ABL_CarrierReference = "-";
			AssertNoMessageErrors(targetPropertyInfo);
		});
	}

	public void TestCheckMandatoryABL_RL_NKOrigin()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Bill.ABL_RL_NKOriginInfo);
		Header.AMA_TransportMode = TransportTypeList.Codes.Air;
		Bill.Validation.ValidateABL_RL_NKOrigin();
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Bill.ABL_RL_NKOriginInfo);

		Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
		ValidationTestHelper.AssertFieldIsNotMandatory(Bill.ABL_RL_NKOriginInfo);
	}

	public void TestCheckABL_SpecialCargoCode()
	{
		Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(Bill.ABL_SpecialCargoCodeInfo, new ZString[] { "X", "XX" }, Bill.Lookups.SpecialCargoCodeList.GetAllCodesZString());
		Header.AMA_TransportMode = TransportTypeList.Codes.Air;
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(Bill.ABL_SpecialCargoCodeInfo, new ZString[] { "X" }, Bill.Lookups.SpecialCargoCodeList.GetAllCodesZString());
	}

	public void TestCheckABL_CargoStatus()
	{
		Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(Bill.ABL_CargoStatusInfo, invalidCode: "XX", "LC");

		Header.AMA_TransportMode = TransportTypeList.Codes.Air;
		ValidationTestHelper.AssertFieldIsNotMandatory(Bill.ABL_CargoStatusInfo);
	}

	public void TestCheckABL_OA_Buyer_WithManifestTypeAndTransportMode()
	{
		CombineAssertions(() =>
		{
			Header.AMA_TransportMode = TransportTypeList.Codes.Air;
			ValidationTestHelper.AssertFieldIsNotMandatory(Bill.ABL_OA_BuyerInfo);

			Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Bill.ABL_OA_BuyerInfo);
		});
	}

	public void TestCheckABL_OA_Buyer_WithBuyerName()
	{
		CombineAssertions(() =>
		{
			Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			Bill.ABL_BuyerName = ZString.Empty;
			Bill.Validation.ValidateABL_OA_Buyer();
			AssertHasMessageErrorContaining("When BuyerName empty", Bill.ABL_OA_BuyerInfo, MandatoryValidation.YouHaveNotEntered);

			Bill.ABL_BuyerName = "XYZ";
			Bill.Validation.ValidateABL_OA_Buyer();
			AssertNoMessageErrorContaining("When BuyerName entered", Bill.ABL_OA_BuyerInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckABL_GoodsDescription()
	{
		var header = Header;
		var targetPropertyInfo = Bill.ABL_GoodsDescriptionInfo;
		var expectedWarning = $"{targetPropertyInfo.HumanReadableName} is more than 30 Char. Only first 30 Characters will be used for CGM filing.";

		AssertCheckABL_GoodsDescription(TransportTypeList.Codes.Air);
		AssertCheckABL_GoodsDescription(TransportTypeList.Codes.Sea);

		void AssertCheckABL_GoodsDescription(string transportMode)
		{
			header.AMA_TransportMode = transportMode;
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetPropertyInfo);
				Bill.ABL_GoodsDescription = "Goods Description";
				AssertNoMessageErrorContaining(targetPropertyInfo, MandatoryValidation.YouHaveNotEntered);

				Bill.ABL_GoodsDescription = new string('a', 31);
				AssertHasWarning($"[{transportMode}] Warning when description length is greater than 30", targetPropertyInfo, expectedWarning);
				Bill.ABL_GoodsDescription = new string('a', 30);
				AssertNoWarning($"[{transportMode}] No Warning when description length is less than 30", targetPropertyInfo, expectedWarning);
			});
		}
	}

	public void TestCheckABL_OH_BondHolder()
	{
		var holder = Factory.NewWithValidTestData<OrgHeader>();
		holder.OH_Code = "ABCD";
		Factory.Save();

		CombineAssertions(() =>
		{
			Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			Bill.ABL_ContainerMode = NatureOfCargoList.Codes.LB;
			Bill.ABL_OH_BondHolder = ZGuid.Empty;
			AssertNoMessageErrorContaining("When IsContainerModeCorCP() is false", Bill.ABL_OH_BondHolderInfo, MandatoryValidation.YouHaveNotEntered);

			Bill.ABL_ContainerMode = NatureOfCargoList.Codes.C;
			Bill.Validation.ValidateABL_OH_BondHolder();
			AssertHasMessageErrorContaining("When IsContainerModeCorCP() is true", Bill.ABL_OH_BondHolderInfo, MandatoryValidation.YouHaveNotEntered);

			Bill.ABL_OH_BondHolder = holder.PK;
			Bill.Validation.ValidateABL_OH_BondHolder();
			AssertNoMessageErrorContaining("When ABL_OH_BondHolder entered", Bill.ABL_OH_BondHolderInfo, MandatoryValidation.YouHaveNotEntered);

			const string mloErrorMessage = "CCC/MLO - Carrier Customs Code/Main Line Operator Code is missing for selected Bond Holder.";
			AssertHasMessageError("When no MLO", Bill.ABL_OH_BondHolderInfo, mloErrorMessage);

			var code = holder.CustomsCodes.AddNew();
			code.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			code.OK_RN_NKCodeCountry = CountryCodes.India;
			code.OK_CustomsRegNo = "1234";
			Factory.Save();
			Bill.Validation.ValidateABL_OH_BondHolder();
			AssertNoMessageError("When has MLO", Bill.ABL_OH_BondHolderInfo, mloErrorMessage);
		});
	}

	public void TestValidateBondNumber()
	{
		CombineAssertions(() =>
		{
			Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			Bill.ABL_ContainerMode = NatureOfCargoList.Codes.LB;
			ValidationTestHelper.AssertFieldIsNotMandatory(Bill.BondNumberInfo);

			ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyHasValue(Bill.BondNumberInfo, Bill.ABL_ContainerModeInfo, new IZType[]
			{
				(ZString)NatureOfCargoList.Codes.C,
				(ZString)NatureOfCargoList.Codes.CP
			}, combineAssertions: false);

			Header.AMA_TransportMode = TransportTypeList.Codes.Air;
			Bill.ABL_ContainerMode = NatureOfCargoList.Codes.C;
			ValidationTestHelper.AssertFieldIsNotMandatory(Bill.BondNumberInfo);
		});
	}

	public void TestCheckABL_ContainerMode()
	{
		CombineAssertions(() =>
		{
			Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Bill.ABL_ContainerModeInfo);

			Header.AMA_TransportMode = TransportTypeList.Codes.Air;
			ValidationTestHelper.AssertFieldIsNotMandatory(Bill.ABL_ContainerModeInfo);
		});
	}

	public void TestCheckABL_ContainerMode_WhenContainersEmpty()
	{
		var expectedMessageError = "You have not entered a Container Number.";

		CombineAssertions(() =>
		{
			Header.AMA_TransportMode = TransportTypeList.Codes.Air;
			Bill.ABL_ContainerMode = NatureOfCargoList.Codes.C;
			AssertNoMessageErrorContaining("Transport mode 'Air' - no message error", Bill.ABL_ContainerModeInfo, expectedMessageError);

			Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			Bill.Validation.ValidateABL_ContainerMode();
			AssertHasMessageErrorContaining("Container mode 'C', Container count 0", Bill.ABL_ContainerModeInfo, expectedMessageError);
			Bill.ABL_ContainerMode = "ABC";
			AssertNoMessageErrorContaining("Container mode 'ABC', Container count 0", Bill.ABL_ContainerModeInfo, expectedMessageError);
			Bill.ABL_ContainerMode = NatureOfCargoList.Codes.CP;
			AssertHasMessageErrorContaining("Container mode 'CP', Container count 0", Bill.ABL_ContainerModeInfo, expectedMessageError);

			Header.Containers.AddNew();
			Bill.Validation.ValidateABL_ContainerMode();
			AssertNoMessageErrorContaining("Container count not 0", Bill.ABL_ContainerModeInfo, expectedMessageError);
		});
	}

	public void TestCheckAtLeastOnePackWhereRelevant()
	{
		var expectedMessageError = "You have not entered the associated Container Information under Packs.";

		CombineAssertions(() =>
		{
			Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			var container = Header.Containers.AddNew();
			Bill.ABL_ContainerMode = "ABC";
			Bill.Validation.ValidateABL_BillNumber();
			AssertHasWarningContaining("Container mode 'ABC' Warning", Bill.ABL_BillNumberInfo, "pack");
			AssertNoMessageErrorContaining("Container mode 'ABC' Message error", Bill.ABL_BillNumberInfo, expectedMessageError);

			Bill.ABL_ContainerMode = NatureOfCargoList.Codes.C;
			Bill.Validation.ValidateABL_BillNumber();
			AssertNoWarningContaining("Container mode 'C'", Bill.ABL_BillNumberInfo, "pack");
			AssertHasMessageErrorContaining("Container mode 'C' Message error", Bill.ABL_BillNumberInfo, expectedMessageError);

			var pack = Bill.Packs.AddNew();
			Bill.Validation.ValidateABL_BillNumber();
			AssertNoWarningContaining("Container mode 'C' with no container pack", Bill.ABL_BillNumberInfo, "pack");
			AssertHasMessageErrorContaining("Container mode 'C' Message error no container pack", Bill.ABL_BillNumberInfo, expectedMessageError);

			pack.ContainerPK = container.PK;
			Bill.Validation.ValidateABL_BillNumber();
			AssertNoWarningContaining("Container mode 'C' with has container pack", Bill.ABL_BillNumberInfo, "pack");
			AssertNoMessageErrorContaining("Container mode 'C' Message error has container pack", Bill.ABL_BillNumberInfo, expectedMessageError);

			Bill.ABL_ContainerMode = NatureOfCargoList.Codes.CP;
			Bill.Packs.Clear();
			Bill.Validation.ValidateABL_BillNumber();
			AssertNoWarningContaining("Container mode 'CP'", Bill.ABL_BillNumberInfo, "pack");

			pack = Bill.Packs.AddNew();
			Bill.Validation.ValidateABL_BillNumber();
			AssertNoWarningContaining("Container mode 'CP' with no container pack", Bill.ABL_BillNumberInfo, "pack");
			AssertHasMessageErrorContaining("Container mode 'CP' Message error no container pack", Bill.ABL_BillNumberInfo, expectedMessageError);

			pack.ContainerPK = container.PK;
			Bill.Validation.ValidateABL_BillNumber();
			AssertNoWarningContaining("Container mode 'CP' with has container pack", Bill.ABL_BillNumberInfo, "pack");
			AssertNoMessageErrorContaining("Container mode 'CP' Message error has container pack", Bill.ABL_BillNumberInfo, expectedMessageError);
		});
	}

	public void TestCheckABL_BillNumber()
	{
		var expectedMessageError = "You have entered invalid HBL/Bill Number.";
		var invalidInputs = new[] { "a b", "a.b", "a#b", "a@b" };

		foreach (var invalidInput in invalidInputs)
		{
			CombineAssertions(() =>
			{
				Header.AMA_TransportMode = TransportTypeList.Codes.Air;
				Bill.ABL_BillNumber = invalidInput;
				Bill.Validation.ValidateABL_BillNumber();
				AssertNoMessageError("No Message Error for AirCGM", Bill.ABL_BillNumberInfo, expectedMessageError);

				Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
				Bill.Validation.ValidateABL_BillNumber();
				AssertHasMessageError("Message Error for SeaCGM, invalid input", Bill.ABL_BillNumberInfo, expectedMessageError);
			});
		}
		Bill.ABL_BillNumber = "121ADSF3";
		AssertNoMessageError("No Message Error for SeaCGM, valid input", Bill.ABL_BillNumberInfo, expectedMessageError);
	}

	public void TestCheckABL_BillIssueDate()
	{
		Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Bill.ABL_BillIssueDateInfo);

		Header.AMA_TransportMode = TransportTypeList.Codes.Air;
		ValidationTestHelper.AssertFieldIsNotMandatory(Bill.ABL_BillIssueDateInfo);
	}

	public void TestCheckABL_MarksAndNumbers()
	{
		CombineAssertions(() =>
		{
			Header.AMA_TransportMode = TransportTypeList.Codes.Air;
			ValidationTestHelper.AssertFieldIsNotMandatory(Bill.ABL_MarksAndNumbersInfo);

			Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Bill.ABL_MarksAndNumbersInfo);
		});
	}

	public void TestCheckABL_BillStatus()
	{
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(Bill.ABL_BillStatusInfo, new ZString[] { "X", "XX" }, Bill.Lookups.CustomsStatusList.GetAllCodesZString());
	}

	[TestDate(2022, 3, 16)]
	public void TestCheckABL_CustomsFinalDestinationPort()
	{
		var today = ZDateTime.Now;
		var yesterday = today.AddDays(-1);
		var tomorrow = today.AddDays(1);
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCusCodeType(RefCusCodeListTypes.Codes.CustomsOffice, "India Customs EDI Location");
		helper.CreateCusCodeList(Core.Constants.CountryCodes.India, RefCusCodeListTypes.Codes.CustomsOffice, "ABC123", yesterday, tomorrow);
		Factory.Save();

		Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
		Bill.ABL_LocationInformation = DestinationCodeList.Codes.CUS;

		var errorMessage = MandatoryValidation.YouHaveNotEnteredMessage("Final Destination [Customs House]");
		CombineAssertions("CUS", () =>
		{
			Bill.ABL_CustomsFinalDestinationPort = ZString.Empty;
			AssertHasMessageError(Bill.ABL_CustomsFinalDestinationPortInfo, errorMessage);
			Bill.ABL_CustomsFinalDestinationPort = "ABC345";
			AssertNoMessageError(Bill.ABL_CustomsFinalDestinationPortInfo, errorMessage);

			errorMessage = "The Customs House Code you have selected is not in the list.";
			AssertHasMessageError(Bill.ABL_CustomsFinalDestinationPortInfo, errorMessage);
			Bill.ABL_CustomsFinalDestinationPort = "ABC123";
			AssertNoMessageError(Bill.ABL_CustomsFinalDestinationPortInfo, errorMessage);
		});

		Bill.ABL_LocationInformation = DestinationCodeList.Codes.CFS;
		errorMessage = "You have entered invalid CFS Code.";
		CombineAssertions("CFS", () =>
		{
			errorMessage = "You have entered invalid CFS Code.";
			Bill.ABL_CustomsFinalDestinationPort = ZString.Empty;
			AssertHasMessageError("When empty", Bill.ABL_CustomsFinalDestinationPortInfo, errorMessage);
			Bill.ABL_CustomsFinalDestinationPort = "123456789";
			AssertHasMessageError("When length less than 10", Bill.ABL_CustomsFinalDestinationPortInfo, errorMessage);
			Bill.ABL_CustomsFinalDestinationPort = "1234567890";
			AssertNoMessageError(Bill.ABL_CustomsFinalDestinationPortInfo, errorMessage);
		});

		Header.AMA_TransportMode = TransportTypeList.Codes.Air;
		Bill.ABL_CustomsFinalDestinationPort = "123456789";
		AssertNoMessageError("Only check for Sea", Bill.ABL_CustomsFinalDestinationPortInfo, errorMessage);
	}

	public void TestCheckABL_InlandTransportMode()
	{
		Header.AMA_TransportMode = TransportTypeList.Codes.Air;
		ValidationTestHelper.AssertFieldIsNotMandatory(Bill.ABL_InlandTransportModeInfo);

		Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
		Bill.ABL_CargoStatus = CargoMovementList.Codes.LocalCargo;
		ValidationTestHelper.AssertFieldIsNotMandatory(Bill.ABL_InlandTransportModeInfo);

		Bill.ABL_CargoStatus = CargoMovementList.Codes.TranshipmentCargo;
		Bill.ABL_InlandTransportMode = ZString.Empty;
		var errorMessage = MandatoryValidation.YouHaveNotEnteredMessage("Transshipment Mode Of Transport (M.O.T)");
		AssertHasMessageError(Bill.ABL_InlandTransportModeInfo, errorMessage);
		Bill.ABL_InlandTransportMode = ModeOfTransportList.Codes.Road;
		AssertNoMessageError(Bill.ABL_InlandTransportModeInfo, errorMessage);
	}

	public void TestCheckABL_OH_LocalTransportCarrier()
	{
		Header.AMA_TransportMode = TransportTypeList.Codes.Air;
		ValidationTestHelper.AssertFieldIsNotMandatory(Bill.ABL_OH_LocalTransportCarrierInfo);

		Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
		Bill.ABL_CargoStatus = CargoMovementList.Codes.LocalCargo;
		ValidationTestHelper.AssertFieldIsNotMandatory(Bill.ABL_OH_LocalTransportCarrierInfo);

		var carrier = Factory.NewWithValidTestData<OrgHeader>();
		carrier.OH_Code = "ABCD";
		carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "1234", CountryCodes.India);

		Bill.ABL_CargoStatus = CargoMovementList.Codes.TranshipmentCargo;
		Bill.ABL_OH_LocalTransportCarrier = ZGuid.Empty;
		var errorMessage = MandatoryValidation.YouHaveNotEnteredMessage("Transshipment Carrier");
		var errorMessageCode = "CCC - Carrier Customs Code is missing for selected Carrier.";
		AssertHasMessageError(Bill.ABL_OH_LocalTransportCarrierInfo, errorMessage);
		Bill.ABL_OH_LocalTransportCarrier = carrier.PK;
		AssertNoMessageError(Bill.ABL_OH_LocalTransportCarrierInfo, errorMessage);
		AssertNoMessageError(Bill.ABL_OH_LocalTransportCarrierInfo, errorMessageCode);

		carrier.CustomsCodes.RemoveAll();
		Bill.Validation.ValidateABL_OH_LocalTransportCarrier();
		AssertHasMessageError(Bill.ABL_OH_LocalTransportCarrierInfo, errorMessageCode);
	}

	public void TestCheckABL_Manifest()
	{
		const string errorMessage = "cannot be negative.";

		CombineAssertions(() =>
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Bill.ABL_ManifestQtyInfo);

			Bill.ABL_ManifestQty = -10;
			AssertHasErrorContaining("Negative, AIR CGM", Bill.ABL_ManifestQtyInfo, errorMessage);
			Bill.ABL_ManifestQty = 10;
			AssertNoErrorContaining("Positive, AIR CGM", Bill.ABL_ManifestQtyInfo, errorMessage);

			Bill.ABL_ManifestUQ = Core.Constants.PkgUnit.Bag;
			AssertNoNotifications("ABL_ManifestUQNotMappedMessageError check", Bill.ABL_ManifestUQInfo);
		});
	}

	CGMAsycudaBill Bill => bill ??= Header.Bills.AddNew();
	CGMAsycudaBill bill;

	CGMAsycudaManifestHeader Header => header ??= Factory.New<CGMAsycudaManifestHeader>();
	CGMAsycudaManifestHeader header;
}
