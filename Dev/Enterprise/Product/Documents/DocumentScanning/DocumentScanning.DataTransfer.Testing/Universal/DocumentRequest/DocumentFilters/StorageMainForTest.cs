using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;

namespace Enterprise.DocumentScanning.DataTransfer.Test.Universal.DocumentRequest.DocumentFilters
{
	internal class StorageMainForTest : StorageMain
	{
		public StorageMainForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ZString DocumentOwnerDescription => documentOwnerDescription;

		public void SetDocumentOwnerDescription(ZString documentOwnerDescription)
		{
			this.documentOwnerDescription = documentOwnerDescription;
		}

		ZString documentOwnerDescription;
	}
}
