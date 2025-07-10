using System.Diagnostics;
using CargoWise.ServiceManager.Next.Shared;
using ServiceManager.Common.Abstractions;

namespace CargoWise.ServiceManager.Next.Launcher;

public sealed class NextProcessRunner : INextProcessRunner
{
	readonly ILogger logger;
	readonly Lazy<IProcess> currentProcess;
	readonly IProcessFactory processFactory;
	readonly INextRunnerOptions nextRunnerOptions;

	public int ProcessId => currentProcess is { IsValueCreated: true, Value.Id: var validPid } ? validPid : 0;
	public bool HasExited => currentProcess is { IsValueCreated: true, Value.HasExited: true };
	public string RunnerCode { get; }

	public NextProcessRunner(ILogger<NextProcessRunner> logger, IProcessFactory processFactory, INextRunnerOptions nextRunnerOptions, string runnerCode)
	{
		this.logger = logger;
		this.processFactory = processFactory;
		this.nextRunnerOptions = nextRunnerOptions;
		RunnerCode = runnerCode;
		currentProcess = new Lazy<IProcess>(CreateNewProcessAndStart);
	}

	public void Dispose()
	{
		if (currentProcess is { IsValueCreated: true, Value: { } process })
		{
			if (!process.HasExited)
			{
				logger.LogInformation("{RunnerCode} PID={PID} : Killing {FileName}", process.Id, RunnerCode, process.StartInfo?.FileName);
				process.Kill();
			}
			process.OutputDataReceived -= ProcessOnOutputDataReceived;
			process.ErrorDataReceived -= ProcessOnErrorDataReceived;
			process.Dispose();
		}
	}

	public async Task<int> StartAsync(CancellationToken cancellationToken)
	{
		if (HasExited)
		{
			throw new InvalidOperationException($"{RunnerCode} PID={ProcessId}: Process has exited.");
		}

		var runningProcess = (currentProcess is { IsValueCreated: true, Value: { } process }) ? process : null;

		runningProcess ??= await Task.Run(() => currentProcess.Value, cancellationToken);
		return runningProcess.Id;
	}

	IProcess CreateNewProcessAndStart()
	{
		var process = processFactory.Create(GetStartInfo(), enableRaisingEvents: true);
		process.OutputDataReceived += ProcessOnOutputDataReceived;
		process.ErrorDataReceived += ProcessOnErrorDataReceived;
		if (!process.Start())
		{
			throw new InvalidOperationException($"{RunnerCode} PID={ProcessId}: Already running {process.StartInfo?.FileName}.");
		}

		process.BeginErrorReadLine();
		process.BeginOutputReadLine();
		logger.LogInformation("{RunnerCode} PID={PID}: Started {FileName}", process.Id, RunnerCode, process.StartInfo?.FileName);
		return process;
	}

	ProcessStartInfo GetStartInfo()
	{
		var runnerExe = RunnerCode switch
		{
			"token" => "CargoWise.ServiceManager.Next.Runner.exe",
			_ => throw new NotImplementedException($"Invalid runner code: {RunnerCode}."),
		};

		var fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, runnerExe);
		var arguments = $"{INextRunnerOptions.LauncherHubOption}{nextRunnerOptions.LauncherHub} {nextRunnerOptions.ServerName} {nextRunnerOptions.DatabaseName}";
		return new ProcessStartInfo(fileName, arguments)
		{
			WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
		};
	}

	void ProcessOnErrorDataReceived(object sender, DataReceivedEventArgs e)
	{
		logger.LogInformation("{RunnerCode} PID={PID}: Error Received: {Data}", RunnerCode, ProcessId, e.Data);
	}

	void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
	{
		logger.LogDebug("{RunnerCode} PID={PID}: Data Received: {Data}", RunnerCode, ProcessId, e.Data);
	}
}
