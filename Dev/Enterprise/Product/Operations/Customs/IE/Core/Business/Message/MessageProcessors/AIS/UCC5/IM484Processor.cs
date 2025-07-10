using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging.UCC5;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM484Processor : AISUCC5MessageProcessor<AISUCC5InboundEDIMessage, IM484Provider>
	{
		public IM484Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("ABE5CC88-D0A1-4191-B6C7-BC2C121C2650", "IM484: Document Presentation Request");

		protected override void UpdateMessageAttacheeCore(Messaging.IMessageAttachee messageAttachee, IM484Provider provider)
		{
			if (messageAttachee is CusEntryHeader entryHeader)
			{
				MessageProcessorHelper.PopulateRequestedDocuments(entryHeader.EntryInstruction, provider.AdditionalInformations, provider.RequestDate, provider.DateLimit, EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.PhysicallyPresentDocument);
			}
		}

		protected override Type MessageInterpreterType => typeof(IM484MessageInterpreter);
	}
}
