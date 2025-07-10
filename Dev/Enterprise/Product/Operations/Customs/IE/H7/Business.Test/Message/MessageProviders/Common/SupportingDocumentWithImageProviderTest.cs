using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	sealed class SupportingDocumentWithImageProviderTest : DataProviderTestCase<SupportingDocumentWithImageProvider>
	{
		public void TestDocumentImage()
		{
			AssertType<IE.Business.DocumentImageProvider>("Type of DocumentImage.", Provider.DocumentImage);
		}

		public void TestDescription()
		{
			AssertEquals("Description", "Commercial Invoice", Provider.Description);
		}

		protected override SupportingDocumentWithImageProvider GetProvider()
		{
			var bill = Factory.New<AsycudaBill>();
			var eDoc = bill.DocManagerInfo().AddFileOrDocument(new byte[1], "Supp1.pdf", "CIV");

			var requestedDocument = bill.RequestedDocuments.AddNew();

			var addInfoSendingObject = new AdditionalInfoSendingObject(bill, null, requestedDocument);
			var sendingObject = new DocumentSendingObject(addInfoSendingObject);
			sendingObject.EDoc = eDoc.UniqueKey;

			return new SupportingDocumentWithImageProvider(sendingObject);
		}
	}
}
