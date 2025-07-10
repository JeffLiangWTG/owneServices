using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM862;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(IM862Processor))]
	sealed class IM862ProcessorTest : EntryHeaderMessageProcessorTest<IM862Processor, AISUCC5InboundEDIMessage, AISUCC5OutboundEDIMessage, IM862Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM862;

		protected override (JobDeclaration declaration, CusEntryHeader messageAttachee, EDIMessage outgoingMessage, AISUCC5InboundEDIMessage incomingMessage) CreateSetupData(string incomingMessageText = null)
		{
			var basicTime = new ZDateTime(2024, 1, 1, 10, 0, 0);
			var result = base.CreateSetupData(incomingMessageText);

			var previousMessage = Factory.New<AISUCC5InboundEDIMessage>();
			previousMessage.EM_ApplicationCode = "IE5";
			previousMessage.EM_MessageType = AISInterchangeTypeList.Codes.IM429;
			previousMessage.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(TransactionID, AISInterchangeProcessorTestHelper.GetStandardUCC5IM429Text(), includeResponseWrap: false, includeEncoding: false);
			previousMessage.EM_SystemCreateTimeUtc = basicTime;
			previousMessage.EM_Status = EDIMessage.Status.ProcessedOK;
			result.messageAttachee.Messages.Add(previousMessage);

			var previous462Message = Factory.New<AISUCC5InboundEDIMessage>();
			previous462Message.EM_ApplicationCode = "IE5";
			previous462Message.EM_MessageType = AISInterchangeTypeList.Codes.IM462;
			previous462Message.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(TransactionID, AISInterchangeProcessorTestHelper.GetStandardUCC5IM462Text(), includeResponseWrap: false, includeEncoding: false);
			previous462Message.EM_SystemCreateTimeUtc = basicTime.AddMinutes(30);
			previous462Message.EM_Status = EDIMessage.Status.ProcessedOK;
			result.messageAttachee.Messages.Add(previous462Message);

			var incoming864Message = result.incomingMessage;
			incoming864Message.EM_SystemCreateTimeUtc = basicTime.AddMinutes(60);
			Factory.Save();

			return result;
		}

		protected override ZString MessageText => Serialize(new Im862
		{
			Declaration = new DeclarationType
			{
				Mrn = "21IEDUB11A782454R2",
				CaseId = "11111111-1111-1111-1111-111111111111",
				AmendmentRequestCancellationReason = "Sample Text"
			}
		});

		protected override void AssertProcessResultCore(CusEntryHeader entry, AISUCC5InboundEDIMessage incomingMessage)
		{
			AssertEquals("ACC", entry.CH_Status);
			AssertEquals("Entry status should have reverted", "REL", entry.CH_EntryStatus);
			var jobNumber = entry.Declaration.JE_DeclarationReference;
			var messageInterpretation = $@"A Declaration Amendment Request Cancellation (IM862) message has been received for Job {jobNumber}.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Case Id</td><td>11111111-1111-1111-1111-111111111111</td></tr><tr><td>Amendment Request Cancellation Reason</td><td>Sample Text</td></tr></table>";
			AssertMessageInterpretation(incomingMessage, messageInterpretation);
			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for " + jobNumber, new[] { messageInterpretation }, new string[] { "staff1@where.com" });
		}

		protected override ZString MessageFriendlyName => "IM862: Declaration Amendment Request Cancellation";

		protected override IM862Processor Processor => new IM862Processor(logger, typeof(Im862));
	}
}
