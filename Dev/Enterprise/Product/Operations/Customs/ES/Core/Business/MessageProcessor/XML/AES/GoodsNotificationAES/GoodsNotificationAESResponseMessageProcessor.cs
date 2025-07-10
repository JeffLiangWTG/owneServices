using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CC511C_v514.CC511CV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business;

public class GoodsNotificationAESResponseMessageProcessor : AESCommonResponseMessageProcessor<Cc511Cv1Sal>
{
	public GoodsNotificationAESResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver) : base(logger, eDocsSaver)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"Export Notification Declaration Message Processor";

	protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameCC511CV1Sal;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.ExportNotification };

	protected override IMessagePrettyFormatter GetNewMessagePrettyFormatter(Cc511Cv1Sal response, EDIMessage message, CusEntryHeader entryHeader) => new GoodsNotificationAESMessagePrettyFormatter(response);

	protected override ZString ProcessAcceptedDeclaration(Cc511Cv1Sal response, EDIMessage message, CusEntryHeader entryHeader)
	{
		var correctResponseData = response.DatosRespuestaCorrecta;
		if (correctResponseData != null)
		{
			SetMovementReferenceNumberAndCircuitCan(entryHeader, response.PreparationDateAndTime, correctResponseData.CircuitoAeat, correctResponseData.CircuitoAtc);

			if (correctResponseData.FechaLevante != null)
			{
				entryHeader.CH_EntryReleaseDate = response.ControlRespuesta.CodigoRespuesta switch
				{
					ResponseCodeL => (ZDateTime)response.PreparationDateAndTime,
					_ => (ZDateTime)correctResponseData.FechaLevante
				};
			}

			entryHeader.ZG_CSVT2L = correctResponseData.CsvDocumentoT2L;
			var csvClearance = correctResponseData.CsvLevanteExportacion;
			SetCSVClearanceAndTriggerDocumentRequest(entryHeader, message, csvClearance);
			entryHeader.IndirectExport = correctResponseData.FlagDirectaIndirecta == IndirectFlagCode;
		}

		SetEntryStatus(response, entryHeader);

		TriggerInboxRequestsAES(message, entryHeader);

		return ZString.Empty;
	}

	void SetEntryStatus(Cc511Cv1Sal response, CusEntryHeader entryHeader)
	{
		ZString entryStatus = response.ControlRespuesta.CodigoRespuesta switch
		{
			ResponseCodeL when entryHeader.EntryInstruction?.IsSubStyleBOrC == true => EntryStatusCodes.ClearedWithPendingComplementaryDeclarations,
			ResponseCodeL => EntryStatusCodes.Cleared,
			ResponseCodeB => EntryStatusCodes.CustomsDeclarationAccepted,
			ResponseCodeC => EntryStatusCodes.PendingForEuOffice,
			_ => ZString.Empty
		};

		if (!entryStatus.IsEmpty)
		{
			entryHeader.CH_EntryStatus = entryStatus;
		}
	}

	protected override void ProcessRejectedDeclaration(Cc511Cv1Sal response, EDIMessage message, CusEntryHeader entryHeader)
	{
		if (EU.Business.TemporaryStorageHelper.IsTemporaryStorageRegisterEnabled(entryHeader.CountryCode))
		{
			EU.Business.TemporaryStorageHelper.CancelPendingRegLineTransactions(message.Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType);
		}
	}

	const string XsdSchemaNameCC511CV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.AES.Incoming.CC511CV1Sal.xsd";
}
