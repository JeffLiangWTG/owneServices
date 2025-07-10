using System;
using System.Linq;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class EX884Processor : AESMessageProcessor<AESInboundEDIMessage, EX884Provider>
	{
		public EX884Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("20EB1A91-F4C2-42B0-8BE2-3C818B68F22A", "EX884: Documents Presentation Request Cancellation");

		public override string GetLogicalStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, EX884Provider provider) => null;
		public override string GetEntryStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, EX884Provider provider) => null;
		protected override Type MessageInterpreterType => typeof(EX884MessageInterpreter);

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, EX884Provider provider)
		{
			if (messageAttachee is CusEntryHeader entryHeader)
			{
				CancelRequestedDocuments(entryHeader.EntryInstruction, provider);
			}
		}

		void CancelRequestedDocuments(CusEntryInstruction instruction, EX884Provider message)
		{
			var requestedDocumentsNeedUpdate = instruction?.RequestedDocuments?.Cast<EU.Business.RequestedDocument>().Where(p => p.CSI_Status == EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.PhysicallyPresentDocument) ?? Array.Empty<EU.Business.RequestedDocument>();

			foreach (var requestedDocument in requestedDocumentsNeedUpdate)
			{
				requestedDocument.CSI_Status = EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestCancelled;
			}
		}
	}
}
