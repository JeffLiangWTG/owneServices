using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsCargoDescFeePhase4ValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckBFE_ChargeType_MandatoryValidation()
	{
		var fee = Factory.New<NctsCargoDescFee>();
		fee.BFE_ChargeType = ZString.Empty;
		AssertHasMessageErrorContaining(fee.BFE_ChargeTypeInfo, MandatoryValidation.YouHaveNotEntered);

		fee.BFE_ChargeType = "XXX";
		AssertNoMessageErrorContaining(fee.BFE_ChargeTypeInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckBFE_ChargeType_ListValidation()
	{
		var fee = Factory.New<NctsCargoDescFee>();
		fee.BFE_ChargeType = "XXX";
		AssertHasMessageErrorContaining(fee.BFE_ChargeTypeInfo, ListValidation.InvalidCodeMessageError);

		fee.BFE_ChargeType = NctsCargoDescFeeChargeTypeList.Codes._149;
		AssertNoMessageErrors(fee.BFE_ChargeTypeInfo);
	}

	public void TestCheckBFE_RateOverrideReasonCode_ListValidation()
	{
		var fee = Factory.New<NctsCargoDescFee>();
		fee.BFE_RateOverrideReasonCode = "XXX";
		AssertHasErrorContaining(fee.BFE_RateOverrideReasonCodeInfo, ListValidation.InvalidCodeError);

		fee.BFE_RateOverrideReasonCode = "EXC";
		AssertHasErrorContaining(fee.BFE_RateOverrideReasonCodeInfo, ListValidation.InvalidCodeError);

		fee.BFE_RateOverrideReasonCode = ZString.Empty;
		AssertNoErrors(fee.BFE_RateOverrideReasonCodeInfo);

		fee.BFE_RateOverrideReasonCode = RateOverrideReasonList.Codes.Additional;
		AssertNoErrors(fee.BFE_RateOverrideReasonCodeInfo);

		fee.BFE_RateOverrideReasonCode = RateOverrideReasonList.Codes.Override;
		AssertNoErrors(fee.BFE_RateOverrideReasonCodeInfo);
	}

	public void TestCheckBFE_BaseValue_NegativeValueValidation()
	{
		var fee = Factory.New<NctsCargoDescFee>();
		fee.BFE_BaseValue = -1m;
		AssertHasErrorContaining(fee.BFE_BaseValueInfo, MandatoryValidation.ValueCannotBeNegative);

		fee.BFE_BaseValue = 0m;
		AssertNoErrors(fee.BFE_BaseValueInfo);

		fee.BFE_BaseValue = 1m;
		AssertNoErrors(fee.BFE_BaseValueInfo);
	}

	public void TestCheckBFE_MethodOfCalculation_MandatoryValidation()
	{
		var fee = Factory.New<NctsCargoDescFee>();
		fee.BFE_MethodOfCalculation = ZString.Empty;
		AssertHasErrorContaining(fee.BFE_MethodOfCalculationInfo, MandatoryValidation.MustBeEntered);

		fee.BFE_MethodOfCalculation = "XXX";
		AssertNoErrors(fee.BFE_MethodOfCalculationInfo);
	}

	public void TestCheckBFE_Rate_NegativeValueValidation()
	{
		var fee = Factory.New<NctsCargoDescFee>();
		fee.BFE_Rate = -1m;
		AssertHasErrorContaining(fee.BFE_RateInfo, MandatoryValidation.ValueCannotBeNegative);

		fee.BFE_Rate = 0m;
		AssertNoErrors(fee.BFE_RateInfo);

		fee.BFE_Rate = 1m;
		AssertNoErrors(fee.BFE_RateInfo);
	}

	public void TestCheckBFE_ChargeAmount_NegativeValueValidation()
	{
		var fee = Factory.New<NctsCargoDescFee>();
		fee.BFE_ChargeAmount = -1m;
		AssertHasErrorContaining(fee.BFE_ChargeAmountInfo, MandatoryValidation.ValueCannotBeNegative);

		fee.BFE_ChargeAmount = 0m;
		AssertNoErrors(fee.BFE_ChargeAmountInfo);

		fee.BFE_ChargeAmount = 1m;
		AssertNoErrors(fee.BFE_ChargeAmountInfo);
	}

	public void TestCheckBFE_MethodOfPayment_ListValidation()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "Method of Payment");
		var cusCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "A", "A", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode.PK, RefCusCodeListAttributeTypes.Codes.Category, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47);
		Factory.Save();

		var fee = Factory.New<NctsCargoDescFee>();
		fee.BFE_MethodOfPayment = "Z";
		AssertHasErrorContaining(fee.BFE_MethodOfPaymentInfo, ListValidation.InvalidCodeError);

		fee.BFE_MethodOfPayment = ZString.Empty;
		AssertNoErrors(fee.BFE_MethodOfPaymentInfo);

		fee.BFE_MethodOfPayment = "A";
		AssertNoErrors(fee.BFE_MethodOfPaymentInfo);
	}

	public void TestCheckBFE_MethodOfCalculation_ListValidation()
	{
		var fee = Factory.New<NctsCargoDescFee>();

		fee.BFE_MethodOfCalculation = "XXX";
		AssertHasMessageErrorContaining(fee.BFE_MethodOfCalculationInfo, ListValidation.InvalidCodeMessageError);

		fee.BFE_MethodOfCalculation = NctsCargoDescFeeMethodOfCalculationList.Codes.Tonne;
		AssertNoMessageErrorContaining(fee.BFE_MethodOfCalculationInfo, ListValidation.InvalidCodeMessageError);
	}

	public void TestCheckBFE_BaseAmount_MandatoryValidation()
	{
		var fee = Factory.New<NctsCargoDescFee>();

		fee.BFE_BaseValue = 0;
		AssertHasMessageErrorContaining(fee.BFE_BaseValueInfo, MandatoryValidation.YouHaveNotEntered);

		fee.BFE_BaseValue = 10;
		AssertNoMessageErrorContaining(fee.BFE_BaseValueInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckBFE_MethodOfPayment_MandatoryValidation()
	{
		var fee = Factory.New<NctsCargoDescFee>();

		fee.BFE_MethodOfPayment = "";
		AssertHasMessageErrorContaining(fee.BFE_MethodOfPaymentInfo, MandatoryValidation.YouHaveNotEntered);

		fee.BFE_MethodOfPayment = "Z";
		AssertNoMessageErrorContaining(fee.BFE_MethodOfPaymentInfo, MandatoryValidation.YouHaveNotEntered);
	}
}
