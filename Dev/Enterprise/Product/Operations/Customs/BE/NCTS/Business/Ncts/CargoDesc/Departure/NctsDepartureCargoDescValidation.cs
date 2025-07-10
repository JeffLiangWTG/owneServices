using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business;

public class NctsDepartureCargoDescValidation : NctsDepartureCargoDescPhase5Validation
{
	public NctsDepartureCargoDescValidation(EU.NCTS.Business.NctsDepartureCargoDesc parent) : base(parent)
	{
	}

	protected override void CheckBY_CommercialReferenceNumber()
	{
		if (!Parent.IsInPhase5TransitionPeriod)
		{
			base.CheckBY_CommercialReferenceNumber();
		}
	}
}
