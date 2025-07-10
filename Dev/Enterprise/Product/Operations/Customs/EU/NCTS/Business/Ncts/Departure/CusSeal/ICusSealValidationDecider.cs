namespace Enterprise.Customs.EU.NCTS.Business;

public interface ICusSealValidationDecider
{
	bool IsRuleNR0029Active { get; }
}

public interface ICusSealPhase5ValidationDecider : ICusSealValidationDecider
{
	bool IsRuleN0003Active { get; }

	bool IsRuleTR0045Active { get; }
}
