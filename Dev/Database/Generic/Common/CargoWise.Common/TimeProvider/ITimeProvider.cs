using System;

namespace CargoWise.Common
{
	public interface ITimeProvider
	{
		DateTime GetCurrentUtcDateTime();
		DateTime GetCurrentLocalMachineDateTime();
		ITimerAdaptor GetTimer();
		ITimerAdaptor GetTimer(double interval);

		void Sleep(int milliseconds);
		void Sleep(TimeSpan timeSpan);
	}
}