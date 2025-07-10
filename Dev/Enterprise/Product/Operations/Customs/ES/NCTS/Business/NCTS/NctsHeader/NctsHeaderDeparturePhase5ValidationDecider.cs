using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public sealed class NctsHeaderDeparturePhase5ValidationDecider : INctsHeaderDeparturePhase5ValidationDecider
	{
		public bool IsRuleB1823Active => false;

		public bool IsRuleC0001Active => true;

		public bool IsRuleC0001_1Active => false;

		public bool IsRuleC0001_4Active => false;

		public bool IsRuleC0001_6Active => false;

		public bool IsRuleC0050Active => false;

		public bool IsRuleG0001_1Active => false;

		public bool IsRuleNR0068Active => false;
		
		public bool IsRuleNR0069Active => false;

		public bool IsRuleNR0071Active => false;

		public bool IsRuleNR0074Active => false;

		public bool IsRuleTR0079Active => true;
	}
}
