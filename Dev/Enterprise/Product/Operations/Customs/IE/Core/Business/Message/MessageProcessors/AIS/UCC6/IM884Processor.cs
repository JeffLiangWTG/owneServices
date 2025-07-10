using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM884Processor : AISMessageProcessor<AISInboundEDIMessage, IM884Provider>
	{
		public IM884Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("4F7DE579-5476-44FD-A59E-3B982950F977", "IM884: Documents Presentation Request Cancellation");

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, IM884Provider provider)
		{
			if (messageAttachee is IAISMessageAttachee aisMessageAttachee)
			{
				foreach (var requestedDocument in aisMessageAttachee.RequestedDocumentsProvider.RequestedDocuments.Where(x => x.CSI_Status == RequestedDocumentStatusList.Codes.PhysicallyPresentDocument))
				{
					requestedDocument.CSI_Status = RequestedDocumentStatusList.Codes.RequestCancelled;
				}
			}
		}

		protected override Type MessageInterpreterType => typeof(IM884MessageInterpreter);
	}
}
