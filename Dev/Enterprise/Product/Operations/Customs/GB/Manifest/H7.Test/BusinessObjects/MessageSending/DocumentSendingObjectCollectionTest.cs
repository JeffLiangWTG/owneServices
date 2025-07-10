using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.H7.Business.Testing
{
	[TestedType(typeof(DocumentSendingObjectCollection))]
	class DocumentSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocumentSendingObjectCollection>
	{
		protected override DocumentSendingObjectCollection GetCollectionToTest() => new DocumentSendingObjectCollection(testBizObj, uploadDocumentsSendingAction);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new DocumentSendingObject(uploadDocumentsSendingAction);

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<AsycudaManifestHeader>();
			testBizObj = header.Bills.AddNew();

			uploadDocumentsSendingAction = new UploadDocumentsSendingAction(testBizObj);
		}

		AsycudaBill testBizObj;
		UploadDocumentsSendingAction uploadDocumentsSendingAction;
	}
}
