using System;
using ServiceManager.Common.Abstractions;

namespace ServiceManager.Host.Abstractions
{
	public class ProcessStartedEventArgs : EventArgs
	{
		public ProcessStartedEventArgs(IProcess process)
		{
			Process = process;
		}

		public IProcess Process { get; }
	}
}
