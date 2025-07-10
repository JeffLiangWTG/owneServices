using System;
using System.Data;
using System.IO;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	sealed class MessageSenderTest : TestCaseWithFactory
	{
		public void TestSend()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var branch = header.Branch.Company.Branches.AddNew();
			header.AMA_GB = branch.PK;
			var bill = header.Bills.AddNew();
			var sendingObject = new MessageSendingObject(bill);
			sendingObject.Action = "ABCD";
			var messageSender = new MessageSenderForTest(sendingObject);
			var message = messageSender.Send();
			AssertCollectionContains("A new EDIMessage created for bill", message, bill.Messages);
			CombineAssertions("message details", () =>
			{
				AssertEquals("EM_GB", branch.PK, message.EM_GB);
				AssertEquals("Message Type", "ABC", message.EM_MessageType);
				AssertEquals("Receive Transmit", "TRX", message.EM_ReceiveTransmit);
				AssertEquals("Status", "QUE", message.EM_Status);
				AssertEquals("Message Status", "", bill.ABL_MessageStatus);
			});
		}
	}

	public class MessageSenderForTest : MessageSender
	{
		public MessageSenderForTest(MessageSendingObject sendingObject) : base(sendingObject)
		{
		}

		public Func<EDIMessage, IXmlMessageBuilder> CreateMessageBuilderForTesting;

		protected override IXmlMessageBuilder CreateMessageBuilder(EDIMessage message)
		{
			return CreateMessageBuilderForTesting?.Invoke(message) ?? new XmlMessageBuilderForTest(() =>
				new XmlMessageForTest("Test Name", message.EM_MessageType + " HELLO WORLD"));
		}

		public Func<BusinessObjectFactory, EDIMessage> CreateOutboundEDIMessageForTesting;
		protected override EDIMessage CreateOutboundEDIMessage(BusinessObjectFactory factory) => CreateOutboundEDIMessageForTesting?.Invoke(factory) ?? factory.New<EDIMessageForTest>();

		public class XmlMessageBuilderForTest : IXmlMessageBuilder
		{
			public XmlMessageBuilderForTest(Func<IXmlMessage> generateXmlMessage)
			{
				this.generateXmlMessage = generateXmlMessage;
			}
			readonly Func<IXmlMessage> generateXmlMessage;

			public IXmlMessage GenerateXmlMessage() => generateXmlMessage();
		}

		public class XmlMessageForTest : IXmlMessage
		{
			public XmlMessageForTest(string technicalName, string messageText)
			{
				TechnicalName = technicalName;
				this.messageText = messageText;
			}
			readonly string messageText;

			public string GetSerializedString() => messageText;

			public Stream GetSerializedStream() => new StreamReader(messageText).BaseStream;

			public string TechnicalName { get; set; }
		}

		sealed class EDIMessageForTest : EDIMessage
		{
			public EDIMessageForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}

			protected override string GetMessageReferenceNumber() => "MRN001";
		}
	}
}
