using Microsoft.AspNetCore.SignalR.Client;

namespace CargoWise.ServiceManager.Next.Runner;

public abstract class TokenClient<T>(IServiceScopeFactory serviceScopeFactory, IHostApplicationLifetime hostApplicationLifetime) : ITokenClient where T : notnull
{
	protected CancellationToken CancellationToken => hostApplicationLifetime.ApplicationStopping;

	protected async Task CallScopedService(Func<T, Task> func)
	{
		using var scope = serviceScopeFactory.CreateScope();
		var tokenService = scope.ServiceProvider.GetRequiredService<T>();
		await func(tokenService);
	}

	protected async Task<TResult> CallScopedService<TResult>(Func<T, Task<TResult>> func)
	{
		using var scope = serviceScopeFactory.CreateScope();
		var tokenService = scope.ServiceProvider.GetRequiredService<T>();
		return await func(tokenService);
	}

	public abstract void RegisterTokenClient(HubConnection hubConnection);
}
