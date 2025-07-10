namespace Enterprise.Customs.EU.Business
{
	public interface ICusAuthorizationUsageProviderWithValidationDecider
	{
		ICusAuthorizationUsageValidationDecider ValidationDecider { get; }
	}
}
