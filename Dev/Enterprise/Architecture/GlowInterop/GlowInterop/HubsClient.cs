using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;

namespace Enterprise.ZArchitecture.GlowInterop
{
	public interface IHubsClient
	{
		Task StartAsync(Uri baseUri, Guid entityPk, Action<string, string, decimal> logCallback);
	}

	class HubsClient : IHubsClient
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "glow constant")]
		public async Task StartAsync(Uri baseUri, Guid entityPk, Action<string, string, decimal> logCallback)
		{
			try
			{
				var connection = new HubConnection(baseUri.ToString());
				connection.CookieContainer = new CookieContainer();
				var hubProxy = connection.CreateHubProxy("streamlogger");
				hubProxy.On("AppendLog", (Guid key, string type, string message) => logCallback(type, message, default(decimal)));
				hubProxy.On("AppendLogWithProgress", (Guid key, string type, string message, decimal progress) => logCallback(type, message, progress));
				await connection.Start().ConfigureAwait(false);
				await hubProxy.Invoke("start", entityPk).ConfigureAwait(false);
			}
			catch (InvalidOperationException e)
			{
				logCallback("Error", e.Message, 0);
			}
		}
	}
}
