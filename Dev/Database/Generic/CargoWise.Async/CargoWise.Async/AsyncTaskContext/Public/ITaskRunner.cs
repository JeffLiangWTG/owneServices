using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace CargoWise.Async.AsyncTaskContext.Public
{
	/// <summary>
	/// Enable running of tasks within a ITaskContext 
	/// </summary>
	public interface ITaskRunner
	{
		/// <summary>
		/// Enqueue a functor that returns a value
		/// </summary>
		/// <typeparam name="T">Type of returned value</typeparam>
		/// <param name="task">Function to execute in the associated ITaskContext</param>
		/// <returns>An awaitable task</returns>
		/// <example>
		///		int result = await runner.EnqueueTask( () => 42 );
		/// </example>
		Task<T> EnqueueTask<T>(Func<T> task);

		/// <summary>
		/// Enqueue an async functor that returns a value
		/// </summary>
		/// <typeparam name="T">Type of returned value</typeparam>
		/// <param name="task">async function to execute in the associated ITaskContext</param>
		/// <returns>An awaitable task</returns>
		/// <example>
		///		int result = await runner.EnqueueTask( async () => await Get42() );
		/// </example>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		Task<T> EnqueueTask<T>(Func<Task<T>> task);

		/// <summary>
		/// Enqueue a functor that has no return value
		/// </summary>
		/// <param name="task">function to execute in the associated ITaskContext</param>
		/// <returns>An awaitable task</returns>
		/// <example>
		///		await runner.EnqueueTask( () => DoWork() );
		/// </example>
		Task EnqueueTask(Action task);

		/// <summary>
		/// Enqueue an async functor that has no return value
		/// </summary>
		/// <param name="task">async function to execute in the associated ITaskContext</param>
		/// <returns>An awaitable task</returns>
		/// <example>
		///		await runner.EnqueueTask( async () => await DoWork() );
		/// </example>
		Task EnqueueTask(Func<Task> task);
	}

	/// <summary>
	/// Fire and forget style task enquerer. Guaranteed to execute asynchronously.
	/// </summary>
	/// <remarks>
	/// Typical usage is when breaking out of the current stack context is required (i.e. to avoid method rentrancy state corruption issues).
	/// It should be avoided in general as control flow becomes confusing and exceptions cannot be handled.
	/// </remarks>
	public interface IDeferredExecutor
	{
		void EnqueueTask(Action runTask);
	}
}
