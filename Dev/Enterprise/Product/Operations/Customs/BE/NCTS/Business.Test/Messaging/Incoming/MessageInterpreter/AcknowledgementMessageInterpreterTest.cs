using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.BE.Business.Testing;
using Enterprise.Messaging.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(AcknowledgementMessageInterpreter))]
	sealed class AcknowledgementMessageInterpreterTest : MessageInterpreterTestCase<AcknowledgementMessageInterpreter, IAcknowledgementMessageDataProvider>
	{
		public override void TestInterpret()
		{
			var correlationId = ZGuid.NewZGuid().ToString();
			var messageId = "408";
			var incomingMessage = Factory.New<NCTSMessage>();
			SetupEDIInterchangeWithLink(incomingMessage, messageId);

			var mock = new Mock<IAcknowledgementMessageDataProvider>();
			mock.Setup(m => m.CorrelationId).Returns(correlationId);

			var result = Interpreter.Interpret(mock.Object, incomingMessage);

			AssertEquals(
				$"Correlation id: {correlationId}</br>" +
				$"Customs acknowledged the reception of the message with id: {messageId}"
				, result);
		}

		void SetupEDIInterchangeWithLink(NCTSMessage incomingEdiMessage, string originalOutgoingEDIMessageNumber)
		{
			var sessionGUID = ZGuid.BrettsGuid;
			var originalOutgoingEDIMessage = Factory.New<NCTSMessage>();
			originalOutgoingEDIMessage.EM_MessageNum = originalOutgoingEDIMessageNumber;
			originalOutgoingEDIMessage.EM_Status = "SNT";

			var outgoingInterchange = Factory.New<BECInterchange>();
			outgoingInterchange.EI_Status = "SNT";
			outgoingInterchange.EI_SessionGUID = sessionGUID;
			outgoingInterchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			originalOutgoingEDIMessage.EM_EI = outgoingInterchange.PK;

			var interchangeIncoming = Factory.New<BECInterchange>();
			interchangeIncoming.EI_SessionGUID = sessionGUID;
			interchangeIncoming.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;

			incomingEdiMessage.EM_EI = interchangeIncoming.PK;
		}
	}
}
