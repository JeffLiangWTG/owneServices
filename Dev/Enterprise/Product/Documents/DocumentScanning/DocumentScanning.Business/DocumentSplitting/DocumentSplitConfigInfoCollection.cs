using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentScanning.Business
{
	public class DocumentSplitConfigInfoCollection : NonPersistentBusinessObjectCollection<DocumentSplitConfigInfo>
	{
		public DocumentSplitConfigInfoCollection(DocumentSplitManager manager, int pSourceDocumentPageCount)
		{
			splitManager = manager;
			sourceDocumentPageCount = pSourceDocumentPageCount;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			var newRecord = new DocumentSplitConfigInfo(splitManager, SourceDocumentPageCount);
			newRecord.NeedsSplitting = true;
			if (this.Count > 0)
			{
				var previousRecord = this[Count - 1];
				newRecord.DocumentName = previousRecord.DocumentName;
				newRecord.DocumentType = previousRecord.DocumentType;
				newRecord.StartPage = previousRecord.EndPage + 1 >= SourceDocumentPageCount ? SourceDocumentPageCount : previousRecord.EndPage + 1;
				newRecord.EndPage = SourceDocumentPageCount;
				newRecord.AppendPageNumber = previousRecord.AppendPageNumber;
			}
			else
			{
				newRecord.DocumentName = splitManager.DocumentToSplit.SC_FileName;
				newRecord.DocumentType = splitManager.DocumentToSplit.SC_DocType;
				newRecord.StartPage = 1;
				newRecord.EndPage = SourceDocumentPageCount;
			}
			return newRecord;
		}

		public override void Load()
		{
			this.RemoveAll();
			this.HasChanges = false;
		}

		readonly DocumentSplitManager splitManager;

		public int SourceDocumentPageCount
		{
			get
			{
				return sourceDocumentPageCount;
			}
		}
		readonly int sourceDocumentPageCount;

		public bool HasNotSplitElements
		{
			get
			{
				return this.Cast<DocumentSplitConfigInfo>().Any(x => x.NeedsSplitting);
			}
		}

		#region Validation

		public void ValidateAll()
		{
			foreach (DocumentSplitConfigInfo info in this)
			{
				info.Validation.ValidateAll();
			}
		}

		#endregion
	}
}
