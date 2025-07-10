using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentScanning.Business
{
	public class StorageDocsToDelete : AutoStorageDocsToDelete
	{
		public StorageDocsToDelete(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
