using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class ArrivalPhase5CusGoodsLocationValidationDecider : IArrivalPhase5CusGoodsLocationValidationDecider
	{
		public bool IsRuleC0382Active => false;

		public bool IsRuleNR0011Active => false;

		public bool IsRuleNR0012Active => false;

		public bool IsRuleNR0013Active => false;

		public bool IsRuleNR0075Active => false;

		public bool IsRuleTR0061Active => false;

		public bool IsRuleTR0069Active => false;

		public bool IsRuleC0394Active => true;
	}
}
