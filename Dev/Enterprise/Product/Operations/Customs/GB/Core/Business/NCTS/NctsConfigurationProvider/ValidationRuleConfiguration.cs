namespace Enterprise.Customs.GB.Business
{
	public sealed class ValidationRuleConfiguration : EU.NCTS.Business.ValidationRuleConfiguration
	{
		protected override bool IsCountryCodeRequiredToBeSameAsCurrentCompanyCore() => true;
	}
}
