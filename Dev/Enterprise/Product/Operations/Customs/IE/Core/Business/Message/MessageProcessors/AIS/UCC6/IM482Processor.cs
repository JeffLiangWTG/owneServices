using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM482Processor : AISMessageProcessor<AISInboundEDIMessage, IM482Provider>
	{
		public IM482Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("165F5080-B09E-49C9-A33F-5C2E96BABDDA", "IM482: Documents Request");

		protected override Type MessageInterpreterType => typeof(IM482MessageInterpreter);

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, IM482Provider provider)
		{
			if (messageAttachee is IAISMessageAttachee aisMessageAttachee)
			{
				MessageProcessorHelper.PopulateRequestedDocuments(aisMessageAttachee.RequestedDocumentsProvider, provider.AdditionalInformations, provider.RequestDate, provider.DateLimit, EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestOpened);
			}
		}
	}
}
