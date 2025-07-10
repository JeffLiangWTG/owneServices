using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Async.AsyncTaskContext.Public;

namespace CargoWise.Async.AsyncTaskContext.Internal
{
	sealed class BackgroundThreadTaskManager : ITaskContextInternal
	{
		readonly PumpedTaskManager pumpedTaskManager;
		readonly Thread workerThread;
		bool endProcessing;

		public string ContextName { get; } = "Background Thread Task Manager " + Guid.NewGuid(); // Log information

		public bool CausalityTracking
		{
			get
			{
				return pumpedTaskManager.CausalityTracking;
			}
		}

		public BackgroundThreadTaskManager(Action setup, Action tearDown, Action<Exception> exceptionHandler, Func<bool> enableCausalityTracking)
		{
			pumpedTaskManager = new PumpedTaskManager(exceptionHandler, enableCausalityTracking);

			var threadStartedEvent = new AutoResetEvent(false);
			workerThread = new Thread(() =>
			{
				SynchronizationContext.SetSynchronizationContext(new CustomSynchContext(this));
				TaskContext.SetTaskContext(this);

				threadStartedEvent.Set();
				setup.Invoke();

				while (!endProcessing)
				{
					pumpedTaskManager.PumpingRequired.WaitOne();
					pumpedTaskManager.PumpTasks();
				}

				tearDown.Invoke();
			});
			workerThread.IsBackground = true;
			workerThread.SetApartmentState(ApartmentState.STA);
			workerThread.Start();
			threadStartedEvent.WaitOne();
		}

		public void EnqueueSelfObservingTask(Action runTask)
		{
			pumpedTaskManager.EnqueueSelfObservingTask(runTask);
		}

		public Task EnqueueTask(Action runTask)
		{
			return pumpedTaskManager.EnqueueTask(runTask);
		}

		public Task Stop()
		{
			if (endProcessing)
			{
				return Task.CompletedTask;
			}

			var completion = default(AsyncTaskMethodBuilder);
			var result = completion.Task;

			EnqueueTask(() =>
			{
				endProcessing = true;
				completion.SetResult();
			});

			return result;
		}

		[SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", Justification = "Waiter disposed asynchronously")]
		public void Dispose()
		{
			Stop();
		}

		class CustomSynchContext : SynchronizationContext, IThreadSentryPostingControl
		{
			readonly ITaskContext taskContext;

			public CustomSynchContext(ITaskContext taskContext)
			{
				this.taskContext = taskContext;
			}

			public override SynchronizationContext CreateCopy()
			{
				return new CustomSynchContext(this.taskContext);
			}

			public override void Post(SendOrPostCallback d, object state)
			{
				this.taskContext.EnqueueDeferredTask(() => d(state));
			}

			public override void Send(SendOrPostCallback d, object state)
			{
				this.taskContext.EnqueueTask(() => d(state)).Wait();
			}

			bool IThreadSentryPostingControl.EnablePosting => false;
		}
	}
}
