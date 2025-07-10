using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class ReadOnlyPreviousDocumentCollection : NonPersistentBusinessObjectCollection<ReadOnlyPreviousDocument>
	{
		public ReadOnlyPreviousDocumentCollection(CusEntryLine entryLine)
			: base(entryLine.Factory)
		{
			EntryLine = Argument.NotNull(entryLine, nameof(entryLine));
		}

		CusEntryLine EntryLine { get; }

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new InvalidOperationException("It is not possible to create a new grouped previous document object from this collection");
		}

		protected override bool AllowNewCore => false;

		public void LoadNew()
		{
			RemoveAll();

			AddEntryLinePreviousDocuments();
			AddNewOrModifiedMergedPreviousDocuments();

			base.IsLoaded = true;
		}

		void AddEntryLinePreviousDocuments()
		{
			var entryLinePreviousDocuments = EntryLine.GetPreviouslySentPreviousDocuments();
			entryLinePreviousDocuments.ForEach(entryLineDoc => Add(new ReadOnlyPreviousDocument(entryLineDoc)));
		}

		void AddNewOrModifiedMergedPreviousDocuments()
		{
			var mergedDocumentCollection = new ReadOnlyPreviousDocumentCollection(EntryLine);
			mergedDocumentCollection.InitializeWithMergedPreviousDocuments();
			var modifiedOrNewDocuments = mergedDocumentCollection.Cast<ReadOnlyPreviousDocument>().Except(this.Cast<ReadOnlyPreviousDocument>(), new AcceptedDocumentModificationComparer()).Cast<ReadOnlyPreviousDocument>();
			modifiedOrNewDocuments.ForEach(modifiedOrNewDoc => Add(modifiedOrNewDoc));
		}

		void InitializeWithMergedPreviousDocuments()
		{
			if (EntryLine.Declaration.IsExport)
			{
				InitializeWithMergedPreviousDocumentsForExport();
			}
			else if (EntryLine.Declaration.IsImport)
			{
				InitializeWithMergedPreviousDocumentsForImport();
			}
		}

		void InitializeWithMergedPreviousDocumentsForExport()
		{
			var isUcc6 = EntryLine.Header.IsUCC6;
			var hasC651Doc = EntryLine.PreviousDocuments.Any(x => x.CSI_Code == UniversalReferenceConstants.PreviousDocumentType.C651);
			var hasEntryLineHasPRECustomsOffice = EntryLine.HasPRECustomsOffice;
			if (isUcc6 && (hasEntryLineHasPRECustomsOffice || hasC651Doc))
			{
				if (hasEntryLineHasPRECustomsOffice)
				{
					foreach (var previousDocument in EntryLine.PreviousDocuments)
					{
						PreviousDocumentHelper.GetUOMAndQuantityForPreviousDocument((PreviousDocument)previousDocument, EntryLine.InvoiceLines, EntryLine.VehiclesQty, out var uom, out var quantity);
						AddPreviousDocument(previousDocument, uom, quantity);
					}
				}
				else
				{
					foreach (var previousDocument in EntryLine.PreviousDocuments.Where(x => x.CSI_Code == UniversalReferenceConstants.PreviousDocumentType.C651))
					{
						PreviousDocumentHelper.GetUOMAndQuantityForPreviousDocument((PreviousDocument)previousDocument, EntryLine.InvoiceLines, EntryLine.VehiclesQty, out var uom, out var quantity);
						AddPreviousDocument(previousDocument, uom, quantity);
					}
				}
			}
			else if (isUcc6)
			{
				var previousDocuments = EntryLine.RandomLine.GetPreviousDocumentsFromSelfOrParentsForMergeKeyOnly()?.Cast<PreviousDocument>();
				if (previousDocuments != null)
				{
					foreach (var previousDocument in previousDocuments)
					{
						PreviousDocumentHelper.GetUOMAndQuantityForPreviousDocument(previousDocument, EntryLine.InvoiceLines, EntryLine.VehiclesQty, out var uom, out var quantity);
						AddPreviousDocument(previousDocument, uom, quantity);
					}
				}	
			}
			else
			{
				EntryLine.RandomLine.GetPreviousDocumentsFromSelfOrParentsForMergeKeyOnly()?.Cast<PreviousDocument>().ForEach(previousDocument => AddPreviousDocument(previousDocument));
			}
		}

		void InitializeWithMergedPreviousDocumentsForImport()
		{
			var entryInstruction = EntryLine.RandomLine.EntryInstruction;
			if (entryInstruction != null && (entryInstruction.IsT2L || entryInstruction.IsT2C || entryInstruction.IsH2))
			{
				EntryLine.Declaration.PreviousDocuments.Cast<PreviousDocument>().ForEach(prevDoc => AddPreviousDocument(prevDoc));
				EntryLine.Header.InvoiceHeaders.ForEach(header => ((JobComInvoiceHeader)header).PreviousDocuments.Cast<PreviousDocument>().ForEach(prevDoc => AddPreviousDocument(prevDoc)));
				EntryLine.InvoiceLines.ForEach(line => ((JobComInvoiceLine)line).PreviousDocuments.Cast<PreviousDocument>().ForEach(prevDoc => AddPreviousDocument(prevDoc)));
			}
			else
			{
				EntryLine.RandomLine.GetPreviousDocumentsFromSelfOrParentsForMergeKeyOnly()?.Cast<PreviousDocument>().ForEach(previousDocument => AddPreviousDocument(previousDocument));
			}
		}

		void AddPreviousDocument(EU.Business.Declaration.MultiLineAddInfos.PreviousDocument previousDocument, string uom = "", decimal quantity = 0)
		{
			Add(new ReadOnlyPreviousDocument((PreviousDocument)previousDocument, uom, quantity));
		}

		#region AcceptedDocumentModificationComparer

		public class AcceptedDocumentModificationComparer : IEqualityComparer<IPreviousDocumentEqualityKey>
		{
			public bool Equals(IPreviousDocumentEqualityKey x, IPreviousDocumentEqualityKey y)
			{
				return x.CSI_Code == y.CSI_Code
					&& x.CSI_SubType == y.CSI_SubType
					&& x.CSI_ReferenceNumber == y.CSI_ReferenceNumber
					&& x.CSI_DateOfIssue == y.CSI_DateOfIssue
					&& x.CSI_LineNo == y.CSI_LineNo
					&& x.CSI_UnitOfQuantity == y.CSI_UnitOfQuantity
					&& x.CSI_Quantity == y.CSI_Quantity;
			}

			public int GetHashCode(IPreviousDocumentEqualityKey obj) => FixedHashCodeOnlyForObjectEqualityComparison;
			const int FixedHashCodeOnlyForObjectEqualityComparison = 1;
		}

		#endregion
	}
}
