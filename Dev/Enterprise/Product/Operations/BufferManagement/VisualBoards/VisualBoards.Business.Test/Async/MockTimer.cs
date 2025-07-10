using System;
using CargoWise.Common;

namespace Enterprise.VisualBoards.Business.Test
{
	public class MockTimer : IWindowsTimer
	{
		public int Interval
		{
			get { return interval; }
			set
			{
				SetIntervalCore(value);
			}
		}

		int interval;

		protected virtual void SetIntervalCore(int value)
		{
			interval = value;
		}

		public void FireTicker()
		{
			Ticker.Invoke(this, EventArgs.Empty);
		}

		event EventHandler Ticker;

		event EventHandler IWindowsTimer.Tick
		{
			add { Ticker += value; }
			remove { Ticker -= value; }
		}

		void IDisposable.Dispose()
		{
			// Do nothing
		}

		void IWindowsTimer.Start()
		{
			StartCore();
		}

		void IWindowsTimer.Stop()
		{
			// Do nothing
		}

		bool IWindowsTimer.Enabled
		{
			get => throw new NotImplementedException();
			set => throw new NotImplementedException();
		}

		protected virtual void StartCore()
		{
			// Do nothing
		}
	}
}
