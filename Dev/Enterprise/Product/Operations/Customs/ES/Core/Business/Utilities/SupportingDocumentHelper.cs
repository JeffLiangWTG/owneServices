using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ES.Business;

public static class SupportingDocumentHelper
{
	public const string SupportingDocumentCodeN380 = "N380";
	public const string SupportingDocumentCode1217 = "1217";

	public static ZBool MatchesAnyPreviouslySentDocument(this SupportingDocument document, SupportingDocument[] previouslySentDocuments)
	{
		return previouslySentDocuments.Any(x => document.MatchesPreviouslySentDocument(x));
	}

	public static void ProcessImportEntryLineSupportingDocuments(this CusEntryLine entryLine)
	{
		var entryLineDocumentsToRemove = entryLine.GetPreviouslySentSupportingDocuments().Where(x => x.CSI_SubType != SupportingDocumentSubType.LIQ
																											|| (x.CSI_SubType == SupportingDocumentSubType.LIQ
																												&& x.CSI_Status == DocumentStatus.Accepted));
		foreach (var docu in entryLineDocumentsToRemove)
		{
			docu.Delete();
		}

		var entryHeaderDocumentsToRemove = entryLine.Header.GetPreviouslySentSupportingDocuments().Where(x => x.CSI_SubType != SupportingDocumentSubType.LIQ
																													|| (x.CSI_SubType == SupportingDocumentSubType.LIQ
																														&& x.CSI_Status == DocumentStatus.Accepted));
		foreach (var docu in entryHeaderDocumentsToRemove)
		{
			docu.Delete();
		}

		entryLine.ReadOnlySupportingDocuments.LoadNew();
		var documentsToAdd = entryLine.ReadOnlySupportingDocuments.Cast<ReadOnlySupportingDocument>().Where(x => x.CSI_SubType != SupportingDocumentSubType.LIQ);
		var entryHeader = entryLine.Header;

		foreach (ReadOnlySupportingDocument readOnlySupDoc in documentsToAdd)
		{
			if (readOnlySupDoc.IsDocumentHeader)
			{
				CopyToEntryHeader(readOnlySupDoc, entryHeader);
			}
			else
			{
				CopyToEntryLine(readOnlySupDoc, entryLine);
			}
		}
		entryLine.UpdateLIQSupportingDocuments();
	}

	public static void ProcessDVD5018EntryLineSupportingDocuments(this CusEntryLine entryLine)
	{
		var documentsToRemove = entryLine.GetPreviouslySentSupportingDocuments().Where(x => x.CSI_SubType == SupportingDocumentSubType.LIQ
																							&& x.CSI_Status == DocumentStatus.Accepted
																							&& x.CSI_Code == SupportingDocumentType.WHLOCAuthorisation);

		foreach (var docu in documentsToRemove)
		{
			docu.Delete();
		}

		entryLine.UpdateLIQSupportingDocuments(code: SupportingDocumentType.WHLOCAuthorisation);
	}

	static void UpdateLIQSupportingDocuments(this CusEntryLine entryLine, string code = "")
	{
		UpdateLIQSupportingDocumentsEntryLine(entryLine, code);
		if (code.IsEmpty())
		{
			UpdateLIQSupportingDocumentsEntryHeader(entryLine.Header);
		}
	}

	static void UpdateLIQSupportingDocumentsEntryLine(this CusEntryLine entryLine, string code = "")
	{
		var liqDocumentsToUpdate = entryLine.GetPreviouslySentSupportingDocuments().Where(x => x.CSI_SubType == SupportingDocumentSubType.LIQ
																								&& x.CSI_Status != DocumentStatus.Accepted);
		UpdateLIQSupportingDocumentsCSI_Status(liqDocumentsToUpdate, code);
	}

	static void UpdateLIQSupportingDocumentsEntryHeader(this CusEntryHeader entryHeader)
	{
		var liqDocumentsToUpdate = entryHeader.GetPreviouslySentSupportingDocuments().Where(x => x.CSI_SubType == SupportingDocumentSubType.LIQ
																								&& x.CSI_Status != DocumentStatus.Accepted);
		UpdateLIQSupportingDocumentsCSI_Status(liqDocumentsToUpdate);
	}

