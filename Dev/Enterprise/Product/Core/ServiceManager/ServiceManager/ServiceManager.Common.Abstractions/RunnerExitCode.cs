namespace ServiceManager.Common.Abstractions
{
	public enum RunnerExitCode
	{
		NoIssues = 0,
		ServiceTaskUnhandledException = -1,
		ServiceTaskCorruptedTheEnvironment = -2,
		RunnerFailure = -3,
		ServiceTaskLockNotReleased = -4,
	}
}
