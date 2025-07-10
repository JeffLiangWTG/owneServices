using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.Testing
{
	public static class CusAuthorisationRuleExtensions
	{
		public static CusAuthorisationRule WithValue(this CusAuthorisationRule rule, string value)
		{
			rule.CPR_ValueFrom = value;
			return rule;
		}
	}
}
