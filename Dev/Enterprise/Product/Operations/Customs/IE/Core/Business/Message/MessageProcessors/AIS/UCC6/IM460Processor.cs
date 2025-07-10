using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using AIS_H7_Version1_0 = CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM460;
using AISVersion2_0 = CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM460;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM460Processor : AISMessageProcessor<AISInboundEDIMessage, IIM460Provider>
	{
		public IM460Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("21F6B7B7-DEB9-4C8D-B72D-48D570419487", "IM460 – Control Notice");

		public override string GetLogicalStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IIM460Provider provider) => MessageInterpreterType == typeof(UCC6.V1.IM460MessageInterpreter) ? LogicalStatusList.Codes.Accepted : null;

		public override string GetEntryStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IIM460Provider provider) => AISEntryStatusList.Codes.Control;

		protected override Type MessageInterpreterType
		{
			get
			{
				if (xmlObjectType.IsAssignableFrom(typeof(AIS_H7_Version1_0.Im460)))
				{
					return typeof(UCC6.V1.IM460MessageInterpreter);
				}
				else if (xmlObjectType.IsAssignableFrom(typeof(AISVersion2_0.Im460)))
				{
					return typeof(IM460MessageInterpreter);
				}
				else
				{
					throw new InvalidOperationException("Unknown provider type.");
				}
			}
		}

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, IIM460Provider provider)
		{
			if (messageAttachee is IAISMessageAttachee aisMessageAttachee && provider is IM460Provider aisV2Provider)
			{
				MessageProcessorHelper.PopulateRequestedDocuments(aisMessageAttachee.RequestedDocumentsProvider, aisV2Provider.RequestedDocuments, aisV2Provider.NotificationDate, aisV2Provider.AnticipatedControlDate, EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestOpened);
			}
		}
	}
}
