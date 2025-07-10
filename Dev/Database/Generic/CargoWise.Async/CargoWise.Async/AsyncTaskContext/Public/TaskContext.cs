using System;
using System.Threading;
using CargoWise.Async.AsyncTaskContext.Internal;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Async.AsyncTaskContext.Public
{
	public static class TaskContext
	{
		[ThreadSafe]
		static readonly ThreadLocal<ITaskContext> currentTaskContext = new ThreadLocal<ITaskContext>();

		/// <summary>
		/// Gets or sets the ITaskContext for the current thread.
		/// </summary>
		public static ITaskContext Current
		{
			get
			{
				if (currentTaskContext.Value == null)
				{
					currentTaskContext.Value = CreatePumpedTaskContext();
				}

				return currentTaskContext.Value;
			}

			set
			{
				if (value is BackgroundThreadTaskManager)
				{
					throw new InvalidOperationException("Cannot assign a ThreadTaskContext to an existing thread");
				}

				currentTaskContext.Value = value;
			}
		}

		/// <summary>
		/// Create a pumped ITaskContext for GUI thread use
		/// </summary>
		/// <param name="exceptionHandler">A functor that will handle unhandled exceptions thrown by tasks enqueued in this context</param>
		/// <param name="enableCausalityTracking">A functor that will return true if causality tracking is to be enabled for tasks in this context</param>
		/// <returns>A new IPumpedTaskContext</returns>
		public static IPumpedTaskContext CreatePumpedTaskContext(Action<Exception> exceptionHandler = null, Func<bool> enableCausalityTracking = null)
		{
			if (exceptionHandler == null)
			{
				exceptionHandler = ex => ex.ThrowExceptionOnMainThread();
			}

			if (enableCausalityTracking == null)
			{
				enableCausalityTracking = () => true;
			}

			return new PumpedTaskManager(exceptionHandler, enableCausalityTracking);
		}

		/// <summary>
		/// Create a thread associated ITaskContext
		/// </summary>
		/// <param name="exceptionHandler">A functor that will handle unhandled exceptions thrown by tasks enqueued in this context</param>
		/// <param name="enableCausalityTracking">A functor that will return true if causality tracking is to be enabled for tasks in this context</param>
		/// <returns>A new ITaskContext</returns>
		public static ITaskContext CreateThreadTaskContext(Action<Exception> exceptionHandler = null, Func<bool> enableCausalityTracking = null)
		{
			if (exceptionHandler == null)
			{
				exceptionHandler = ex => ex.ThrowExceptionOnMainThread();
			}

			if (enableCausalityTracking == null)
			{
				enableCausalityTracking = () => true;
			}

			return new BackgroundThreadTaskManager(() => { }, () => { }, exceptionHandler, enableCausalityTracking);
		}

		/// <summary>
		/// Create a thread associated ITaskContext
		/// </summary>
		/// <param name="setup">Functor to run in thread context before task processing begins</param>
		/// <param name="teardown">Functor to run in thread context after task procesing is finished</param>
		/// <param name="exceptionHandler">A functor that will handle unhandled exceptions thrown by tasks enqueued in this context</param>
		/// <param name="enableCausalityTracking">A functor that will return true if causality tracking is to be enabled for tasks in this context</param>
		/// <returns>A new ITaskContext</returns>
		public static ITaskContext CreateThreadTaskContext(Action setup, Action teardown, Action<Exception> exceptionHandler = null, Func<bool> enableCausalityTracking = null)
		{
			if (exceptionHandler == null)
			{
				exceptionHandler = ex => ex.ThrowExceptionOnMainThread();
			}

			if (enableCausalityTracking == null)
			{
				enableCausalityTracking = () => true;
			}

			return new BackgroundThreadTaskManager(setup, teardown, exceptionHandler, enableCausalityTracking);
		}

		internal static void SetTaskContext(ITaskContext context)
		{
			currentTaskContext.Value = context;
		}
	}
}