	static void UpdateLIQSupportingDocumentsCSI_Status(IEnumerable<SupportingDocument> liqDocumentsToUpdate, string code = "")
	{
		if (!string.IsNullOrEmpty(code))
		{
			liqDocumentsToUpdate = liqDocumentsToUpdate.Where(x => x.CSI_Code == code);
		}

		foreach (var doc in liqDocumentsToUpdate)
		{
			doc.CSI_Status = DocumentStatus.Accepted;
		}
	}

	public static void ProcessExportEntryLineSupportingDocuments(this CusEntryLine entryLine)
	{
		var entryLineDocumentsToRemove = entryLine.GetPreviouslySentSupportingDocuments().Where(x => x.CSI_SubType != SupportingDocumentSubType.LIQ);
		foreach (var docu in entryLineDocumentsToRemove)
		{
			docu.Delete();
		}

		var entryHeaderDocumentsToRemove = entryLine.Header.GetPreviouslySentSupportingDocuments().Where(x => x.CSI_SubType != SupportingDocumentSubType.LIQ);
		foreach (var docu in entryHeaderDocumentsToRemove)
		{
			docu.Delete();
		}

		entryLine.ReadOnlySupportingDocuments.LoadNew();
		var readOnlySupportingDocuments = entryLine.ReadOnlySupportingDocuments.Cast<ReadOnlySupportingDocument>().Where(x => x.CSI_SubType != SupportingDocumentSubType.LIQ);
		var entryHeader = entryLine.Header;

		foreach (ReadOnlySupportingDocument readOnlySupDoc in readOnlySupportingDocuments)
		{
			if (readOnlySupDoc.IsDocumentHeader)
			{
				CopyToEntryHeader(readOnlySupDoc, entryHeader);
			}
			else
			{
				CopyToEntryLine(readOnlySupDoc, entryLine);
			}
		}

		entryLine.UpdateLIQSupportingDocuments();
	}

	public static void ProcessComplXExportEntryLineSupportingDocuments(this CusEntryLine entryLine)
	{
		var previouslySentSupportingDocumentsEntryLine = GetPreviouslySentComplXExportAcceptedDocumentsEntryLine(entryLine).ToArray();
		var previouslySentSupportingDocumentsHeader = GetPreviouslySentComplXExportAcceptedDocumentsEntryHeader(entryLine.Header).ToArray();
		var readOnlySupportingDocuments = GetReadOnlyComplXExportAcceptedDocuments(entryLine);
		var entryHeader = entryLine.Header;

		foreach (ReadOnlySupportingDocument readOnlySupDoc in readOnlySupportingDocuments)
		{
			var isHeader = readOnlySupDoc.IsDocumentHeader;
			var supportingDocument = readOnlySupDoc.GetSupportingDocument() as SupportingDocument;
			if (supportingDocument != null)
			{
				if (isHeader && !supportingDocument.MatchesAnyPreviouslySentDocument(previouslySentSupportingDocumentsHeader))
				{
					CopyToEntryHeader(readOnlySupDoc, entryHeader);
				}
				else if (!isHeader && !supportingDocument.MatchesAnyPreviouslySentDocument(previouslySentSupportingDocumentsEntryLine))
				{
					CopyToEntryLine(readOnlySupDoc, entryLine);
				}
			}
		}
	}

