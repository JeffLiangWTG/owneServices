using CargoWise.EntityFramework;

namespace Enterprise.DocumentScanning.Business
{
	public class DocumentSplitResultInfoCollection : NonPersistentBusinessObjectCollection<DocumentSplitResultInfo>
	{
		public DocumentSplitResultInfoCollection(DocumentSplitManager manager)
		{
			splitManager = manager;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DocumentSplitResultInfo(splitManager);
		}

		public override void Load()
		{
			this.RemoveAll();
			this.HasChanges = false;
		}

		readonly DocumentSplitManager splitManager;
	}
}
