using System.IO;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.Common.MessageAcknowledgement;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(MessageAcknowledgementServiceErrorInterpreter))]
	class MessageAcknowledgementServiceErrorInterpreterTest : InboundMessageInterpreterAbstractTest<AESInboundEDIMessage, MessageAcknowledgementServiceErrorInterpreter, ITransaction>
	{
		public void TestGetMessageInterpretation_EMCS()
		{
			var originalData = MessageAcknowledgementProcessorTest.CreateOriginalDataForEMCS(Factory);
			originalData.outgoingMessage.EM_MessageNum = "IEM00000001";
			var incomingMessage = InterchangeProcessorTestHelper.CreateMessageAcknowledgementServiceErrorMessage<Enterprise.Messaging.Business.EDIMessage>(Factory, EDIMessage.ApplicationCodes.IECustomsEMCS, "815", "ERRCODE");
			var interpreter = new MessageAcknowledgementServiceErrorInterpreter(originalData.outgoingMessage, GetProvider(incomingMessage.GetEM_MessageTextReader()));
			var expectedInterpretation = $@"Error submitting message. Error Code: ERRCODE - <br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Transaction ID</td><td>TRS0000001</td></tr><tr><td>Message Type</td><td>815</td></tr><tr><td>Message Number</td><td>{originalData.outgoingMessage.EM_MessageNum}</td></tr></table><br />Declaration message status has been set FAL<br />";

			AssertXMLEquals(expectedInterpretation, interpreter.GetInterpretation());
		}

		protected override ZString MessageType => CommonInterchangeTypeList.Codes.MessageAcknowledge;

		protected override ZString GetExpectedInterpretation(AESInboundEDIMessage message) => $@"Error submitting message. Error Code: ERRCODE - <br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Transaction ID</td><td>TRS0000001</td></tr><tr><td>Message Type</td><td>{AESOutgoingMessageTypeList.Codes.ArrivalAtExit} - {AESOutgoingMessageTypeList.Descriptions.ArrivalAtExit}</td></tr><tr><td>Message Number</td><td>{outgoingMessage.EM_MessageNum}</td></tr></table><br />Entry status has been set ERR<br />";

		protected override AESInboundEDIMessage CreateIncomingMessageToTest()
		{
			var originalData = MessageAcknowledgementProcessorTest.CreateOriginalData(Factory);
			outgoingMessage = originalData.outgoingMessage;
			var incomingMessage = InterchangeProcessorTestHelper.CreateMessageAcknowledgementServiceErrorMessage<AESInboundEDIMessage>(Factory, EDIMessage.ApplicationCodes.IECustomsExport, AESOutgoingMessageTypeList.Codes.ArrivalAtExit, "ERRCODE");

			return incomingMessage;
		}
		AESOutboundEDIMessage outgoingMessage;

		protected override ITransaction GetProvider(TextReader reader) => IEXmlObjectSerializer.Deserialize<MessageAcknowledgement>(reader);

		protected override MessageAcknowledgementServiceErrorInterpreter GetInterpreterCore(AESInboundEDIMessage incomingMessage, ITransaction provider) => new MessageAcknowledgementServiceErrorInterpreter(outgoingMessage, provider);
	}
}
