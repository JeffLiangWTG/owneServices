using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;

namespace Enterprise.Customs.IE.Business.Testing
{
	class InboundEDIMessageTest : TestCaseWithFactory
	{
		public void TestGetOriginalMessage()
		{
			var outgoingMessage1 = CreateOriginalMessage("TID1", "IEE", ZDateTime.BrettsBirthday, ZGuid.Empty);
			var outgoingMessage2 = CreateOriginalMessage("TID2", "IEE", ZDateTime.BrettsBirthday, ZGuid.Empty);
			var outgoingMessage3 = CreateOriginalMessage("TID2", "IEE", ZDateTime.BrettsBirthday.AddMinutes(1), ZGuid.Empty);

			CombineAssertions(() =>
			{
				AssertEquals("Match found", outgoingMessage1, InboundEDIMessage.GetOriginalMessage(Factory, "IEE", "TID1"));
				AssertNull("Match not found - empty TID", InboundEDIMessage.GetOriginalMessage(Factory, "IEE", ""));
				AssertNull("Match not found - incorrect TID", InboundEDIMessage.GetOriginalMessage(Factory, "IEE", "TIDX"));
				AssertNull("Match not found - incorrect Application Code", InboundEDIMessage.GetOriginalMessage(Factory, "DEE", "TID2"));
				AssertEquals("Match found - most recent", outgoingMessage3, InboundEDIMessage.GetOriginalMessage(Factory, "IEE", "TID2"));
			});
		}

		public void TestGetOriginalMessageWithoutTID()
		{
			var guid1 = ZGuid.NewZGuid();
			var guid2 = ZGuid.NewZGuid();
			var guid3 = ZGuid.NewZGuid();

			var outgoingMessage1 = CreateOriginalMessage("TID", "IEE", ZDateTime.BrettsBirthday, guid1);
			var outgoingMessage2 = CreateOriginalMessage("TID", "IEE", ZDateTime.BrettsBirthday, guid2);
			var outgoingMessage3 = CreateOriginalMessage("TID", "IEE", ZDateTime.BrettsBirthday.AddMinutes(1), guid2);
			var outgoingMessage4 = CreateOriginalMessage("TID", "IEE", ZDateTime.BrettsBirthday, guid3);
			outgoingMessage4.EM_Status = EDIMessage.Status.Acknowledged;

			CombineAssertions(() =>
			{
				AssertEquals("Match found", outgoingMessage1, InboundEDIMessage.GetOriginalMessageWithoutTID(Factory, "IEE", guid1, ZDateTime.BrettsBirthday.AddMinutes(1)));
				AssertNull("Match not found - incoming before outgoing", InboundEDIMessage.GetOriginalMessageWithoutTID(Factory, "IEE", guid1, ZDateTime.BrettsBirthday.AddMinutes(-1)));
				AssertNull("Match not found - empty guid", InboundEDIMessage.GetOriginalMessageWithoutTID(Factory, "IEE", ZGuid.Empty, ZDateTime.BrettsBirthday.AddMinutes(1)));
				AssertNull("Match not found - incorrect guid", InboundEDIMessage.GetOriginalMessageWithoutTID(Factory, "IEE", ZGuid.NewZGuid(), ZDateTime.BrettsBirthday.AddMinutes(1)));
				AssertNull("Match not found - incorrect Application Code", InboundEDIMessage.GetOriginalMessageWithoutTID(Factory, "DEE", guid1, ZDateTime.BrettsBirthday.AddMinutes(1)));
				AssertEquals("Match found - most recent", outgoingMessage3, InboundEDIMessage.GetOriginalMessageWithoutTID(Factory, "IEE", guid2, ZDateTime.BrettsBirthday.AddMinutes(2)));
				AssertEquals("Match found - acknowledged", outgoingMessage4, InboundEDIMessage.GetOriginalMessageWithoutTID(Factory, "IEE", guid3, ZDateTime.BrettsBirthday.AddMinutes(1)));
			});
		}

		public void TestResetToQueuedStatus()
		{
			var message = InterchangeProcessorTestHelper.CreateMessageAcknowledgementMessage<AESInboundEDIMessage>(
				factory: Factory,
				applicationCode: EDIMessage.ApplicationCodes.IECustomsExport,
				messageType: AESIncomingMessageTypeList.Codes.IE504,
				transactionId: Guid.NewGuid().ToString()
			);
			message.ResetToQueuedStatus();
			CombineAssertions("Message types should be Preserved", () =>
			{
				AssertEquals("EM_MessageType", AESIncomingMessageTypeList.Codes.IE504, message.EM_MessageType);
				AssertEquals("EM_MessageSubType", CommonInterchangeTypeList.Codes.MailboxAcknowledge, message.EM_MessageSubType);
			});
		}

		OutboundEDIMessage CreateOriginalMessage(string transactionId, string applicationCode, ZDateTime createdTime, ZGuid linkedUniqueId)
		{
			var outgoingMessage = Factory.New<AESOutboundEDIMessage>();
			outgoingMessage.EM_ApplicationCode = applicationCode;
			outgoingMessage.EM_ApplicationReference = transactionId;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingMessage.EM_SystemCreateTimeUtc = createdTime;
			outgoingMessage.EM_LinkUniqueID = new ZGuid(linkedUniqueId);
			return outgoingMessage;
		}
	}
}
