using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.GB.H7.Business.Testing
{
	[TestedType(typeof(UploadDocumentsSendingAction))]
	class UploadDocumentsSendingActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaultShouldSendIsFalse()
		{
			Assert(!uploadDocumentsSendingAction.ShouldSend);
		}

		public void TestAllDocumentsAreAttached()
		{
			uploadDocumentsSendingAction.EDocsCollection.AddNew().EDoc = ZGuid.NewZGuid();

			Assert("Should be marked as all attached when eDoc PK is specified", uploadDocumentsSendingAction.DocumentsAttached);
		}

		protected override BusinessObject GetNewBusinessObject() => uploadDocumentsSendingAction;

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
