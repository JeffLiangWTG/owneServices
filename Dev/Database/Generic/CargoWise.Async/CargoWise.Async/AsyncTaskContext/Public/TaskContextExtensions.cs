using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using CargoWise.Async.AsyncTaskContext.Internal;

namespace CargoWise.Async.AsyncTaskContext.Public
{
	public static class TaskContextExtensions
	{
		/// <summary>
		/// Obtain an ITaskRunner for a specific context
		/// </summary>
		/// <param name="context">Context that tasks will be executed within</param>
		/// <returns>A new ITaskRunner</returns>
		public static ITaskRunner GetTaskRunnerForContext(this ITaskContext context)
		{
			var internalContext = (ITaskContextInternal)context;
			return new TaskRunner(internalContext, internalContext.CausalityTracking);
		}

		/// <summary>
		/// Obtain an IDeferredExecutor for a specific context
		/// </summary>
		/// <remarks>
		/// Typical usage is when breaking out of the current stack context is required (i.e. to avoid method rentrancy state corruption issues).
		/// It should be avoided in general as control flow becomes confusing and exceptions cannot be handled.
		/// </remarks>
		/// <param name="context">Context that tasks will be executed within</param>
		/// <returns>A new IDeferredExecutor</returns>
		public static IDeferredExecutor GetDeferredExecutorForContext(this ITaskContext context)
		{
			var internalContext = (ITaskContextInternal)context;
			return new DeferredExecutor(internalContext, internalContext.CausalityTracking);
		}

		/// <summary>
		/// Enqueue a functor that returns a value
		/// </summary>
		/// <typeparam name="T">Type of returned value</typeparam>
		/// <param name="context">Context that tasks will be executed within</param>
		/// <param name="task">Function to execute in the associated ITaskContext</param>
		/// <returns>An awaitable task</returns>
		/// <example>
		///		int result = await TaskContext.Current.EnqueueTask( () => 42 );
		/// </example>
		public static Task<T> EnqueueTask<T>(this ITaskContext context, Func<T> task)
		{
			var internalContext = (ITaskContextInternal)context;
			return new TaskRunner(internalContext, internalContext.CausalityTracking).EnqueueTask(task);
		}

		/// <summary>
		/// Enqueue an async functor that returns a value
		/// </summary>
		/// <typeparam name="T">Type of returned value</typeparam>
		/// <param name="context">Context that tasks will be executed within</param>
		/// <param name="task">async function to execute in the associated ITaskContext</param>
		/// <returns>An awaitable task</returns>
		/// <example>
		///		int result = await TaskContext.Current.EnqueueTask( async () => await Get42() );
		/// </example>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static Task<T> EnqueueTask<T>(this ITaskContext context, Func<Task<T>> task)
		{
			var internalContext = (ITaskContextInternal)context;
			return new TaskRunner(internalContext, internalContext.CausalityTracking).EnqueueTask(task);
		}

		/// <summary>
		/// Enqueue a functor that has no return value
		/// </summary>
		/// <param name="context">Context that tasks will be executed within</param>
		/// <param name="task">function to execute in the associated ITaskContext</param>
		/// <returns>An awaitable task</returns>
		/// <example>
		///		await TaskContext.Current.EnqueueTask( () => DoWork() );
		/// </example>
		public static Task EnqueueTask(this ITaskContext context, Action task)
		{
			var internalContext = (ITaskContextInternal)context;
			return new TaskRunner(internalContext, internalContext.CausalityTracking).EnqueueTask(task);
		}

		/// <summary>
		/// Enqueue an async functor that has no return value
		/// </summary>
		/// <param name="context">Context that tasks will be executed within</param>
		/// <param name="task">async function to execute in the associated ITaskContext</param>
		/// <returns>An awaitable task</returns>
		/// <example>
		///		await TaskContext.Current.EnqueueTask( async () => await DoWork() );
		/// </example>
		public static Task EnqueueTask(this ITaskContext context, Func<Task> task)
		{
			var internalContext = (ITaskContextInternal)context;
			return new TaskRunner(internalContext, internalContext.CausalityTracking).EnqueueTask(task);
		}

		/// <summary>
		/// Fire and forget style task enquerer. Guarenteed to execute asynchronously.
		/// </summary>
		/// <remarks>
		/// Typical usage is when breaking out of the current stack context is required (i.e. to avoid method rentrancy state corruption issues).
		/// It should be avoided in general as control flow becomes confusing and exceptions cannot be handled.
		/// </remarks>
		/// <param name="context">Context that tasks will be executed within</param>
		/// <param name="task">function to execute in the associated ITaskContext</param>
		/// <example>
		///		otherTaskContext.EnqueueDeferredTask( () => DoWork() );
		/// </example>
		public static void EnqueueDeferredTask(this ITaskContext context, Action task)
		{
			var internalContext = (ITaskContextInternal)context;
			new DeferredExecutor(internalContext, internalContext.CausalityTracking).EnqueueTask(task);
		}
	}
}
