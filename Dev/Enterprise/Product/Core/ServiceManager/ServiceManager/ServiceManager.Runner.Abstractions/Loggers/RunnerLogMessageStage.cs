namespace ServiceManager.Runner.Abstractions
{
	public enum RunnerLogMessageStage
	{
		ReceivedCommand,
		PreparingExecution,
		AssemblyLoaded,
		IgnoredDuplicatedCommand,
		ServiceTaskLockAcquired,
		ServiceTaskLockReleased,
		ServiceTaskLockNotAcquired,
		GroupLockAcquired,
		GroupLockReleased,
		GroupLockNotAcquired,
		ExecutingCommand,
		CompletedCommand,
		CorruptedEnvironment,
		ServiceTaskLockNotReleased,
	}
}
