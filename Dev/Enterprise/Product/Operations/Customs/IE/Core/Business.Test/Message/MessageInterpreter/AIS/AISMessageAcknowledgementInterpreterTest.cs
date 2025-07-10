using System.IO;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.Common.MessageAcknowledgement;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(AISMessageAcknowledgementInterpreter))]
	sealed class AISMessageAcknowledgementInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, AISMessageAcknowledgementInterpreter, ITransaction>
	{
		protected override ZString MessageType => AISOutgoingMessageTypeList.Codes.CustomsDeclaration;

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => $@"Message IEE00000000000001 (IM415: Customs Declaration) was transmitted and the following statuses apply:<br/><br/><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Transaction ID</td><td>TRS0000001</td></tr><tr><td>Message Status</td><td>Accepted</td></tr><tr><td>Transaction ID Status</td><td>Accepted</td></tr></table>";

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B012345678";
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var outgoingMessage = Factory.New<AISOutboundEDIMessage>();
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsImport;
			outgoingMessage.EM_MessageType = AISOutgoingMessageTypeList.Codes.CustomsDeclaration;
			outgoingMessage.EM_ApplicationReference = "TRS0000001";
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_LinkedObject = entryHeader;
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;

			var incomingMessage = InterchangeProcessorTestHelper.CreateMessageAcknowledgementMessage<AISInboundEDIMessage>(Factory, EDIMessage.ApplicationCodes.IECustomsImport, AISOutgoingMessageTypeList.Codes.CustomsDeclaration, outgoingMessage.EM_ApplicationReference);
			incomingMessage.EM_ApplicationReference = "TRS0000001";
			incomingMessage.EM_MessageNum = "IEE00000000000001";
			incomingMessage.EM_MessageSubType = "ACK";
			incomingMessage.EM_LinkedObject = entryHeader;

			return incomingMessage;
		}

		protected override ITransaction GetProvider(TextReader reader) => IEXmlObjectSerializer.Deserialize<MessageAcknowledgement>(reader);

		protected override AISMessageAcknowledgementInterpreter GetInterpreterCore(AISInboundEDIMessage incomingMessage, ITransaction provider) => new AISMessageAcknowledgementInterpreter(incomingMessage, provider);
	}
}
