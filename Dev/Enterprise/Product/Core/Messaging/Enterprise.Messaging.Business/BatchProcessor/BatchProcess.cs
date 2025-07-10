using System;
using System.Threading;

namespace Enterprise.BatchProcessor
{
	public abstract class BatchProcess : MarshalByRefObject, IDisposable
	{
		protected BatchProcess()
		{
			Logger = new LoggingInformation();
		}

		public BatchProcess(LoggingInformation logger)
		{
			if (logger == null)
			{
				throw new ArgumentNullException(nameof(logger));
			}
			this.Logger = logger;
		}

		public int NumberOfOperationsAttempted;
		public int NumberOfOperationsSucceeded;
		public LoggingInformation Logger { get; set; }

		public void ExecuteBatch(CancellationToken cancellationToken)
		{
			NumberOfOperationsAttempted = 0;
			NumberOfOperationsSucceeded = 0;
			Logger.ClearLogs();

			if (IsEnvironmentDataValid())
			{
				Execute(cancellationToken);
			}
		}

		public virtual string HumanReadableName
		{
			get { return String.Empty; }
		}

		#region Implementation

		public virtual void Dispose()
		{
		}

#if NETCOREAPP
		[Obsolete("InitializeLifetimeService() is not supported in .NET Core.")]
#endif
		public override object InitializeLifetimeService()
		{
			return null;
		}

		protected virtual bool IsEnvironmentDataValid()
		{
			return true;
		}

		protected abstract void Execute(CancellationToken cancellationToken);

		/// <summary>
		/// This is intended to allow one logging information class to Retrieve log info events from another 'sub' logging info
		/// </summary>
		protected void HandleNestedLogEvent(string logMsg, bool debugOnly)
		{
			logMsg = logMsg.Replace("\t", string.Empty);
			if (debugOnly)
			{
				Logger.DebugLog(logMsg);
			}
			else
			{
				Logger.Log(logMsg);
			}
		}

#if DEBUG

		public void ExecuteBatch()
		{
			ExecuteBatch(CancellationToken.None);
		}

		protected void Execute()
		{
			Execute(CancellationToken.None);
		}

#endif

#endregion
	}
}
