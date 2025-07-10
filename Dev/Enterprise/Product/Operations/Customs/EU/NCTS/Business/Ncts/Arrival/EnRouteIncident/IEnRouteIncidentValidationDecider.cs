namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface IEnRouteIncidentValidationDecider
	{
		bool IsRuleC0240_1Active { get; }

		bool IsRuleTR0010Active { get; }

		bool IsRuleTR0012Active { get; }

		bool IsRuleTR0013Active { get; }

		bool IsRuleTR0014Active { get; }

		bool IsRuleTR0015Active { get; }
	}
}
