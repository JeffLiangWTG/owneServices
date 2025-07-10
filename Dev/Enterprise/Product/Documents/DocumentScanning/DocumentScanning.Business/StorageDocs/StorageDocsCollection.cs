
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine;

namespace Enterprise.DocumentScanning.Business
{
	public class StorageDocsCollection : StorageDocsCollectionBase<StorageDocs>, IDeliverableCollection
	{
		public StorageDocsCollection(NumberedBusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject AddNewCore()
		{
			StorageDocs document = (StorageDocs)base.AddNewCore();
			MasterFactory.CreateParentFor(document);
			return document;
		}

		protected override bool IsAddNewSupported
		{
			get { return true; }
		}
	}
}
