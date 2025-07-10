using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.eHub.Adapter;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.eHubMessaging.ServiceTasks.eAdaptor.eAdaptor
{
	public abstract class RestOutboundAdapterBase : IeHubAdapter
	{
		public RestOutboundAdapterBase(string serverAddress, INotifications notifier, string clientName)
		{
			Argument.NotNullOrEmpty(serverAddress, nameof(serverAddress));
			this.serverAddress = serverAddress;
			this.notifier = notifier;
			this.clientName = clientName;
		}

		readonly string serverAddress;
		protected readonly INotifications notifier;
		protected readonly string clientName;

		public IMessageOutbox Outbox { get; } = new MessageOutbox();

		void IeHubAdapter.SendMessages()
		{
			try
			{
				//Will be refactored in future to support cancellationTokens as IeHubAdapter does not support REST
				var cancellationToken = CancellationToken.None;
				using (var clientHandler = new HttpClientHandler())
				{
					if (eAdaptorRegistry.Instance.IgnoreUnknownSSLCertificate.Value)
					{
						clientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
					}

					using (var client = new HttpClient(clientHandler))
					{
						client.BaseAddress = new Uri(serverAddress);
						client.Timeout = TimeSpan.FromSeconds(eAdaptorRegistry.Instance.OutboundTimeout.Value);
						try
						{
							foreach (var message in Outbox)
							{
								var isCachedAuthHeaderValue = ShouldSetAuthorizationHeader && SetAuthorizationHeader(client, cancellationToken);
								SetCustomHeader(client, message.RecipientID, clientName);
								SendMessage(client, message, isCachedAuthHeaderValue, cancellationToken);
							}
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							throw new eHubAdapterException("Exception thrown when trying to connect to endpoint.", ex);
						}
					}
				}
			}
			catch (AggregateException ex) when (!ex.IsCriticalException())
			{
				throw new eHubAdapterException("Error during processing", ex);
			}
		}

		protected virtual bool ShouldSetAuthorizationHeader => true;

		protected abstract bool SetAuthorizationHeader(HttpClient client, CancellationToken cancellationToken);

		protected abstract bool HandleSendMessageFailure(HttpClient client, IeHubMessage message, bool isCachedAuthHeaderValue, CancellationToken cancellationToken);

		protected void SendMessage(HttpClient client, IeHubMessage message, bool isCachedAuthHeaderValue, CancellationToken cancellationToken)
		{
			using (var ms = new UndisposableSteam(message.MessageStream))
			{
				ms.Position = 0;
				using (var content = new StreamContent(ms))
				{
					content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/xml");
					content.Headers.ContentLength = message.MessageStream.Length;

					var result = client.PostAsync(serverAddress, content).Result;

					if (!result.IsSuccessStatusCode)
					{
						var isHandled = HandleSendMessageFailure(client, message, isCachedAuthHeaderValue, cancellationToken);
						if (!isHandled)
						{
							var contentData = result.Content.ReadAsStringAsync().Result;
							throw new eHubAdapterException(FormattableString.Invariant($"Address did not return success status. Result:\r\n{contentData}"));
						}
					}
				}
				ms.ShouldDispose();
			}
		}
		void SetCustomHeader(HttpClient client, string recipientID, string clientName)
		{
			client.DefaultRequestHeaders.Remove("eAdaptor-RecipientID");
			client.DefaultRequestHeaders.Remove("eAdaptor-EDIClientName");

			if (!string.IsNullOrEmpty(recipientID))
			{
				client.DefaultRequestHeaders.Add("eAdaptor-RecipientID", recipientID);
			}
			if (!string.IsNullOrEmpty(clientName))
			{
				client.DefaultRequestHeaders.Add("eAdaptor-EDIClientName", clientName);
			}
		}

		#region Unsupported cruft

		IMessageInbox IeHubAdapter.Inbox => throw new NotSupportedException();

		void IDisposable.Dispose()
		{
		}

		Dictionary<Guid, OutboundMessageStatus> IeHubAdapter.GetOutboundMessageStatuses(Guid[] trackingIDs)
		{
			throw new NotSupportedException();
		}

		void IeHubAdapter.RetrieveMessages()
		{
			throw new NotSupportedException();
		}

		#endregion
	}
}
