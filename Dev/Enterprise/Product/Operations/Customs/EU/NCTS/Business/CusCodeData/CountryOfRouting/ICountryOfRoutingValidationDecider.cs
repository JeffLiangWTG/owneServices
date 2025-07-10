namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface ICountryOfRoutingValidationDecider
	{
	}

	public interface ICountryOfRoutingPhase5ValidationDecider : ICountryOfRoutingValidationDecider
	{
	}

	public interface ICountryOfRoutingDeparturePhase5ValidationDecider : ICountryOfRoutingPhase5ValidationDecider
	{
		bool IsRuleB1836Active { get; }
		bool IsRuleC0030Active { get; }
		bool IsRuleC0586Active { get; }
	}
}
