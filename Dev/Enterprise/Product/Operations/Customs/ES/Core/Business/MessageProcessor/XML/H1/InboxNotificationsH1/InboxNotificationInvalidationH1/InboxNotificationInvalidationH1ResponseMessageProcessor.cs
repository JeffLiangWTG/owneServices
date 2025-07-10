using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.ComunicaAnulacionV1Sal;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business;

public class InboxNotificationInvalidationH1ResponseMessageProcessor : XMLResponseMessageProcessor<ComunicaAnulacionV1Sal, IMessagePrettyFormatter>
{
	public InboxNotificationInvalidationH1ResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"Inbox Notification for Import H1 Invalidation Declaration Message Processor";

	protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameComunicaInvalidacionV1Sal;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.ImportH1InvalidationCommunication };

	protected sealed override ZString AcceptedResponseCode => ZString.Empty;
	protected sealed override ZBool IsOnlyAcceptedDeclaration => true;
	protected override ZBool IsInboxDeclaration => true;

	protected sealed override CusEntryHeader FindRelevantBusinessObjectCore(EDIMessage message, BusinessObject[] sentBusinessObjects, bool isDirectxTMessage)
	{
		return MessageProcessorHelper.GetRelevantBusinessObjectFromMRNCode<ComunicaAnulacionV1Sal>(message, XsdSchemaEmbeddedResourceName, sentBusinessObjects);
	}

	protected override IMessagePrettyFormatter GetNewMessagePrettyFormatter(ComunicaAnulacionV1Sal response, EDIMessage message, CusEntryHeader entryHeader) => new InboxNotificationInvalidationH1MessagePrettyFormatter(response);

	protected override ZString ProcessAcceptedDeclaration(ComunicaAnulacionV1Sal response, EDIMessage message, CusEntryHeader entryHeader)
	{
		var correctResponseData = response.ComunicaAnulacion.ImportOperation;
		if (correctResponseData != null)
		{
			entryHeader.CH_EntryStatus = correctResponseData.InvalidationInitiatedByCustoms.Equals("1")
				? EntryStatusCodes.Invalidated
				: EntryStatusCodes.Cancelled;
		}

		return ZString.Empty;
	}

	const string XsdSchemaNameComunicaInvalidacionV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.H1.Incoming.ComunicaAnulacionV1Sal.xsd";
}
