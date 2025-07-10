using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Websocket.Client;
using Websocket.Client.Models;

namespace Enterprise.Customs.Common.GUI.Testing
{
	public class WebSocketClientMock : IWebsocketClient
	{
		int reconnectionHappenedCount;
		int startedCount;

		public WebSocketClientMock()
		{
			Messages = new List<string>();
		}

		public int ReconnectionHappenedCount => reconnectionHappenedCount;
		public int StartedCount => startedCount;

		public List<string> Messages { get; set; }

		public Subject<ResponseMessage> MessageReceivedSubject = new Subject<ResponseMessage>();
		public Subject<ReconnectionInfo> ReconnectionSubject = new Subject<ReconnectionInfo>();
		public Subject<DisconnectionInfo> DisconnectedSubject = new Subject<DisconnectionInfo>();
		public Uri Url { get; set; }

		public IObservable<ResponseMessage> MessageReceived => MessageReceivedSubject.AsObservable();

		public IObservable<ReconnectionInfo> ReconnectionHappened =>
			Observable.Create<ReconnectionInfo>(observer =>
			{
				reconnectionHappenedCount++;
				return ReconnectionSubject.Subscribe(observer);
			});

		public IObservable<DisconnectionInfo> DisconnectionHappened => DisconnectedSubject.AsObservable();

		public TimeSpan? ReconnectTimeout { get; set; }
		public TimeSpan? ErrorReconnectTimeout { get; set; }
		public string Name { get; set; }

		public bool IsStarted { get; set; }

		public bool IsRunning { get; set; }

		public bool IsReconnectionEnabled { get; set; }
		public bool IsTextMessageConversionEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public ClientWebSocket NativeClient { get; set; }

		public Encoding MessageEncoding { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public void Dispose()
		{
			IsRunning = false;
			IsStarted = false;

			MessageReceivedSubject.Dispose();
			ReconnectionSubject.Dispose();
			DisconnectedSubject.Dispose();
		}

		public Task Reconnect()
		{
			throw new NotImplementedException();
		}

		public Task ReconnectOrFail()
		{
			throw new NotImplementedException();
		}

		public void Send(string message)
		{
			Messages.Add(message);
		}

		public void Send(byte[] message)
		{
			throw new NotImplementedException();
		}

		public void Send(ArraySegment<byte> message)
		{
			throw new NotImplementedException();
		}

		public Task SendInstant(string message)
		{
			throw new NotImplementedException();
		}

		public Task SendInstant(byte[] message)
		{
			throw new NotImplementedException();
		}

		public Task Start()
		{
			startedCount++;
			IsRunning = true;
			return Task.CompletedTask;
		}

		public Task StartOrFail()
		{
			throw new NotImplementedException();
		}

		public Task<bool> Stop(WebSocketCloseStatus status, string statusDescription)
		{
			throw new NotImplementedException();
		}

		public Task<bool> StopOrFail(WebSocketCloseStatus status, string statusDescription)
		{
			throw new NotImplementedException();
		}

		public void StreamFakeMessage(ResponseMessage message)
		{
			throw new NotImplementedException();
		}
	}
}
