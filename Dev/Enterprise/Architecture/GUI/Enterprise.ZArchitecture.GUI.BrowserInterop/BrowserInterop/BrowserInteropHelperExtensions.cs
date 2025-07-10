using System;

namespace Enterprise.ZArchitecture.GUI.BrowserInterop
{
	public static class BrowserInteropHelperExtensions
	{
		public static void AddBrowserCloseCommandHandler(this IBrowserInteropWindow browserWindow, Action<string> onResult)
		{
			browserWindow.MessageTransportLayer.AddCommandHandler<string>(WebViewCommands.SelectRow, browserEventArgs =>
			{
				onResult(browserEventArgs.Payload);
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Correctness", "WTG2001:Do not use ConfigureAwait from an async void method.", Justification = "Not related to main thread, on the other hand, callstack can be catched in ClientLinkWindowLauncher")]
		public static void AddBrowserListenerCommandHandler(this IBrowserInteropWindow browserWindow, Func<IUpdateFilterData> getUpdateFilterData)
		{
			browserWindow.MessageTransportLayer.AddCommandHandler<string>(WebViewCommands.Ready, async browserEventArgs =>
			{
				await browserWindow.MessageTransportLayer
					.SendToBrowserAsync(WebViewCommands.SetFilters, getUpdateFilterData()).ConfigureAwait(false);
			});
		}

		public static void AddPeerDisconnectedCommandHandler(this IBrowserInteropWindow browserWindow, Action handler)
		{
			browserWindow.MessageTransportLayer.AddCommandHandler<string>(WebViewCommands.PeerDisconnected, e =>
			{
				handler();
			});
		}

		public static void AddBrowserReadyCommandHandler(this IBrowserInteropWindow browserWindow, Action handler)
		{
			browserWindow.MessageTransportLayer.AddCommandHandler<string>(WebViewCommands.Ready, e =>
			{
				handler();
			});
		}
	}
}
