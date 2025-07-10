using System;
using System.Timers;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.Core
{
	public class TimerAdaptor : ITimerAdaptor
	{
		public TimerAdaptor()
		{
			timer = new Timer();
		}

		public TimerAdaptor(double interval)
		{
			timer = new Timer(interval);
		}

		public double Interval
		{
			get => timer.Interval;
			set
			{
				timer.Interval = value;
			}
		}

		public event ElapsedEventHandler Elapsed
		{
			add
			{
				timer.Elapsed += value;
			}
			remove
			{
				timer.Elapsed -= value;
			}
		}

		public void Start()
		{
			timer.Start();
		}

		public void Stop()
		{
			timer.Stop();
		}

		public bool Enabled
		{
			get => timer.Enabled;
			set
			{
				timer.Enabled = value;
			}
		}

		readonly Timer timer;

		#region IDisposable Support

		bool disposed;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					timer.Dispose();
				}

				disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion
	}
}
