using System;
using System.Windows.Forms;

namespace Enterprise.ErrorReporting.GUI.Test
{
	static class ControlExtensions
	{
		public static IDisposable ShowForTest(this Control control)
		{
			return new ControlShowDisposable(control);
		}
	}
}
