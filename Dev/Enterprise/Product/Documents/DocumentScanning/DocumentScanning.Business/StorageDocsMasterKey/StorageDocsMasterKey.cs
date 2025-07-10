using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentScanning.Business
{
	public class StorageDocsMasterKey : AutoStorageDocsMasterKey
	{
		public StorageDocsMasterKey(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
