namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsArrivalCargoDescValidation : EU.NCTS.Business.NctsArrivalCargoDescValidation
{
	public NctsArrivalCargoDescValidation(NctsArrivalCargoDesc parent) : base(parent)
	{
	}

	protected new NctsArrivalCargoDesc Parent => (NctsArrivalCargoDesc)base.Parent;

	protected override void BY_HarmonisedTariffCharacterCheck()
	{
		PassarValidation.CheckNP70205(Parent.BY_HarmonisedTariffInfo, Parent);
	}

	protected override void CheckBY_HarmonisedTariffIsValid()
	{
	}

	protected override void CheckBY_UnloadedState()
	{
		base.CheckBY_UnloadedState();
		PassarValidation.CheckNP70237(Parent.BY_UnloadedStateInfo, Parent);
	}
}
