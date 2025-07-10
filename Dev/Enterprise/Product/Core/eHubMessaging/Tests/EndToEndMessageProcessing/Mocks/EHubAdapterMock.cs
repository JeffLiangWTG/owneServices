using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using Enterprise.eHubMessaging.ServiceTasks;
using Moq;

namespace Enterprise.eHubMessaging.Tests.EndToEndMessageProcessing.Mocks
{
	class EHubAdapterMock : IeHubAdapter
	{
		public event EventHandler OnRetrieve;
		public event EventHandler OnSend;

		public EHubAdapterMock()
		{
			Outbox = new MessageOutboxMock();
			Inbox = new MessageInboxMock();
		}

		public EHubAdapterMock(IeHubMessage[] retrieveMessages)
		{
			Outbox = new MessageOutboxMock();
			var inbox = new MessageInboxMock();
			Inbox = inbox;
			inbox.AddMessage(Argument.NotNull(retrieveMessages, "retrieveMessages"));
		}

		public void Dispose()
		{
		}

		public virtual void SendMessages()
		{
			OnSend?.Invoke(this, null);
		}

		public void RetrieveMessages()
		{
			OnRetrieve?.Invoke(this, null);
		}

		public Dictionary<Guid, OutboundMessageStatus> GetOutboundMessageStatuses(Guid[] trackingIDs)
		{
			throw new NotImplementedException();
		}

		public IMessageOutbox Outbox { get; private set; }
		public IMessageInbox Inbox { get; private set; }
	}

	class MessageOutboxMock : MessageBox, IMessageOutbox
	{
		public void AddMessage(IeHubMessage message)
		{
			messageList.Add(message);
		}

		public long SizeInBytes
		{
			get
			{
				long num = 0;
				foreach (IeHubMessage message in this.messageList)
				{
					num += message.MessageStream.Length;
				}
				return num;
			}
		}

		public List<IeHubMessage> MessageList => messageList;
	}

	class MessageInboxMock : MessageBox, IMessageInbox
	{
		public void ReadMessageBatch(Guid batchID, eHubGatewayMessage[] messages)
		{
		}

		public bool CanRetrieveMessages { get; set; }

		public void MarkAsRead()
		{
			messageList.Clear();
		}

		public void AddMessage(IeHubMessage[] retrieveMessages)
		{
			messageList.AddRange(retrieveMessages);
		}
	}

	abstract class AdaptorFactoryMock : IAdaptorFactory
	{
		protected List<IeHubAdapter> adapters = new List<IeHubAdapter>();

		public virtual AdapterType AdapterType => AdapterType.Mock;
		public IeHubAdapter[] Adapters => adapters.ToArray();

		public abstract IeHubAdapter Create(string licenceCode, string password, INotifications notifier, string serverAddress);
	}

	class OutboundAdaptorFactoryMock : AdaptorFactoryMock
	{
		readonly string expectedServerAddress;

		public OutboundAdaptorFactoryMock(string expectedServerAddress = null)
		{
			this.expectedServerAddress = expectedServerAddress;
		}

		public override IeHubAdapter Create(string licenceCode, string password, INotifications notifier, string serverAddress)
		{
			if (expectedServerAddress != null && !expectedServerAddress.Equals(serverAddress, StringComparison.InvariantCultureIgnoreCase))
			{
				throw new InvalidOperationException($"Expected server address={expectedServerAddress} but was {serverAddress}");
			}

			var adapter = new Mock<IeHubAdapter>(MockBehavior.Strict);
			adapter.Setup(m => m.Dispose());
			adapter.Setup(m => m.SendMessages());

			var outBox = new Mock<IMessageOutbox>(MockBehavior.Strict);
			adapter.Setup(m => m.Outbox).Returns(outBox.Object);
			outBox.Setup(m => m.Count).Returns(1);
			outBox.Setup(m => m.SizeInKiloBytes).Returns(5);
			outBox.Setup(m => m.AddMessage(It.IsAny<IeHubMessage>()));
			outBox.Setup(m => m.Clear());

			adapters.Add(adapter.Object);
			return adapter.Object;
		}
	}

	class InboundAdaptorFactoryMock : AdaptorFactoryMock
	{
		readonly string expectedServerAddress;

		public InboundAdaptorFactoryMock(string expectedServerAddress = null)
		{
			this.expectedServerAddress = expectedServerAddress;
		}

		public override IeHubAdapter Create(string licenceCode, string password, INotifications notifier, string serverAddress)
		{
			if (expectedServerAddress != null && !expectedServerAddress.Equals(serverAddress, StringComparison.InvariantCultureIgnoreCase))
			{
				throw new InvalidOperationException($"Expected server address={expectedServerAddress} but was {serverAddress}");
			}

			var adapter = new Mock<IeHubAdapter>(MockBehavior.Strict);
			adapter.Setup(m => m.Dispose());
			adapter.Setup(m => m.RetrieveMessages());

			var inBox = new Mock<IMessageInbox>(MockBehavior.Strict);
			adapter.Setup(m => m.Inbox).Returns(inBox.Object);
			inBox.Setup(m => m.Count).Returns(0);
			inBox.Setup(m => m.MarkAsRead());

			adapters.Add(adapter.Object);
			return adapter.Object;
		}
	}

	class AdaptorFactoryMockThatThrowsOnCreate : AdaptorFactoryMock
	{
		readonly Exception exceptionToThrow;

		public AdaptorFactoryMockThatThrowsOnCreate(Exception exceptionToThrow)
		{
			this.exceptionToThrow = exceptionToThrow;
		}

		public override IeHubAdapter Create(string licenceCode, string password, INotifications notifier, string serverAddress) => throw exceptionToThrow;
	}

	class AdaptorFactoryMockWithOneAdaptor : AdaptorFactoryMock
	{
		readonly IeHubAdapter adapter;

		public AdaptorFactoryMockWithOneAdaptor(IeHubAdapter adapter)
		{
			this.adapter = adapter;
		}

		public override IeHubAdapter Create(string licenceCode, string password, INotifications notifier, string serverAddress) => adapter;
	}

	class GatewayAdaptorFactoryMockWithOneAdaptor : AdaptorFactoryMockWithOneAdaptor
	{
		public GatewayAdaptorFactoryMockWithOneAdaptor(IeHubAdapter adapter)
			: base(adapter)
		{
		}

		public override AdapterType AdapterType => AdapterType.GatewayAdapter;
	}

	class AdaptorMockThatThrowsOnSend : IeHubAdapter
	{
		readonly Exception exceptionToThrow;

		public AdaptorMockThatThrowsOnSend(Exception exceptionToThrow)
		{
			Outbox = new TestMessageOutbox();
			this.exceptionToThrow = exceptionToThrow;
		}

		public IMessageOutbox Outbox { get; }

		public IMessageInbox Inbox => throw new NotImplementedException();

		public void Dispose()
		{
		}

		public Dictionary<Guid, OutboundMessageStatus> GetOutboundMessageStatuses(Guid[] trackingIDs) => throw new NotImplementedException();
		public void RetrieveMessages() => throw new NotImplementedException();
		public void SendMessages() => throw exceptionToThrow;
	}

	class TestMessageOutbox : IMessageOutbox
	{
		readonly List<IeHubMessage> messages = new List<IeHubMessage>();

		public IEnumerator<IeHubMessage> GetEnumerator()
		{
			return messages.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Clear()
		{
			messages.Clear();
		}

		public long SizeInKiloBytes
		{
			get { return Count; }
		}

		public int Count
		{
			get { return messages.Count; }
		}

		public void AddMessage(IeHubMessage message)
		{
			messages.Add(message);
		}
	}
}
