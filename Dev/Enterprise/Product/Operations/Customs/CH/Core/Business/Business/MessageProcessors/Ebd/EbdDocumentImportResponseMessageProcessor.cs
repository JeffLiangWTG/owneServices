using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Ebd;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using static CargoWise.EventReference.Constants;

namespace Enterprise.Customs.CH.Business;

internal class EbdDocumentImportResponseMessageProcessor : BaseResponseMessageProcessor<IDocumentImportResponseDetail>
{
	public EbdDocumentImportResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => Res.GetString("75B8B5FF-526B-4A6B-827C-ED09CD5881C1", "Customs EBD Message Response Processor");

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeCodeList.Codes.EBD };

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.Accepted, MessageSubTypeCodeList.Codes.CustomsRejected };

	protected override BusinessObject FindLinkedObject(EDIMessage message, IDocumentImportResponseDetail xmlObject) => FindLinkedObjectByOutgoingSessionID(message);

	protected override void ProcessResponseMessage(CHEDIMessage message, IDocumentImportResponseDetail customsResponse)
	{
		if (customsResponse != null && message.EM_LinkedObject is CusEntryHeader entryHeader)
		{
			foreach (var accompanyingDocument in customsResponse.AccompanyingDocuments)
			{
				var reference = StmALog.GenerateEventReference(ZString.Empty, new Dictionary<string, string>()
					{
						{ EventReferenceParameters.Codes.File, accompanyingDocument.Filename },
					});

				if (customsResponse.IsAcceptance)
				{
					entryHeader.Logs.AddNew(Events.DocumentDelivered, reference, customsResponse.DateAndTime);
				}
				else if (customsResponse.IsRejection)
				{
					entryHeader.Logs.AddNew(Events.DocumentNotDelivered, reference, customsResponse.DateAndTime);
				}
			}
		}
	}
}
