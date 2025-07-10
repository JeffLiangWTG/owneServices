using System.Threading;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace Enterprise.MailManager.Integration
{
	public interface IMs365OAuth2AuthenticationHelper
	{
		Task<AuthenticationResult> AcquireTokenAsync(CancellationToken token = default);
	}
}
