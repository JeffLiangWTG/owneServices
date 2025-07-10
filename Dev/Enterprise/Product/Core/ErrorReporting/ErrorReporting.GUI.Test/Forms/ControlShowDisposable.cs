using System;
using System.Windows.Forms;

namespace Enterprise.ErrorReporting.GUI.Test
{
	sealed class ControlShowDisposable : IDisposable
	{
		public ControlShowDisposable(Control control)
		{
			this.control = control;
			control.Show();
		}

		readonly Control control;

		public void Dispose()
		{
			control.Hide();
		}
	}
}
