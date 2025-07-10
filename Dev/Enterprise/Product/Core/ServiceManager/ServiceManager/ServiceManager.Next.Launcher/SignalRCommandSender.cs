using CargoWise.ServiceManager.Next.Shared.Services;
using Microsoft.AspNetCore.SignalR;

namespace CargoWise.ServiceManager.Next.Launcher;

public sealed class SignalRCommandSender : ICommandSender
{
	readonly ILogger logger;
	readonly IHubContext<NextHub, ITokenHubClient> hubContext;
	readonly Dictionary<string, Task<int>> connections = new();

	public SignalRCommandSender(ILogger<SignalRCommandSender> logger, IHubContext<NextHub, ITokenHubClient> hubContext)
	{
		this.logger = logger;
		this.hubContext = hubContext;
	}

	public void AddConnection(string connectionId, CancellationToken cancellationToken)
	{
		var task = SingleClient(connectionId).ProcessId(cancellationToken);
		task.ContinueWith(t =>
		{
			logger.LogInformation(t.Exception, "Error getting process ID for connection {ConnectionId}", connectionId);
			RemoveConnection(connectionId);
		}, TaskContinuationOptions.OnlyOnFaulted);
		connections.Add(connectionId, task);
	}

	public void RemoveConnection(string connectionId)
	{
		connections.Remove(connectionId);
	}

	public async Task<TResponse> SendRequestAsync<TRequest, TResponse>(INextProcessRunner nextProcessRunner, TokenHandlerDelegate<TRequest, TResponse> handler, TRequest request, CancellationToken cancellationToken)
	{
		logger.LogDebug("Received request for runner {RunnerCode}: {Request}", nextProcessRunner.RunnerCode, request);
		var connectionId = await WaitForConnectionAsync(nextProcessRunner, cancellationToken);
		var singleClient = SingleClient(connectionId);
		return await handler(singleClient)(request, cancellationToken);
	}

	public async Task CloseRunnerAsync(INextProcessRunner nextProcessRunner, CancellationToken cancellationToken)
	{
		var connectionId = GetConnection(nextProcessRunner.ProcessId);
		if (connectionId is null || nextProcessRunner.HasExited)
		{
			logger.LogDebug("Runner {RunnerCode} PID={ProcessId} is disconnected.", nextProcessRunner.RunnerCode, nextProcessRunner.ProcessId);
			return;
		}

		logger.LogDebug("Closing runner {RunnerCode} PID={ProcessId}.", nextProcessRunner.RunnerCode, nextProcessRunner.ProcessId);
		await SingleClient(connectionId).Close(cancellationToken);
		SpinWait.SpinUntil(() => cancellationToken.IsCancellationRequested || nextProcessRunner.HasExited);
		cancellationToken.ThrowIfCancellationRequested();
		logger.LogDebug("Runner {RunnerCode} PID={ProcessId} is closed.", nextProcessRunner.RunnerCode, nextProcessRunner.ProcessId);
	}

	ITokenHubClient SingleClient(string connectionId)
	{
		return hubContext.Clients.Client(connectionId);
	}

	async Task<string> WaitForConnectionAsync(INextProcessRunner nextProcessRunner, CancellationToken cancellationToken)
	{
		var pid = await nextProcessRunner.StartAsync(cancellationToken);
		SpinWait.SpinUntil(() => cancellationToken.IsCancellationRequested || nextProcessRunner.HasExited || GetConnection(pid) != null);
		cancellationToken.ThrowIfCancellationRequested();
		if (nextProcessRunner.HasExited)
		{
			throw new InvalidOperationException($"{nextProcessRunner.RunnerCode} PID={nextProcessRunner.ProcessId}: Process has exited.");
		}

		return GetConnection(pid) ?? throw new InvalidOperationException($"{nextProcessRunner.RunnerCode} PID={nextProcessRunner.ProcessId}: Process is disconnected");
	}

	string? GetConnection(int processId)
	{
		return connections.FirstOrDefault(pair => pair.Value.IsCompleted && pair.Value.Result == processId).Key;
	}
}