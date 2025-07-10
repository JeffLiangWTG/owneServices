using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

[TestedType(typeof(CGMAsycudaBillValidationForMasterChild))]
sealed class CGMAsycudaBillValidationForMasterChildTest : BusinessObjectValidationTestCase
{
	public void TestCheckABL_SpecialCargoCode()
	{
		Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(MasterBill.ABL_SpecialCargoCodeInfo, new ZString[] { "X", "XX" }, MasterBill.Lookups.SpecialCargoCodeList.GetAllCodesZString());
		Header.AMA_TransportMode = TransportTypeList.Codes.Air;
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(MasterBill.ABL_SpecialCargoCodeInfo, new ZString[] { "X" }, MasterBill.Lookups.SpecialCargoCodeList.GetAllCodesZString());
	}

	public void TestCheckABL_BillIssueDate()
	{
		Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
		MasterBill.Validation.ValidateABL_BillIssueDate();
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(MasterBill.ABL_BillIssueDateInfo);

		Header.AMA_TransportMode = TransportTypeList.Codes.Air;
		ValidationTestHelper.AssertFieldIsNotMandatory(MasterBill.ABL_BillIssueDateInfo);
	}

	public void TestCheckMandatoryABL_E_DEP()
	{
		var message = "An Estimated Departure Time is required";
		Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
		ValidationTestHelper.AssertFieldIsNotMandatory(MasterBill.ABL_E_DEPInfo, message);

		Header.AMA_TransportMode = TransportTypeList.Codes.Air;
		ValidationTestHelper.AssertFieldIsNotMandatory(MasterBill.ABL_E_DEPInfo, message);
	}

	public void TestCheckMandatoryABL_E_ARV()
	{
		Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
		ValidationTestHelper.AssertFieldIsNotMandatory(MasterBill.ABL_E_ARVInfo);

		Header.AMA_TransportMode = TransportTypeList.Codes.Air;
		ValidationTestHelper.AssertFieldIsNotMandatory(MasterBill.ABL_E_ARVInfo);
	}

	public void TestCheckABL_RL_NKPortOfLoading()
	{
		CombineAssertions(() =>
		{
			Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			ValidationTestHelper.AssertFieldIsNotMandatory(MasterBill.ABL_RL_NKPortOfLoadingInfo);
			MasterBill.ABL_RL_NKPortOfLoading = ZString.Empty;
			AssertNoErrors("For CGM - Sea", MasterBill.ABL_RL_NKPortOfLoadingInfo);

			Header.AMA_TransportMode = TransportTypeList.Codes.Air;
			ValidationTestHelper.AssertFieldIsNotMandatory(MasterBill.ABL_RL_NKPortOfLoadingInfo);
			MasterBill.ABL_RL_NKPortOfLoading = ZString.Empty;
			AssertNoErrors("For CGM - Air", MasterBill.ABL_RL_NKPortOfLoadingInfo);
		});
	}

	public void TestCheckABL_RL_NKPortOfDischarge()
	{
		CombineAssertions(() =>
		{
			Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			ValidationTestHelper.AssertFieldIsNotMandatory(MasterBill.ABL_RL_NKPortOfDischargeInfo);
			MasterBill.ABL_RL_NKPortOfDischarge = ZString.Empty;
			AssertNoErrors("For CGM - Sea", MasterBill.ABL_RL_NKPortOfDischargeInfo);

			Header.AMA_TransportMode = TransportTypeList.Codes.Air;
			ValidationTestHelper.AssertFieldIsNotMandatory(MasterBill.ABL_RL_NKPortOfDischargeInfo);
			MasterBill.ABL_RL_NKPortOfDischarge = ZString.Empty;
			AssertNoErrors("For CGM - Air", MasterBill.ABL_RL_NKPortOfDischargeInfo);
		});
	}

