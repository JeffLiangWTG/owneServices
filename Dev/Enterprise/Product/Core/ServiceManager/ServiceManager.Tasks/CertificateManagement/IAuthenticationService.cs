using System.Threading;
using System.Threading.Tasks;

namespace Enterprise.ServiceManager.Tasks.CertificateManagement
{
	interface IAuthenticationService
	{
		string GetAccessToken();
		Task<string> GetAccessTokenAsync(CancellationToken cancellationToken);
	}
}
