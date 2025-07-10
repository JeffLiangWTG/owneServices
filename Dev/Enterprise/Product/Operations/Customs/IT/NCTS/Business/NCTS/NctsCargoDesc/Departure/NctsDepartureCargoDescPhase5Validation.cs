using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsDepartureCargoDescPhase5Validation : EU.NCTS.Business.NctsDepartureCargoDescPhase5Validation
{
	public NctsDepartureCargoDescPhase5Validation(NctsDepartureCargoDesc parent) : base(parent)
	{
	}

	protected override void CheckBY_Status()
	{
		base.CheckBY_Status();
		ListValidation.ErrorIfInvalidCode(Parent.BY_StatusInfo);
	}

	protected override void CheckCountryOfDispatchOrDestinationIsAtLeastOneC0009Code()
	{
		// Intentionally kept blank
	}

	protected override void CheckBY_HarmonisedTariffLength()
	{
		// intentionally kept blank 
	}

	protected override void BY_HarmonisedTariffCharacterCheck()
	{
		// intentionally kept blank
	}

	protected override bool IsRuleC0343_1Applicable => base.IsRuleC0343_1Applicable && !Parent.IsInPhase5TransitionPeriod;

	protected override bool IsRuleC0837Applicable => base.IsRuleC0837Applicable && !Parent.IsInPhase5TransitionPeriod;

	protected override bool IsRuleC0909_1Applicable => base.IsRuleC0909_1Applicable && !Parent.IsInPhase5TransitionPeriod;
}
