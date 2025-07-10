using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using CargoWise.Common;

namespace CargoWise.Async
{
	public class ThreadSentry : IThreadSentry
	{
		internal ThreadSentry(
			bool reportingEnabled = true,
			IManagedByThreadSentry parent = null,
			bool allowChangingThreadOwnership = true,
			bool takeStackTraces = true,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = -1)
		{
			CreationThread = OwnerThread = new ThreadLog(takeOwnership: true, useFullTrace: takeStackTraces, callerInfo: (callerFilePath, callerMemberName, callerLineNumber));
			SyncContext = SynchronizationContext.Current;
			this.reportingEnabled = reportingEnabled;
			threadSentryDispatcher = new ThreadSentryDispatchQueue(this);
			this.parent = parent;
			this.allowChangingThreadOwnership = allowChangingThreadOwnership;
		}

		readonly ThreadSentryDispatchQueue threadSentryDispatcher;
		readonly IManagedByThreadSentry parent;
		readonly bool allowChangingThreadOwnership;

		public ThreadLog CreationThread { get; private set; }
		public ThreadLog OwnerThread { get; private set; }

		bool reportingEnabled;

		int CurrentThreadID
		{
			get { return Environment.CurrentManagedThreadId; }
		}

		public void EnsureCurrentThreadIsOwner(string diagnosticLabel) => EnsureCurrentThreadIsOwner(() => diagnosticLabel);

		public void EnsureCurrentThreadIsOwner(Func<string> diagnosticLabelProvider = null)
		{
			if (!reportingEnabled)
			{
				return;
			}

			string message = null;
			lock (this)
			{
				if (OwnerThread.ThreadID == null)
				{
					message = FormatErrorMessage(AttemptedAccessNullOwnedObjectMessage, diagnosticLabelProvider?.Invoke());
				}
				else if (!IsOwner)
				{
					message = FormatErrorMessage(OnlyOwnerThreadCanAccessThisObjectMessage, diagnosticLabelProvider?.Invoke());
				}
			}

			ReportThreadErrorIfFound(message);
		}

		public bool IsOwner
		{
			get { return OwnerThread.ThreadID == CurrentThreadID; }
		}

		public void RelinquishThreadOwnership(string diagnosticLabel) => RelinquishThreadOwnership(() => diagnosticLabel);

		public void RelinquishThreadOwnership(Func<string> diagnosticLabelProvider = null) => RelinquishThreadOwnershipCore(diagnosticLabelProvider);

#if DEBUG
		public void ForciblyRelinquishThreadOwnership_ForTest(string diagnosticLabel) => ForciblyRelinquishThreadOwnership_ForTest(() => diagnosticLabel);

		public void ForciblyRelinquishThreadOwnership_ForTest(Func<string> diagnosticLabelProvider = null) => RelinquishThreadOwnershipCore(diagnosticLabelProvider, forceRelinquishIfNotAllowed: true);

		public void ExecuteAll_ForTest() => threadSentryDispatcher.ExecuteAll();
#endif

		void RelinquishThreadOwnershipCore(Func<string> diagnosticLabelProvider, bool forceRelinquishIfNotAllowed = false)
		{
			string message = null;
			lock (this)
			{
				if (OwnerThread.ThreadID == null)
				{
					message = FormatErrorMessage(CanNotRelinquishOwnershipOfObjectWithNoOwnershipMessage, diagnosticLabelProvider?.Invoke());
				}
				else if (!IsOwner)
				{
					message = FormatErrorMessage(OnlyCurrentThreadOwnerCanRelinquishThreadOwnershipMessage, diagnosticLabelProvider?.Invoke());
				}
				else if (!allowChangingThreadOwnership && !forceRelinquishIfNotAllowed)
				{
					message = FormatErrorMessage(RelinquishingOwnershipIsNotAllowed, diagnosticLabelProvider?.Invoke());
				}
				else
				{
					OwnerThread = new ThreadLog(takeOwnership: false);
					SyncContext = null;
					parent?.NotifyThreadSentryOwnershipRelinquished();
				}
			}

			ReportThreadErrorIfFound(message);
		}

		public void TakeThreadOwnership(string diagnosticLabel) => TakeThreadOwnership(() => diagnosticLabel);

		public void TakeThreadOwnership(Func<string> diagnosticLabelProvider = null)
		{
			string message = null;
			lock (this)
			{
				if (IsOwner)
				{
					message = FormatErrorMessage(ThisThreadAlreadyHasThreadOwnershipOfThisObjectMessage, diagnosticLabelProvider?.Invoke());
				}
				else if (OwnerThread.ThreadID != null)
				{
					message = FormatErrorMessage(CanNotTakeThreadOwnershipOfAnOwnedObjectMessage, diagnosticLabelProvider?.Invoke());
				}
				else
				{
					TakeThreadOwnership(new ThreadLog(takeOwnership: true), SynchronizationContext.Current);
				}
			}

			ReportThreadErrorIfFound(message);
		}

