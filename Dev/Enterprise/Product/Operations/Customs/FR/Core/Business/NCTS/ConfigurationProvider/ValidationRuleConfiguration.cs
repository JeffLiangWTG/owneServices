namespace Enterprise.Customs.FR.Business.NCTS
{
	public sealed class ValidationRuleConfiguration : EU.NCTS.Business.ValidationRuleConfiguration
	{
		protected override bool IsCountryCodeRequiredToBeSameAsCurrentCompanyCore() => true;

		protected override bool IsRuleC0065ActiveCore() => false;

		protected override bool IsRuleC0839ActiveCore() => false;

		protected override bool IsRuleE1406ActiveCore() => false;
	}
}
