using System;
using System.Linq;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class EX882Processor : AESMessageProcessor<AESInboundEDIMessage, EX882Provider>
	{
		public EX882Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("E6D232DD-C2DE-4D64-BAC5-B911993A8D02", "EX882: Documents Upload Request Cancellation");

		public override string GetLogicalStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, EX882Provider provider) => null;

		public override string GetEntryStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, EX882Provider provider) => null;

		protected override Type MessageInterpreterType => typeof(EX882MessageInterpreter);

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, EX882Provider provider)
		{
			if (messageAttachee is CusEntryHeader entryHeader)
			{
				CancelRequestedDocuments(entryHeader.EntryInstruction);
			}
		}

		void CancelRequestedDocuments(CusEntryInstruction instruction)
		{
			var requestedDocumentsNeedUpdate = instruction?.RequestedDocuments?.Cast<EU.Business.RequestedDocument>().Where(p => p.CSI_Status == EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestOpened) ?? Array.Empty<EU.Business.RequestedDocument>();

			foreach (var requestedDocument in requestedDocumentsNeedUpdate)
			{
				requestedDocument.CSI_Status = EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestCancelled;
			}
		}
	}
}
