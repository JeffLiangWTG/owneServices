using System;
using System.Linq;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class TR084CProcessor : NCTSDepartureMessageProcessor<NCTSInboundEDIMessage, TR084CProvider>
	{
		public TR084CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("3B976997-D959-4907-8EB8-E921AF23A68E", "TR084C: REQUEST DOCUMENT PRESENTATION");

		public override string GetLogicalStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, TR084CProvider provider) => LogicalStatusList.Codes.Accepted;

		protected override Type MessageInterpreterType => typeof(TR084CMessageInterpreter);

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, TR084CProvider provider)
		{
			if (messageAttachee is NctsDepartureMovementHeader movementHeader && movementHeader.Header is NctsHeader nctsHeader)
			{
				TR082CProcessor.PopulateRequestedDocuments(nctsHeader.RequestedDocuments, EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.PhysicallyPresentDocument, provider.RequestDate, provider.DateLimit, provider.AdditionalInformations.Select(x => (x.DocumentType, x.DocumentComplementaryInformation)));
			}
		}
	}
}
