using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	public class CommonAnnexDocWrapperTest : TestCaseWithFactory
	{
		public void TestReferenceNumber()
		{
			AssertEquals("Expect ReferenceNumber to be AdditionalInfoSendingObject.ReferenceNumber", sendingAction.AddInfoCollection[0].ReferenceNumber, wrapper.ReferenceNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.MovementReferenceNumber = "test";
			sendingAction = new UploadDocumentsSendingAction(bill);
			var supportingDocument = bill.DocManagerInfo().AddFileOrDocument(new byte[1], "SuppDoc.pdf", "CIV");
			var requestedDocument = bill.RequestedDocuments.AddNew();
			requestedDocument.CSI_Status = "OPE";
			docSendingObject = sendingAction.AddInfoCollection[0].EDocsCollection.AddNew();
			docSendingObject.EDoc = supportingDocument.UniqueKey;
			sendingAction.AddInfoCollection[0].ReferenceNumber = "test number";

			wrapper = new CommonAnnexDocWrapper(docSendingObject.Document, docSendingObject.Document.FileName, sendingAction.AddInfoCollection[0]);
		}

		CommonAnnexDocWrapper wrapper;
		UploadDocumentsSendingAction sendingAction;
		DocumentSendingObject docSendingObject;
	}
}
