using CargoWise.EntityFramework;

namespace Enterprise.DocumentScanning.Business
{
	public class NonPersistentUnallocatedObjectCollection : StorageDocsCollection
	{
		public NonPersistentUnallocatedObjectCollection(DocumentFactory masterFactory)
			: base(masterFactory)
		{
		}

		/// <summary>
		/// This filter is designed to return no rows. 
		/// </summary>
		protected override ZQuery CreateAdditionalFilter()
		{
			return ZQuery.NoResultQuery;
		}
	}
}
