using System.Threading;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace Enterprise.ZArchitecture.Core
{
	public interface IAcquireTokenInteractiveServer
	{
		Task<AuthenticationResult> AcquireByDeviceCodeAsync(string[] scopes, IPublicClientApplication pca, CancellationToken token = default);
	}
}
