using System;
using System.Linq;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class TR882CProcessor : NCTSDepartureMessageProcessor<NCTSInboundEDIMessage, TR882CProvider>
	{
		public TR882CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("B10D8858-AC84-404D-835C-B56A57FFC2F2", "TR882C: DOCUMENT UPLOAD REQUEST CANCELLATION");

		public override string GetLogicalStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, TR882CProvider provider) => LogicalStatusList.Codes.Accepted;

		protected override Type MessageInterpreterType => typeof(TR882CMessageInterpreter);

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, TR882CProvider provider)
		{
			if (messageAttachee is NctsDepartureMovementHeader movementHeader && movementHeader.Header is NctsHeader nctsHeader)
			{
				PopulateRequestedDocuments(nctsHeader);
			}
		}

		void PopulateRequestedDocuments(NctsHeader nctsHeader)
		{
			var requestedDocumentsNeedUpdate = nctsHeader.RequestedDocuments.Cast<EU.Business.RequestedDocument>().Where(p => p.CSI_Status.EqualsIgnoringCase(EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestOpened));

			foreach (var requestedDocument in requestedDocumentsNeedUpdate)
			{
				requestedDocument.CSI_Status = EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestCancelled;
			}
		}
	}
}
