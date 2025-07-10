using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public sealed class NctsESOfficeCodeDeparturePhase5ValidationDecider : INctsEuOfficeCodeDeparturePhase5ValidationDecider
	{
		public bool IsRuleB1831Active => false;

		public bool IsRuleB1836Active => false;

		public bool IsRuleB1904Active => false;

		public bool IsRuleC0030Active => false;

		public bool IsRuleC0030_1Active => false;

		public bool IsRuleC0598Active => false;

		public bool IsRuleG0034Active => true;

		public bool IsRuleR0005Active => false;

		public bool IsRuleR0006Active => false;

		public bool IsRuleR0103Active => true;

		public bool IsRuleR0900Active => false;
	}
}
