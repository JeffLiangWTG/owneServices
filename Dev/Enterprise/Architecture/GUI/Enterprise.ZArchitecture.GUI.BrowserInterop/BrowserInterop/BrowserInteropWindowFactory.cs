using System;

namespace Enterprise.ZArchitecture.GUI.BrowserInterop
{
	public class BrowserInteropWindowFactory : IBrowserInteropWindowFactory
	{
		public IBrowserInteropWindow CreateBrowserInteropWindow(string windowName, Uri address)
		{
#if !WINZOR
			return new ClientLinkWindowLauncher(windowName, address);
#else
			return new ClientLinkWinzorLauncher(address);
#endif
		}
	}

	public interface IBrowserInteropWindowFactory
	{
		IBrowserInteropWindow CreateBrowserInteropWindow(string windowName, Uri address);
	}
}
