using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ComunicaDisconformeExporV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business;

public class InboxNotificationNonConformityAESResponseMessageProcessor : AESCommonInboxNotificationResponseMessageProcessor<ComunicaDisconformeExporV1Sal>
{
	public InboxNotificationNonConformityAESResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"Inbox Notification Export Non-Conformity Declaration Message Processor";

	protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameComunicaDisconformeExporV1Sal;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.ExportNonConformityCommunication };

	protected override IMessagePrettyFormatter GetNewMessagePrettyFormatter(ComunicaDisconformeExporV1Sal response, EDIMessage message, CusEntryHeader entryHeader) => new InboxNotificationNonConformityAESMessagePrettyFormatter(response);

	protected override ZString ProcessAcceptedDeclaration(ComunicaDisconformeExporV1Sal response, EDIMessage message, CusEntryHeader entryHeader)
	{
		entryHeader.CH_EntryStatus = EntryStatusCodes.Invalidated;

		CommonProcessCancelationResponse(entryHeader, response.PreparationDateAndTime, TransactionsAESCommentPrefix);

		return ZString.Empty;
	}

	const string XsdSchemaNameComunicaDisconformeExporV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.AES.Incoming.ComunicaDisconformeExporV1Sal.xsd";
}