		void TakeThreadOwnership(ThreadLog ownerThread, SynchronizationContext currentSyncContext)
		{
			OwnerThread = ownerThread;
			SyncContext = currentSyncContext;
			parent?.NotifyThreadSentryOwnershipTaken();
		}

		public bool IsPostable
		{
			get { return isPostable; }
		}
		bool isPostable;

		[SuppressMessage("CargoWiseOne", "CW1021")]
		public static bool IsPostableProcess { get; set; }

		public void Post(SendOrPostCallback d, object state, IThreadSentry callbackSentry, SendOrPostCallback callback, string callingMethodName)
		{
			if ((callback != null) && (callbackSentry == null))
			{
				throw new ArgumentNullException(nameof(callbackSentry));
			}

			if (IsOwner)
			{
				d(state);
				if (callback != null)
				{
					callbackSentry.Post(callback, state, callingMethodName);
				}
			}
			else if (IsPostable && SyncContext != null)
			{
				if (threadSentryDispatcher.Enqueue(d, state, callbackSentry, callback, callingMethodName))
				{
					SyncContext.Post(DispatchSyncContext, null);
				}
			}
		}

		public void Post(SendOrPostCallback d, object state, string callingMethodName)
		{
			Post(d, state, null, null, callingMethodName);
		}

		void DispatchSyncContext(object state)
		{
			if (IsOwner)
			{
				threadSentryDispatcher.ExecuteAll();
			}
			else if (IsPostable && SyncContext != null)
			{
				SyncContext.Post(DispatchSyncContext, null);
			}
		}

		public IDisposable SuppressReporting()
		{
			if (!reportingEnabled)
			{
				return DisposableAction.NoAction;
			}

			reportingEnabled = false;

			return new DisposableAction(() => reportingEnabled = true);
		}

		void ReportThreadErrorIfFound(string message)
		{
			if (!string.IsNullOrEmpty(message) && reportingEnabled)
			{
				ErrorReporter.ReportOnce(message, new CrossThreadAccessException(message));
				reportingEnabled = false;
			}
		}

		string FormatErrorMessage(string errorMessage, string diagnosticLabel)
		{
			var message = errorMessage + ThreadStackTraces;

			if (parent != null)
			{
				var labelMessage = diagnosticLabel != null ? string.Format(CultureInfo.InvariantCulture, ", Label: [{0}]", diagnosticLabel) : null;
				message = string.Format(CultureInfo.InvariantCulture, "ThreadSentry Parent: [{0}]{1}{2}{2}{3}", parent.DisplayName, labelMessage, Environment.NewLine, message); // Error message text.
			}

			return message;
		}

		public static string OnlyOwnerThreadCanAccessThisObjectMessage
		{
			get
			{
				return @"Attempted to access an object owned by another thread. Please call RelinquishThreadOwnership() from the thread that owns the object, and TakeThreadOwnership() on the the new thread before attempting to use the object."; // Error message text.
			}
		}

		public static string AttemptedAccessNullOwnedObjectMessage
		{
			get
			{
				return @"Attempted to access an object with null thread ownership. Another thread has relinquished thread ownership of this object using RelinquishThreadOwnership(). Please TakeThreadOwnership() on the the new thread before attempting to use the object."; // Error message text.
			}
		}

		public static string CanNotTakeThreadOwnershipOfAnOwnedObjectMessage
		{
			get
			{
				return @"Attempted to take thread ownership on an object owned by another thread. Please call RelinquishThreadOwnership() from the thread that owns the object, and TakeThreadOwnership() on the the new thread before attempting to use the object."; // Error message text.
			}
		}

		public static string ThisThreadAlreadyHasThreadOwnershipOfThisObjectMessage
		{
			get
			{
				return "Attempted to take ownership of an object that this thread already owns. Please call RelinquishThreadOwnership() if you wish to relinquish this threads ownership, and TakeThreadOwnership() on the the new thread before attempting to use the object."; // Error message text.
			}
		}

		public static string OnlyCurrentThreadOwnerCanRelinquishThreadOwnershipMessage
		{
			get
			{
				return @"Attempted to call RelinquishThreadOwnership() on an object that the thread did not own. Please call RelinquishThreadOwnership() if you wish to relinquish this threads ownership, and TakeThreadOwnership() on the the new thread before attempting to use the object."; // Error message text.
			}
		}

		public static string CanNotRelinquishOwnershipOfObjectWithNoOwnershipMessage
		{
			get
			{
				return @"Attempted to call RelinquishThreadOwnership() on an object that had no thread owner. Please call RelinquishThreadOwnership() if you wish to relinquish this threads ownership, and TakeThreadOwnership() on the the new thread before attempting to use the object."; // Error message text.
			}
		}

