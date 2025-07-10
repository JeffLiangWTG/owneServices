using CargoWise.ServiceManager.Next.Shared.Services;
using CargoWise.SystemToSystemTrust;
using CargoWise.SystemToSystemTrust.DataContracts;
using Microsoft.AspNetCore.SignalR.Client;

namespace CargoWise.ServiceManager.Next.Runner;

public class TokenConfigWriterServiceClient(IServiceScopeFactory serviceScopeFactory, IHostApplicationLifetime hostApplicationLifetime)
	: TokenClient<ITokenConfigWriterService>(serviceScopeFactory, hostApplicationLifetime)
{
	public override void RegisterTokenClient(HubConnection hubConnection)
	{
		hubConnection.On<PrepareNewCertificateRequest, PrepareNewCertificateResponse>(nameof(ITokenHubClient.PrepareNewCertificate), OnPrepareNewCertificate);
		hubConnection.On<ResetAccessTokenRequest, ResetAccessTokenResponse>(nameof(ITokenHubClient.ResetAccessToken), OnResetAccessToken);
		hubConnection.On<SetNewCertificateCredentialsRequest, SetNewCertificateCredentialsResponse>(nameof(ITokenHubClient.SetNewCertificateCredentials), OnSetNewCertificateCredentials);
		hubConnection.On<SetOperationIdRequest, SetOperationIdResponse>(nameof(ITokenHubClient.SetOperationId), OnSetOperationId);
	}

	async Task<PrepareNewCertificateResponse> OnPrepareNewCertificate(PrepareNewCertificateRequest request)
	{
		return await CallScopedService(tokenService => tokenService.PrepareNewCertificateAsync(request, CancellationToken));
	}

	async Task<ResetAccessTokenResponse> OnResetAccessToken(ResetAccessTokenRequest request)
	{
		return await CallScopedService(tokenService => tokenService.ResetAccessTokenAsync(request, CancellationToken));
	}

	async Task<SetOperationIdResponse> OnSetOperationId(SetOperationIdRequest request)
	{
		return await CallScopedService(tokenService => tokenService.SetOperationIdAsync(request, CancellationToken));
	}

	async Task<SetNewCertificateCredentialsResponse> OnSetNewCertificateCredentials(SetNewCertificateCredentialsRequest request)
	{
		return await CallScopedService(tokenService => tokenService.SetNewCertificateCredentialsAsync(request, CancellationToken));
	}
}
