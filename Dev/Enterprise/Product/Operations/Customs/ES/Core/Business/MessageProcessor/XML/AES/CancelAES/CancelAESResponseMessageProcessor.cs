using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CC514C_v514.CC514CV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business;

public class CancelAESResponseMessageProcessor : AESCommonResponseMessageProcessor<Cc514Cv1Sal>
{
	public CancelAESResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	const string CancellationOKCode = "I";
	const string ConsultCustomsCode = "C";

	protected override string MessageFriendlyNameCore => (NoResString)"Export Cancel Declaration Message Processor";

	protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameCC514CV1Sal;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { DeclarationMessageTypeList.Codes.ExportCancellation };

	protected override IMessagePrettyFormatter GetNewMessagePrettyFormatter(Cc514Cv1Sal response, EDIMessage message, CusEntryHeader entryHeader) => new CancelAESMessagePrettyFormatter(response);

	protected override ZString ProcessAcceptedDeclaration(Cc514Cv1Sal response, EDIMessage message, CusEntryHeader entryHeader)
	{
		var responseCode = response.ControlRespuesta.CodigoRespuesta;
		if (responseCode == CancellationOKCode)
		{
			entryHeader.CH_EntryStatus = EntryStatusCodes.Cancelled;
		}
		else if (responseCode == ConsultCustomsCode)
		{
			TriggerInboxRequest(entryHeader, message, new ZString[] { DeclarationMessageTypeList.Codes.ExportInvalidationCommunication });
		}

		CommonProcessCancelationResponse(entryHeader, response.PreparationDateAndTime, TransactionsAESCommentPrefix);

		return ZString.Empty;
	}

	const string XsdSchemaNameCC514CV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.AES.Incoming.CC514CV1Sal.xsd";
}
