using CargoWise.SystemToSystemTrust.DataContracts;

namespace CargoWise.ServiceManager.Next.Shared.Services;

public delegate Func<TRequest, CancellationToken, Task<TResponse>> TokenHandlerDelegate<in TRequest, TResponse>(ITokenHubClient client);

public interface ITokenHubClient : INextHubClient
{
	Task<SignCwTokenResponse> SignCwToken(SignCwTokenRequest request, CancellationToken cancellationToken);
	Task<PrepareNewCertificateResponse> PrepareNewCertificate(PrepareNewCertificateRequest request, CancellationToken cancellationToken);
	Task<ResetAccessTokenResponse> ResetAccessToken(ResetAccessTokenRequest request, CancellationToken cancellationToken);
	Task<SetNewCertificateCredentialsResponse> SetNewCertificateCredentials(SetNewCertificateCredentialsRequest request, CancellationToken cancellationToken);
	Task<SetOperationIdResponse> SetOperationId(SetOperationIdRequest request, CancellationToken cancellationToken);
}
