using System;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	abstract class AISMessageAcknowledgementAdditionalProcessorTest : AdditionalProcessorAbstractTest
	{
		const string transactionId = "TRS0000001";

		protected override ZString MessageSubType => EDIMessage.Status.Acknowledged;

		protected override IAdditionalMessageProcessing Processor => new AISMessageAcknowledgementAdditionalProcessor();

		protected override EDIMessage MessageToProcess
		{
			get
			{
				if (messageToProcess is null)
				{
					messageToProcess = InterchangeProcessorTestHelper.CreateMessageAcknowledgementMessage<AISInboundEDIMessage>(Factory, ApplicationCode, AESInboundEDIMessage.Status.Acknowledged, Guid.NewGuid().ToString());
					messageToProcess.EM_ApplicationReference = transactionId;
					messageToProcess.EM_LinkedObject = cusEntryHeader;
				}

				return messageToProcess;
			}
		}
		EDIMessage messageToProcess;

		protected override EDIMessage OriginalMessage
		{
			get
			{
				if (originalMessage is null)
				{
					originalMessage = Factory.New<AISOutboundEDIMessage>();
					originalMessage.EM_ApplicationCode = ApplicationCode;
					originalMessage.EM_MessageType = MessageType;
					originalMessage.EM_MessageNum = "IEE00000000000001";
					originalMessage.EM_ApplicationReference = transactionId;
					originalMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
					originalMessage.EM_LinkedObject = cusEntryHeader;
					originalMessage.EM_Status = EDIMessage.Status.Sent;
				}
				return originalMessage;
			}
		}
		EDIMessage originalMessage;

		protected override void AssertProcessMessageResultsCore()
		{
			CombineAssertions(() =>
			{
				var logs = declaration.Logs.GetAllLogs();
				AssertGreaterThan(logs.Count, 0);
				AssertEquals("Entry header declaration logged ECM event", "ECM", logs[logs.Count - 1].Event.SE_Code);

				var expectedInterpretation = $@"Message IEE00000000000001 ({Factory.GetCachedValue<AISOutgoingMessageTypeList>().GetDescriptionFromCode(MessageType)}) was transmitted and the following statuses apply:<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Transaction ID</td><td>TRS0000001</td></tr><tr><td>Message Status</td><td>Accepted</td></tr><tr><td>Transaction ID Status</td><td>Accepted</td></tr></table>";
				AssertXMLEquals("Interpretation", expectedInterpretation, MessageToProcess.EM_MessageInterpretation);
			});
		}

		protected override void SetUp()
		{
			declaration = Factory.New<JobDeclaration>();
			cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_DeclarationReference = "B012345678";
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		}
		JobDeclaration declaration;
		CusEntryHeader cusEntryHeader;
	}
}
