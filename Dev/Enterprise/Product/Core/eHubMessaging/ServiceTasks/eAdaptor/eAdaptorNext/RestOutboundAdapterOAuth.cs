using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.eHub.Adapter;
using Enterprise.eHubMessaging.ServiceTasks.eAdaptor.eAdaptor;
using Enterprise.Integration;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	public class RestOutboundAdapterOAuth : RestOutboundAdapterBase
	{
		public RestOutboundAdapterOAuth(string serverAddress, INotifications notifier, IOAuth2Parameters oAuth2Parameters, string clientName) : base(serverAddress, notifier, clientName)
		{
			this.oAuth2Parameters = oAuth2Parameters;
		}

		readonly IOAuth2Parameters oAuth2Parameters;

		protected override bool HandleSendMessageFailure(HttpClient client, IeHubMessage message, bool isCachedAuthHeaderValue, CancellationToken cancellationToken)
		{
			OAuth2ConnectFactory.ClearTokens(oAuth2Parameters);
			if (isCachedAuthHeaderValue)
			{
				notifier.Add(new Notification(NotificationType.Information, eAdaptorNextLogs.TokenCacheCleared()));
				SetAuthorizationHeader(client, cancellationToken);
				SendMessage(client, message, false, cancellationToken);
				return true;
			}
			return false;
		}

		protected override bool ShouldSetAuthorizationHeader => true;

		protected override bool SetAuthorizationHeader(HttpClient client, CancellationToken cancellationToken)
		{
			try
			{
				var (authToken, isCached) = OAuth2ConnectFactory.GetToken(oAuth2Parameters, notifier, cancellationToken).Result;
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authToken.TokenType.Trim(), authToken.AccessToken);
				return isCached;
			}
			catch (OAuth2Exception ex)
			{
				OAuth2ConnectFactory.ClearTokens(oAuth2Parameters);
				throw new eHubAdapterException("Exception thrown when trying to connect to endpoint.", ex);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				OAuth2ConnectFactory.ClearTokens(oAuth2Parameters);
				throw new eHubAdapterException("Exception thrown when trying to connect to authorization server.", ex);
			}
		}
	}
}
