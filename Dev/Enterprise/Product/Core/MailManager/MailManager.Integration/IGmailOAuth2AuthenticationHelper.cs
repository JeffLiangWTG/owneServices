using System.Threading;
using System.Threading.Tasks;

namespace Enterprise.MailManager.Integration
{
	public interface IGmailOAuth2AuthenticationHelper
	{
		Task<IGmailAuthenticationResult> AcquireTokenSilentlyAsync(CancellationToken token = default);
	}

	public interface IGmailAuthenticationResult
	{
		string AccessToken { get; }
		string Email { get; }
	}
}
