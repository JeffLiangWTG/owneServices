using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM882;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(IM882Processor))]
	sealed class IM882ProcessorTest : EntryHeaderMessageProcessorTest<IM882Processor, AISUCC5InboundEDIMessage, AISUCC5OutboundEDIMessage, IM882Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM882;

		protected override ZString MessageText => Serialize(new Im882
		{
			Declaration = new DeclarationType
			{
				Mrn = "12MRN345CDEFG678R9",
				CaseId = "QWERTYUIOPASDFGHJKLZXCVBNM1234567890",
				DocumentsUploadRequestCancellationReason = "Sample Text"
			}
		});

		protected override void AssertProcessResultCore(CusEntryHeader entry, AISUCC5InboundEDIMessage incomingMessage)
		{
			AssertEquals(entry.CH_Status, "ACC");
			var jobNumber = entry.Declaration.JE_DeclarationReference;
			var messageInterpretation = $@"A Documents Upload Request Cancellation (IM882) message has been received for Job {jobNumber}.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr><tr><td>Case Id</td><td>QWERTYUIOPASDFGHJKLZXCVBNM1234567890</td></tr><tr><td>Documents Upload Request Cancellation Reason</td><td>Sample Text</td></tr></table>";
			AssertMessageInterpretation(incomingMessage, messageInterpretation);
			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for " + jobNumber, new[] { messageInterpretation }, new string[] { "staff1@where.com" });
			foreach (var item in entry.EntryInstruction.RequestedDocuments)
			{
				AssertEquals("All OPE RequestedDocuments should be CAN", false, item.CSI_Status.EqualsIgnoringCase("OPE"));
			}
		}

		protected override ZString MessageFriendlyName => "IM882: Document Upload Request Cancellation";

		protected override IM882Processor Processor => new IM882Processor(logger, typeof(Im882));

		protected override (JobDeclaration declaration, CusEntryHeader messageAttachee, EDIMessage outgoingMessage, AISUCC5InboundEDIMessage incomingMessage) CreateSetupData(string incomingMessageText = null)
		{
			(JobDeclaration declaration, CusEntryHeader messageAttachee, EDIMessage outgoingMessage, AISUCC5InboundEDIMessage incomingMessage) = base.CreateSetupData(incomingMessageText);
			var requestedDoc = messageAttachee.EntryInstruction.RequestedDocuments.AddNew();
			requestedDoc.CSI_Status = "OPE";
			return (declaration, messageAttachee, outgoingMessage, incomingMessage);
		}
	}
}
