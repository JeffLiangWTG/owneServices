using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Logging;
using Microsoft.Extensions.Logging;
using ServiceManager.Common.Abstractions;
using ServiceManager.Logging.Abstractions;
using ServiceManager.Runner.Abstractions;

namespace Enterprise.ServiceManager.Runner
{
	class ServiceTaskRunner : IServiceTaskRunner
	{
		public ServiceTaskRunner(
			IServiceTaskLogger serviceTaskLogger,
			IRunnerLogger runnerLogger,
			IProcessEnvironmentRecorder processEnvironmentRecorder,
			IEnvironmentCheckerStrategy environmentCheckerStrategy,
			IErrorReporterProxy errorReporterProxy,
			IServiceTaskHandlerInitializer serviceTaskHandlerInitializer,
			ICategorizedApplicationLoggerFactory loggerFactory)
		{
			this.serviceTaskLogger = serviceTaskLogger ?? throw new ArgumentNullException(nameof(serviceTaskLogger));
			this.runnerLogger = runnerLogger ?? throw new ArgumentNullException(nameof(runnerLogger));
			this.environmentCheckerStrategy = environmentCheckerStrategy ?? throw new ArgumentNullException(nameof(environmentCheckerStrategy));
			this.errorReporterProxy = errorReporterProxy ?? throw new ArgumentNullException(nameof(errorReporterProxy));
			this.serviceTaskHandlerInitializer = serviceTaskHandlerInitializer ?? throw new ArgumentNullException(nameof(serviceTaskHandlerInitializer));
			this.processEnvironmentRecorder = processEnvironmentRecorder ?? throw new ArgumentNullException(nameof(processEnvironmentRecorder));
			this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
		}

		public ServiceTaskRunResult RunServiceTask(IRunCommandInfo runCommandInfo, CancellationTokenSource cancellationTokenSource)
		{
			_ = runCommandInfo ?? throw new ArgumentNullException(nameof(runCommandInfo));

			var unhandledException = false;
			var environmentCorrupted = false;

			using (serviceTaskLogger.SetTaskLoggerCode(runCommandInfo.Code))
			using (var disposableServiceTaskHandler = serviceTaskHandlerInitializer.CreateServiceTaskHandler(runCommandInfo.AssemblyName, runCommandInfo.Code, runCommandInfo.ConfigString))
			{
				runnerLogger.Log(LogLevel.Debug, runCommandInfo.FormatRequestToLogMessage(RunnerLogMessageStage.AssemblyLoaded));

				try
				{
					environmentCheckerStrategy.Initialize(runCommandInfo);
					using (processEnvironmentRecorder.MonitorProcess(serviceTaskLogger, runCommandInfo))
					using (var activity = loggerFactory
						.CreateCategorizedLogger(LoggerCategory.ServiceTask, runCommandInfo.Code, Array.Empty<KeyValuePair<string, object>>())
						.ActivitySource
						.StartActivity("ServiceTaskRunner.RunServiceTask"))
					{
						activity.AddTag("ServiceTaskCode", runCommandInfo.Code);

						disposableServiceTaskHandler.Run(cancellationTokenSource.Token);
					}
					environmentCheckerStrategy.ExecuteOnServiceTaskCompletion(disposableServiceTaskHandler);
				}
				catch (OperationCanceledException) when (cancellationTokenSource.IsCancellationRequested)
				{
					serviceTaskLogger.Log(LogLevel.Warning, "Service task run has been canceled");
				}
				catch (EnvironmentCorruptedException environmentCorruptedException)
				{
					runnerLogger.Log(LogLevel.Error, environmentCorruptedException, nameof(environmentCheckerStrategy.ExecuteOnServiceTaskCompletion));
					errorReporterProxy.ReportOnce(nameof(environmentCheckerStrategy.ExecuteOnServiceTaskCompletion), environmentCorruptedException);

					environmentCorrupted = true;
				}
				catch (EnvironmentCheckerException)
				{
					throw;
				}
				catch (Exception exception)
				{
					unhandledException = true;
					disposableServiceTaskHandler.HandleException(exception, runCommandInfo.Code);

					try
					{
						environmentCheckerStrategy.ExecuteOnServiceTaskException(disposableServiceTaskHandler);
					}
					catch (EnvironmentCorruptedException environmentCorruptedException)
					{
						runnerLogger.Log(LogLevel.Error, environmentCorruptedException, nameof(environmentCheckerStrategy.ExecuteOnServiceTaskException));
						errorReporterProxy.ReportOnce(nameof(environmentCheckerStrategy.ExecuteOnServiceTaskException), environmentCorruptedException);

						environmentCorrupted = true;
					}
				}
			}

			return unhandledException
					? ServiceTaskRunResult.UnhandledException
					: environmentCorrupted
						? ServiceTaskRunResult.EnvironmentCorrupted
						: cancellationTokenSource.IsCancellationRequested
							? ServiceTaskRunResult.Cancelled
							: ServiceTaskRunResult.Success;
		}

		readonly ICategorizedApplicationLoggerFactory loggerFactory;
		readonly IRunnerLogger runnerLogger;
		readonly IServiceTaskLogger serviceTaskLogger;
		readonly IEnvironmentCheckerStrategy environmentCheckerStrategy;
		readonly IErrorReporterProxy errorReporterProxy;
		readonly IServiceTaskHandlerInitializer serviceTaskHandlerInitializer;
		readonly IProcessEnvironmentRecorder processEnvironmentRecorder;
	}
}
