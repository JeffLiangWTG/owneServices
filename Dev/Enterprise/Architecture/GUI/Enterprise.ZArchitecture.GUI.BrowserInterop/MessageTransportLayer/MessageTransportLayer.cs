using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Enterprise.ZArchitecture.GUI.BrowserInterop
{
	class MessageTransportLayer : IMessageTransportLayer
	{
		// JToken supports string types as well as json object types
		readonly ISendMessageToBrowser parent;
		public readonly List<(string kind, Action<BrowserMessageEventArgs<JToken>> handler)> handlers = new List<(string, Action<BrowserMessageEventArgs<JToken>>)>();

		public MessageTransportLayer(ISendMessageToBrowser parent)
		{
			this.parent = parent;
		}

		#region IMessageTransportLayer

		Action<BrowserMessageEventArgs<JToken>> IMessageTransportLayer.DefaultHandler { get; set; }

		async Task IMessageTransportLayer.SendToBrowserAsync<DataType>(string kind, DataType payload)
		{
			await parent.SendToBrowserAsync(SerializeMessage(kind, payload)).ConfigureAwait(false);
		}

		void IMessageTransportLayer.AddCommandHandler<DataType>(string kind, Action<BrowserMessageEventArgs<DataType>> handler)
		{
			Action<BrowserMessageEventArgs<JToken>> clientLinkHandler = (clientLinkMessage) =>
			{
				var browserMessageEvent = new BrowserMessageEventArgs<DataType>
				{
					Kind = kind,
					Payload = clientLinkMessage.Payload?.ToObject<DataType>()
				};
				handler(browserMessageEvent);
			};
			handlers.Add((kind, clientLinkHandler));
		}

		void IMessageTransportLayer.HandleFromBrowser(string message)
		{
			var messageObject = JsonConvert.DeserializeObject<BrowserMessageEventArgs<JToken>>(message);
			HandleFromBrowser(messageObject);
		}

		#endregion

		void HandleFromBrowser(BrowserMessageEventArgs<JToken> message)
		{
			var handlersToUse = handlers.Where(t => t.kind.Equals(message.Kind));
			foreach (var handlerTuple in handlersToUse)
			{
				handlerTuple.handler(message);
			}

			if (!handlersToUse.Any())
			{
				(this as IMessageTransportLayer).DefaultHandler?.Invoke(message);
			}
		}

		static string SerializeMessage<DataType>(string kind, DataType payload = null) where DataType : class
		{
			var message = new BrowserMessageEventArgs<DataType>
			{
				Kind = kind,
				Payload = payload,
			};
			return JsonConvert.SerializeObject(message);
		}
	}
}
