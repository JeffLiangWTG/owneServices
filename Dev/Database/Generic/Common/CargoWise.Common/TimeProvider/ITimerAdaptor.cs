using System;
using System.Timers;

namespace CargoWise.Common
{
	public interface ITimerAdaptor : IDisposable
	{
		void Start();
		void Stop();
		bool Enabled { get; set; }
		double Interval { get; set; }
		event ElapsedEventHandler Elapsed;
	}
}
