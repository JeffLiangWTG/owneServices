namespace Enterprise.Customs.EU.Business
{
	public sealed class UCC6ImportCusAuthorizationUsageValidationDecider : ICusAuthorizationUsageValidationDecider
	{
		public bool IsRuleR0010Active => true;
		public bool IsRuleR0675Active => false;
	}
}
