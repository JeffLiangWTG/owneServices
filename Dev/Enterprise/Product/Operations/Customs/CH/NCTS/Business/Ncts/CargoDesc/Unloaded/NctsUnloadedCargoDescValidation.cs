namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsUnloadedCargoDescValidation : EU.NCTS.Business.NctsUnloadedCargoDescValidation
{
	public NctsUnloadedCargoDescValidation(NctsUnloadedCargoDesc parent) : base(parent)
	{
	}

	protected new NctsUnloadedCargoDesc Parent => (NctsUnloadedCargoDesc)base.Parent;

	protected override void BY_HarmonisedTariffCharacterCheck()
	{
		PassarValidation.CheckNP70205(Parent.BY_HarmonisedTariffInfo, Parent);
	}

	protected override void CheckBY_HarmonisedTariffIsValid()
	{
	}
}
