using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.EX864;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	[TestedType(typeof(EX864Processor))]
	class EX864ProcessorTest : EntryHeaderMessageProcessorTest<EX864Processor, AESInboundEDIMessage, AESOutboundEDIMessage, EX864Provider>
	{
		protected override (JobDeclaration declaration, CusEntryHeader messageAttachee, EDIMessage outgoingMessage, AESInboundEDIMessage incomingMessage) CreateSetupData(string incomingMessageText = null)
		{
			var basicTime = new ZDateTime(2022, 7, 20, 10, 0, 0);
			var result = base.CreateSetupData(incomingMessageText);

			var previous509Message = Factory.New<AESInboundEDIMessage>();
			previous509Message.EM_MessageType = AESIncomingMessageTypeList.Codes.IE509;
			previous509Message.EM_MessageText = AESInterchangeProcessorTestHelper.GetStandardAESCC509CMailboxItemText(
				transactionID: TransactionID, lrn: "LRN123456789", mrn: "21IEDUB11A782454R2", invalidationInitiatedByCustoms: "1", includeResponseWrap: false
			);
			previous509Message.EM_SystemCreateTimeUtc = basicTime;
			previous509Message.EM_Status = EDIMessage.Status.ProcessedOK;
			result.messageAttachee.Messages.Add(previous509Message);

			var previous564Message = Factory.New<AESInboundEDIMessage>();
			previous564Message.EM_MessageType = AESIncomingMessageTypeList.Codes.EX564;
			previous564Message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(TransactionID, AESInterchangeProcessorTestHelper.GetStandardEX564Text("21IEDUB11A782454R2"));
			previous564Message.EM_SystemCreateTimeUtc = basicTime.AddMinutes(30);
			previous564Message.EM_Status = EDIMessage.Status.ProcessedOK;
			result.messageAttachee.Messages.Add(previous564Message);

			var incoming864Message = result.incomingMessage;
			incoming864Message.EM_SystemCreateTimeUtc = basicTime.AddMinutes(60);
			Factory.Save();

			return result;
		}

		protected override void AssertProcessResultCore(CusEntryHeader entry, AESInboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_Status", LogicalStatusList.Codes.Accepted, entry.CH_Status);
			AssertEquals("CH_EntryStatus should have been reverted to the value after processing the immediate message before the previous EX564 Message.", AESEntryStatusList.Codes.Cancelled, entry.CH_EntryStatus);
			var jobNumber = entry.Declaration.JE_DeclarationReference;
			var messageInterpretation = $@"A Declaration Invalidation Request Cancellation message has been received from Customs for Job {jobNumber} through the EX864 message stating that Revenue have now decided to cancel the Invalidation request which was earlier sent through EX564 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Case Id</td><td>CASEID</td></tr><tr><td>Invalidation Request Cancellation Reason</td><td>REASON</td></tr></table>";
			AssertMessageInterpretation(incomingMessage, messageInterpretation);
			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for " + jobNumber, new[] { messageInterpretation }, new string[] { "staff1@where.com" });
		}

		protected override ZString MessageFriendlyName => "EX864: Response message advising to DECLARATION INVALIDATION REQUEST CANCELLATION";

		protected override EX864Processor Processor => new EX864Processor(logger, typeof(Ex864));

		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.EX864;

		protected override ZString MessageText => AESInterchangeProcessorTestHelper.GetStandardEX864Text("21IEDUB11A782454R2");
	}
}
