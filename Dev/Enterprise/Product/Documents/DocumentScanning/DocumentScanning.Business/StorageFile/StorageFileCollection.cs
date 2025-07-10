
namespace Enterprise.DocumentScanning.Business
{
	public class StorageFileCollection : StorageDocsCollectionBase<StorageFile>
	{
		public StorageFileCollection(NumberedBusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override bool IsAddNewSupported
		{
			get { return true; }
		}
	}
}
