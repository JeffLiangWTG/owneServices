using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.Business.Testing
{
	class SupportingDocumentWithImageProviderTest : DataProviderTestCase<SupportingDocumentWithImageProvider>
	{
		public void TestDocumentImage()
		{
			AssertType<DocumentImageProvider>("Type of DocumentImage.", Provider.DocumentImage);
		}

		public void TestDescription()
		{
			AssertEquals("Description", "Commercial Invoice", Provider.Description);
		}

		protected override SupportingDocumentWithImageProvider GetProvider() => new SupportingDocumentWithImageProvider(sendingObject);

		protected override void SetUp()
		{
			base.SetUp();
			var testBizObjs = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);
			var entryHeaderWrapper = testBizObjs.entryHeaderWrapper;
			var supp1 = entryHeaderWrapper.Declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Supp1.pdf", "CIV");
			sendingObject = new DocumentSendingObject(entryHeaderWrapper.EntryHeader, null);
			sendingObject.EDoc = supp1.UniqueKey;
		}

		DocumentSendingObject sendingObject;
	}
}
