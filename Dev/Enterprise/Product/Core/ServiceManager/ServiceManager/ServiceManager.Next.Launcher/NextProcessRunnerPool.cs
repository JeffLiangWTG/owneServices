using System.Collections.Concurrent;
using CargoWise.ServiceManager.Next.Shared;
using CargoWise.ServiceManager.Next.Shared.Services;
using Enterprise.ServiceManager.Shared;

namespace CargoWise.ServiceManager.Next.Launcher;

public sealed class NextProcessRunnerPool : INextProcessRunnerPool, IAsyncDisposable
{
	readonly INextProcessRunnerFactory nextProcessRunnerFactory;
	readonly ICommandSender commandSender;
	readonly NextRunnerOptions nextRunnerOptions;
	readonly Dictionary<INextProcessRunner, Timer> runnerTimers = new();
	readonly ConcurrentDictionary<string, ConcurrentBag<INextProcessRunner>> availableRunnersByRunnerCode = new();
	readonly TimeSpan gracefulStopTimeout = TimeSpan.FromSeconds(60);
	readonly TimeSpan runnerIdleLifetime;

	public NextProcessRunnerPool(INextProcessRunnerFactory nextProcessRunnerFactory, ICommandSender commandSender, INextLauncherOptions nextLauncherOptions)
	{
		this.nextProcessRunnerFactory = nextProcessRunnerFactory;
		this.commandSender = commandSender;
		var baseUri = ServiceManagerHelper.GetLocalBaseUri(nextLauncherOptions.ServiceType, nextLauncherOptions.EnterpriseCode, nextLauncherOptions.ServerCode);
		var launcherHubUri = new Uri(baseUri, nextLauncherOptions.LauncherHubEndpoint);
		nextRunnerOptions = new NextRunnerOptions(
			launcherHubUri,
			nextLauncherOptions.DatabaseName,
			nextLauncherOptions.ServerName);
		runnerIdleLifetime = nextLauncherOptions.RunnerIdleLifetime;
	}

	public async ValueTask DisposeAsync()
	{
		INextProcessRunner? runner;
		do
		{
			runner = runnerTimers.Keys.FirstOrDefault();
			await DisposeAsync(runner, stopRunner: false).ConfigureAwait(false);
		}
		while (runner is not null);
	}

	public async Task<TResponse> RunAsync<TRequest, TResponse>(
		string runnerCode,
		TokenHandlerDelegate<TRequest, TResponse> handler,
		TRequest request,
		CancellationToken cancellationToken)
	{
		var processRunner = await GetOrCreateRunnerAsync(runnerCode, cancellationToken).ConfigureAwait(false);
		try
		{
			var result = await commandSender.SendRequestAsync(processRunner, handler, request, cancellationToken).ConfigureAwait(false);
			return result;
		}
		finally
		{
			if (runnerTimers.TryGetValue(processRunner, out var timer))
			{
				timer.Change(runnerIdleLifetime, Timeout.InfiniteTimeSpan);
				var availableBag = availableRunnersByRunnerCode.GetOrAdd(processRunner.RunnerCode, _ => new ConcurrentBag<INextProcessRunner>());
				availableBag.Add(processRunner);
			}
		}
	}

	async Task DisposeAsync(INextProcessRunner? processRunner, bool stopRunner)
	{
		if (processRunner is not null && runnerTimers.Remove(processRunner, out var timer))
		{
			await timer.DisposeAsync().ConfigureAwait(false);
			using (processRunner)
			{
				if (stopRunner && !processRunner.HasExited)
				{
					using var cts = new CancellationTokenSource(gracefulStopTimeout);
					await commandSender.CloseRunnerAsync(processRunner, cts.Token).ConfigureAwait(false);
				}
			}
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "availableRunner disposal is handled")]
	async Task<INextProcessRunner> GetOrCreateRunnerAsync(string runnerCode, CancellationToken cancellationToken)
	{
		if (availableRunnersByRunnerCode.TryGetValue(runnerCode, out var availableBag))
		{
			while (availableBag.TryTake(out var availableRunner) && !cancellationToken.IsCancellationRequested)
			{
				if (runnerTimers.TryGetValue(availableRunner, out var currentTimer))
				{
					if (availableRunner.HasExited)
					{
						await DisposeAsync(availableRunner, stopRunner: false).ConfigureAwait(false);
					}
					else
					{
						currentTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
						return availableRunner;
					}
				}
			}
		}

		var processRunner = nextProcessRunnerFactory.Create(runnerCode, nextRunnerOptions);
		var timer = new Timer(async state => await OnRunnerTimerElapsed(state), processRunner, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
		runnerTimers[processRunner] = timer;
		return processRunner;
	}

	async Task OnRunnerTimerElapsed(object? state)
	{
		if (state is not INextProcessRunner processRunner)
		{
			throw new InvalidOperationException($"Invalid state object: {state}");
		}

		if (!availableRunnersByRunnerCode.TryGetValue(processRunner.RunnerCode, out var availableBag) || !availableBag.Contains(processRunner))
		{
			return;
		}

		await DisposeAsync(processRunner, stopRunner: true).ConfigureAwait(false);
	}

	record NextRunnerOptions(Uri LauncherHub, string DatabaseName, string ServerName) : INextRunnerOptions;
}
