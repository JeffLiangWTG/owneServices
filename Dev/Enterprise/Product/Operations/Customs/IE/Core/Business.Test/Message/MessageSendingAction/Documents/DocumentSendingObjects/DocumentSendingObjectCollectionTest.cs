using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(DocumentSendingObjectCollection))]
	class DocumentSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocumentSendingObjectCollection>
	{
		public void TestOnAddedAttachesShouldSendChangedEventHandler()
		{
			var entryHeader = testBizObjs.entryHeaderWrapper.EntryHeader;
			var uploadDocumentsSendingAction = new UploadDocumentsSendingAction(entryHeader);
			var additionalInfoSendingObject = new AdditionalInfoSendingObject(entryHeader, uploadDocumentsSendingAction, null);
			var collection = new DocumentSendingObjectCollection(entryHeader, additionalInfoSendingObject);
			var documentSendingObjectForTest = new Mock<DocumentSendingObject>(entryHeader, additionalInfoSendingObject);

			collection.Add(documentSendingObjectForTest.Object);
			uploadDocumentsSendingAction.ShouldSend = !(ZBool)uploadDocumentsSendingAction.ShouldSendInfo.Value;

			documentSendingObjectForTest.Verify(v => v.ValidateAllAndRefreshBinding(It.IsAny<object>(), It.IsAny<EventArgs>()), Times.Once);

			Assert("Needed to avoid 'Empty test' error", true);
		}

		public void TestOnRemovedRemovesShouldSendChangedEventHandler()
		{
			var entryHeader = testBizObjs.entryHeaderWrapper.EntryHeader;
			var uploadDocumentsSendingAction = new UploadDocumentsSendingAction(entryHeader);
			var additionalInfoSendingObject = new AdditionalInfoSendingObject(entryHeader, uploadDocumentsSendingAction, null);
			var collection = new DocumentSendingObjectCollection(entryHeader, additionalInfoSendingObject);
			var documentSendingObjectForTest = new Mock<DocumentSendingObject>(entryHeader, additionalInfoSendingObject);

			collection.Add(documentSendingObjectForTest.Object);
			collection.Remove(documentSendingObjectForTest.Object);
			uploadDocumentsSendingAction.ShouldSend = !(ZBool)uploadDocumentsSendingAction.ShouldSendInfo.Value;

			documentSendingObjectForTest.Verify(v => v.ValidateAllAndRefreshBinding(It.IsAny<object>(), It.IsAny<EventArgs>()), Times.Never);

			Assert("Needed to avoid 'Empty test' error", true);
		}

		protected override DocumentSendingObjectCollection GetCollectionToTest() => new DocumentSendingObjectCollection(testBizObjs.entryHeaderWrapper.EntryHeader, null);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new DocumentSendingObject(testBizObjs.entryHeaderWrapper.EntryHeader, null);

		protected override void SetUp()
		{
			base.SetUp();
			testBizObjs = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);
		}

		(EntryHeaderWrapper entryHeaderWrapper, EntryLineWrapper entryLineWrapper) testBizObjs;
	}
}
