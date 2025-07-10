using System;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos
{
	public class ReadOnlySupportingDocumentCollection : NonPersistentBusinessObjectCollection<ReadOnlySupportingDocument>
	{
		public ReadOnlySupportingDocumentCollection(CusEntryLine entryLine) : base(entryLine.Factory)
		{
			EntryLine = Argument.NotNull(entryLine, nameof(entryLine));
		}

		protected CusEntryLine EntryLine { get; }

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new InvalidOperationException("It is not possible to create a new grouped supporting document object from this collection");
		}

		protected override bool AllowNewCore => false;

		public void LoadNew()
		{
			RemoveAll();
			LoadNewCore();
			IsLoaded = true;
		}

		protected virtual void LoadNewCore()
		{
			InitializeWithMergedSupportingDocuments();
		}

		protected void InitializeWithMergedSupportingDocuments()
		{
			var mergedHeaderSupportingDocuments = EntryLine.Header.SupportingDocuments;
			mergedHeaderSupportingDocuments.ForEach(supportingDocument => AddSupportingDocument(supportingDocument));

			var mergedLineSupportingDocuments = EntryLine.SupportingDocuments;
			mergedLineSupportingDocuments.ForEach(supportingDocument => AddSupportingDocument(supportingDocument));
		}

		protected virtual void AddSupportingDocument(SupportingDocument supportingDocument)
		{
			Add(new ReadOnlySupportingDocument(supportingDocument));
		}
	}
}
