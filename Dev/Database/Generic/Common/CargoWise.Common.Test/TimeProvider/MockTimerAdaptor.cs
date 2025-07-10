using System;
using System.Timers;

namespace CargoWise.Common
{
	public class MockTimerAdaptor : ITimerAdaptor
	{
		public MockTimerAdaptor(ITimeProvider timeProvider)
		{
			this.timeProvider = timeProvider;
		}

		public MockTimerAdaptor(double interval, ITimeProvider timeProvider)
		{
			if (interval <= 0)
			{
				throw new ArgumentException("Invalid argument.", nameof(interval));
			}

			Interval = interval;
			this.timeProvider = timeProvider;
		}

		public event ElapsedEventHandler Elapsed;

		public double Interval
		{
			get => interval;
			set
			{
				if (value == 0)
				{
					throw new ArgumentException("'0' is not a valid value for 'Interval'. 'Interval' must be greater than 0.");
				}

				interval = value;
			}
		}

		double interval;

		public void Start()
		{
			Enabled = true;
			UpdateNextTickTime();
		}

		public void Stop()
		{
			Enabled = false;
		}

		public bool Enabled { get; set; }

		public void CheckIfTimerElapsed()
		{
			if (Enabled && timeProvider.GetCurrentLocalMachineDateTime() >= nextTickTime)
			{
				Tick();
			}
		}

		#region IDisposable Support

		protected virtual void Dispose(bool disposing)
		{
			//do nothing
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

		#region Implementation

		readonly ITimeProvider timeProvider;

		protected virtual void OnElapsed(ElapsedEventArgs e)
		{
			Elapsed?.Invoke(this, e);
		}

		DateTime nextTickTime;

		void Tick()
		{
			OnElapsed(null);
			UpdateNextTickTime();
		}

		void UpdateNextTickTime()
		{
			nextTickTime = timeProvider.GetCurrentLocalMachineDateTime().AddMilliseconds(Interval);
		}

		#endregion
	}
}
