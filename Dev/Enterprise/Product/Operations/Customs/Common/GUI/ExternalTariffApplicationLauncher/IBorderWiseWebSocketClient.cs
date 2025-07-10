using System;
using System.Threading.Tasks;
using CargoWise.Types;
using Enterprise.Customs.Common.GUI.ExternalTariffApplicationLauncher;
using Websocket.Client;

namespace Enterprise.Customs.Common.GUI
{
	public interface IBorderWiseWebSocketClient
	{
		void EnsureWebSocketConnection();

		void DisposeWebSocketClient();

		bool SendMessage(BorderWiseWebSocketMessageStatus status, string data, string mode = "Single", string borderWiseWebSocketMessageType = null, Guid? clientId = null, int maxConnectionWaitRetries = 10, int connectionWaitRetryDelayMs = 100);

		void SendMessageAndDisposeWebSocketClient(BorderWiseWebSocketMessageStatus borderWiseWebSocketMessageStatus, string data, bool isDispose = true, string mode = "Single", bool sendMessage = true);

		void ClearWebSocketDataReceivedEvent();

		IWebsocketClient WebSocketClient { get; set; }

		Task WebSocketClientDisposeDelayTask { get; set; }

		ZGuid WebSocketClientId { get; set; }

		string JobStatusInBW { get; set; }

		BorderWiseTariffExchangeModelV2 BorderWiseTariffExchangeModelV2 { get; set; }

		bool IsMessageReceived { get; set; }

		event WebsocketDataReceivedEventHandler WebSocketDataReceived;
	}
}
