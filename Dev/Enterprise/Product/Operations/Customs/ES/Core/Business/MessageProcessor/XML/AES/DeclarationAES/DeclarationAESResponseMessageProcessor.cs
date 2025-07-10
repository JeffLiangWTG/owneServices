using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CC515C_v514.CC515CV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business;

public class DeclarationAESResponseMessageProcessor : AESCommonResponseMessageProcessor<Cc515Cv1Sal>
{
	public DeclarationAESResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver) : base(logger, eDocsSaver)
	{
	}

	const string ResponseCodeP = "P";

	protected override string MessageFriendlyNameCore => (NoResString)"Export Declaration Message Processor";

	protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameCC515CV1Sal;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.ExportUcc6, Messaging.DeclarationMessageTypeList.Codes.ExportPreDeclaration };

	protected override IMessagePrettyFormatter GetNewMessagePrettyFormatter(Cc515Cv1Sal response, EDIMessage message, CusEntryHeader entryHeader) => new DeclarationAESMessagePrettyFormatter(response);

	protected override ZString ProcessAcceptedDeclaration(Cc515Cv1Sal response, EDIMessage message, CusEntryHeader entryHeader)
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
			entryHeader.IndirectExport = correctResponseData.FlagDirectaIndirecta == IndirectFlagCode;
			SetCSVClearanceAndTriggerDocumentRequest(entryHeader, message, correctResponseData.CsvLevanteExportacion);
		}

		UpdateEntryInstructionSubStyleFromCToY(entryHeader, message);

		SetEntryStatusAndTriggerRequestIfNeeded(response, entryHeader, message);

		ProcessDocuments(entryHeader);

		TriggerInboxRequestsAES(message, entryHeader);

		return ZString.Empty;
	}

	void SetEntryStatusAndTriggerRequestIfNeeded(Cc515Cv1Sal response, CusEntryHeader entryHeader, EDIMessage message)
	{
		ZString entryStatus = response.ControlRespuesta.CodigoRespuesta switch
		{
			ResponseCodeP => EntryStatusCodes.PreDeclarationAccepted,
			ResponseCodeL => GetEntryStatusResponseCodeL(entryHeader),
			ResponseCodeB => EntryStatusCodes.CustomsDeclarationAccepted,
			ResponseCodeC => EntryStatusCodes.PendingForEuOffice,
			_ => ZString.Empty
		};

		if (!entryStatus.IsEmpty)
		{
			entryHeader.CH_EntryStatus = entryStatus;
		}

		if (response.ControlRespuesta.CodigoRespuesta == ResponseCodeL)
		{
			TriggerRequestIfNeeded(entryHeader, message);
		}
	}

	string GetEntryStatusResponseCodeL(CusEntryHeader entryHeader)
	{
		var entrySubStyleIsBOrC = entryHeader.EntryInstruction?.IsSubStyleBOrC ?? false;
		var entrySubStyleIsZ = (entryHeader.EntryInstruction?.CEI_SubStyle ?? ZString.Empty) == EntrySubStyleList.Codes.Z;

		return entrySubStyleIsZ
				? EntryStatusCodes.EffectiveDeparture
				: entrySubStyleIsBOrC
					? EntryStatusCodes.ClearedWithPendingComplementaryDeclarations
					: EntryStatusCodes.Cleared;
	}

	void TriggerRequestIfNeeded(CusEntryHeader entryHeader, EDIMessage message)
	{
		if (entryHeader.CH_EntryStatus == EntryStatusCodes.EffectiveDeparture)
		{
			var certificateName = MessageProcessorHelper.GetInterchangeCertificateName(message, Logger);
			MessageRequest.CreateEDIMessageForEffectiveDepCertRequest(message.Factory, entryHeader, certificateName);
		}
	}

	protected override void ProcessRejectedDeclaration(Cc515Cv1Sal response, EDIMessage message, CusEntryHeader entryHeader)
	{
		if (EU.Business.TemporaryStorageHelper.IsTemporaryStorageRegisterEnabled(entryHeader.CountryCode))
		{
			EU.Business.TemporaryStorageHelper.CancelPendingRegLineTransactions(message.Factory, entryHeader.TemporaryStorageTransactionInternalReferenceNumber, entryHeader.TemporaryStorageTransactionInternalReferenceType);
		}
	}

	const string XsdSchemaNameCC515CV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.AES.Incoming.CC515CV1Sal.xsd";
}
