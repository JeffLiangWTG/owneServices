using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.GUI.ExternalTariffApplicationLauncher;
using Websocket.Client;
using Websocket.Client.Models;

namespace Enterprise.Customs.Common.GUI.Testing
{
	public class BorderWiseWebSocketClientTest : TestCaseWithFactory
	{
		readonly ZGuid clientId = ZGuid.NewZGuid();
		readonly string orgCode = "ORG";
		readonly string countryCode = "AU";
		readonly string cwClientId = "CWID";
		readonly int dbNumber = 1;

		WebSocketClientMock webSocketClientMock;

		public void TestSendMessage_ShouldSend_WhenConnected()
		{
			webSocketClientMock.IsRunning = true;
			var client = new BorderWiseWebSocketClient(clientId, orgCode, countryCode, cwClientId, dbNumber)
			{
				WebSocketClient = webSocketClientMock
			};

			client.SendMessage(BorderWiseWebSocketMessageStatus.STA, "data");

			var webSocketExchangeMessage = Utilities.DeserializeFromJson<BorderWiseExchangeMessage>(webSocketClientMock.Messages[0]);
			AssertEquals(webSocketClientMock.Messages.Count, 1);
			AssertEquals(webSocketExchangeMessage.Data, "data");
		}

		public void TestDisposeWebSocketClient_ShouldDisposeAndNullify()
		{
			var client = new BorderWiseWebSocketClient(clientId, orgCode, countryCode, cwClientId, dbNumber)
			{
				WebSocketClient = webSocketClientMock
			};

			client.DisposeWebSocketClient();

			AssertNull(client.WebSocketClient);
			AssertEquals(client.IsMessageReceived, false);
		}

		public void TestSendMessageAndDisposeWebSocketClient_ShouldSendAndDispose()
		{
			webSocketClientMock.IsRunning = true;
			var client = new BorderWiseWebSocketClient(clientId, orgCode, countryCode, cwClientId, dbNumber)
			{
				WebSocketClient = webSocketClientMock
			};

			client.SendMessageAndDisposeWebSocketClient(BorderWiseWebSocketMessageStatus.STA, "data", isDispose: true, sendMessage: true);
			client.WebSocketClientDisposeDelayTask.Wait();

			AssertEquals(webSocketClientMock.Messages.Count, 1);
			AssertNull(client.WebSocketClient);
		}

		public void TestSendMessageAndDisposeWebSocketClient_ShouldDisposeWithoutSend_WhenSendMessageFalse()
		{
			webSocketClientMock.IsRunning = true;
			var client = new BorderWiseWebSocketClient(clientId, orgCode, countryCode, cwClientId, dbNumber)
			{
				WebSocketClient = webSocketClientMock
			};

			client.SendMessageAndDisposeWebSocketClient(BorderWiseWebSocketMessageStatus.STA, "data", isDispose: true, sendMessage: false);

			client.WebSocketClientDisposeDelayTask.Wait();

			AssertEquals(webSocketClientMock.Messages.Count, 0);
			AssertNull(client.WebSocketClient);
		}

		public void TestClearWebSocketDataReceivedEvent_ShouldClearEvent()
		{
			var client = new BorderWiseWebSocketClient(clientId, orgCode, countryCode, cwClientId, dbNumber);
			bool called = false;
			client.WebSocketDataReceived += (s, e) => called = true;

			client.ClearWebSocketDataReceivedEvent();

			// Simulate event
			client.GetType().GetMethod("OnWebSocketDataReceived", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
				.Invoke(client, new object[] { new WebSocketDataReceivedEventArgs(Guid.NewGuid(), new List<BorderWiseInvoiceLine>()) });

			AssertEquals(called, false);
		}

		public void TestEnsureWebSocketConnection_ShouldNotReconnect_WhenAlreadyRunning()
		{
			webSocketClientMock.IsRunning = true;
			var client = new BorderWiseWebSocketClient(clientId, orgCode, countryCode, cwClientId, dbNumber)
			{
				WebSocketClient = webSocketClientMock
			};
			webSocketClientMock.Start();

			client.EnsureWebSocketConnection();

			AssertEquals(webSocketClientMock.StartedCount, 1);
		}

		public void TestEnsureWebSocketConnection_ShouldAttemptConnect_WhenNotRunning()
		{
			webSocketClientMock.IsRunning = false;
			var client = new BorderWiseWebSocketClient(clientId, orgCode, countryCode, cwClientId, dbNumber)
			{
				WebSocketClient = webSocketClientMock
			};

			client.EnsureWebSocketConnection();

			Thread.Sleep(100);

			AssertEquals(webSocketClientMock.StartedCount, 1);
			AssertEquals(webSocketClientMock.IsRunning, true);
		}

		public void TestEnsureWebSocketConnection_ShouldDisposePreviousSubscriptions()
		{
			// Arrange
			var client = new BorderWiseWebSocketClient(
				ZGuid.NewZGuid(),
				"ORG",
				"AU",
				"ClientId",
				1
			)
			{
				WebSocketClient = webSocketClientMock
			};

			// Act
			client.EnsureWebSocketConnection(); // First call to create subscriptions
			client.EnsureWebSocketConnection(); // Second call should dispose the first

			webSocketClientMock.ReconnectionSubject.OnNext(ReconnectionInfo.Create(ReconnectionType.Initial));

			AssertEquals(webSocketClientMock.ReconnectionHappenedCount, 1);
		}

		protected override void SetUp()
		{
			webSocketClientMock = new WebSocketClientMock();
		}

		protected override void TearDown()
		{
			webSocketClientMock?.Dispose();
			webSocketClientMock = null;
		}
	}
}
