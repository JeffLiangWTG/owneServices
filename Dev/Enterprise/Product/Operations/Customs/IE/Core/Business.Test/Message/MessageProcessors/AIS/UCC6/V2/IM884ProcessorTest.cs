using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM884;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM884Processor))]
	class IM884ProcessorTest : EntryHeaderMessageProcessorTest<IM884Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IM884Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM884;

		protected override ZString MessageText => GetMessageText();

		protected virtual ZString GetMessageText()
		{
			return Serialize(new Im884
			{
				ImportOperation = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MCciOperationType884
				{
					Mrn = "12MRN345ABCDE678R9",
					CaseId = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ",
					DocumentsPresentRequestCancellationReason = "Documents Present Request Cancellation Reason",
				},
			});
		}

		protected override ZString MessageFriendlyName => "IM884: Documents Presentation Request Cancellation";

		protected override IM884Processor Processor => new IM884Processor(logger, typeof(Im884));

		protected override (JobDeclaration declaration, CusEntryHeader messageAttachee, EDIMessage outgoingMessage, AISInboundEDIMessage incomingMessage) CreateSetupData(string incomingMessageText = null)
		{
			var testData = base.CreateSetupData(incomingMessageText);
			var instruction = testData.messageAttachee.EntryInstruction;
			instruction.RequestedDocuments.AddNew().CSI_Status = RequestedDocumentStatusList.Codes.PhysicallyPresentDocument;
			instruction.RequestedDocuments.AddNew().CSI_Status = RequestedDocumentStatusList.Codes.PhysicallyPresentDocument;
			instruction.RequestedDocuments.AddNew().CSI_Status = RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived;
			return testData;
		}

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			var cancelledRequestedDocuments = messageAttachee.EntryInstruction.RequestedDocuments.Where(x => x.CSI_Status == RequestedDocumentStatusList.Codes.RequestCancelled);
			AssertEquals("Cancelled RequestedDocument Count", 2, cancelledRequestedDocuments.Count());

			AssertMessageInterpretation(incomingMessage, @"A Documents Presentation Request Cancellation (IM884) message has been received for Job B00001000.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345ABCDE678R9</td></tr><tr><td>Case Id</td><td>1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ</td></tr><tr><td>Documents Presentation Request Cancellation Reason</td><td>Documents Present Request Cancellation Reason</td></tr></table>");

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Documents Presentation Request Cancellation (IM884) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}
	}
}
