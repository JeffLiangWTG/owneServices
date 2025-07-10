using CargoWise.Common;
using CargoWise.Data;
using Microsoft.Extensions.Logging;
using ServiceManager.Common.CW;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Runner.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Runner
{
	class ServiceTaskRunnerStrategy : IServiceTaskRunnerStrategy, IServiceTaskRunnerCanceler
	{
		public ServiceTaskRunnerStrategy(
			IRunnerLogger runnerLogger,
			ISqlMutexLocker serviceTaskLocker,
			IServiceTaskRunnerWithNextRunTimeCheckFactory serviceTaskRunnerWithNextRunTimeCheckFactory,
			IClientHostedServiceAttributeProvider hostedServiceAttributeProvider)
		{
			this.runnerLogger = runnerLogger ?? throw new ArgumentNullException(nameof(runnerLogger));
			this.serviceTaskLocker = serviceTaskLocker ?? throw new ArgumentNullException(nameof(serviceTaskLocker));
			this.serviceTaskRunnerWithNextRunTimeCheckFactory = serviceTaskRunnerWithNextRunTimeCheckFactory ?? throw new ArgumentNullException(nameof(serviceTaskRunnerWithNextRunTimeCheckFactory));
			this.hostedServiceAttributeProvider = hostedServiceAttributeProvider ?? throw new ArgumentNullException(nameof(hostedServiceAttributeProvider));
		}

		public ServiceTaskRunResult Run(IRunCommandInfo runCommandInfo)
		{
			_ = runCommandInfo ?? throw new ArgumentNullException(nameof(runCommandInfo));

			using (tokenSource = new CancellationTokenSource())
			using (new DisposableAction(() => tokenSource = null))
			{
				return hostedServiceAttributeProvider.GetClientHostedServiceAttribute(runCommandInfo.Code).AllowsMultipleInstances
					? RunServiceTask(runCommandInfo, tokenSource)
					: RunServiceTaskLocked(runCommandInfo, tokenSource);
			}
		}

		public void Cancel()
		{
			tokenSource?.Cancel();
		}

		ServiceTaskRunResult RunServiceTask(IRunCommandInfo runCommandInfo, CancellationTokenSource cancellationTokenSource)
		{
			runnerLogger.Log(LogLevel.Debug, runCommandInfo.FormatRequestToLogMessage(RunnerLogMessageStage.ExecutingCommand));
			return serviceTaskRunnerWithNextRunTimeCheckFactory.CreateRunner().RunServiceTask(runCommandInfo, cancellationTokenSource);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "lockResult disposed in ReleaseLockWithLogging")]
		ServiceTaskRunResult RunServiceTaskLocked(IRunCommandInfo runCommandInfo, CancellationTokenSource cancellationTokenSource)
		{
			if (serviceTaskLocker.TryAcquireLock(runCommandInfo.Code, out var lockResult))
			{
				var ignoreLockReleaseExceptions = false;
				var result = ServiceTaskRunResult.Success;
				try
				{
					runnerLogger.Log(LogLevel.Debug, runCommandInfo.FormatRequestToLogMessage(RunnerLogMessageStage.ServiceTaskLockAcquired));
					result = RunTaskOrGroup(runCommandInfo, cancellationTokenSource);
					ignoreLockReleaseExceptions = result.HasFlag(ServiceTaskRunResult.IgnoreReleaseLockError);
				}
				catch (Exception)
				{
					ignoreLockReleaseExceptions = true;
					throw;
				}
				finally
				{
					ReleaseLockWithLogging(lockResult, runCommandInfo, ignoreLockReleaseExceptions, ref result);
				}

				return result;
			}

			runnerLogger.Log(LogLevel.Debug, runCommandInfo.FormatRequestToLogMessage(RunnerLogMessageStage.ServiceTaskLockNotAcquired));
			return ServiceTaskRunResult.ServiceTaskLockNotAcquired;
		}

		ServiceTaskRunResult RunTaskOrGroup(IRunCommandInfo runCommandInfo, CancellationTokenSource cancellationTokenSource)
		{
			var taskGroup = hostedServiceAttributeProvider.GetClientHostedServiceAttribute(runCommandInfo.Code).MutuallyExclusiveTaskGroup;
			if (taskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup)
			{
				return RunServiceTask(runCommandInfo, cancellationTokenSource);
			}
			else
			{
				return RunGroup(runCommandInfo, cancellationTokenSource, taskGroup.ToString());
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "groupLockResult disposed in ReleaseLockWithLogging")]
		ServiceTaskRunResult RunGroup(IRunCommandInfo runCommandInfo, CancellationTokenSource cancellationTokenSource, string taskGroup)
		{
			if (serviceTaskLocker.TryAcquireLock(taskGroup, out var groupLockResult))
			{
				var ignoreLockReleaseExceptions = false;
				var result = ServiceTaskRunResult.Success;

				try
				{
					runnerLogger.Log(LogLevel.Debug, runCommandInfo.FormatRequestToLogMessage(RunnerLogMessageStage.GroupLockAcquired, taskGroup));
					result = RunServiceTask(runCommandInfo, cancellationTokenSource);
					ignoreLockReleaseExceptions = result.HasFlag(ServiceTaskRunResult.IgnoreReleaseLockError);
				}
				catch (Exception)
				{
					ignoreLockReleaseExceptions = true;
					throw;
				}
				finally
				{
					ReleaseLockWithLogging(groupLockResult, runCommandInfo, ignoreLockReleaseExceptions, ref result, taskGroup);
				}

				return result;
			}

			runnerLogger.Log(LogLevel.Debug, runCommandInfo.FormatRequestToLogMessage(RunnerLogMessageStage.GroupLockNotAcquired, taskGroup));
			return ServiceTaskRunResult.GroupLockNotAcquired;
		}

		void ReleaseLockWithLogging(IDisposable lockResult, IRunCommandInfo runCommandInfo, bool ignoreLockReleaseExceptions, ref ServiceTaskRunResult result, string? taskGroup = null)
		{
			var isGroup = !string.IsNullOrEmpty(taskGroup);
			try
			{
				if (Db.DatabaseUpgradedExceptionHasBeenThrownInConnection)
				{
					// We still need to dispose the lock since it contains a timer.
					// This causes a second DatabaseUpgradedException which can generate an error report about DatabaseUpgradedException thrown multiple times
					// unless we suppress that...
					Db.Connection.DatabaseUpgradedExceptionHasBeenThrown = false;
				}

				lockResult.Dispose();
				var logText = isGroup
					? runCommandInfo.FormatRequestToLogMessage(RunnerLogMessageStage.GroupLockReleased, taskGroup)
					: runCommandInfo.FormatRequestToLogMessage(RunnerLogMessageStage.ServiceTaskLockReleased);
				runnerLogger.Log(LogLevel.Debug, logText);
			}
			catch (Exception ex)
			{
				if (ignoreLockReleaseExceptions)
				{
					// If we already had an exception, ignore the new one
					return;
				}

				if (ex is DatabaseUpgradedException)
				{
					// bubble up unchanged since the locks have all been deleted by the upgrade
					throw;
				}

				if (ex is SqlMutexLockReleaseException)
				{
					if (isGroup)
					{
						runnerLogger.Log(LogLevel.Error, $"Service task [{runCommandInfo.AssemblyName}|{runCommandInfo.Code}]: Failed to release group lock [{taskGroup}]", ex);
					}
					else
					{
						runnerLogger.Log(LogLevel.Error, $"Service task [{runCommandInfo.AssemblyName}|{runCommandInfo.Code}]: Failed to release service task lock", ex);
					}
					result = ServiceTaskRunResult.LockNotReleased;
					return;
				}

				if (isGroup)
				{
					runnerLogger.Log(LogLevel.Error, $"Service task [{runCommandInfo.AssemblyName}|{runCommandInfo.Code}]: Failed to release group lock [{taskGroup}]", ex);
					throw new MutualExclusiveLockReleaseException(ex, runCommandInfo.Code, taskGroup);
				}
				runnerLogger.Log(LogLevel.Error, $"Service task [{runCommandInfo.AssemblyName}|{runCommandInfo.Code}]: Failed to release service task lock", ex);
				throw new ServiceTaskLockReleaseException(ex, runCommandInfo.Code);
			}
		}

		readonly IRunnerLogger runnerLogger;
		readonly IServiceTaskLocker serviceTaskLocker;
		readonly IServiceTaskRunnerWithNextRunTimeCheckFactory serviceTaskRunnerWithNextRunTimeCheckFactory;
		readonly IClientHostedServiceAttributeProvider hostedServiceAttributeProvider;
		CancellationTokenSource? tokenSource;
	}
}
