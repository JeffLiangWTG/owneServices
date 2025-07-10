namespace Enterprise.Customs.EU.NCTS.Business
{
	public sealed class CountryOfRoutingDeparturePhase5ValidationDecider : ICountryOfRoutingDeparturePhase5ValidationDecider
	{
		public bool IsRuleB1836Active => true;

		public bool IsRuleC0030Active => true;

		public bool IsRuleC0586Active => true;
	}
}
