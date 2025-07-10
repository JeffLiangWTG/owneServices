using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.eHub.Adapter;
using Enterprise.eHubMessaging.ServiceTasks.eAdaptor.eAdaptor;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	public sealed class RestOutboundAdapterBasic : RestOutboundAdapterBase
	{
		public RestOutboundAdapterBasic(string serverAddress, INotifications notifier, string username, string password, string clientName) : base(serverAddress, notifier, clientName)
		{
			authHeaderValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
		}

		readonly string authHeaderValue;

		protected override bool HandleSendMessageFailure(HttpClient client, IeHubMessage message, bool isCachedAuthHeaderValue, CancellationToken cancellationToken)
		{
			return false;
		}

		protected override bool ShouldSetAuthorizationHeader => true;

		protected override bool SetAuthorizationHeader(HttpClient client, CancellationToken cancellationToken)
		{
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue((NoResString)"Basic", authHeaderValue);
			return false;
		}
	}
}
