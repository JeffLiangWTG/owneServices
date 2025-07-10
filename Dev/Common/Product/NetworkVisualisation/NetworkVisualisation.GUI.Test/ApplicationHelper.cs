using System;
using System.Windows.Threading;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	public static class ApplicationHelper
	{
		public static void DoEvents()
		{
			var frame = new DispatcherFrame();
			Action doExit = () => frame.Continue = false;
			Dispatcher.CurrentDispatcher.BeginInvoke(doExit, DispatcherPriority.ApplicationIdle);
			Dispatcher.PushFrame(frame);
		}
	}
}
