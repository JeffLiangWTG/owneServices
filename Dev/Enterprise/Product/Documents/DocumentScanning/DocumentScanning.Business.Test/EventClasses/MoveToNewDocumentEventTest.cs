using CargoWise.EntityFramework;

namespace Enterprise.DocumentScanning.Business.Test
{
	public class MoveToNewDocumentEventTest : TestCaseWithDocumentFactory
	{
		public void TestConstructor()
		{
			StorageDocs doc = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			StorageFile file = StorageFile.NewWithParent_DEBUG(MasterFactory);
			SerializableEDocCollection collection = SerializableEDocCollection.New(new BusinessObject[] { doc, file });
			MoveToNewDocumentEventArgs args = new MoveToNewDocumentEventArgs(collection);
			AssertEquals("should be the same instance passed in", collection, args.SerializableEDocs);
		}

		protected override void TearDown()
		{
			base.TearDown();
			DeleteTempFiles();
		}
	}
}
