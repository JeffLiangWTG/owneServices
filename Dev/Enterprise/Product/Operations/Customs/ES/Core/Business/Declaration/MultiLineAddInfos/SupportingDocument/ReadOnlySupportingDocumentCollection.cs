using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.ES.Business.Declaration;

public class ReadOnlySupportingDocumentCollection : EU.Business.Declaration.MultiLineAddInfos.ReadOnlySupportingDocumentCollection
{
	public ReadOnlySupportingDocumentCollection(CusEntryLine entryLine) : base(entryLine)
	{
	}

	protected new CusEntryLine EntryLine => (CusEntryLine)base.EntryLine;

	protected override void LoadNewCore()
	{
		AddEntryHeaderSupportingDocuments();
		AddEntryLineSupportingDocuments();
		AddNewOrModifiedMergedSupportingDocuments();
	}

	void AddEntryHeaderSupportingDocuments()
	{
		var entryHeaderSupportingDocuments = EntryLine.Header.GetPreviouslySentSupportingDocuments();
		entryHeaderSupportingDocuments.ForEach(entryLineDoc => Add(new ReadOnlySupportingDocument(entryLineDoc, EntryLine.Header)));
	}

	void AddEntryLineSupportingDocuments()
	{
		var auxDocumentCollection = new ReadOnlySupportingDocumentCollection(EntryLine);
		var entryLineSupportingDocuments = EntryLine.GetPreviouslySentSupportingDocuments();

		entryLineSupportingDocuments.ForEach(supportingDocument => auxDocumentCollection.AddSupportingDocument(supportingDocument));

		var documentsWithoutRepetitions = auxDocumentCollection.Cast<ReadOnlySupportingDocument>().Except(this.Cast<ReadOnlySupportingDocument>(), new AcceptedDocumentModificationComparer()).Cast<ReadOnlySupportingDocument>();
		documentsWithoutRepetitions.ForEach(documentWithoutRepetitions => Add(documentWithoutRepetitions));
	}

	void AddNewOrModifiedMergedSupportingDocuments()
	{
		var mergedDocumentCollection = new ReadOnlySupportingDocumentCollection(EntryLine);
		mergedDocumentCollection.InitializeWithMergedSupportingDocuments();
		var modifiedOrNewDocuments = mergedDocumentCollection.Cast<ReadOnlySupportingDocument>().Except(this.Cast<ReadOnlySupportingDocument>(), new AcceptedDocumentModificationComparer()).Cast<ReadOnlySupportingDocument>();
		modifiedOrNewDocuments.ForEach(modifiedOrNewDoc => Add(modifiedOrNewDoc));
	}

	protected override void AddSupportingDocument(EU.Business.Declaration.MultiLineAddInfos.SupportingDocument supportingDocument)
	{
		Add(new ReadOnlySupportingDocument((SupportingDocument)supportingDocument, EntryLine.Header));
	}

	public new ReadOnlySupportingDocument this[int index] => (ReadOnlySupportingDocument)Elements[index];

	public new ReadOnlySupportingDocument AddNew() => (ReadOnlySupportingDocument)base.AddNew();

	#region AcceptedDocumentModificationComparer

	public class AcceptedDocumentModificationComparer : IEqualityComparer<ISupportingDocumentEqualityKey>
	{
		public bool Equals(ISupportingDocumentEqualityKey x, ISupportingDocumentEqualityKey y)
		{
			return x.CSI_Code == y.CSI_Code
				&& x.CSI_ReferenceNumber.ToUpper() == y.CSI_ReferenceNumber.ToUpper()
				&& x.CSI_DateOfExpiry == y.CSI_DateOfExpiry
				&& x.CSI_DateOfIssue == y.CSI_DateOfIssue
				&& x.CSI_Procedure == y.CSI_Procedure
				&& x.CSI_AdditionalDescription == y.CSI_AdditionalDescription
				&& x.CSI_ItemNumber == y.CSI_ItemNumber;
		}

		public int GetHashCode(ISupportingDocumentEqualityKey obj) => FixedHashCodeOnlyForObjectEqualityComparison;
		const int FixedHashCodeOnlyForObjectEqualityComparison = 1;
	}

	#endregion

	#region AcceptedDocumentModificationAllButProcedureComparer

	public class AcceptedDocumentModificationAllButProcedureComparer : IEqualityComparer<ISupportingDocumentEqualityKey>
	{
		public bool Equals(ISupportingDocumentEqualityKey x, ISupportingDocumentEqualityKey y)
		{
			return x.CSI_Code == y.CSI_Code
				&& x.CSI_ReferenceNumber.ToUpper() == y.CSI_ReferenceNumber.ToUpper()
				&& x.CSI_DateOfExpiry == y.CSI_DateOfExpiry
				&& x.CSI_DateOfIssue == y.CSI_DateOfIssue
				&& x.CSI_AdditionalDescription == y.CSI_AdditionalDescription
				&& x.CSI_ItemNumber == y.CSI_ItemNumber;
		}

		public int GetHashCode(ISupportingDocumentEqualityKey obj) => FixedHashCodeOnlyForObjectEqualityComparison;
		const int FixedHashCodeOnlyForObjectEqualityComparison = 1;
	}

	#endregion
}
