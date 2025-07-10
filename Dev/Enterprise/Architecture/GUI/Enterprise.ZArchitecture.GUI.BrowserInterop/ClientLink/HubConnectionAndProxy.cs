using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;

namespace Enterprise.ZArchitecture.GUI.BrowserInterop
{
	public class HubConnectionAndProxy : IHubConnectionAndProxy
	{
		HubConnection HubConnection { get; }
		public IHubProxy HubProxy { get; }

		public HubConnectionAndProxy(string glowServiceUri, string clientLinkId)
		{
			HubConnection = new HubConnection(glowServiceUri);
			HubConnection.Headers.Add("ClientLink-Session", clientLinkId);
			HubProxy = HubConnection.CreateHubProxy("ClientLink");
		}

		public event Action Closed
		{
			add => HubConnection.Closed += value;
			remove => HubConnection.Closed -= value;
		}

		public async Task Start()
		{
			await HubConnection.Start();
		}

		void IDisposable.Dispose()
		{
			HubConnection.Dispose();
		}
	}

	public interface IHubConnectionAndProxy : IDisposable
	{
		IHubProxy HubProxy { get; }

		public event Action Closed;

		public Task Start();
	}
}
