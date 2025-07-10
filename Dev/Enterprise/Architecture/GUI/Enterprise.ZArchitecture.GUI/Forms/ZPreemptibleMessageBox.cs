using System;
using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	class ZPreemptibleMessageBox : ZMessageBox
	{
		readonly Func<bool> keepOpen;
		readonly Timer timer;

		public ZPreemptibleMessageBox(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, Func<bool> keepOpen, TimeSpan interval)
			: base(message, caption, buttons, icon, defaultButton)
		{
			this.keepOpen = keepOpen;

			timer = new Timer();
			timer.Interval = (int)interval.TotalMilliseconds;
			timer.Tick += TimerElapsed;

			Shown += (s, e) =>
			{
				timer.Start();
			};
		}

		void TimerElapsed(object sender, EventArgs e)
		{
			if (!keepOpen())
			{
				Close();
				timer.Stop();
			}
		}
	}
}