		public static string RelinquishingOwnershipIsNotAllowed
		{
			get
			{
				return @"Attempted to call RelinquishThreadOwnership() on an object that does now allow changing thread ownership."; // Error message text.
			}
		}

		string ThreadStackTraces
		{
			get
			{
				string CreateStackInfo() => FormatStack(CreationThread.ThreadStackTrace) ?? CreationThread.LightTraceInfo;
				string OwnerStackInfo() => FormatStack(OwnerThread.ThreadStackTrace) ?? OwnerThread.LightTraceInfo;
				if (CreationThread == OwnerThread)
				{
					return string.Format(CultureInfo.InvariantCulture,
@"

Creation and Owner Thread, Name: [{0}], ID: {1}, Stacktrace:

{2}

Current Thread, Name: [{3}], ID: {4}, Stacktrace:

{5}", // Error message text
						CreationThread.ThreadName,
						CreationThread.ThreadID,
						CreateStackInfo(),
						Thread.CurrentThread.Name,
						Environment.CurrentManagedThreadId,
						FormatStack(new StackTrace())
						);
				}
				else
				{
					return string.Format(CultureInfo.InvariantCulture,
@"
					
Creation Thread, Name: [{0}], ID: {1}, Stacktrace:
					
{2}
					
Owner Thread, Name: [{3}], ID: {4}, Stacktrace:
					
{5}
					
Current Thread, Name: [{6}], ID: {7}, Stacktrace:

{8}", // Error message text
						CreationThread.ThreadName,
						CreationThread.ThreadID,
						CreateStackInfo(),
						OwnerThread.ThreadName,
						OwnerThread.ThreadID,
						OwnerStackInfo(),
						Thread.CurrentThread.Name,
						Environment.CurrentManagedThreadId,
						FormatStack(new StackTrace())
						);
				}
			}
		}

		static string FormatStack(StackTrace stackTrace)
		{
			if (stackTrace == null)
			{
				return null;
			}
			else
			{
				var stack = stackTrace.ToString();
				TrimStack(ref stack);

				return stack;
			}
		}

		[Conditional("DEBUG")]
		static void TrimStack(ref string stack)
		{
			Argument.NotNull(stack, nameof(stack)); // Suggested By ReviewBot 

			var index = stack.IndexOf("at NUnit.Framework", StringComparison.OrdinalIgnoreCase);
			if (index > 0)
			{
				stack = stack.Substring(0, index);
			}
		}

		public SynchronizationContext SyncContext
		{
			get
			{
				if (IsOwner && syncContext != SynchronizationContext.Current)
				{
					throw new InvalidOperationException("Error when checking syncContext");
				}

				return syncContext;
			}
			private set
			{
				isPostable = IsPostableProcess
					&& value != null
					&& value.GetType() != typeof(SynchronizationContext)
					&& ((value as IThreadSentryPostingControl)?.EnablePosting ?? true);
				syncContext = value;
			}
		}

		SynchronizationContext syncContext;

		#region For Test
#if DEBUG
		public IDisposable ForcefullyBorrowThreadOwnership_ForTest()
		{
			var previousOwnerThread = OwnerThread;
			var previousSyncContext = SyncContext;

			lock (this)
			{
				TakeThreadOwnership(new ThreadLog(takeOwnership: true), SynchronizationContext.Current);
			}

			return new DisposableAction(() =>
			{
				lock (this)
				{
					TakeThreadOwnership(previousOwnerThread, previousSyncContext);
				}
			});
		}
#endif
		#endregion

		public IDisposable ForcefullyBorrowThreadOwnership()
		{
			if (!IsOwner)
			{
				var previousOwnerThread = OwnerThread;

				lock (this)
				{
					OwnerThread = new ThreadLog(true);
				}

				return new DisposableAction(() =>
				{
					lock (this)
					{
						OwnerThread = previousOwnerThread;
					}
				});
			}
			else
			{
				return null;
			}
		}
	}

	public class ThreadSentryCallbackData<DataType1, DataType2> : ThreadSentryCallbackData<DataType1>
	{
		public DataType2 Data2
		{
			get;
			set;
		}

		public ThreadSentryCallbackData(DataType1 data1, DataType2 data2)
			: base(data1)
		{
			Data2 = data2;
		}

		public ThreadSentryCallbackData(DataType1 data1)
			: base(data1)
		{
		}
	}

	public class ThreadSentryCallbackData<DataType1>
	{
		public DataType1 Data1
		{
			get;
			set;
		}

		public ThreadSentryCallbackData(DataType1 data1)
		{
			Data1 = data1;
		}
	}
}
