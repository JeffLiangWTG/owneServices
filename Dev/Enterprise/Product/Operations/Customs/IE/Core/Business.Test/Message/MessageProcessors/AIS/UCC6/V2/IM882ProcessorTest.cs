using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM882;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM882Processor))]
	class IM882ProcessorTest : EntryHeaderMessageProcessorTest<IM882Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IM882Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM882;

		protected override ZString MessageText => GetMessageText();

		protected virtual ZString GetMessageText()
		{
			return Serialize(new Im882
			{
				ImportOperation = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MCciOperationType882
				{
					Mrn = "12MRN345ABCDE678R9",
					CaseId = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ",
					DocumentsUploadRequestCancellationReason = "Documents Upload Request Cancellation Reason",
				},
			});
		}

		protected override ZString MessageFriendlyName => "IM882: Documents Upload Request Cancellation";

		protected override IM882Processor Processor => new IM882Processor(logger, typeof(Im882));

		protected override (JobDeclaration declaration, CusEntryHeader messageAttachee, EDIMessage outgoingMessage, AISInboundEDIMessage incomingMessage) CreateSetupData(string incomingMessageText = null)
		{
			var testData = base.CreateSetupData(incomingMessageText);
			var instruction = testData.messageAttachee.EntryInstruction;
			instruction.RequestedDocuments.AddNew().CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;
			instruction.RequestedDocuments.AddNew().CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;
			instruction.RequestedDocuments.AddNew().CSI_Status = RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived;
			return testData;
		}

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			var cancelledRequestedDocuments = messageAttachee.EntryInstruction.RequestedDocuments.Where(x => x.CSI_Status == RequestedDocumentStatusList.Codes.RequestCancelled);
			AssertEquals("Cancelled RequestedDocument Count", 2, cancelledRequestedDocuments.Count());

			AssertMessageInterpretation(incomingMessage, @"A Documents Upload Request Cancellation (IM882) message has been received for Job B00001000.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345ABCDE678R9</td></tr><tr><td>Case Id</td><td>1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ</td></tr><tr><td>Documents Upload Request Cancellation Reason</td><td>Documents Upload Request Cancellation Reason</td></tr></table>");

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Documents Upload Request Cancellation (IM882) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}
	}
}
