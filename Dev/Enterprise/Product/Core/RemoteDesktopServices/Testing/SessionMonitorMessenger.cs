using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using CargoWise.Interop;

namespace Enterprise.RemoteDesktopServices.Testing
{
	class SessionMonitorMessenger
	{
		readonly Dispatcher sessionMonitorDispatcher;
		public SessionMonitorMessenger(Dispatcher sessionMonitorDispatcher)
		{
			this.sessionMonitorDispatcher = sessionMonitorDispatcher;
		}

		const int WM_WTSSESSION_CHANGE = 0x02B1;
		const int WTS_REMOTE_CONNECT = 0x3; // A session was connected to the remote session. 
		const int WTS_REMOTE_DISCONNECT = 0x4; // A session was disconnected from the remote session. 

		public void DisconnectServer()
		{
			SendSessionMessage(WTS_REMOTE_DISCONNECT);
		}

		public void ReconnectServer()
		{
			SendSessionMessage(WTS_REMOTE_CONNECT);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void SendSessionMessage(int msg)
		{
			try
			{
				sessionMonitorDispatcher.Invoke(() =>
				{
					UnsafeNativeMethods.PostMessage(
						((MockWtsApi)WtsApi.Instance).SessionNotificationHandle,
						WM_WTSSESSION_CHANGE,
						new IntPtr(msg),
						IntPtr.Zero);
					Application.DoEvents();
				}, DispatcherPriority.Send, CancellationToken.None, TimeSpan.FromSeconds(10));
			}
			catch (TimeoutException)
			{
				throw new TaskCanceledException();
			}
		}

		public DispatcherOperation DisconnectServerAsync()
		{
			return PostSessionMessage(WTS_REMOTE_DISCONNECT);
		}

		public DispatcherOperation ReconnectServerAsync()
		{
			return PostSessionMessage(WTS_REMOTE_CONNECT);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		DispatcherOperation PostSessionMessage(int msg)
		{
			return sessionMonitorDispatcher.BeginInvoke(() =>
			{
				UnsafeNativeMethods.PostMessage(
					((MockWtsApi)WtsApi.Instance).SessionNotificationHandle,
					WM_WTSSESSION_CHANGE,
					new IntPtr(msg),
					IntPtr.Zero);
				Application.DoEvents();
			});
		}
	}
}
