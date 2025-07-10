using System.Threading;
using System.Threading.Tasks;
using Enterprise.MailManager.Integration;

namespace Enterprise.ZArchitecture.Core
{
	public interface IAcquireGmailTokenInteractiveServer
	{
		Task<IGmailAuthenticationResult> AcquireByServiceAccountAsync(CancellationToken token = default);
	}
}