	public static void ProcessBox44ImportEntryLineSupportingDocuments(this CusEntryLine entryLine)
	{
		var previouslySentSupportingDocumentsEntryLine = entryLine.GetPreviouslySentSupportingDocuments();
		var previouslySentSupportingDocumentsEntryHeader = entryLine.Header.GetPreviouslySentSupportingDocuments();

		entryLine.ReadOnlySupportingDocuments.LoadNew();
		var entryHeader = entryLine.Header;
		foreach (ReadOnlySupportingDocument readOnlySupDoc in entryLine.ReadOnlySupportingDocuments)
		{
			var isHeader = readOnlySupDoc.IsDocumentHeader;
			var supportingDocument = readOnlySupDoc.GetSupportingDocument() as SupportingDocument;
			if (supportingDocument != null)
			{
				if (isHeader && !supportingDocument.MatchesAnyPreviouslySentDocument(previouslySentSupportingDocumentsEntryHeader))
				{
					CopyToEntryHeader(readOnlySupDoc, entryHeader);
				}
				else if (!isHeader && !supportingDocument.MatchesAnyPreviouslySentDocument(previouslySentSupportingDocumentsEntryLine))
				{
					CopyToEntryLine(readOnlySupDoc, entryLine);
				}
			}
		}
	}

	static void CopyToEntryHeader(ReadOnlySupportingDocument readOnlySupDoc, CusEntryHeader entryHeader)
	{
		var document = SupportingDocument.CopyFrom(readOnlySupDoc);
		document.CSI_Status = DocumentStatus.Accepted;
		document.CSI_ParentID = entryHeader.PK;
		document.CSI_ParentTableCode = entryHeader.TablePrefix;
	}

	static void CopyToEntryLine(ReadOnlySupportingDocument readOnlySupDoc, CusEntryLine entryLine)
	{
		var document = SupportingDocument.CopyFrom(readOnlySupDoc);
		document.CSI_Status = DocumentStatus.Accepted;
		document.CSI_ParentID = entryLine.PK;
		document.CSI_ParentTableCode = entryLine.TablePrefix;
	}

	public static SupportingDocument GetSupportingDocumentByCode(SupportingDocumentCollection documentsList, ZString code) => documentsList.Cast<SupportingDocument>().FirstOrDefault(x => x.CSI_Code == code);

	public static ZBool IsDocumentInComplXExportAcceptedDocumentList(this SupportingDocument document) => ComplXExportAcceptedDocumentList.Contains(document.CSI_Code);
	public static ZBool IsDocumentInComplXExportAcceptedDocumentList(this ReadOnlySupportingDocument document) => ComplXExportAcceptedDocumentList.Contains(document.CSI_Code);

	public static ZBool DoesNotMatchAnyPreviouslySentComplXExportDocument(this SupportingDocument doc, SupportingDocument[] previouslySentSupDocs) => doc.IsDocumentInComplXExportAcceptedDocumentList() && !doc.MatchesAnyPreviouslySentDocument(previouslySentSupDocs);

	public static IEnumerable<SupportingDocument> GetPreviouslySentComplXExportAcceptedDocumentsEntryLine(this CusEntryLine entryLine) => entryLine.GetPreviouslySentSupportingDocuments().Where(x => ComplXExportAcceptedDocumentList.Contains(x.CSI_Code));

	public static IEnumerable<SupportingDocument> GetPreviouslySentComplXExportAcceptedDocumentsEntryHeader(this CusEntryHeader entryHeader) => entryHeader.GetPreviouslySentSupportingDocuments().Where(x => ComplXExportAcceptedDocumentList.Contains(x.CSI_Code));
	public static IEnumerable<ReadOnlySupportingDocument> GetReadOnlyComplXExportAcceptedDocuments(this CusEntryLine entryLine)
	{
		entryLine.ReadOnlySupportingDocuments.LoadNew();
		return entryLine.ReadOnlySupportingDocuments.Cast<ReadOnlySupportingDocument>().Where(x => ComplXExportAcceptedDocumentList.Contains(x.CSI_Code));
	}

	public static ZBool HasComplXExportDocuments(SupportingDocument[] supDocs) => supDocs.Any(doc => doc.IsDocumentInComplXExportAcceptedDocumentList());

	public static SupportingDocument GetFirstComplXExportDocument(SupportingDocument[] supDocs) => supDocs.FirstOrDefault(doc => doc.IsDocumentInComplXExportAcceptedDocumentList());

	static readonly ImmutableHashSet<ZString> ComplXExportAcceptedDocumentList = new ZString[] { "N380", "N325", "D005", "D008", "N935", "1001", "1003", "1004" }.ToImmutableHashSet();
}
