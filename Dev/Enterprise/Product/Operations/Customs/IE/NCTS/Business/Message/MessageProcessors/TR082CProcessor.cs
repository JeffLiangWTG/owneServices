using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	class TR082CProcessor : NCTSDepartureMessageProcessor<NCTSInboundEDIMessage, TR082CProvider>
	{
		public TR082CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("9A54FCE0-204A-462C-961E-E3AF65703EB5", "TR082C: DOCUMENTS REQUEST");

		public override string GetLogicalStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, TR082CProvider provider) => LogicalStatusList.Codes.Accepted;

		protected override Type MessageInterpreterType => typeof(TR082CMessageInterpreter);

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, TR082CProvider provider)
		{
			if (messageAttachee is NctsDepartureMovementHeader movementHeader && movementHeader.Header is NctsHeader nctsHeader)
			{
				PopulateRequestedDocuments(nctsHeader.RequestedDocuments, EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestOpened, provider.RequestDate, provider.DateLimit, provider.AdditionalInformations.Select(x => (x.DocumentType, x.DocumentComplementaryInformation)));
			}
		}

		public static void PopulateRequestedDocuments(EU.Business.RequestedDocumentCollection requestedDocumentCollection, ZString status, ZDateTime requestDate, ZDateTime dateLimit, IEnumerable<(ZString documentType, ZString documentComplementaryInformation)> additionalInformations)
		{
			var existingRequestedDocuments = requestedDocumentCollection.Cast<EU.Business.RequestedDocument>().GroupBy(x => x.CSI_Code.ToUpperInvariant()).ToDictionary(x => x.Key, y => y.ToList());
			foreach (var additionalInfo in additionalInformations)
			{
				var documentType = additionalInfo.documentType.ToUpperInvariant();
				if (existingRequestedDocuments.TryGetValue(documentType, out var list))
				{
					list.ForEach(existingRequestedDocument =>
					{
						existingRequestedDocument.CSI_DateOfIssue = requestDate;
						existingRequestedDocument.CSI_DateOfExpiry = dateLimit;
						existingRequestedDocument.CSI_Status = status;
					});
				}
				else
				{
					var newRequestedDocument = requestedDocumentCollection.AddNew();
					newRequestedDocument.CSI_Code = documentType;
					newRequestedDocument.RequestInformation = additionalInfo.documentComplementaryInformation;
					newRequestedDocument.CSI_DateOfIssue = requestDate;
					newRequestedDocument.CSI_DateOfExpiry = dateLimit;
					newRequestedDocument.CSI_Status = status;
				}
			}
		}
	}
}
