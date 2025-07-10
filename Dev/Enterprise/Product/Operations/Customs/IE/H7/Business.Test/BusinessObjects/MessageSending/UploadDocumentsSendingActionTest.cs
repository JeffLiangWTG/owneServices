using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	[TestedType(typeof(UploadDocumentsSendingAction))]
	class UploadDocumentsSendingActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			var newUploadDocumentsSendingAction =
				new UploadDocumentsSendingAction(Factory.NewWithValidTestData<AsycudaBill>());
			CombineAssertions(() =>
			{
				AssertEquals("ShouldSend", true, newUploadDocumentsSendingAction.ShouldSend);
				AssertEquals("MessageType (Action)", AISOutgoingMessageTypeList.Codes.DocumentsReceived,
					newUploadDocumentsSendingAction.Action);
			});
		}

		public void TestCreateSender()
		{
			var sender = uploadDocumentsSendingAction.CreateSender();
			AssertType<IEMessageSender>(sender);
		}

		public void TestAddInfoCollection()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected collection size", 1, uploadDocumentsSendingAction.AddInfoCollection.Count);
				AssertType<AdditionalInfoSendingObjectCollection>("Expected collection type", uploadDocumentsSendingAction.AddInfoCollection);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => uploadDocumentsSendingAction;

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<AsycudaManifestHeader>();
			testBizObj = header.Bills.AddNew();
			var requestedDocument = testBizObj.RequestedDocuments.AddNew();
			requestedDocument.CSI_Status = EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestOpened;

			uploadDocumentsSendingAction = new UploadDocumentsSendingAction(testBizObj);
		}

		AsycudaBill testBizObj;
		UploadDocumentsSendingAction uploadDocumentsSendingAction;
	}
}
