using CargoWise.ServiceManager.Next.Shared.Services;
using CargoWise.SystemToSystemTrust.DataContracts;

namespace CargoWise.ServiceManager.Next.Launcher;

public interface ICommandSender
{
	Task CloseRunnerAsync(INextProcessRunner nextProcessRunner, CancellationToken cancellationToken);
	Task<TResponse> SendRequestAsync<TRequest, TResponse>(INextProcessRunner nextProcessRunner, TokenHandlerDelegate<TRequest, TResponse> handler, TRequest request, CancellationToken cancellationToken);
	void AddConnection(string connectionId, CancellationToken cancellationToken);
	void RemoveConnection(string connectionId);

	public static TokenHandlerDelegate<SignCwTokenRequest, SignCwTokenResponse> SignCwTokenHandler => client => client.SignCwToken;
	public static TokenHandlerDelegate<PrepareNewCertificateRequest, PrepareNewCertificateResponse> PrepareNewCertificateHandler => client => client.PrepareNewCertificate;
	public static TokenHandlerDelegate<ResetAccessTokenRequest, ResetAccessTokenResponse> ResetAccessTokenHandler => client => client.ResetAccessToken;
	public static TokenHandlerDelegate<SetNewCertificateCredentialsRequest, SetNewCertificateCredentialsResponse> SetNewCertificateCredentialsHandler => client => client.SetNewCertificateCredentials;
	public static TokenHandlerDelegate<SetOperationIdRequest, SetOperationIdResponse> SetOperationIdHandler => client => client.SetOperationId;
}
