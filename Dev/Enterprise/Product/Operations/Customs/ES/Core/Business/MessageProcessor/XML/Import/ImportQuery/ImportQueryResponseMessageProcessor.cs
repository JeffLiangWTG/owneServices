using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.ConsultaImportacionV2Sal;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.Incoming;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.TD;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business;

public class ImportQueryResponseMessageProcessor : ImportGenericResponseMessageProcessor<ConsultaImportacionV2Sal>
{
	public ImportQueryResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	const string CancelledDeclarationStatus = "A";
	const string AccountingStatusForWriteOff = "IT";
	ZString NotWrittenOffWrongStatusText => Res.GetString("70CCB354-6C33-4A0E-8B31-673C104A57D6", "Not Written Off (status not IT)");

	protected override string MessageFriendlyNameCore => (NoResString)"Import Query Declaration Message Processor";

	protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameConsultaImportacionV2Sal;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.ImportQuery };

	protected override ZBool IsQueryMessage => true;

	protected override CusEntryHeader FindRelevantBusinessObjectCore(EDIMessage message, BusinessObject[] sentBusinessObjects, bool isDirectxTMessage)
	{
		var businessObject = base.FindRelevantBusinessObjectCore(message, sentBusinessObjects, isDirectxTMessage);
		return businessObject ?? MessageProcessorHelper.GetRelevantBusinessObjectFromMRNCode<ConsultaImportacionV2Sal>(message, XsdSchemaEmbeddedResourceName, sentBusinessObjects);
	}

	protected override IMessagePrettyFormatter GetNewMessagePrettyFormatter(ConsultaImportacionV2Sal response, EDIMessage message, CusEntryHeader entryHeader) => new ImportQueryMessagePrettyFormatter(response, entryHeader);

	protected override ZString ProcessAcceptedDeclaration(ConsultaImportacionV2Sal response, EDIMessage message, CusEntryHeader entryHeader)
	{
		var extraDataForPrettyFormatter = ZString.Empty;

		var procedimientoSolicitado = response.C012ProcedimientoSolicitado.IsNullOrEmpty() ? null : response.C012ProcedimientoSolicitado;
		var procedureCode = procedimientoSolicitado ?? entryHeader.EntryInstruction?.CEI_SubStyle ?? ZString.Empty;
		if (!importQueryProcedureTypeList.Contains(procedureCode))
		{
			throw new InvalidOperationException(Res.GetString("511AAC5F-A5B8-473C-882F-96794F771883", "Unexpected response {0}/Entry Instruction value {1}.", "C012ProcedimientoSolicitado", procedureCode));
		}

		UpdateEntryInstructionSubStyleToXOrY(entryHeader, procedimientoSolicitado);

		SetReleaseData(response, entryHeader);
		TryGetCircuitCode(response.Circuito, response.CircuitoValueSpecified, out var circuit);

		SetCircuitAndPaymentData(entryHeader, response, circuit);

		ZDateTime.TryParseExact(response.FechaAdmision, out var acceptanceDate, CustomsDateTimeExtension.DateFormat);

		var entryStatus = GetEntryStatus(response, entryHeader);
		SetGenericAcceptedDeclarationData(response, entryHeader, acceptanceDate, entryStatus, false, response.Administracion);
		entryHeader.CH_EntryStatus = entryStatus;
		SetCSVData(response, entryHeader, message);
		TriggerDocument031CaptureInGreenCircuitWithNoCsvClearance(entryHeader, message);

		ResetGuaranteesAmountAndAddTransactions(response, entryHeader);

		var outgoingMessageSubType = MessageProcessorHelper.GetOutgoingMessage(message)?.EM_MessageSubType ?? ZString.Empty;
		if (outgoingMessageSubType == DeclarationMessageSubTypeList.Codes.Guarantee)
		{
			var dataFromWriteOff = GetDataFromWriteOffAndAddGuaranteesWriteOffIfNeeded(entryHeader, response.EstadoContable);
			extraDataForPrettyFormatter += ExtraDataFromProcessing.GuaranteeStatusPrefixForPrettyFormatter + dataFromWriteOff + ExtraDataFromProcessing.SymbolToSeparateExtraDataForPrettyFormatter;
		}

		ProcessResponseLines(response.InformacionDePartida, entryHeader, acceptanceDate);

		CommonProcessCancelationResponse(entryHeader, entryHeader.MovementReferenceNumberIssueDate, TransactionsCommentPrefix, CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse);
		return extraDataForPrettyFormatter;
	}

	ZString GetEntryStatus(ConsultaImportacionV2Sal response, CusEntryHeader entryHeader) => response.EstadoDelDespacho == CancelledDeclarationStatus ? EntryStatusCodes.Cancelled : GetEntryStatusNotCancelled(response, entryHeader);

	ZString GetEntryStatusNotCancelled(ConsultaImportacionV2Sal response, CusEntryHeader entryHeader)
	{
		return response.TipoDeDeclaracion switch
		{
			ImportQueryDeclarationType.PDI => EntryStatusCodes.IncompletePreDeclaration,
			ImportQueryDeclarationType.PDS or ImportQueryDeclarationType.PDC => EntryStatusCodes.PreDeclarationAccepted,
			_ when !response.CsVdeLevante.IsNullOrEmpty() => GetEntryStatusWhenCSVLevante(response, entryHeader),
			_ when response.Circuito == CircuitoTd.A => EntryStatusCodes.ClearedWithPendingDocuments,
			_ => EntryStatusCodes.CustomsDeclarationAccepted
		};
	}

	ZString GetEntryStatusWhenCSVLevante(ConsultaImportacionV2Sal response, CusEntryHeader entryHeader)
	{
		var procedimientoSolicitado = response.C012ProcedimientoSolicitado.IsNullOrEmpty() ? null : response.C012ProcedimientoSolicitado;
		return GetEntryStatusFromProcedure(response, procedimientoSolicitado ?? entryHeader.EntryInstruction?.CEI_SubStyle ?? ZString.Empty);
	}

	ZString GetEntryStatusFromProcedure(ConsultaImportacionV2Sal response, ZString procedureCode)
	{
		return (string)procedureCode switch
		{
			ImportQueryProcedureType.B or ImportQueryProcedureType.C => EntryStatusCodes.ClearedWithPendingComplementaryDeclarations,
			ImportQueryProcedureType.A or ImportQueryProcedureType.X or ImportQueryProcedureType.Y => EntryStatusCodes.Cleared,
			ImportQueryProcedureType.Z => GetEntryStatusFromProcedureZ(response),
			_ => ZString.Empty,
		};
	}

	ZString GetEntryStatusFromProcedureZ(ConsultaImportacionV2Sal response)
	{
		return response.InformacionDePartida != null
				&& response.InformacionDePartida.Any(x =>
					x.C44DocumentoJustificativo != null
					&& x.C44DocumentoJustificativo.Any()
				)
			? EntryStatusCodes.Cleared
			: EntryStatusCodes.ClearedWithPendingComplementaryDeclarations;
	}

	void SetCSVData(ConsultaImportacionV2Sal response, CusEntryHeader entryHeader, EDIMessage message)
	{
		if (!string.IsNullOrEmpty(response.CsVdelCertificadoDeImportacion))
		{
			entryHeader.ZG_CSVImportCertificate = response.CsVdelCertificadoDeImportacion;
		}

		if (!string.IsNullOrEmpty(response.CsVdeLevante))
		{
			var csvClearance = response.CsVdeLevante;
			SetCSVClearanceAndTriggerDocumentRequest(entryHeader, message, csvClearance);
		}
	}

	void SetReleaseData(ConsultaImportacionV2Sal response, CusEntryHeader entryHeader)
	{
		if (!string.IsNullOrEmpty(response.FechaLevante))
		{
			ZDateTime.TryParseExact(response.FechaLevante, out var entryReleaseDate, CustomsDateTimeExtension.DateFormat);
			entryHeader.CH_EntryReleaseDate = entryReleaseDate;
		}
	}

	protected override void RemoveFeesBeforeAdding(CusEntryLine entryLine, string administration)
	{
		var feesToRemove = new List<CusEntryLineFee>();
		if (administration == CustomsAdministration.AEAT)
		{
			feesToRemove = entryLine.Fees.Cast<CusEntryLineFee>().Where(fee => !(fee.CF_ChargeType.StartsWith("3") || fee.CF_ChargeType.StartsWith("4"))).ToList();
		}
		else
		{
			feesToRemove = entryLine.Fees.Cast<CusEntryLineFee>().Where(fee => fee.CF_ChargeType.StartsWith("3") || fee.CF_ChargeType.StartsWith("4")).ToList();
		}
		foreach (var fee in feesToRemove)
		{
			entryLine.Fees.RemoveAndDelete(fee);
		}
	}

	void SetCircuitAndPaymentData(CusEntryHeader entryHeader, ConsultaImportacionV2Sal response, ZString circuit)
	{
		var paymentProofNumber = response.NumJustificantePago;

		if (!ZDateTime.TryParseExact(response.FechaLimitePago, out var limitPaymentDate, CustomsDateTimeExtension.DateFormat)
			|| limitPaymentDate.IsEmpty)
		{
			limitPaymentDate = ZDateTime.Empty;
		}

		if (response.Administracion == CustomsAdministration.AEAT)
		{
			entryHeader.SetMovementReferenceNumberEntryStatus(circuit);
			entryHeader.ZG_PaymentProofNumber = paymentProofNumber;
			entryHeader.ZG_LimitPaymentDate = limitPaymentDate;
		}
		else
		{
			entryHeader.SetCircuitCan(circuit);
			entryHeader.ZG_ATCPaymentProofNumber = paymentProofNumber;
			entryHeader.ZG_ATCLimitPaymentDate = limitPaymentDate;
		}
	}

	ZString GetDataFromWriteOffAndAddGuaranteesWriteOffIfNeeded(CusEntryHeader entryHeader, string accountingStatus)
	{
		if (accountingStatus == AccountingStatusForWriteOff)
		{
			var writeOffTransactionCreator = new WriteOffTransactionCreator();
			return writeOffTransactionCreator.AddGuaranteesWriteOffTransactionsEntryHeader(entryHeader, Logger);
		}
		return NotWrittenOffWrongStatusText;
	}

	void ProcessResponseLines(IEnumerable<PartidaTd> informacionDePartida, CusEntryHeader entryHeader, ZDateTime acceptanceDate)
	{
		if (informacionDePartida is null)
		{
			return;
		}

		foreach (var entryLine in entryHeader.AllEntryLines)
		{
			var item = informacionDePartida.FirstOrDefault(x => x.C32NumeroDePartida == entryLine.CL_LineNumber);
			if (item != null)
			{
				ProcessEntryLineSupportingDocuments(item, entryLine, acceptanceDate);
				ProcessEntryLinePreviousDocuments(item, entryLine);
			}
		}
	}

	void ProcessEntryLinePreviousDocuments(PartidaTd responseLine, CusEntryLine entryLine)
	{
		var documentsToRemove = entryLine.GetPreviouslySentPreviousDocuments();

		foreach (var docu in documentsToRemove)
		{
			docu.Delete();
		}

		if (responseLine.C40DocumentoCargoPrecedente != null)
		{
			AddNewPreviousDocumentFromC40DocumentoCargoPrecedente(responseLine.C40DocumentoCargoPrecedente, entryLine);
		}
		var docSumariaDatadaSUM = responseLine.C40SumariaDatada?.FirstOrDefault(doc => doc.C40SumariaClase == MaritimeSummaryDeclaration);
		if (docSumariaDatadaSUM != null)
		{
			AddNewPreviousDocumentFromC40SumariaDatada(docSumariaDatadaSUM, entryLine);
		}
	}

	void ProcessEntryLineSupportingDocuments(PartidaTd responseLine, CusEntryLine entryLine, ZDateTime acceptanceDate)
	{
		var documentsToRemove = entryLine.GetPreviouslySentSupportingDocuments();

		foreach (var docu in documentsToRemove)
		{
			docu.Delete();
		}

		responseLine.C44DocumentosYCertificados?.ForEach(doc => AddNewSupportingDocument(doc, entryLine, acceptanceDate, DocumentStatus.Accepted));

		responseLine.C44CertificadoAportadoOrganismo?.ForEach(doc => AddNewSupportingDocument(doc, entryLine, acceptanceDate, DocumentStatus.Accepted));

		responseLine.C44CertificadoAportadoOperador?.ForEach(doc => AddNewSupportingDocument(doc, entryLine, acceptanceDate, DocumentStatus.Accepted));

		if (responseLine.C44DocumentoJustificativo != null)
		{
			foreach (var doc in responseLine.C44DocumentoJustificativo)
			{
				var docProcedure = doc.C44IndicadorAcordeRegularizar;
				var status = docProcedure == SupportingDocumentProcedure.Accord
											|| docProcedure == SupportingDocumentProcedure.Regularize
							? DocumentStatus.Accepted
							: DocumentStatus.Cancelled;

				AddNewSupportingDocument(doc, entryLine, acceptanceDate, status);
			}
		}
	}

	void AddNewPreviousDocumentFromC40DocumentoCargoPrecedente(Cas40Td doc, CusEntryLine entryLine)
	{
		var document = entryLine.Factory.New<PreviousDocument>();
		document.CSI_Code = doc.C40ClaseDocumento;
		document.CSI_SubType = doc.C40TipoDocumento;
		document.CSI_ReferenceNumber = doc.C40ReferenciaDocumento;
		document.CSI_Status = DocumentStatus.Accepted;
		document.CSI_ParentID = entryLine.PK;
		document.CSI_ParentTableCode = entryLine.TablePrefix;
	}

	void AddNewPreviousDocumentFromC40SumariaDatada(Cas40SumariaDatadaTd doc, CusEntryLine entryLine)
	{
		var reference = (ZString)doc.C40SumariaReferencia;
		var document = entryLine.Factory.New<PreviousDocument>();
		document.CSI_Code = doc.C40SumariaClase;
		document.CSI_SubType = "X";
		document.CSI_ReferenceNumber = reference.SubstringSafe(0, reference.Length - 5);
		document.CSI_LineNo = Convert.ToInt32(reference.SubstringSafe(reference.Length - 5, 5));
		document.CSI_Status = DocumentStatus.Accepted;
		document.CSI_ParentID = entryLine.PK;
		document.CSI_ParentTableCode = entryLine.TablePrefix;
	}

	void AddNewSupportingDocument(IQueryDocument doc, CusEntryLine entryLine, ZDateTime acceptanceDate, ZString status)
	{
		var document = entryLine.Factory.New<SupportingDocument>();
		document.CSI_Code = doc.Type;
		document.CSI_ReferenceNumber = doc.Reference;

		var docDateCorrect = ZDateTime.TryParseExact(doc.Date, out var docDate, CustomsDateTimeExtension.DateFormatSpain);
		docDate = docDateCorrect && !docDate.IsEmpty ? docDate : ZDateTime.Empty;
		if (docDate <= acceptanceDate)
		{
			document.CSI_DateOfIssue = docDate;
		}
		else
		{
			document.CSI_DateOfExpiry = docDate;
		}
		document.CSI_Quantity = doc.Quantity;
		document.CSI_UnitOfQuantity = ((ZString)doc.Unit).ConvertESToCargoWise(document.Factory);
		document.CSI_Status = status;
		document.CSI_ParentID = entryLine.PK;
		document.CSI_ParentTableCode = entryLine.TablePrefix;
	}

	readonly ImmutableHashSet<ZString> importQueryProcedureTypeList = ImmutableHashSet.Create<ZString>(ImportQueryProcedureType.A, ImportQueryProcedureType.B, ImportQueryProcedureType.C, ImportQueryProcedureType.X, ImportQueryProcedureType.Y, ImportQueryProcedureType.Z);

	const string XsdSchemaNameConsultaImportacionV2Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.Import.Incoming.ConsultaImportacionV2Sal.xsd";
	const string MaritimeSummaryDeclaration = "SUM";

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	const string TransactionsCommentPrefix = "DUA:";
}
