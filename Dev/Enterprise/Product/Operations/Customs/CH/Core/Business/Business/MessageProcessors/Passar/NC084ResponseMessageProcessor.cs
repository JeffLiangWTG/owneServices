using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public class NC084ResponseMessageProcessor : PassarGetMessageAcknowledgeMessageProcessor<INC084ResponseDetail>
{
	public NC084ResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"NC084 - Document Notification Message Processor";

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.PassarDocumentNotification };

	internal protected override bool UpdateApplicationReference => false;

	protected override BusinessObject FindLinkedObject(EDIMessage message, INC084ResponseDetail xmlObject)
	{
		return MessageProcessorHelper.FindCusEntryNumParent(message.Company, xmlObject.GDRN);
	}

	protected override void ProcessResponseMessage(CHEDIMessage message, INC084ResponseDetail customsResponse)
	{
		var company = message.Company;
		var documentId = customsResponse.DocumentId;
		var docTransaction = company.LoadTransactionByDocumentId(documentId);

		if (docTransaction == null)
		{
			company.CreateDocumentDeliveryTransaction(ApplicationCodeList.Codes.CHCustomsCharteraOutput, documentId);
			message.EM_ApplicationReference = documentId;
		}
	}
}
