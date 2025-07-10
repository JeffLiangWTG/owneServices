using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsCargoDescFeePhase4Validation : EU.NCTS.Business.NctsCargoDescFeeValidation
{
	public NctsCargoDescFeePhase4Validation(NctsCargoDescFee parent) : base(parent)
	{
	}

	new NctsCargoDescFee Parent => (NctsCargoDescFee)base.Parent;

	protected override void CheckBFE_ChargeType()
	{
		base.CheckBFE_ChargeType();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BFE_ChargeTypeInfo);
	}

	protected override void CheckBFE_RateOverrideReasonCode()
	{
		base.CheckBFE_RateOverrideReasonCode();
		ListValidation.ErrorIfInvalidCode(Parent.BFE_RateOverrideReasonCodeInfo);
	}

	protected override void CheckBFE_BaseValue()
	{
		base.CheckBFE_BaseValue();
		var baseValueInfo = Parent.BFE_BaseValueInfo;

		MandatoryValidation.CheckNotNegative(baseValueInfo);
		MandatoryValidation.MessageErrorIfNotEntered(baseValueInfo);
	}

	protected override void CheckBFE_MethodOfCalculation()
	{
		base.CheckBFE_MethodOfCalculation();
		var methodOfCalculationInfo = Parent.BFE_MethodOfCalculationInfo;

		MandatoryValidation.CheckEntered(methodOfCalculationInfo);
		ListValidation.MessageErrorIfInvalidCode(methodOfCalculationInfo);
	}

	protected override void CheckBFE_Rate()
	{
		base.CheckBFE_Rate();
		MandatoryValidation.CheckNotNegative(Parent.BFE_RateInfo);
	}

	protected override void CheckBFE_ChargeAmount()
	{
		base.CheckBFE_ChargeAmount();
		MandatoryValidation.CheckNotNegative(Parent.BFE_ChargeAmountInfo);
	}

	protected override void CheckBFE_MethodOfPayment()
	{
		base.CheckBFE_MethodOfPayment();
		var methodOfPaymentInfo = Parent.BFE_MethodOfPaymentInfo;

		ListValidation.ErrorIfInvalidCode(methodOfPaymentInfo);
		MandatoryValidation.MessageErrorIfNotEntered(methodOfPaymentInfo);
	}
}
