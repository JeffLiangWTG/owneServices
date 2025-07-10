using WTG.TrustedMessaging;

namespace Enterprise.TrustedMessaging.Business.SecuredHttp
{
	public interface ISecuredHttpTrustedClientConfiguration : ITrustedClientConfiguration
	{
		string ProductCode { get; }
		string SystemId { get; }
	}
}
