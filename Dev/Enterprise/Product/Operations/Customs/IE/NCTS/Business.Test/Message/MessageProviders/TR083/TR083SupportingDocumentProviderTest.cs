using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class TR083SupportingDocumentProviderTest : Customs.Business.Testing.DataProviderTestCase<TR083SupportingDocumentProvider>
	{
		public void TestDocumentImage()
		{
			AssertType<DocumentImageProvider>("Type of DocumentImage.", Provider.DocumentImage);
		}

		public void TestDescription()
		{
			AssertEquals("Description", "Commercial Invoice", Provider.Description);
		}

		protected override TR083SupportingDocumentProvider GetProvider() => new TR083SupportingDocumentProvider(sendingObj);

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var suppDoc = nctsHeader.DocManagerInfo.AddFileOrDocument(new byte[1], "SuppDoc.pdf", "CIV");
			sendingObj = new DocumentSendingObject(nctsHeader);
			sendingObj.EDoc = suppDoc.UniqueKey;
		}
		DocumentSendingObject sendingObj;
		NctsHeader nctsHeader;
	}
}
