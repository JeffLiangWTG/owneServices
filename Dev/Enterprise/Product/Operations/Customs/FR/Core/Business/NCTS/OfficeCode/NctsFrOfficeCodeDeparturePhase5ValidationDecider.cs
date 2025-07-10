using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.Business.NCTS
{
	public sealed class NctsFrOfficeCodeDeparturePhase5ValidationDecider : INctsEuOfficeCodeDeparturePhase5ValidationDecider
	{
		public bool IsRuleB1831Active => true;

		public bool IsRuleB1836Active => true;

		public bool IsRuleB1904Active => false;

		public bool IsRuleC0030Active => true;

		public bool IsRuleC0030_1Active => false;

		public bool IsRuleC0598Active => true;

		public bool IsRuleG0034Active => true;

		public bool IsRuleR0005Active => true;

		public bool IsRuleR0006Active => true;

		public bool IsRuleR0103Active => true;

		public bool IsRuleR0900Active => true;
	}
}
