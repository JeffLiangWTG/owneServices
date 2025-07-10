using System.Runtime.CompilerServices;

namespace CargoWise.Async
{
	public static class ThreadSentryProvider
	{
		public static IThreadSentry GetThreadSentry(
			bool crossThreadErrorReportingEnabled,
			IManagedByThreadSentry parent = null,
			bool allowChangingThreadOwnership = true,
			bool takeStackTraces = true,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = -1)
		{
			return new ThreadSentry(crossThreadErrorReportingEnabled, parent, allowChangingThreadOwnership, takeStackTraces, callerFilePath, callerMemberName, callerLineNumber);
		}
	}
}
