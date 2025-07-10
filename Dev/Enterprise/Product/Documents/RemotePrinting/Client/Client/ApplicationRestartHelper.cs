using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Enterprise.RemotePrinting.Client;

public static class ApplicationRestartHelper
{
	public const int HWND_BROADCAST = 0xFFFF;

	public static readonly int WM_WPR_RESTART = RegisterWindowMessage("WM_WPR_RESTART");

	[DllImport("user32")]
	public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);

	[DllImport("user32")]
	static extern int RegisterWindowMessage(string message);

	public const int wParam = 0xCDCD;
	public const int lParam = 0xEFEF;

	public static void RestartAllInstances()
	{
		PostMessage(
			(IntPtr)HWND_BROADCAST,
			WM_WPR_RESTART,
			new IntPtr(wParam),
			new IntPtr(lParam)
		);
	}

	public const string AutoConfigRegistryAccess = "Global\\Enterprise.RemotePrinting.Client.51085E45ED7F4C948329B8468C25CF48";

	static readonly object setupLock = new object();

	public static Mutex TryGetAppLock(string mutexKey)
	{
		Mutex mutex;

		lock (setupLock)
		{
			mutex = new Mutex(false, mutexKey);
			try
			{
				if (!mutex.WaitOne(new TimeSpan(10000), false))
				{
					mutex.Dispose();
					mutex = null;
				}
			}
			catch (AbandonedMutexException)
			{
				mutex = null;
			}
		}

		return mutex;
	}
}
