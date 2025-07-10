using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class EX584Processor : AESMessageProcessor<AESInboundEDIMessage, EX584Provider>
	{
		public EX584Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("FBEABC53-8C01-4229-8E45-3B215DD916BF", "EX584: Request Document Presentation");

		public override string GetLogicalStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, EX584Provider provider) => null;
		public override string GetEntryStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, EX584Provider provider) => null;

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, EX584Provider provider)
		{
			if (messageAttachee is CusEntryHeader entryHeader)
			{
				MessageProcessorHelper.PopulateRequestedDocuments(entryHeader.EntryInstruction, provider.AdditionalInformation, provider.RequestDate, provider.DateLimit, EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.PhysicallyPresentDocument);
			}
		}
		protected override Type MessageInterpreterType => typeof(EX584MessageInterpreter);
	}
}
