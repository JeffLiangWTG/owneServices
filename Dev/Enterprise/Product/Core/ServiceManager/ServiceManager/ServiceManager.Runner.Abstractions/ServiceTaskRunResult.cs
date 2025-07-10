using System;

namespace ServiceManager.Runner.Abstractions
{
	[Flags]
	public enum ServiceTaskRunResult
	{
		Success = 0,
		UnhandledException = 1 << 1,
		Cancelled = 1 << 2,
		EnvironmentCorrupted = (1 << 3) | IgnoreReleaseLockError,
		ServiceTaskLockNotAcquired = 1 << 4,
		GroupLockNotAcquired = 1 << 5,
		LockNotReleased = 1 << 6 | IgnoreReleaseLockError,
		IgnoreReleaseLockError = 1 << 31,
	}
}
