using CargoWise.ServiceManager.Next.Shared.Services;
using CargoWise.SystemToSystemTrust;
using CargoWise.SystemToSystemTrust.DataContracts;
using Microsoft.AspNetCore.SignalR.Client;

namespace CargoWise.ServiceManager.Next.Runner;

public class TokenGeneratorServiceClient(IServiceScopeFactory serviceScopeFactory, IHostApplicationLifetime hostApplicationLifetime)
	: TokenClient<ITokenGeneratorService>(serviceScopeFactory, hostApplicationLifetime)
{
	public override void RegisterTokenClient(HubConnection hubConnection)
	{
		hubConnection.On<SignCwTokenRequest, SignCwTokenResponse>(nameof(ITokenHubClient.SignCwToken), OnSignCwToken);
	}

	async Task<SignCwTokenResponse> OnSignCwToken(SignCwTokenRequest request)
	{
		return await CallScopedService(tokenService => tokenService.SignCwTokenAsync(request, CancellationToken));
	}
}
