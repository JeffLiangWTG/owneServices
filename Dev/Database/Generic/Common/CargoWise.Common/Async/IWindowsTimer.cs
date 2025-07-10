using System;

namespace CargoWise.Common
{
	public interface IWindowsTimer : IDisposable
	{
		// Summary:
		//     Gets or sets the time, in milliseconds, before the System.Windows.Forms.Timer.Tick
		//     event is raised relative to the last occurrence of the System.Windows.Forms.Timer.Tick
		//     event.
		//
		// Returns:
		//     An System.Int32 specifying the number of milliseconds before the System.Windows.Forms.Timer.Tick
		//     event is raised relative to the last occurrence of the System.Windows.Forms.Timer.Tick
		//     event. The value cannot be less than one.
		int Interval { get; set; }

		// Summary:

		//     Occurs when the specified timer interval has elapsed and the timer is enabled.
		event EventHandler Tick;

		// Summary:

		//     Starts the timer.
		void Start();

		// Summary:

		//     Stops the timer.
		void Stop();

		bool Enabled { get; set; }
	}
}
