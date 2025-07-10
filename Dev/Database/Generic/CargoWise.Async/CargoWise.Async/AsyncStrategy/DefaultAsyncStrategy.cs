using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;

namespace CargoWise.Async
{
	public class DefaultAsyncStrategy : IAsyncStrategy
	{
		#region Construction

		public static IAsyncStrategy Get()
		{
			return new DefaultAsyncStrategy();
		}

		protected DefaultAsyncStrategy()
		{
		}

		#endregion

		#region IAsyncStrategy Members

		Task IAsyncStrategy.DoAsync(Action action, IThreadSentry threadSentry, string threadName)
		{
			var actionThreadSentry = PrepareForExecute(action, threadSentry, threadName);
			var calledId = Thread.CurrentThread.ManagedThreadId;
			var actionToRun = CreateThreadWrapper(action, threadName, actionThreadSentry, calledId);

			return TaskCreated(RunAsync(actionToRun, threadName));
		}

		Action<TaskCompletionSource<object>> CreateThreadWrapper(Action action, string threadName, ThreadHandoverToken actionThreadSentry, int calledId)
		{
			return new Action<TaskCompletionSource<object>>(source => RunAndHandleExceptions(source, () => TakeOwnershipAndExecute(action, actionThreadSentry, threadName, callerThreadID: calledId)));
		}

		Thread IAsyncStrategy.DoAsyncAsThread(Action action, ApartmentState apartmentState, IThreadSentry threadSentry, string threadName)
		{
			var actionThreadSentry = PrepareForExecute(action, threadSentry, threadName);

			var callerId = Thread.CurrentThread.ManagedThreadId;
			var actionToRun = CreateThreadWrapper(action, threadName, actionThreadSentry, callerId);
			return RunAsThread(actionToRun, apartmentState);
		}

		Task<T> IAsyncStrategy.GetAsync<T>(Func<T> func, IThreadSentry threadSentry, string threadName, CancellationTokenSource cancellationTokenSource)
		{
			if (string.IsNullOrEmpty(threadName))
			{
				throw new InvalidOperationException();
			}

			var threadHandoverToken = PrepareForExecuteGetAsync(func, threadSentry, threadName);

			var cancellationToken = cancellationTokenSource != null ? cancellationTokenSource.Token : CancellationToken.None;
			var callerThreadId = Thread.CurrentThread.ManagedThreadId;

			var task = AsyncHelper.RunTask(() =>
			{
				SetThreadName(threadName);
				threadHandoverToken.Sentry.TakeThreadOwnership();
				using (OnBackgroundThreadStarting(threadHandoverToken, callerThreadId))
				{
					return AddExceptionHandlingContinuation(func);
				}
			}, cancellationToken, threadName);
			return TaskCreated(task);
		}

		void IAsyncStrategy.ParallelForEach<T>(IEnumerable<T> source, Action<T> body)
		{
			var sentry = new ThreadSentry(reportingEnabled: false);

			var dynamicHandover = OnPreparingToStartBackgroundThread();

			Parallel.ForEach(source, item =>
			{
				var token = CreateToken(sentry, dynamicHandover);
				using (OnBackgroundThreadStarting(token, null))
				{
					body(item);
				}
			});
		}

		#endregion

		#region Implementation

		#region Preparation

		ThreadHandoverToken PrepareForExecute(Delegate codeToRun, IThreadSentry threadSentry, string threadName)
		{
			Argument.NotNull(codeToRun, nameof(codeToRun));
			if (string.IsNullOrEmpty(threadName))
			{
				throw new InvalidOperationException();
			}

			if (threadSentry == null)
			{
				threadSentry = new ThreadSentry(reportingEnabled: false);
			}

			threadSentry.RelinquishThreadOwnership();

			return CreateToken(threadSentry, OnPreparingToStartBackgroundThread());
		}

		protected virtual ThreadHandoverToken PrepareForExecuteGetAsync(Delegate codeToRun, IThreadSentry threadSentry, string threadName)
		{
			Argument.NotNull(codeToRun, nameof(codeToRun));
			if (string.IsNullOrEmpty(threadName))
			{
				throw new InvalidOperationException();
			}

			return PrepareForExecute(codeToRun, threadSentry, threadName);
		}

		protected virtual object OnPreparingToStartBackgroundThread()
		{
			return null;
		}

		protected virtual ThreadHandoverToken CreateToken(IThreadSentry sentry, object dynamicHandover)
		{
			Argument.NotNull(sentry, nameof(sentry));

			return new ThreadHandoverToken(
				sentry: sentry,
				dynamicHandover: dynamicHandover);
		}

		#endregion

		#region Execution

		Task RunAsync(Action<TaskCompletionSource<object>> action, string threadName)
		{
			return AsyncHelper.RunTask(() => action(null), threadName);
		}

