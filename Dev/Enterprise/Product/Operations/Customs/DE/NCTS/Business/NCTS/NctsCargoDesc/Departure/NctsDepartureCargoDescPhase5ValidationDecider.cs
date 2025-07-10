using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class NctsDepartureCargoDescPhase5ValidationDecider : INctsDepartureCargoDescPhase5ValidationDecider
	{
		public bool IsRuleB1805_1Active => false;

		public bool IsRuleB1875_1Active => false;

		public bool IsRuleB1922Active => false;

		public bool IsRuleB2101Active => false;

		public bool IsRuleB2400_1Active => false;

		public bool IsRuleC0045Active => true;

		public bool IsRuleC0153_1Active => false;

		public bool IsRuleC0343Active => false;

		public bool IsRuleC0343_1Active => true;

		public bool IsRuleC0343_2Active => false;

		public bool IsRuleC0821_1Active => false;

		public bool IsRuleC0837Active => false;

		public bool IsRuleC0837_1Active => false;

		public bool IsRuleC901Active => true;

		public bool IsRuleC0909Active => false;

		public bool IsRuleC0909_1Active => true;

		public bool IsRuleE1107Active => true;

		public bool IsRuleE1107_1Active => false;

		public bool IsRuleE1109Active => true;

		public bool IsRuleE1301Active => false;

		public bool IsRuleE1407Active => false;

		public bool IsRuleNR0020Active => false;

		public bool IsRuleNR0021Active => false;

		public bool IsRuleNR0024Active => false;

		public bool IsRuleNR0025Active => false;

		public bool IsRuleNR0040Active => true;

		public bool IsRuleNR0041Active => true;

		public bool IsRuleNR0042Active => true;

		public bool IsRuleNR0043Active => true;

		public bool IsRuleNR0044Active => true;

		public bool IsRuleNR0045Active => true;

		public bool IsRuleNR0058Active => false;

		public bool IsRuleNR0059Active => false;

		public bool IsRuleNR0060Active => false;

		public bool IsRuleR0221_1Active => true;

		public bool IsRuleR0221_3Active => false;

		public bool IsRuleR0507Active => true;

		public bool IsRuleR0507_1Active => false;

		public bool IsRuleR0601_1Active => false;

		public bool IsRuleR0909Active => false;

		public bool IsRuleRP11Active => false;

		public bool IsRuleTR0068Active => true;

		public bool IsRuleTR0076Active => true;

		public bool IsRuleTR0096Active => true;

		public bool IsRuleTR0100Active => true;
	}
}
