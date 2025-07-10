using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class CountryOfRoutingDeparturePhase5ValidationDecider : ICountryOfRoutingDeparturePhase5ValidationDecider
	{
		public bool IsRuleB1836Active => false;

		public bool IsRuleC0030Active => false;

		public bool IsRuleC0586Active => false;
	}
}
