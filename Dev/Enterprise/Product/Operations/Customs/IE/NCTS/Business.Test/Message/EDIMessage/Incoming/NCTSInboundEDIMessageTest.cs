using CargoWise.Types;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using NUnit.Framework;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(NCTSInboundEDIMessage))]
	sealed class NCTSInboundEDIMessageTest : EDIMessageTest
	{
		public void TestMessageDefaults()
		{
			var message = Factory.New<NCTSInboundEDIMessage>();
			AssertEquals("EM_ReceiveTransmit", EDIInterchange.Direction.Receive, message.EM_ReceiveTransmit);
			AssertEquals("EM_ApplicationCode", EDIInterchange.ApplicationCodes.IECustomsNCTS, message.EM_ApplicationCode);
		}

		public void TestGetOutboundMessage()
		{
			var incomingMessage1 = Factory.New<NCTSInboundEDIMessage>();
			incomingMessage1.EM_ApplicationReference = "TID2";
			var incomingMessage2 = Factory.New<NCTSInboundEDIMessage>();
			incomingMessage2.EM_ApplicationReference = "TID2";

			var outgoingMessage1 = Factory.New<NCTSOutboundEDIMessage>();
			outgoingMessage1.EM_ApplicationReference = "TID1";
			outgoingMessage1.EM_Status = Status.Acknowledged;
			var outgoingMessage2 = Factory.New<NCTSOutboundEDIMessage>();
			outgoingMessage2.EM_ApplicationReference = "TID2";
			outgoingMessage2.EM_ApplicationCode = "!@#";
			outgoingMessage2.EM_Status = Status.Acknowledged;
			var outgoingMessage3 = Factory.New<NCTSOutboundEDIMessage>();
			outgoingMessage3.EM_ApplicationReference = "TID2";
			outgoingMessage3.EM_Status = Status.Acknowledged;
			var outgoingMessage4 = Factory.New<NCTSOutboundEDIMessage>();
			outgoingMessage4.EM_ApplicationReference = "TID3";
			outgoingMessage4.EM_Status = Status.Acknowledged;

			AssertSame("incomingMessage1.GetOutboundMessage()", outgoingMessage3, incomingMessage1.GetOutboundMessage());
			AssertSame("incomingMessage2.GetOutboundMessage()", outgoingMessage3, incomingMessage2.GetOutboundMessage());
		}

		public void TestCorrectlyTypeDeciding()
		{
			var message = Factory.New<NCTSInboundEDIMessage>();
			Factory.Save();
			AssertType<NCTSInboundEDIMessage>("Using base EDIMessage", NewFactory().Load<Enterprise.Messaging.Business.EDIMessage>(message.PK));
			AssertType<NCTSInboundEDIMessage>("Using IE EDIMessage", NewFactory().Load<IE.Business.EDIMessage>(message.PK));
		}

		public void TestGetPreviousDeclarationMessage()
		{
			var guid1 = ZGuid.NewZGuid();
			var outgoingMessage1 = CreateOriginalMessage(NCTSOutgoingDepartureMessageTypeList.Codes.DeclarationData, Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Sent, ZDateTime.BrettsBirthday, guid1);
			var outgoingMessage2 = CreateOriginalMessage(NCTSOutgoingDepartureMessageTypeList.Codes.DeclarationData, Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Sent, ZDateTime.BrettsBirthday.AddMinutes(1), guid1);

			CombineAssertions(() =>
			{
				AssertEquals("Match found - message type 015, status sent", outgoingMessage1, NCTSInboundEDIMessage.GetPreviousDeclarationMessage(Factory, guid1, ZDateTime.BrettsBirthday));

				outgoingMessage1.EM_Status = Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Failed;
				AssertNull("Match not found - status failed", NCTSInboundEDIMessage.GetPreviousDeclarationMessage(Factory, guid1, ZDateTime.BrettsBirthday));

				outgoingMessage1.EM_Status = Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.Acknowledged;
				AssertEquals("Match found - status acknowledged", outgoingMessage1, NCTSInboundEDIMessage.GetPreviousDeclarationMessage(Factory, guid1, ZDateTime.BrettsBirthday));

				outgoingMessage1.EM_MessageType = NCTSOutgoingDepartureMessageTypeList.Codes.DeclarationInvalidationRequest;
				AssertNull("Match not found - message type 014", NCTSInboundEDIMessage.GetPreviousDeclarationMessage(Factory, guid1, ZDateTime.BrettsBirthday));

				outgoingMessage1.EM_MessageType = NCTSOutgoingDepartureMessageTypeList.Codes.DeclarationAmendment;
				AssertEquals("Match found - message type 013", outgoingMessage1, NCTSInboundEDIMessage.GetPreviousDeclarationMessage(Factory, guid1, ZDateTime.BrettsBirthday));

				AssertEquals("Match found - most recent", outgoingMessage2, NCTSInboundEDIMessage.GetPreviousDeclarationMessage(Factory, guid1, ZDateTime.BrettsBirthday.AddMinutes(1)));

				AssertNull("Match not found - incoming before outgoing", NCTSInboundEDIMessage.GetPreviousDeclarationMessage(Factory, guid1, ZDateTime.BrettsBirthday.AddMinutes(-1)));
				AssertNull("Match not found - empty guid", NCTSInboundEDIMessage.GetPreviousDeclarationMessage(Factory, ZGuid.Empty, ZDateTime.BrettsBirthday.AddMinutes(1)));
				AssertNull("Match not found - incorrect guid", NCTSInboundEDIMessage.GetPreviousDeclarationMessage(Factory, ZGuid.NewZGuid(), ZDateTime.BrettsBirthday.AddMinutes(1)));
			});
		}

		OutboundEDIMessage CreateOriginalMessage(ZString messageType, ZString status, ZDateTime createdTime, ZGuid linkedUniqueId)
		{
			var outgoingMessage = Factory.New<NCTSOutboundEDIMessage>();
			outgoingMessage.EM_MessageType = messageType;
			outgoingMessage.EM_ApplicationReference = "TID";
			outgoingMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outgoingMessage.EM_Status = status;
			outgoingMessage.EM_SystemCreateTimeUtc = createdTime;
			outgoingMessage.EM_LinkUniqueID = new ZGuid(linkedUniqueId);
			return outgoingMessage;
		}
	}
}
