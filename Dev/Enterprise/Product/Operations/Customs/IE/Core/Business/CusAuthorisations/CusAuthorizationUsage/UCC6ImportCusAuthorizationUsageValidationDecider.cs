using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Business
{
	public class UCC6ImportCusAuthorizationUsageValidationDecider : ICusAuthorizationUsageValidationDecider
	{
		public bool IsRuleR0010Active => true;
		public bool IsRuleBR2039Active => true;
		public bool IsRuleR0675Active => false;
	}
}
