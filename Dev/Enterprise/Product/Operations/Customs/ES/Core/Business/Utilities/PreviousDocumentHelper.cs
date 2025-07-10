using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using static Enterprise.Customs.Business.UniversalReferenceConstants.RefCusCodeList;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business
{
	public static class PreviousDocumentHelper
	{
		public const string PreviousDocumentCodeSUM = "SUM";
		public const string PreviousDocumentCodeXSUM = "XSUM";
		public const string PreviousDocumentCodeN337 = "N337";
		public const string PreviousDocumentCode337 = "337";
		public const int ImportExportReferenceLineNoLength = 5;

		public static ZBool MatchesAnyPreviouslySentPreviousDocument(this PreviousDocument document, PreviousDocument[] previouslySentPreviousDocuments) => previouslySentPreviousDocuments.Any(x => document.MatchesPreviouslySentPreviousDocument(x));

		public static ZString GetSUMReferenceNumberToSend(PreviousDocument doc)
		{
			var referenceNumber = doc.CSI_ReferenceNumber;

			if (doc.CSI_Code == PreviousDocumentCodeSUM && doc.CSI_LineNo > 0)
			{
				referenceNumber += doc.CSI_LineNo.ToString().PadLeft(ImportExportReferenceLineNoLength, '0');
			}

			return referenceNumber;
		}

		public static void ProcessImportEntryLinePreviousDocuments(this CusEntryLine entryLine)
		{
			RemovePreviousDocumentsAndReloadReadOnlyCollection(entryLine);
			var documentsToAdd = entryLine.ReadOnlyPreviousDocuments.Cast<ReadOnlyPreviousDocument>().FirstOrDefault();
			if (documentsToAdd != null)
			{
				CopyPreviousDocuments(entryLine, documentsToAdd);
			}
		}

		public static void ProcessExportEntryLinePreviousDocuments(this CusEntryLine entryLine)
		{
			RemovePreviousDocumentsAndReloadReadOnlyCollection(entryLine);
			var readOnlyPreviousDocuments = entryLine.ReadOnlyPreviousDocuments.Cast<ReadOnlyPreviousDocument>();
			foreach (ReadOnlyPreviousDocument readOnlyPrevDoc in readOnlyPreviousDocuments)
			{
				CopyPreviousDocuments(entryLine, readOnlyPrevDoc);
			}
		}

		static void RemovePreviousDocumentsAndReloadReadOnlyCollection(this CusEntryLine entryLine)
		{
			var documentsToRemove = entryLine.GetPreviouslySentPreviousDocuments();
			foreach (var docu in documentsToRemove)
			{
				docu.Delete();
			}
			entryLine.ReadOnlyPreviousDocuments.LoadNew();
		}

		static void CopyPreviousDocuments(CusEntryLine entryLine, ReadOnlyPreviousDocument readOnlyPrevDoc)
		{
			var document = PreviousDocument.CopyFrom(entryLine, readOnlyPrevDoc);
			document.CSI_Status = DocumentStatus.Accepted;
			document.CSI_ParentID = entryLine.PK;
			document.CSI_ParentTableCode = entryLine.TablePrefix;
		}

		public static void ProcessC651EntryLinePreviousDocuments(this CusEntryLine entryLine)
		{
			var previouslySentPreviousDocuments = GetPreviouslySentComplXExportAcceptedDocumentsC651(entryLine).ToArray();

			entryLine.ReadOnlyPreviousDocuments.LoadNew();
			var readOnlyPreviousDocuments = entryLine.ReadOnlyPreviousDocuments.Cast<ReadOnlyPreviousDocument>().Where(x => x.CSI_Code == UniversalReferenceConstants.PreviousDocumentType.C651);
			foreach (ReadOnlyPreviousDocument readOnlyPrevDoc in readOnlyPreviousDocuments)
			{
				var previousDocument = readOnlyPrevDoc.GetPreviousDocument();
				if (previousDocument != null && !previousDocument.MatchesAnyPreviouslySentPreviousDocument(previouslySentPreviousDocuments))
				{
					var document = PreviousDocument.CopyFrom(entryLine, readOnlyPrevDoc);
					document.CSI_Status = DocumentStatus.Accepted;
					document.CSI_ParentID = entryLine.PK;
					document.CSI_ParentTableCode = entryLine.TablePrefix;
				}
			}
		}

		public static void GetUOMAndQuantityForPreviousDocument(PreviousDocument prevDoc, Customs.Business.InvoiceLinesForEntryLineCollection invoiceLines, ZInt vehiclesQuantity, out ZString uom, out ZDecimal quantity)
		{
			uom = ZString.Empty;
			quantity = ZDecimal.Zero;
			if (!prevDoc.CSI_UnitOfQuantity.IsEmpty)
			{
				var equalPrevDocs = invoiceLines.SelectMany(invoiceLine => ((JobComInvoiceLine)invoiceLine).PreviousDocuments.Where(x => IsPrevDocumentEqual(prevDoc, (PreviousDocument)x)));
				quantity = equalPrevDocs.Sum(previousDocument => previousDocument.CSI_Quantity);
			}
			else if (!vehiclesQuantity.IsEmpty)
			{
				uom = CustomsUq.Number.NumberOfItems;
				quantity = (ZDecimal)vehiclesQuantity;
			}
		}

		public static void AddPreviousDocumentWithLengthForGoodsItemNumber(CusEntryLine entryLine, List<AESCommonDocumentWrapper> prevDocuments, PreviousDocument doc, ZShort seqNum)
		{
			GetUOMAndQuantityForPreviousDocument(doc, entryLine.InvoiceLines, entryLine.VehiclesQty, out var uom, out var quantity);
			prevDocuments.Add(new AESCommonDocumentWrapper(doc, seqNum, uom, quantity));
		}

		static ZBool IsPrevDocumentEqual(PreviousDocument prevDoc, PreviousDocument prevDocToCompare) =>
			prevDoc.CSI_Code == prevDocToCompare.CSI_Code
			&& prevDoc.CSI_ReferenceNumber == prevDocToCompare.CSI_ReferenceNumber
			&& prevDoc.CSI_LineNo == prevDocToCompare.CSI_LineNo
			&& prevDoc.CSI_UnitOfQuantity == prevDocToCompare.CSI_UnitOfQuantity;

		public static IEnumerable<PreviousDocument> GetPreviouslySentComplXExportAcceptedDocumentsC651(this CusEntryLine entryLine) => entryLine.GetPreviouslySentPreviousDocuments().Where(x => x.CSI_Code == UniversalReferenceConstants.PreviousDocumentType.C651);
	}
}
