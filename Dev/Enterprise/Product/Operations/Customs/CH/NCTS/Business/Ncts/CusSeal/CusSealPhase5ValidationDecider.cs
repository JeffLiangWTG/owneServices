using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class CusSealPhase5ValidationDecider : CusSealValidationDecider, ICusSealPhase5ValidationDecider
{
	public bool IsRuleN0003Active => false;

	public bool IsRuleTR0045Active => true;
}
