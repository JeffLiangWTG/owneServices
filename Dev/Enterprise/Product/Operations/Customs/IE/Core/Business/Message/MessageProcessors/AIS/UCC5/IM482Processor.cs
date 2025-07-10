using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging.UCC5;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM482Processor : AISUCC5MessageProcessor<AISUCC5InboundEDIMessage, IM482Provider>
	{
		public IM482Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("165F5080-B09E-49C9-A33F-5C2E96BABDDA", "IM482: Documents Request");

		public override string GetLogicalStatus(AISUCC5InboundEDIMessage message, Messaging.IMessageAttachee messageAttachee, IM482Provider provider) => LogicalStatusList.Codes.Accepted;

		protected override void UpdateMessageAttacheeCore(Messaging.IMessageAttachee messageAttachee, IM482Provider provider)
		{
			if (messageAttachee is Messaging.IAISMessageAttachee aisMessageAttachee)
			{
				MessageProcessorHelper.PopulateRequestedDocuments(aisMessageAttachee.RequestedDocumentsProvider, provider.AdditionalInformations, provider.RequestDate, provider.DateLimit, EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestOpened);
			}
		}

		protected override Type MessageInterpreterType => typeof(IM482MessageInterpreter);
	}
}
