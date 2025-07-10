using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CCAESC_v514.CCAESCV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business;

public class QueryAESResponseMessageProcessor : AESCommonResponseMessageProcessor<Ccaescv1Sal>
{
	public QueryAESResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver) : base(logger, eDocsSaver)
	{
	}
	const string InvalidationCode = "1";

	protected override ZBool IsQueryMessage => true;

	protected override string MessageFriendlyNameCore => (NoResString)"Export Query Declaration Message Processor";

	protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameCCAESCV1Sal;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.ExportQuery };

	protected override IMessagePrettyFormatter GetNewMessagePrettyFormatter(Ccaescv1Sal response, EDIMessage message, CusEntryHeader entryHeader) => new QueryAESMessagePrettyFormatter(response);

	protected override ZString ProcessAcceptedDeclaration(Ccaescv1Sal response, EDIMessage message, CusEntryHeader entryHeader)
	{
		var oldEntryStatus = entryHeader.CH_EntryStatus;

		var correctResponseData = response.DatosRespuestaCorrecta;
		if (correctResponseData != null)
		{
			UpdateEntryInstructionSubStyleToXOrY(entryHeader, correctResponseData.ExportOperation?.AdditionalDeclarationType);

			var managementData = correctResponseData.AesDatosGestion;
			var acceptanceDate = managementData.FechaAdmision ?? ZDateTime.Empty;
			SetMovementReferenceNumberAndCircuitCan(entryHeader, acceptanceDate, managementData.CircuitoAeat, managementData.CircuitoAtc);

			if (managementData.FechaLevante != null)
			{
				entryHeader.CH_EntryReleaseDate = (ZDateTime)managementData.FechaLevante;
			}

			entryHeader.ZG_CSVT2L = managementData.CsvDocumentoT2L;
			entryHeader.IndirectExport = managementData.FlagDirectaIndirecta == IndirectFlagCode;

			SetCSVClearanceAndTriggerDocumentRequest(entryHeader, message, managementData.CsvLevanteExportacion);

			SetEntryStatusAndTriggerDocumentsIfNeeded(managementData, entryHeader, message, oldEntryStatus);

			if (correctResponseData.GoodsShipment != null)
			{
				ProcessResponseLines(correctResponseData.GoodsShipment.GoodsItem, entryHeader, acceptanceDate);

				ProcessEntryHeaderSupportingDocuments(correctResponseData.GoodsShipment, entryHeader, acceptanceDate);
				ProcessEntryHeaderAdditionalReference(correctResponseData.GoodsShipment, entryHeader);
				ProcessEntryHeaderAdditionalInformation(correctResponseData.GoodsShipment, entryHeader);
				ProcessEntryHeaderTransportDocument(correctResponseData.GoodsShipment, entryHeader);
			}
		}

		if (oldEntryStatus != entryHeader.CH_EntryStatus)
		{
			TriggerInboxRequestsAES(message, entryHeader);
		}

		CommonProcessCancelationResponse(entryHeader, response.PreparationDateAndTime, TransactionsAESCommentPrefix);

		return ZString.Empty;
	}

	void ProcessEntryHeaderAdditionalReference(GoodsShipmentType70 goodsShipment, CusEntryHeader entryHeader)
	{
		DeletePreviouslySentAdditionalInfos(entryHeader, AdditionalDocList.Codes.AdditionalReference);
		goodsShipment.AdditionalReference.ForEach(doc => AddNewAdditionalInfo(entryHeader, AdditionalDocList.Codes.AdditionalReference, doc.Type, doc.ReferenceNumber));
	}

	void ProcessEntryHeaderAdditionalInformation(GoodsShipmentType70 goodsShipment, CusEntryHeader entryHeader)
	{
		DeletePreviouslySentAdditionalInfos(entryHeader, AdditionalDocList.Codes.AdditionalInformation);
		goodsShipment.AdditionalInformation.ForEach(doc => AddNewAdditionalInfo(entryHeader, AdditionalDocList.Codes.AdditionalInformation, doc.Code, doc.Text));
	}

	void ProcessEntryHeaderTransportDocument(GoodsShipmentType70 goodsShipment, CusEntryHeader entryHeader)
	{
		DeletePreviouslySentAdditionalInfos(entryHeader, AdditionalDocList.Codes.TransportDocuments);
		goodsShipment.Consignment?.TransportDocument?.ForEach(doc => AddNewAdditionalInfo(entryHeader, AdditionalDocList.Codes.TransportDocuments, doc.Type, doc.ReferenceNumber));
	}

	void ProcessEntryHeaderSupportingDocuments(GoodsShipmentType70 goodsShipment, CusEntryHeader entryHeader, ZDateTime acceptanceDate)
	{
		var docsToRemove = entryHeader.GetPreviouslySentSupportingDocuments();
		foreach (var doc in docsToRemove)
		{
			doc.Delete();
		}

		goodsShipment.SupportingDocument.ForEach(doc => AddNewSupportingDocument(entryHeader, doc.Type, doc.ReferenceNumber, acceptanceDate, doc.DocumentLineItemNumber, doc.IssuingAuthorityName, doc.ValidityDate));
	}

	void SetEntryStatusAndTriggerDocumentsIfNeeded(AesDatosGestion70 managementData, CusEntryHeader entryHeader, EDIMessage message, ZString oldEntryStatus)
	{
		ZString newEntryStatus = managementData.EstadoAes switch
		{
			AESStatusCodeList.Codes.PendingPresentationOfGoods => EntryStatusCodes.PreDeclarationAccepted,
			AESStatusCodeList.Codes.PreDeclarationInvalidated or AESStatusCodeList.Codes.NotCleared => EntryStatusCodes.Invalidated,
			AESStatusCodeList.Codes.PendingClearance => EntryStatusCodes.CustomsDeclarationAccepted,
			AESStatusCodeList.Codes.DeclarationCancelled => EntryStatusCodes.Cancelled,
			AESStatusCodeList.Codes.EffectiveExit => EntryStatusCodes.EffectiveDeparture,
			AESStatusCodeList.Codes.StopAtExit => EntryStatusCodes.GoodsStoppedAtDeparture,
			AESStatusCodeList.Codes.WaitingPcoDecision => EntryStatusCodes.PendingForEuOffice,
			AESStatusCodeList.Codes.Invalidated when managementData.InvalidacionIniciadaPorLaAduana == InvalidationCode => EntryStatusCodes.Invalidated,
			AESStatusCodeList.Codes.Invalidated => EntryStatusCodes.Cancelled,
			_ => GetEntryStatusFromCSVClearance(managementData.CsvLevanteExportacion, entryHeader),
		};
		entryHeader.CH_EntryStatus = newEntryStatus;

		if (newEntryStatus == EntryStatusCodes.EffectiveDeparture && oldEntryStatus != EntryStatusCodes.EffectiveDeparture)
		{
			var certificateName = MessageProcessorHelper.GetInterchangeCertificateName(message, Logger);
			MessageRequest.CreateEDIMessageForEffectiveDepCertRequest(message.Factory, entryHeader, certificateName);
		}
	}

	ZString GetEntryStatusFromCSVClearance(ZString csvClearance, CusEntryHeader entryHeader)
	{
		if (csvClearance.IsEmpty)
		{
			return EntryStatusCodes.CustomsDeclarationAccepted;
		}

		return (string)entryHeader.EntryInstruction?.CEI_SubStyle switch
		{
			EntrySubStyleList.Codes.B or EntrySubStyleList.Codes.C => EntryStatusCodes.ClearedWithPendingComplementaryDeclarations,
			EntrySubStyleList.Codes.Z => EntryStatusCodes.EffectiveDeparture,
			_ => EntryStatusCodes.Cleared
		};
	}

	void ProcessResponseLines(IEnumerable<GoodsItemType70> goodsItem, CusEntryHeader entryHeader, ZDateTime acceptanceDate)
	{
		if (goodsItem is null)
		{
			return;
		}

		foreach (var entryLine in entryHeader.AllEntryLines)
		{
			var item = goodsItem.FirstOrDefault(x => x.DeclarationGoodsItemNumber == entryLine.CL_LineNumber.ToString());
			if (item != null)
			{
				ProcessEntryLineSupportingDocuments(item, entryLine, acceptanceDate);
				ProcessEntryLinePreviousDocuments(item, entryLine);

				ProcessEntryLineTransportDocument(entryLine, item);
				ProcessEntryLineAdditionalReference(entryLine, item);
				ProcessEntryLineAdditionalInformation(entryLine, item);
			}
		}
	}

	void ProcessEntryLineSupportingDocuments(GoodsItemType70 responseLine, CusEntryLine entryLine, ZDateTime acceptanceDate)
	{
		var documentsToRemove = entryLine.GetPreviouslySentSupportingDocuments();

		foreach (var docu in documentsToRemove)
		{
			docu.Delete();
		}

		responseLine.SupportingDocument?.ForEach(doc => AddNewSupportingDocument(entryLine, doc.Type, doc.ReferenceNumber, acceptanceDate, doc.DocumentLineItemNumber, doc.IssuingAuthorityName, doc.ValidityDate, doc.Quantity, doc.MeasurementUnitAndQualifier, doc.Amount, doc.Currency));
	}

	void ProcessEntryLinePreviousDocuments(GoodsItemType70 responseLine, CusEntryLine entryLine)
	{
		var documentsToRemove = entryLine.GetPreviouslySentPreviousDocuments();

		foreach (var docu in documentsToRemove)
		{
			docu.Delete();
		}

		responseLine.PreviousDocument?.ForEach(doc => AddNewPreviousDocument(doc, entryLine));
	}

	void ProcessEntryLineTransportDocument(CusEntryLine entryLine, GoodsItemType70 item)
	{
		DeletePreviouslySentAdditionalInfos(entryLine, AdditionalDocList.Codes.TransportDocuments);
		item.TransportDocument.ForEach(doc => AddNewAdditionalInfo(entryLine, AdditionalDocList.Codes.TransportDocuments, doc.Type, doc.ReferenceNumber));
	}

	void ProcessEntryLineAdditionalReference(CusEntryLine entryLine, GoodsItemType70 item)
	{
		DeletePreviouslySentAdditionalInfos(entryLine, AdditionalDocList.Codes.AdditionalReference);
		item.AdditionalReference.ForEach(doc => AddNewAdditionalInfo(entryLine, AdditionalDocList.Codes.AdditionalReference, doc.Type, doc.ReferenceNumber));
	}

	void ProcessEntryLineAdditionalInformation(CusEntryLine entryLine, GoodsItemType70 item)
	{
		DeletePreviouslySentAdditionalInfos(entryLine, AdditionalDocList.Codes.AdditionalInformation);
		item.AdditionalInformation.ForEach(doc => AddNewAdditionalInfo(entryLine, AdditionalDocList.Codes.AdditionalInformation, doc.Code, doc.Text));
	}

	void DeletePreviouslySentAdditionalInfos(CusEntryHeader entryHeader, string subType)
	{
		var docsToDelete = entryHeader.GetPreviouslySentAdditionalInfos().Where(doc => doc.CSI_SubType.Equals(subType));

		foreach (var doc in docsToDelete)
		{
			doc.Delete();
		}
	}

	void DeletePreviouslySentAdditionalInfos(CusEntryLine entryLine, string subType)
	{
		var docsToDelete = entryLine.GetPreviouslySentAdditionalInfos().Where(doc => doc.CSI_SubType.Equals(subType));

		foreach (var doc in docsToDelete)
		{
			doc.Delete();
		}
	}

	T AddNewDocument<T>(EnterpriseBusinessObject businessObject, ZString type, ZString referenceNumber)
		where T : Customs.Business.CusSupportingInfo
	{
		var document = businessObject.Factory.New<T>();
		document.CSI_Code = type;
		document.CSI_ReferenceNumber = referenceNumber;

		document.CSI_Status = DocumentStatus.Accepted;
		document.CSI_ParentID = businessObject.PK;
		document.CSI_ParentTableCode = businessObject.TablePrefix;

		return document;
	}

	void AddNewAdditionalInfo(EnterpriseBusinessObject businessObject, ZString subType, string code, string reference)
	{
		var document = AddNewDocument<AdditionalInfo>(businessObject, code, reference);
		document.CSI_SubType = subType;
	}

	void AddNewSupportingDocument(EnterpriseBusinessObject obj, string type, string referenceNumber, ZDateTime acceptanceDate, string documentLineItemNumber, string issuingAuthorityName, DateTime? validityDate = null, decimal? quantity = 0, string unitOfQuantity = "", decimal? value = 0, string currency = "")
	{
		var document = AddNewDocument<SupportingDocument>(obj, type, referenceNumber);

		var docDate = validityDate ?? ZDateTime.Empty;
		if (docDate <= acceptanceDate)
		{
			document.CSI_DateOfIssue = docDate;
		}
		else
		{
			document.CSI_DateOfExpiry = docDate;
		}

		document.CSI_ItemNumber = Convert.ToInt16(documentLineItemNumber);
		document.CSI_AdditionalDescription = issuingAuthorityName;
		document.CSI_Quantity = quantity ?? ZDecimal.Zero;
		document.CSI_UnitOfQuantity = unitOfQuantity ?? "";
		document.CSI_Value = value ?? ZDecimal.Zero;
		document.CSI_RX_NKCurrency = currency ?? "";
	}

	void AddNewPreviousDocument(PreviousDocumentType71 doc, CusEntryLine entryLine)
	{
		var document = AddNewDocument<PreviousDocument>(entryLine, doc.Type, doc.ReferenceNumber);

		document.CSI_LineNo = Convert.ToInt32(doc.GoodsItemNumber);
		document.CSI_Quantity = doc.Quantity ?? ZDecimal.Zero;
		document.CSI_UnitOfQuantity = doc.MeasurementUnitAndQualifier;
	}

	const string XsdSchemaNameCCAESCV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.AES.Incoming.CCAESCV1Sal.xsd";
}
