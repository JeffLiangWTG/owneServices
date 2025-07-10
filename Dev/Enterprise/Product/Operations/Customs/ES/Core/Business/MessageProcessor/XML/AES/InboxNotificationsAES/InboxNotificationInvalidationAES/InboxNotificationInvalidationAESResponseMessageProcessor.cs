using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ComunicaInvalidacionV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business;

public class InboxNotificationInvalidationAESResponseMessageProcessor : AESCommonInboxNotificationResponseMessageProcessor<ComunicaInvalidacionV1Sal>
{
	public InboxNotificationInvalidationAESResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"Inbox Notification Export Invalidation Declaration Message Processor";

	protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameComunicaInvalidacionV1Sal;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.ExportInvalidationCommunication };

	protected override IMessagePrettyFormatter GetNewMessagePrettyFormatter(ComunicaInvalidacionV1Sal response, EDIMessage message, CusEntryHeader entryHeader) => new InboxNotificationInvalidationAESMessagePrettyFormatter(response);

	protected override ZString ProcessAcceptedDeclaration(ComunicaInvalidacionV1Sal response, EDIMessage message, CusEntryHeader entryHeader)
	{
		var correctResponseData = response.DatosComunicacion;
		if (correctResponseData != null)
		{
			entryHeader.CH_EntryStatus = correctResponseData.InvalidacionIniciadaPorLaAduana == Flag.Item1
				? EntryStatusCodes.Invalidated
				: EntryStatusCodes.Cancelled;
		}

		CommonProcessCancelationResponse(entryHeader, response.PreparationDateAndTime, TransactionsAESCommentPrefix);

		return ZString.Empty;
	}

	const string XsdSchemaNameComunicaInvalidacionV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.AES.Incoming.ComunicaInvalidacionV1Sal.xsd";
}
