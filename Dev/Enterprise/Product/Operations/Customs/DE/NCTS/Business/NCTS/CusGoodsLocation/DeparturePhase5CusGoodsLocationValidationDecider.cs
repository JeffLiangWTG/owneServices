using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class DeparturePhase5CusGoodsLocationValidationDecider : IDeparturePhase5CusGoodsLocationValidationDecider
	{
		public bool IsRuleC0382Active => false;

		public bool IsRuleC0394Active => true;

		public bool IsRuleNR0013Active => false;

		public bool IsRuleNR0023Active => false;

		public bool IsRuleNR0050Active => true;

		public bool IsRuleNR0063Active => false;

		public bool IsRuleTR0061Active => false;
	}
}
