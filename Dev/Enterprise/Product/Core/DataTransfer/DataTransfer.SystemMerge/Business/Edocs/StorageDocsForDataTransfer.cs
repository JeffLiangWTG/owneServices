using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.Integration;

namespace Enterprise.DataTransfer.SystemMerge.Business
{
	public class StorageDocsForDataTransfer : StorageDocsWithS3Support, ICanBeSavedByDocumentFactory
	{
		public StorageDocsForDataTransfer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZGuid ParentOrgPk { get; set; }
	}
}
