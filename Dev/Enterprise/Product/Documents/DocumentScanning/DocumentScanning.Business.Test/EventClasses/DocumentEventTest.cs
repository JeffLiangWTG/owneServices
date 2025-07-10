namespace Enterprise.DocumentScanning.Business.Test
{
	public class DocumentEventTest : TestCaseWithDocumentFactory
	{
		public void TestConstructor()
		{
			StorageDocs document = StorageDocs.NewWithParent_DEBUG(MasterFactory);
			DocumentEventArgs args = new DocumentEventArgs(document);
			AssertEquals("Document should be set to what was passed in", document, args.Document);

			StorageFile file = StorageFile.NewWithParent_DEBUG(MasterFactory);
			args = new DocumentEventArgs(file);
			AssertEquals("Document should be set to what was passed in", file, args.Document);
		}
	}
}
