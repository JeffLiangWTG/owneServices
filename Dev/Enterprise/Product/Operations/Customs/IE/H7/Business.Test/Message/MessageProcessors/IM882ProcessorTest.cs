using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM882;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.AIS;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	[TestedType(typeof(IM882Processor))]
	class IM882ProcessorTest :	AISH7MessageProcessorTest<IM882Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IM882Provider>
	{
		protected override IM882Processor Processor => new IM882Processor(logger, typeof(Im882));

		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM882;

		protected override ZString MessageText => GetMessageText();

		protected override ZString MessageFriendlyName => AISInterchangeTypeList.Descriptions.IM882;

		protected override void AssertProcessResultCore(AsycudaBill messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			base.AssertProcessResultCore(messageAttachee, incomingMessage);
			AssertEquals("Logical status", LogicalStatusList.Codes.Accepted, messageAttachee.ABL_MessageStatus);
			AssertEquals("Entry status", AISEntryStatusList.Codes.Control, messageAttachee.ABL_BillStatus);

			var cancelledRequestedDocuments = messageAttachee.RequestedDocuments.Where(x => x.CSI_Status == RequestedDocumentStatusList.Codes.RequestCancelled);
			AssertEquals("Cancelled RequestedDocument Count", 2, cancelledRequestedDocuments.Count());

			AssertMessageInterpretation(incomingMessage, @"A Documents Upload Request Cancellation (IM882) message has been received for Job H7D00000001.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345ABCDE678R9</td></tr><tr><td>Case Id</td><td>1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ</td></tr><tr><td>Documents Upload Request Cancellation Reason</td><td>Documents Upload Request Cancellation Reason</td></tr></table>");
		}

		protected override (AsycudaManifestHeader declaration, AsycudaBill messageAttachee, EDIMessage outgoingMessage, AISInboundEDIMessage incomingMessage) CreateSetupData(string incomingMessageText = null)
		{
			var testData = base.CreateSetupData(incomingMessageText);
			var requestedDocuments = testData.messageAttachee.RequestedDocuments;
			requestedDocuments.AddNew().CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;
			requestedDocuments.AddNew().CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;
			requestedDocuments.AddNew().CSI_Status = RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived;
			return testData;
		}

		ZString GetMessageText()
		{
			return Serialize(new Im882
			{
				Declaration = new DeclarationType
				{
					Mrn = "12MRN345ABCDE678R9",
					CaseId = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ",
					DocumentsUploadRequestCancellationReason = "Documents Upload Request Cancellation Reason",
				},
			});
		}
	}
}
