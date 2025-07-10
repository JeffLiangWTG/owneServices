using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public sealed class CusSealPhase5ValidationDecider : ICusSealPhase5ValidationDecider
{
	public bool IsRuleNR0029Active => false;

	public bool IsRuleN0003Active => true;

	public bool IsRuleTR0045Active => true;
}
