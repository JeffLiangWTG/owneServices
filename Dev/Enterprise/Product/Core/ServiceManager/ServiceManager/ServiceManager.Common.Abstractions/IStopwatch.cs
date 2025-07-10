using System;

namespace ServiceManager.Common.Abstractions
{
	public interface IStopwatch
	{
		void Start();
		void Stop();
		void Restart();
		bool IsRunning { get; }
		long ElapsedMilliseconds { get; }
		TimeSpan Elapsed { get; }
	}
}
