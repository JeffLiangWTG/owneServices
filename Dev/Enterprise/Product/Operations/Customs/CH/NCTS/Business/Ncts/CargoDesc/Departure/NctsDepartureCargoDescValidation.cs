using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsDepartureCargoDescValidation : NctsDepartureCargoDescPhase5Validation
{
	public NctsDepartureCargoDescValidation(NctsDepartureCargoDesc parent) : base(parent)
	{
	}

	protected new NctsDepartureCargoDesc Parent => (NctsDepartureCargoDesc)base.Parent;

	protected override void BY_HarmonisedTariffCharacterCheck()
	{
		PassarValidation.CheckNP70205(Parent.BY_HarmonisedTariffInfo, Parent);
	}

	protected override void CheckBY_HarmonisedTariffLength()
	{
	}

	protected override void CheckBY_HarmonisedTariffDataGroup()
	{
	}

	protected override void CheckBY_NetWeight()
	{
		base.CheckBY_NetWeight();
		PassarValidation.CheckNS30092(Parent.BY_NetWeightInfo, Parent);
	}

	protected override void CheckCountryOfDispatchRule()
	{
		base.CheckCountryOfDispatchRule();
		if (!Parent.MoveHeader.IsNationalTransitSwitzerland)
		{
			PassarValidation.CheckNS30162(Parent.BY_RN_NKCountryOfDispatchInfo, Parent);
		}
	}

	protected override void CheckCountryOfDestinationRule()
	{
		if (!Parent.MoveHeader.IsNationalTransitSwitzerland)
		{
			base.CheckCountryOfDestinationRule();
		}
	}

	protected override void CheckBY_CommercialReferenceNumber()
	{
		base.CheckBY_CommercialReferenceNumber();
		PassarValidation.CheckNS30046NP70279(Parent.BY_CommercialReferenceNumberInfo, Parent, IsBY_CommercialReferenceNumberMandatory());
	}

	protected override bool IsRuleC0502Applicable => base.IsRuleC0502Applicable && (Parent.Header?.IsInPhase5TransitionPeriod ?? false);

	public EU.NCTS.Business.ValidationRuleMessages ValidationRuleMessages => Parent.Header.Configuration.ValidationRuleConfiguration.Messages;
}
