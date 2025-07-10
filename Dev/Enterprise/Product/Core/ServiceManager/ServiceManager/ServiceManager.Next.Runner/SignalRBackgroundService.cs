using CargoWise.ServiceManager.Next.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using ServiceManager.Common.Abstractions;

namespace CargoWise.ServiceManager.Next.Runner;

public class SignalRBackgroundService : BackgroundService
{
	readonly IHostApplicationLifetime hostApplicationLifetime;
	readonly IHttpMessageHandlerFactory handlerFactory;
	readonly INextRunnerOptions nextRunnerOptions;
	readonly ILogger logger;
	readonly IServiceScopeFactory serviceScopeFactory;
	readonly IEnumerable<ITokenClient> tokenClients;
	readonly TaskCompletionSource stopRequested = new();
	readonly TaskCompletionSource<Exception?> connectionLost = new();

	public SignalRBackgroundService(
		ILogger<SignalRBackgroundService> logger,
		IHostApplicationLifetime hostApplicationLifetime,
		INextRunnerOptions nextRunnerOptions,
		IHttpMessageHandlerFactory handlerFactory,
		IServiceScopeFactory serviceScopeFactory,
		IEnumerable<ITokenClient> tokenClients)
	{
		this.logger = logger;
		this.hostApplicationLifetime = hostApplicationLifetime;
		this.nextRunnerOptions = nextRunnerOptions;
		this.handlerFactory = handlerFactory;
		this.serviceScopeFactory = serviceScopeFactory;
		this.tokenClients = tokenClients;

		OnConnectionStarted += hubConnection =>
		{
			logger.LogInformation("Connection started: {ConnectionId}", hubConnection.ConnectionId);
			return Task.CompletedTask;
		};
	}

	public RunnerExitCode? ExitCode { get; private set; }

	public event Func<HubConnection, Task>? OnConnectionStarted;

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		try
		{
			ExitCode = await ExecuteCoreAsync(stoppingToken).ConfigureAwait(false);
		}
		finally
		{
			if (!stoppingToken.IsCancellationRequested)
			{
				hostApplicationLifetime.StopApplication();
			}
		}
	}

	async Task<RunnerExitCode> ExecuteCoreAsync(CancellationToken stoppingToken)
	{
		try
		{
			await using var hubConnection = new HubConnectionBuilder()
				.WithUrl(
					nextRunnerOptions.LauncherHub,
					o => o.HttpMessageHandlerFactory = _ => handlerFactory.CreateHandler(nameof(SignalRBackgroundService)))
				.Build();
			hubConnection.Closed += HubConnectionClosed;
			hubConnection.On("Close", HubConnectionOnClose);
			hubConnection.On("ProcessId", HubConnectionOnProcessId);
			foreach (var tokenClient in tokenClients)
			{
				tokenClient.RegisterTokenClient(hubConnection);
			}

			await hubConnection.StartAsync(stoppingToken).ConfigureAwait(false);
			OnConnectionStarted?.Invoke(hubConnection);

			await Task.WhenAny(
					connectionLost.Task,
					stopRequested.Task,
					Task.Delay(Timeout.Infinite, stoppingToken)).ConfigureAwait(false);

			hubConnection.Closed -= HubConnectionClosed;
			if (connectionLost.Task.IsCompletedSuccessfully)
			{
				var exception = await connectionLost.Task.ConfigureAwait(false);
				logger.LogInformation(exception, "Closing the application after SignalR disconnection");
				return RunnerExitCode.RunnerFailure;
			}

			if (stopRequested.Task.IsCompletedSuccessfully)
			{
				logger.LogInformation("Closing the application after request from Hub");
				return RunnerExitCode.NoIssues;
			}

			logger.LogInformation("Application is shutting down");
			return RunnerExitCode.NoIssues;
		}
		catch (OperationCanceledException e) when (stoppingToken.IsCancellationRequested)
		{
			logger.LogInformation(e, "Application is shutting down");
			return RunnerExitCode.NoIssues;
		}
		catch (Exception e)
		{
			logger.LogWarning(e, $"Exception caught while running {nameof(SignalRBackgroundService)}, closing the application");
			return RunnerExitCode.ServiceTaskUnhandledException;
		}
	}

	int HubConnectionOnProcessId()
	{
		logger.LogInformation("ProcessID requested from hub");
		return Environment.ProcessId;
	}

	bool HubConnectionOnClose()
	{
		logger.LogInformation("ShutDown requested from hub");
		stopRequested.SetResult();
		return true;
	}

	Task HubConnectionClosed(Exception? exception)
	{
		logger.LogInformation(exception, "Connection terminated, closing the application");
		connectionLost.SetResult(exception);
		return Task.CompletedTask;
	}
}
