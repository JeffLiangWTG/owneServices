using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AES
{
	public class EX582Processor : AESMessageProcessor<AESInboundEDIMessage, EX582Provider>
	{
		public EX582Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("E96D1E9D-AC5A-4013-9123-3FD2AF328D15", "EX582: DOCUMENTS REQUEST");

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, EX582Provider provider)
		{
			if (messageAttachee is CusEntryHeader entryHeader && entryHeader.EntryInstruction is CusEntryInstruction entryInstruction)
			{
				MessageProcessorHelper.PopulateRequestedDocuments(entryInstruction, provider.AdditionalInformations, provider.RequestDate, provider.DateLimit, EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestOpened);
			}
		}

		protected override Type MessageInterpreterType => typeof(EX582MessageInterpreter);
	}
}
