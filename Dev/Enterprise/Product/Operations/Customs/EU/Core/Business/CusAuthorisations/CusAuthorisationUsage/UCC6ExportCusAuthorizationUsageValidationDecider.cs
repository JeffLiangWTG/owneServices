namespace Enterprise.Customs.EU.Business
{
	public sealed class UCC6ExportCusAuthorizationUsageValidationDecider : ICusAuthorizationUsageValidationDecider
	{
		public bool IsRuleR0010Active => false;
		public bool IsRuleR0675Active => false;
	}
}
