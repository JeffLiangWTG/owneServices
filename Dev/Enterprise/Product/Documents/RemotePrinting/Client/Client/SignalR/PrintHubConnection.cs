using System;
using CargoWise.Common;
using Microsoft.AspNet.SignalR.Client;

namespace Enterprise.RemotePrinting.Client
{
	public class PrintHubConnection : HubConnection
	{
		public PrintHubConnection(string url)
			: base(url)
		{
			// Needed for sticky session cookie from load balancer / HAProxy
			CookieContainer = new System.Net.CookieContainer();
		}

		public event Action<string> Logged;

		void Log(string message)
		{
			Logged?.Invoke(message);
		}

#if DEBUG
		public bool ThrowExceptionForTest;
#endif

		protected override void OnClosed()
		{
			try
			{
#if DEBUG
				if (ThrowExceptionForTest)
				{
					throw new AggregateException(new InvalidOperationException("Connection was disconnected before invocation result was received."));
				}
#endif
				base.OnClosed();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Log($"Error in SignalR when Hub Connection was closed: '{ErrorReporter.GetExceptionMessage(ex)}'");
			}
		}
	}
}
