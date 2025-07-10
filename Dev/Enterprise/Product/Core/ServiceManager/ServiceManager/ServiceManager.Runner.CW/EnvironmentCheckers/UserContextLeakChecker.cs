using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Environment;
using ServiceManager.Common.Abstractions;
using ServiceManager.Runner.Abstractions;

namespace Enterprise.ServiceManager.Runner
{
	sealed class UserContextLeakChecker : IEnvironmentChecker
	{
		const int MaxSwitches = 1000;
		volatile int queueCount;
		readonly ConcurrentQueue<ContextSwitch> contextSwitches = new ConcurrentQueue<ContextSwitch>();
		UserContextManager? userContextManager;
		string commandCode = string.Empty;
		int errorReportingDisabled;
		bool enableStackTraceInUserContextSwitcher;
		bool corrupted;
		readonly IErrorReporterProxy errorReporterProxy;
		readonly IRunnerRegistrySettings runnerRegistry;

		public UserContextLeakChecker(IErrorReporterProxy errorReporterProxy, IRunnerRegistrySettings runnerRegistry)
		{
			this.errorReporterProxy = errorReporterProxy ?? throw new ArgumentNullException(nameof(errorReporterProxy));
			this.runnerRegistry = runnerRegistry ?? throw new ArgumentNullException(nameof(runnerRegistry));
		}

		public void Initialize(IRunCommandInfo runCommandInfo)
		{
			commandCode = runCommandInfo.Code;
			enableStackTraceInUserContextSwitcher = runnerRegistry.EnableStackTraceInUserContextSwitcher;
			userContextManager = new UserContextManager(OnUserContextChanging, OnUserContextChanged, OnDispose);
		}

		public void CheckOnServiceTaskCompletion(IServiceTaskHandler serviceTaskHandler)
		{
			userContextManager?.Dispose();
			if (corrupted)
			{
				throw new UserContextCorruptedException(serviceTaskHandler.HostedServiceAttribute);
			}
		}

		public void CheckOnServiceTaskException(IServiceTaskHandler serviceTaskHandler) { }

		public void BlockErrorReporting()
		{
			Interlocked.Increment(ref errorReportingDisabled);
		}

		public void EnableErrorReporting()
		{
			Interlocked.Decrement(ref errorReportingDisabled);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccessRule")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		void OnUserContextChanging(IUserContextChangingEventArgs userContextChangingDetails)
		{
			BlockErrorReporting();
			if (queueCount > MaxSwitches)
			{
				contextSwitches.TryDequeue(out _);
				Interlocked.Decrement(ref queueCount);
			}

			var callstackString = enableStackTraceInUserContextSwitcher
				? new System.Diagnostics.StackTrace().ToString().Trim()
				: "";

			contextSwitches.Enqueue(new ContextSwitch()
			{
				OldUserContext = new UserContextInfo(userContextChangingDetails.OldUserContext),
				NewUserContext = new UserContextInfo(userContextChangingDetails.NewUserContext),
				CallerFilePath = userContextChangingDetails.CallerFilePath,
				CallerMemberName = userContextChangingDetails.CallerMemberName,
				CallerLineNumber = userContextChangingDetails.CallerLineNumber,
				IsRevert = userContextChangingDetails.IsRevert,
				ThreadID = Thread.CurrentThread.ManagedThreadId,
				TimeOfLog = DateTime.UtcNow,
				CallStack = callstackString
			});

			Interlocked.Increment(ref queueCount);
		}

		void OnUserContextChanged(IUserContextChangingEventArgs userContextChangingDetails)
		{
			EnableErrorReporting();
		}

		void OnDispose(IUserContext startingContext, IUserContext endingContext)
		{
			if (!Equals(startingContext, endingContext))
			{
				if (errorReportingDisabled == 0)
				{
					using (Db.DisposableActionForDbConnection())
					{
						errorReporterProxy.ReportOnce(
							FormattableString.Invariant(
								$"Service task [{commandCode}] changed user context without using a temporary context."),
							FormattableString.Invariant(
								$@"Service task [{commandCode}] changed user context without using a temporary context.
Starting User Context:
Company: {startingContext.Company?.Code} - PK: {startingContext.Company?.PK}
Branch: {startingContext.Branch?.Code} - PK: {startingContext.Branch?.PK}
User: {startingContext.User?.LoginName} - PK: {startingContext.User?.PK}
Department: {startingContext.Department?.Code} - PK: {startingContext.Department?.PK}
Current User Context:
Company: {endingContext.Company?.Code} - PK: {endingContext.Company?.PK}
Branch: {endingContext.Branch?.Code} - PK: {endingContext.Branch?.PK}
User: {endingContext.User?.LoginName} - PK: {endingContext.User?.PK}
Department: {endingContext.Department?.Code} - PK: {endingContext.Department?.PK}
{GetContextLog()}"));
					}
				}

				corrupted = true;
			}
		}

		string GetContextLog()
		{
			var sb = new StringBuilder();

			while (contextSwitches.TryDequeue(out var contextSwitch))
			{
				sb.Append(
					$"[{contextSwitch.TimeOfLog.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)}] [{contextSwitch.ThreadID}] [{contextSwitch.CallerFilePath}] [{contextSwitch.CallerMemberName}] [{contextSwitch.CallerLineNumber}] [{(contextSwitch.IsRevert ? "Reset" : "Set")}] (OldUserContext: {contextSwitch.OldUserContext.Print()}), (NewUserContext: {contextSwitch.NewUserContext.Print()})\n");

				if (enableStackTraceInUserContextSwitcher)
				{
					sb.Append($"Stack: {contextSwitch.CallStack}\n");
				}
			}

			return sb.ToString();
		}

		struct ContextSwitch
		{
			public UserContextInfo OldUserContext;
			public UserContextInfo NewUserContext;
			public string CallerFilePath;
			public string CallerMemberName;
			public int CallerLineNumber;
			public bool IsRevert;
			public int ThreadID;
			public DateTime TimeOfLog;
			public string CallStack;
		}
	}
}
