using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public sealed class NctsEuOfficeCodeDeparturePhase5ValidationDecider : INctsEuOfficeCodeDeparturePhase5ValidationDecider
	{
		public bool IsRuleB1831Active => false;

		public bool IsRuleB1836Active => false;

		public bool IsRuleB1904Active => false;

		public bool IsRuleC0030Active => false;

		public bool IsRuleC0030_1Active => false;

		public bool IsRuleC0598Active => true;

		public bool IsRuleG0034Active => false;

		public bool IsRuleR0005Active => true;

		public bool IsRuleR0006Active => false;

		public bool IsRuleR0103Active => true;
	}
}
