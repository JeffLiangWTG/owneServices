using System;
using CargoWise.Common;

namespace CargoWise.Windows.UI
{
	public class ProxyWindowsTimer : IWindowsTimer
	{
		// Summary:
		//     Initializes a new instance of the System.Windows.Forms.Timer class.
		public ProxyWindowsTimer()
		{
			timer = new WinFormTimerWrapper();
		}

#if DEBUG
		protected
#endif
		readonly WinFormTimerWrapper timer;

		#region IWindowsTimer Members

		int IWindowsTimer.Interval
		{
			get { return timer.Interval; }
			set { timer.Interval = value; }
		}

		void IWindowsTimer.Start()
		{
			timer.Start();
		}

		void IWindowsTimer.Stop()
		{
			timer.Stop();
		}

		event EventHandler IWindowsTimer.Tick
		{
			add { timer.Tick += value; }
			remove { timer.Tick -= value; }
		}

		bool IWindowsTimer.Enabled
		{
			get { return timer.Enabled; }
			set { timer.Enabled = value; }
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				timer.Dispose();
			}
		}

		#endregion
	}
}
