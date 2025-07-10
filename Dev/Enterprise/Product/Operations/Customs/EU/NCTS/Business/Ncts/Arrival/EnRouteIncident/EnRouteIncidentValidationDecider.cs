namespace Enterprise.Customs.EU.NCTS.Business
{
	public sealed class EnRouteIncidentValidationDecider : IEnRouteIncidentValidationDecider
	{
		public bool IsRuleC0240_1Active => true;

		public bool IsRuleTR0010Active => true;

		public bool IsRuleTR0012Active => true;

		public bool IsRuleTR0013Active => true;

		public bool IsRuleTR0014Active => true;

		public bool IsRuleTR0015Active => true;
	}
}
