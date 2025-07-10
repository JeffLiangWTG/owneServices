using System;

namespace ServiceManager.Runner.Abstractions
{
	/// <summary>
	/// Top level application exception handling logic.
	/// Decides whether to log and/or report, or ignore.
	/// </summary>
	public interface IApplicationExceptionHandler
	{
		/// <summary>
		/// Handle exceptions during runner initialization.
		/// The actual service task will not have been run yet.
		/// 
		/// Catches and handles any additional exceptions that occur.
		/// </summary>
		/// <param name="loggerProvider">logger provider to use if logging is needed. May return null.</param>
		void HandleFromInitialization(Exception ex, Func<IRunnerLogger> loggerProvider);

		/// <summary>
		/// A database upgrade exception occurred.
		/// Needed for some special logic to suppress further logging in this case.
		/// </summary>
		bool WasDatabaseUpgradeHandledFromInitialization { get; }

		/// <summary>
		/// Handle exceptions from the actual service task.
		/// These are expected to only be critical exceptions that the task itself cannot handle.
		/// 
		/// Catches and handles any additional exceptions that occur.
		/// </summary>
		/// <param name="loggerProvider">logger provider to use if logging is needed. May return null.</param>
		void HandleFromTask(Exception ex, Func<IRunnerLogger> loggerProvider);
	}
}
