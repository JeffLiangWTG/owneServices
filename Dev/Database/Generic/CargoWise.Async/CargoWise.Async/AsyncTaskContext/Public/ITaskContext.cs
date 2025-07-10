using System;
using System.Threading;

namespace CargoWise.Async.AsyncTaskContext.Public
{
	/// <summary>
	/// Represents a context in which tasks will be executed. 
	/// </summary>
	/// <remarks>
	/// Typically this will be a thread context.
	/// </remarks>
	public interface ITaskContext : IDisposable
	{
		string ContextName { get; }
	}

	/// <summary>
	/// Represents a context in which tasks will be executed that is manually pumped. 
	/// </summary>
	/// <remarks>
	/// Typically this will be a windows GUI thread and pumped inside the message pump.
	/// </remarks>
	public interface IPumpedTaskContext : ITaskContext
	{
		EventWaitHandle PumpingRequired { get; }
		void PumpTasks();
	}
}
