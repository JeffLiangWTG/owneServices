using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class CusSealValidationDecider : ICusSealValidationDecider
{
	public bool IsRuleNR0029Active => true;
}
