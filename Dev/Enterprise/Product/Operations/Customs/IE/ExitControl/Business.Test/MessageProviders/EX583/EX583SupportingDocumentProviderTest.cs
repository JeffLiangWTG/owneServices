using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business;

namespace Enterprise.Customs.IE.ExitControl.Business.AES.Testing
{
	class EX583SupportingDocumentProviderTest : DataProviderTestCase<EX583SupportingDocumentProvider>
	{
		public void TestDocumentImage()
		{
			AssertType<DocumentImageProvider>("Type of DocumentImage.", Provider.DocumentImage);
		}

		public void TestDescription()
		{
			AssertEquals("Description", "Commercial Invoice", Provider.Description);
		}

		protected override EX583SupportingDocumentProvider GetProvider() => new EX583SupportingDocumentProvider(sendingObject);

		protected override void SetUp()
		{
			base.SetUp();
			var exitHeader = Factory.New<CusExitHeader>();
			var exitReport = exitHeader.CusExitReports.AddNew();
			var supp1 = exitHeader.DocManagerInfo.AddFileOrDocument(new byte[1], "Supp1.pdf", "CIV");
			sendingObject = new DocumentSendingObject(exitReport);
			sendingObject.EDoc = supp1.UniqueKey;
		}
		DocumentSendingObject sendingObject;
	}
}
