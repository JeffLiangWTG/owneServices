using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	public class BackgroundThreadActionQueue : IBackgroundThreadActionQueue
	{
		public BackgroundThreadActionQueue(CancellationToken cancellationToken, IAsyncDelayProvider delayProvider = null)
		{
			threadId = Thread.CurrentThread.ManagedThreadId;
			this.delayProvider = delayProvider ?? new DelayProvider();
			this.cancellationToken = cancellationToken;
			lockingSemaphore = new SemaphoreSlim(1, 1);
		}

		public void SetMainThreadId()
		{
			lockingSemaphore.Wait(cancellationToken);
			try
			{
				threadId = Thread.CurrentThread.ManagedThreadId;
			}
			finally
			{
				lockingSemaphore.Release();
			}
		}

		void CheckThreadId()
		{
			if (Thread.CurrentThread.ManagedThreadId != threadId)
			{
				throw new InvalidOperationException("Should be executed only on the queue's background thread");
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccessRule", Justification = "Ensures each pass is a fixed batch of actions, and not a potentially increasing set")]
		public void InvokeActions()
		{
			CheckThreadId();

			var actionsThisPass = actions.Count;
			for (var i = 0; i < actionsThisPass; i++)
			{
				if (actions.TryDequeue(out var action))
				{
					action();
				}
				else
				{
					break;
				}
			}
		}

		public void InvokeActionsWhileWaiting(TimeSpan upToTimeout, Func<bool> exitCondition)
		{
			CheckThreadId();

			var stopWatch = Stopwatch.StartNew();
			do
			{
				if (actions.TryDequeue(out var action))
				{
					action();
				}
				else
				{
					dispatchingLoopResumeRequest.WaitOne(TimeSpan.FromMilliseconds(10));
				}
			}
			while (stopWatch.Elapsed < upToTimeout && !exitCondition());
		}

		public void Enqueue(Action action)
		{
			if (isDisposed)
			{
				return;
			}

			actions.Enqueue(action);
			Wake();
		}

		public void Enqueue(TimeSpan delaySpan, Action action)
		{
			delayProvider
				.DelayAsync(delaySpan, cancellationToken)
				.ContinueWith(t => Enqueue(action), cancellationToken)
				.ContinueWith(t => { }, TaskContinuationOptions.OnlyOnCanceled);
		}

		public bool WaitForEnqueue(TimeSpan timeout, CancellationToken cancellationToken)
		{
			CheckThreadId();

			try
			{
				return WaitHandle.WaitAny(
					new[] { dispatchingLoopResumeRequest, cancellationToken.WaitHandle },
					timeout) == 0;
			}
			catch (OperationCanceledException)
			{
				return false;
			}
		}

		public void Wake()
		{
			if (isDisposed)
			{
				return;
			}

			dispatchingLoopResumeRequest.Set();
		}

		public IDisposable CreateTimer(Action timerAction, TimeSpan dueTime, TimeSpan period)
		{
			return new BackgroundThreadTimer(this, timerAction, dueTime, period);
		}

		readonly ConcurrentQueue<Action> actions = new ConcurrentQueue<Action>();
		readonly IAsyncDelayProvider delayProvider;
		readonly AutoResetEvent dispatchingLoopResumeRequest = new AutoResetEvent(false);
		int threadId;
		readonly CancellationToken cancellationToken;
		bool isDisposed;
		readonly SemaphoreSlim lockingSemaphore;

		class BackgroundThreadTimer : IDisposable
		{
			public BackgroundThreadTimer(IActionQueue actionQueue, Action timerAction, TimeSpan dueTime, TimeSpan period)
			{
				this.actionQueue = actionQueue;
				this.timerAction = timerAction;
				timer = new Timer(TimerCallback, null, (int)dueTime.TotalMilliseconds, (int)period.TotalMilliseconds);
			}

			void TimerCallback(object stateObj)
			{
				if (alreadyInTheQueue)
				{
					return;
				}

				alreadyInTheQueue = true;
				actionQueue.Enqueue(() =>
				{
					alreadyInTheQueue = false;

					if (!disposed)
					{
						timerAction();
					}
				});
			}

			readonly IActionQueue actionQueue;
			readonly Timer timer;
			readonly Action timerAction;
			bool alreadyInTheQueue;
			bool disposed;

			#region IDisposable

			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			protected virtual void Dispose(bool disposing)
			{
				if (!disposed)
				{
					disposed = true;
					if (disposing)
					{
						timer.Dispose();
					}
				}
			}

			#endregion
		}

		#region IDisposable

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (!isDisposed)
				{
					isDisposed = true;
					dispatchingLoopResumeRequest.Dispose();
				}
			}
		}

		#endregion
	}
}