	public void TestCheckABL_RL_NKOrigin()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(MasterBill.ABL_RL_NKOriginInfo);
	}

	public void TestCheckABL_RL_NKFinalDestination()
	{
		Header.AMA_TransportMode = TransportTypeList.Codes.Air;
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(MasterBill.ABL_RL_NKFinalDestinationInfo);

		Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
		ValidationTestHelper.AssertFieldIsNotMandatory(MasterBill.ABL_RL_NKFinalDestinationInfo);
	}

	public void TestCheckABL_CustomsDischargePort()
	{
		Header.AMA_TransportMode = TransportTypeList.Codes.Sea;

		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(MasterBill.ABL_CustomsDischargePortInfo);

		Header.AMA_TransportMode = TransportTypeList.Codes.Air;
		ValidationTestHelper.AssertFieldIsNotMandatory(MasterBill.ABL_CustomsDischargePortInfo);
	}

	public void TestCheckABL_CarrierReference()
	{
		var targetPropertyInfo = (ZPropertyInfoString)MasterBill.ABL_CarrierReferenceInfo;
		var expectedMessage = ValidationMessages.Shared.GetFieldIsNotWithinRangeMessage(targetPropertyInfo.HumanReadableName, 1, 9999);
		CombineAssertions(() =>
		{
			Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			Header.AMA_CarrierReference = ZString.Empty;
			AssertHasMessageErrorContaining("Message error when AMA_CarrierReference is empty", targetPropertyInfo, MandatoryValidation.YouHaveNotEntered);
			Header.AMA_CarrierReference = "3241";
			AssertNoMessageErrors(targetPropertyInfo);

			Header.AMA_TransportMode = TransportTypeList.Codes.Air;
			Header.AMA_CarrierReference = ZString.Empty;
			AssertNoMessageErrors(targetPropertyInfo);

			Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			Header.AMA_CarrierReference = "-";
			AssertHasMessageErrorContaining("Message error when AMA_CarrierReference is _", targetPropertyInfo, expectedMessage);

			Header.AMA_TransportMode = TransportTypeList.Codes.Air;
			Header.AMA_CarrierReference = "-";
			AssertNoMessageErrors(targetPropertyInfo);
		});
	}

	public void TestCheckABL_GoodsDescription()
	{
		var targetPropertyInfo = MasterBill.ABL_GoodsDescriptionInfo;
		var expectedWarning = $"{targetPropertyInfo.HumanReadableName} is more than 30 Char. Only first 30 Characters will be used for CGM filing.";

		CombineAssertions(() =>
		{
			Header.AMA_TransportMode = TransportTypeList.Codes.Air;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetPropertyInfo);
			Header.AMA_GoodsDescription = "Goods Description";
			AssertNoMessageErrorContaining(targetPropertyInfo, MandatoryValidation.YouHaveNotEntered);

			Header.AMA_GoodsDescription = new string('a', 31);
			AssertHasWarning("Warning when description length is greater than 30", targetPropertyInfo, expectedWarning);
			Header.AMA_GoodsDescription = new string('a', 30);
			AssertNoWarning("No Warning when description length is less than 30", targetPropertyInfo, expectedWarning);

			Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			ValidationTestHelper.AssertFieldIsNotMandatory(targetPropertyInfo);
			Header.AMA_GoodsDescription = new string('a', 31);
			AssertNoWarning(targetPropertyInfo, expectedWarning);
		});
	}

	public void TestCheckABL_GrossWeight()
	{
		const string errorMessage = "cannot be negative.";

		CombineAssertions(() =>
		{
			Header.AMA_TransportMode = TransportTypeList.Codes.Air;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(MasterBill.ABL_GrossWeightInfo);
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(MasterBill.ABL_GrossWeightUQInfo);

			Header.GrossWeight = 10m;
			AssertNoErrorContaining("Positive, AIR CGM", Header.GrossWeightInfo, errorMessage);
			Header.GrossWeight = -10m;
			AssertHasErrorContaining("Negative, AIR CGM", Header.GrossWeightInfo, errorMessage);

			Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			ValidationTestHelper.AssertFieldIsNotMandatory(MasterBill.ABL_GrossWeightInfo);
			ValidationTestHelper.AssertFieldIsNotMandatory(MasterBill.ABL_GrossWeightUQInfo);
			AssertNoErrorContaining("Negative, SEA CGM", Header.GrossWeightInfo, errorMessage);

			const string maxValueMessage = "the maximum value allowed for Gross Weight is 999,999.999.";
			const decimal maxValue = 999999.999m;

			Header.GrossWeight = maxValue + 0.001m;
			AssertHasErrorContaining("Above Maximum", Header.GrossWeightInfo, maxValueMessage);
			Header.GrossWeight = maxValue;
			AssertNoErrorContaining("Below Maximum", Header.GrossWeightInfo, maxValueMessage);
		});
	}

	public void TestCheckABL_ManifestQty()
	{
		const string errorMessage = "cannot be negative.";

		CombineAssertions(() =>
		{
			Header.AMA_TransportMode = TransportTypeList.Codes.Air;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(MasterBill.ABL_ManifestQtyInfo);

			Header.ManifestQty = -10;
			AssertHasErrorContaining("Negative, AIR CGM", MasterBill.ABL_ManifestQtyInfo, errorMessage);
			Header.ManifestQty = 10;
			AssertNoErrorContaining("Positive, AIR CGM", MasterBill.ABL_ManifestQtyInfo, errorMessage);

			Header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			ValidationTestHelper.AssertFieldIsNotMandatory(MasterBill.ABL_ManifestQtyInfo);
			AssertNoErrorContaining("Negative, SEA CGM", MasterBill.ABL_ManifestQtyInfo, errorMessage);
		});
	}

	CGMAsycudaBill MasterBill => Header.MasterBill;

	CGMAsycudaManifestHeader Header => header ??= Factory.New<CGMAsycudaManifestHeader>();
	CGMAsycudaManifestHeader header;
}
