using Enterprise.Customs.EU.NCTS.Business;
using ValidationCaptions = Enterprise.Customs.IT.Business.ValidationCaptions;

namespace Enterprise.Customs.IT.NCTS.Business;

sealed class CountryOfRoutingValidation : EU.NCTS.CountryOfRoutingValidation
{
	public CountryOfRoutingValidation(CountryOfRouting parent) : base(parent)
	{
	}

	public override void ValidateAll()
	{
		Parent.ClearRowNotifications();
		base.ValidateAll();

		ValidateConditionB1848();
	}

	void ValidateConditionB1848()
	{
		if (Parent.Parent is NctsHeader header
			&& header.IsInPhase5TransitionPeriod
			&& header.IsPhase5Departure
			&& header.MovementHeader is NctsDepartureMovementHeader movementHeader
			&& movementHeader.BM_TypeOfSecurity == NctsTypeOfSecurityList.Codes.NON)
		{
			Parent.AddRowMessageError(ValidationCaptions.NctsHeader.CountryOfRoutingRuleB1848);
		}
	}

	new CountryOfRouting Parent => (CountryOfRouting)base.Parent;
}
