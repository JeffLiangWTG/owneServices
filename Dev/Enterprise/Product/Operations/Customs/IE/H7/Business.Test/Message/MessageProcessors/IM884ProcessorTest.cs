using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM884;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.AIS;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	[TestedType(typeof(IM884Processor))]
	class IM884ProcessorTest : AISH7MessageProcessorTest<IM884Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IM884Provider>
	{
		protected override IM884Processor Processor => new IM884Processor(logger, typeof(Im884));

		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM884;

		protected override ZString MessageText => GetMessageText();

		protected override ZString MessageFriendlyName => AISInterchangeTypeList.Descriptions.IM884;

		protected override void AssertProcessResultCore(AsycudaBill messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			base.AssertProcessResultCore(messageAttachee, incomingMessage);
			AssertEquals("Logical status", string.Empty, messageAttachee.ABL_MessageStatus);
			AssertEquals("Entry status", string.Empty, messageAttachee.ABL_BillStatus);

			var cancelledRequestedDocuments = messageAttachee.RequestedDocuments.Where(x => x.CSI_Status == RequestedDocumentStatusList.Codes.RequestCancelled);
			AssertEquals("Cancelled RequestedDocument Count", 2, cancelledRequestedDocuments.Count());

			AssertMessageInterpretation(incomingMessage, @"A Documents Presentation Request Cancellation (IM884) message has been received for Job H7D00000001.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345ABCDE678R9</td></tr><tr><td>Case Id</td><td>1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ</td></tr><tr><td>Documents Presentation Request Cancellation Reason</td><td>Documents Present Request Cancellation Reason</td></tr></table>");
		}

		protected override (AsycudaManifestHeader declaration, AsycudaBill messageAttachee, EDIMessage outgoingMessage, AISInboundEDIMessage incomingMessage) CreateSetupData(string incomingMessageText = null)
		{
			var testData = base.CreateSetupData(incomingMessageText);
			var requestedDocuments = testData.messageAttachee.RequestedDocuments;
			requestedDocuments.AddNew().CSI_Status = RequestedDocumentStatusList.Codes.PhysicallyPresentDocument;
			requestedDocuments.AddNew().CSI_Status = RequestedDocumentStatusList.Codes.PhysicallyPresentDocument;
			requestedDocuments.AddNew().CSI_Status = RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived;
			return testData;
		}

		ZString GetMessageText()
		{
			return Serialize(new Im884
			{
				Declaration = new DeclarationType
				{
					Mrn = "12MRN345ABCDE678R9",
					CaseId = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ",
					DocumentsPresentRequestCancellationReason = "Documents Present Request Cancellation Reason",
				},
			});
		}
	}
}
