using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business;

public sealed class ArrivalPhase5CusGoodsLocationValidationDecider : IArrivalPhase5CusGoodsLocationValidationDecider
{
	public bool IsRuleC0382Active => true;

	public bool IsRuleNR0011Active => true;

	public bool IsRuleNR0012Active => true;

	public bool IsRuleNR0013Active => true;

	public bool IsRuleNR0075Active => false;

	public bool IsRuleTR0061Active => true;

	public bool IsRuleTR0069Active => false;

	public bool IsRuleC0394Active => true;
}
