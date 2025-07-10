using System.Threading;
using System.Threading.Tasks;
using Enterprise.Integration;

namespace Enterprise.ServiceManager.Tasks.CertificateManagement;

internal interface IApplicationRedirectUrlProcessor
{
	Task ProcessAsync(ISystemToSystemTrustApiHelper systemTrustApiHelper, IAuthenticationService authenticationService, ILogger logger, CancellationToken cancellationToken);
}
