using System;
using ServiceManager.Runner.Abstractions;

namespace Enterprise.ServiceManager.Runner
{
	[Serializable]
	public class DirectRunCommandInfo : IDirectRunCommandInfo
	{
		public DirectRunCommandInfo(string assemblyName, string code, Guid requestId, string configString = null)
		{
			AssemblyName = assemblyName ?? throw new ArgumentNullException(nameof(assemblyName));
			Code = code ?? throw new ArgumentNullException(nameof(code));
			Id = requestId;
			ConfigString = configString ?? string.Empty;
		}

		public override string ToString()
		{
			return FormattableString.Invariant($"[{Code}/{Id}], assembly [{AssemblyName}], config string [{ConfigString}]");
		}

		public string FormatRequestToLogMessage(RunnerLogMessageStage logMessageStage, params object[] values)
		{
			switch (logMessageStage)
			{
				case RunnerLogMessageStage.ReceivedCommand:
					return $"[{Code}/{Id}] Nudge run command received: assembly [{AssemblyName}], config string [{ConfigString}].";
				case RunnerLogMessageStage.PreparingExecution:
					return $"[{Code}/{Id}] Preparing for command execution.";
				case RunnerLogMessageStage.AssemblyLoaded:
					return $"[{Code}/{Id}] Assembly is loaded for Nudge run command.";
				case RunnerLogMessageStage.ServiceTaskLockAcquired:
					return $"[{Code}/{Id}] Lock is acquired for single instance service task.";
				case RunnerLogMessageStage.ServiceTaskLockReleased:
					return $"[{Code}/{Id}] Lock released for single instance Service task.";
				case RunnerLogMessageStage.ServiceTaskLockNotAcquired:
					return $"[{Code}/{Id}] Lock for task could not be acquired, because the other Runner was already running the task. Reenqueueing Nudge run command.";
				case RunnerLogMessageStage.GroupLockAcquired:
					return $"[{Code}/{Id}] Lock is acquired for mutual exclusive group {GetParam<string>(0)}.";
				case RunnerLogMessageStage.GroupLockReleased:
					return $"[{Code}/{Id}] Lock released for mutual exclusive group {GetParam<string>(0)}.";
				case RunnerLogMessageStage.GroupLockNotAcquired:
					return $"[{Code}/{Id}] Lock for mutual exclusive group {GetParam<string>(0)} could not be acquired, because the other Runner was already running the other task from this group. Reenqueueing Nudge run command.";
				case RunnerLogMessageStage.ExecutingCommand:
					return $"[{Code}/{Id}] Executing command.";
				case RunnerLogMessageStage.CompletedCommand:
					return $"[{Code}/{Id}] Command is completed.";
				case RunnerLogMessageStage.CorruptedEnvironment:
					return $"[{Code}/{Id}] Environment is corrupted by command. Runner is stopping.";
				case RunnerLogMessageStage.ServiceTaskLockNotReleased:
					return $"[{Code}/{Id}] Lock not released by service task [{GetParam<string>(0)}/{GetParam<Guid>(1)}]. Runner is stopping.";
				default:
					throw new ArgumentOutOfRangeException(nameof(logMessageStage));
			}

			T GetParam<T>(int paramNumber)
			{
				if (values.Length <= paramNumber)
				{
					throw new ArgumentOutOfRangeException($"please provide parameter number {paramNumber} of {typeof(T)} for {logMessageStage}");
				}
				return (T)values[paramNumber];
			}
		}

		public string AssemblyName { get; }
		public string Code { get; }
		public string ConfigString { get; }
		public Guid Id { get; }
	}
}
