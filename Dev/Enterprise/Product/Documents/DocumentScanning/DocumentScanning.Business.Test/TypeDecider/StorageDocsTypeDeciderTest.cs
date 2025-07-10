using CargoWise.EntityFramework;

namespace Enterprise.DocumentScanning.Business.Test
{
	public class StorageDocsTypeDeciderTest : TestCaseWithDocumentFactory
	{
		public void TestGetTypeForLoad()
		{
			StorageDocsTypeDecider typeDecider = new StorageDocsTypeDecider();

			StorageFile file = StorageFile.NewWithParent_DEBUG(MasterFactory);
			file.SC_DataType = "PDF";
			AssertEquals("Type", typeof(StorageFile), typeDecider.GetTypeForLoad(((INeedRow)file).Row, MasterFactory));

			file.SC_DataType = "TIF";
			AssertEquals("Type", typeof(StorageDocs), typeDecider.GetTypeForLoad(((INeedRow)file).Row, MasterFactory));

			file.SC_DataType = "JPG";
			AssertEquals("Type", typeof(StorageDocs), typeDecider.GetTypeForLoad(((INeedRow)file).Row, MasterFactory));

			file.SC_DataType = "JPEG";
			AssertEquals("Type", typeof(StorageDocs), typeDecider.GetTypeForLoad(((INeedRow)file).Row, MasterFactory));
		}

		public void TestGetTypeForNew()
		{
			StorageDocsTypeDecider typeDecider = new StorageDocsTypeDecider();
			AssertEquals(@"Should return the base class for now; each individual collection should
				take care of creating the correct type of bizO to add", typeof(StorageDocsBase), typeDecider.GetTypeForNew());
		}

		public void TestGetTypeForBinding()
		{
			StorageDocsTypeDecider typeDecider = new StorageDocsTypeDecider();
			AssertEquals("should return the base class for now", typeof(StorageDocsBase), typeDecider.GetTypeForBinding());
		}
	}
}
