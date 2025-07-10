using System;

namespace ServiceManager.Host.Abstractions
{
	public interface IServiceRunner : IDisposable
	{
		int ProcessId { get; }
		bool TaskRunning { get; }
		bool IsIdle { get; }
		bool IsAllocatingTask { get; set; }
		string TaskGroup { get; }
		TimeSpan ElapsedFromLastRun { get; }
		event EventHandler<ProcessStartedEventArgs> ProcessStarted;
		event EventHandler Exited;
		event EventHandler TaskRunRequestCompleted;

		bool Run(ITaskRunRequest runRequest);
		void Stop();
		void Kill(bool withLogging = true);
		void CheckIdleStatus(IHostLogger logger, TimeSpan idleProcessExpiry);
	}

	public interface IServiceRunnerFactory
	{
		IServiceRunner Create(ITaskScheduler taskScheduler, string taskGroup);
	}
}