		static Thread RunAsThread(Action<TaskCompletionSource<object>> action, ApartmentState apartmentState = ApartmentState.MTA)
		{
			var taskCompletionSource = new TaskCompletionSource<object>();
			var thread = new Thread(() =>
			{
				try
				{
					action(taskCompletionSource);
				}
				catch (Exception ex)
				{
					taskCompletionSource.SetException(ex);
					throw;
				}
			});
			if (apartmentState == ApartmentState.STA)
			{
				thread.TrySetApartmentState(ApartmentState.STA);
				activeSTAThreads.TryAdd(thread);
			}
			thread.IsBackground = true;
			thread.Start();
			return thread;
		}

		static readonly ConcurrentHashSet<Thread> activeSTAThreads = new ConcurrentHashSet<Thread>();

		public static Thread[] GetActiveSTAThreads()
		{
			return activeSTAThreads.ToArray();
		}

		protected virtual void TakeOwnershipAndExecute(Action action, ThreadHandoverToken threadTokenHandover, string threadName, int callerThreadID)
		{
			Argument.NotNull(threadTokenHandover, nameof(threadTokenHandover)); // Suggested By ReviewBot 
			Argument.NotNull(threadTokenHandover.Sentry, nameof(threadTokenHandover.Sentry)); // Suggested By ReviewBot 
			Argument.NotNull(action, nameof(action)); // Suggested By ReviewBot 
			if (string.IsNullOrEmpty(threadName))
			{
				throw new InvalidOperationException();
			}

			SetThreadName(threadName);

			threadTokenHandover.Sentry.TakeThreadOwnership();
			using (OnBackgroundThreadStarting(threadTokenHandover, callerThreadID))
			{
				action();
			}
		}

		protected virtual IDisposable OnBackgroundThreadStarting(ThreadHandoverToken token, int? callerThreadID)
		{
			Argument.NotNull(token, nameof(token)); // Suggested By ReviewBot 
			Argument.NotNull(token.Sentry, nameof(token.Sentry)); // Suggested By ReviewBot 

			return new DisposableAction(() =>
			{
				token.Sentry.RelinquishThreadOwnership();

				if (callerThreadID != null)
				{
					ReturnOwnershipToCallerThread(token.Sentry, callerThreadID.Value);
				}
			});
		}

		static void ReturnOwnershipToCallerThread(IThreadSentry threadSentry, int callerThreadID)
		{
			Argument.NotNull(threadSentry, nameof(threadSentry)); // Suggested By ReviewBot 
			ThreadsInvoke.ActionByThread(callerThreadID, () => threadSentry.TakeThreadOwnership());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Reporting on a different thread")]
		static T AddExceptionHandlingContinuation<T>(Func<T> func)
		{
			Argument.NotNull(func, nameof(func)); // Suggested By ReviewBot 
			var result = default(T);
			try
			{
				result = func();
			}
			catch (ObjectDisposedException)
			{
				throw;
			}
			catch (OperationCanceledException)
			{
				throw; // Nom nom nom. Throwing expected cancellation exceptions on the main thread is rude.
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ex.ThrowExceptionOnMainThread();
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Reporting on a different thread")]
		static void RunAndHandleExceptions(TaskCompletionSource<object> taskCompletionSource, Action codeToRun)
		{
			Argument.NotNull(codeToRun, nameof(codeToRun)); // Suggested By ReviewBot 
			try
			{
				if (ApplicationDispatcher.Current == SynchronizationContext.Current)
				{
					ErrorReporter.ReportOnce("Somehow we managed to get the main thread?");
				}
				codeToRun();
				if (taskCompletionSource != null)
				{
					taskCompletionSource.SetResult(null);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (taskCompletionSource != null)
				{
					taskCompletionSource.SetException(ex);
				}
				ex.ThrowExceptionOnMainThread();
			}
			finally
			{
				if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
				{
					activeSTAThreads.TryRemove(Thread.CurrentThread);
				}
			}
		}

		static void SetThreadName(string threadName)
		{
			try
			{
				if (!string.IsNullOrEmpty(threadName))
				{
					Thread.CurrentThread.Name = threadName;
				}
			}
			catch (InvalidOperationException)
			{
			}
		}

		T TaskCreated<T>(T task)
			where T : Task
		{
			Argument.NotNull(task, nameof(task));

			OnTaskCreated(task);
			return task;
		}

		protected virtual void OnTaskCreated(Task task)
		{
			// Do nuthin.
		}

		#endregion

		#endregion

		#region GetAutoRefresher

		IAutoRefresher IAsyncStrategy.GetAutoRefresher(AutoRefreshAction task, TimeSpan delay, Func<bool> shouldRefresh)
		{
			return GetAutoRefresherCore(task, delay, shouldRefresh);
		}

		protected virtual IAutoRefresher GetAutoRefresherCore(AutoRefreshAction task, TimeSpan delay, Func<bool> shouldRefresh)
		{
			Argument.NotNull(task, nameof(task));
			return new AutoRefresher(task, delay, shouldRefresh);
		}

		#endregion

		protected class ThreadHandoverToken
		{
			public ThreadHandoverToken(IThreadSentry sentry, object dynamicHandover)
			{
				Argument.NotNull(sentry, nameof(sentry));
				Sentry = sentry;
				DynamicHandover = dynamicHandover;
			}

			public IThreadSentry Sentry { get; }
			public object DynamicHandover { get; }
		}
	}
}
