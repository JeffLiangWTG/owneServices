namespace Enterprise.Customs.EU.Business
{
	public interface ICusAuthorizationUsageValidationDecider
	{
		bool IsRuleR0010Active { get; }
		bool IsRuleR0675Active { get; }
	}
}
