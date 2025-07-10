using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.Messaging;
using AIS_H7_Version1_0 = CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM484;
using AISVersion2_0 = CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM484;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM484Processor : AISMessageProcessor<AISInboundEDIMessage, IIM484Provider>
	{
		public IM484Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("E8B263F1-48C8-41AE-9101-8455A9AFD8B6", "IM484: Document Presentation Request");

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, IIM484Provider provider)
		{
			if (messageAttachee is IAISMessageAttachee aisMessageAttachee)
			{
				MessageProcessorHelper.PopulateRequestedDocuments(aisMessageAttachee.RequestedDocumentsProvider, provider.AdditionalInformations, provider.RequestDate, provider.DateLimit, EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.PhysicallyPresentDocument);
			}
		}

		protected override Type MessageInterpreterType
		{
			get
			{
				if (xmlObjectType.IsAssignableFrom(typeof(AIS_H7_Version1_0.Im484)))
				{
					return typeof(UCC6.V1.IM484MessageInterpreter);
				}
				else if (xmlObjectType.IsAssignableFrom(typeof(AISVersion2_0.Im484)))
				{
					return typeof(IM484MessageInterpreter);
				}
				else
				{
					throw new InvalidOperationException("Unknown provider type.");
				}
			}
		}
	}
}
