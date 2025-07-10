using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM884;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(IM884Processor))]
	sealed class IM884ProcessorTest : EntryHeaderMessageProcessorTest<IM884Processor, AISUCC5InboundEDIMessage, AISUCC5OutboundEDIMessage, IM884Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM884;

		protected override ZString MessageText => Serialize(new Im884
		{
			Declaration = new DeclarationType
			{
				Mrn = "12MRN345CDEFG678R9",
				CaseId = "QWERTYUIOPASDFGHJKLZXCVBNM1234567890",
				DocumentsPresentRequestCancellationReason = "Sample Text"
			}
		});

		protected override void AssertProcessResultCore(CusEntryHeader entry, AISUCC5InboundEDIMessage incomingMessage)
		{
			AssertEquals(entry.CH_Status, "ACC");
			var jobNumber = entry.Declaration.JE_DeclarationReference;
			var messageInterpretation = $@"A Documents Presentation Request Cancellation (IM884) message has been received for Job {jobNumber}.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr><tr><td>Case Id</td><td>QWERTYUIOPASDFGHJKLZXCVBNM1234567890</td></tr><tr><td>Documents Presentation Request Cancellation Reason</td><td>Sample Text</td></tr></table>";
			AssertMessageInterpretation(incomingMessage, messageInterpretation);
			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for " + jobNumber, new[] { messageInterpretation }, new string[] { "staff1@where.com" });

				foreach (var item in entry.EntryInstruction.RequestedDocuments)
				{
					AssertEquals("All PRP RequestedDocuments should be CAN", false, item.CSI_Status.EqualsIgnoringCase("PRP"));
				}
		}

		protected override ZString MessageFriendlyName => "IM884: Document Presentation Request Cancellation";

		protected override IM884Processor Processor => new IM884Processor(logger, typeof(Im884));

		protected override (JobDeclaration declaration, CusEntryHeader messageAttachee, EDIMessage outgoingMessage, AISUCC5InboundEDIMessage incomingMessage) CreateSetupData(string incomingMessageText = null)
		{
			(JobDeclaration declaration, CusEntryHeader messageAttachee, EDIMessage outgoingMessage, AISUCC5InboundEDIMessage incomingMessage) = base.CreateSetupData(incomingMessageText);
			var requestedDoc = messageAttachee.EntryInstruction.RequestedDocuments.AddNew();
			requestedDoc.CSI_Status = "PRP";
			return (declaration, messageAttachee, outgoingMessage, incomingMessage);
		}
	}
}
