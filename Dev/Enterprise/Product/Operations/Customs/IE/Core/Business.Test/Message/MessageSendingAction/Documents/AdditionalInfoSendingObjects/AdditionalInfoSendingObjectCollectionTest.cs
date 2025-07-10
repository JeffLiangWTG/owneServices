using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(AdditionalInfoSendingObjectCollection))]
	class AdditionalInfoSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AdditionalInfoSendingObjectCollection>
	{
		public void TestMaxCount()
		{
			AssertEquals("Should have set MaxCount.", 99, ((ISupportMaxCountValidation)Collection).MaxCountValidator.MaxCount);
		}

		public void TestLoadElements()
		{
			var anotherRequestedDocument = testBizObjs.entryHeaderWrapper.EntryHeader.EntryInstruction.RequestedDocuments.AddNew();
			anotherRequestedDocument.CSI_Status = EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestCancelled;
			anotherRequestedDocument.CSI_Code = "ADD2";

			Collection.LoadElements();
			CombineAssertions("When anotherRequestedDocument.CSI_Status is not OPE", () =>
			{
				AssertEquals("Count", 1, Collection.Count);
				AssertEquals("DocumentType", "ADD1", Collection[0].DocumentType);
			});

			anotherRequestedDocument.CSI_Status = EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestOpened;
			Collection.LoadElements();
			CombineAssertions("When both are OPE", () =>
			{
				AssertEquals("Count", 2, Collection.Count);
				AssertEquals("ADD2 should be included as status changes to OPE", true, Collection.Cast<AdditionalInfoSendingObject>().Any(info => info.DocumentType == "ADD2"));
			});

			Collection.LoadElements((doc) => doc.CSI_Code == "ADD2");
			CombineAssertions("When filter ADD2", () =>
			{
				AssertEquals("Count", 1, Collection.Count);
				AssertEquals("DocumentType", "ADD2", Collection[0].DocumentType);
			});
		}

		public void TestComplementaryInformationIsSupplied()
		{
			var anotherRequestedDocument = testBizObjs.entryHeaderWrapper.EntryHeader.EntryInstruction.RequestedDocuments.AddNew();
			anotherRequestedDocument.CSI_Status = EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestOpened;
			anotherRequestedDocument.CSI_Code = "ADD2";
			anotherRequestedDocument.RequestInformation = "ExtraInfo";

			Collection.LoadElements();
			AssertEquals("ComplementaryInformation", "ExtraInfo", Collection[1].DocumentInformation);
		}

		protected override AdditionalInfoSendingObjectCollection GetCollectionToTest() => new AdditionalInfoSendingObjectCollection(testBizObjs.entryHeaderWrapper.EntryHeader, parentObject);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var entryHeader = testBizObjs.entryHeaderWrapper.EntryHeader;
			var newRequestedDocument = entryHeader.EntryInstruction.RequestedDocuments.AddNew();
			return new AdditionalInfoSendingObject(entryHeader, null, newRequestedDocument);
		}

		public void TestOnAddedAttachesShouldSendChangedEventHandler()
		{
			var entryHeader = testBizObjs.entryHeaderWrapper.EntryHeader;
			var uploadDocumentsSendingAction = new UploadDocumentsSendingAction(entryHeader);
			var additionalInfoSendingObject = new Mock<AdditionalInfoSendingObject>(entryHeader, uploadDocumentsSendingAction, null);
			var additionalInfoSendingObjectCollection = new AdditionalInfoSendingObjectCollection(entryHeader, uploadDocumentsSendingAction);

			additionalInfoSendingObjectCollection.Add(additionalInfoSendingObject.Object);
			uploadDocumentsSendingAction.ShouldSend = !(ZBool)uploadDocumentsSendingAction.ShouldSendInfo.Value;

			additionalInfoSendingObject.Verify(v => v.ValidateAllAndRefreshBinding(It.IsAny<object>(), It.IsAny<EventArgs>()), Times.Once);

			Assert("Needed to avoid 'Empty test' error", true);
		}

		public void TestOnRemovedRemovesShouldSendChangedEventHandler()
		{
			var entryHeader = testBizObjs.entryHeaderWrapper.EntryHeader;
			var uploadDocumentsSendingAction = new UploadDocumentsSendingAction(entryHeader);
			var additionalInfoSendingObject = new Mock<AdditionalInfoSendingObject>(entryHeader, uploadDocumentsSendingAction, null);
			var additionalInfoSendingObjectCollection = new AdditionalInfoSendingObjectCollection(entryHeader, uploadDocumentsSendingAction);

			additionalInfoSendingObjectCollection.Add(additionalInfoSendingObject.Object);
			additionalInfoSendingObjectCollection.Remove(additionalInfoSendingObject.Object);
			uploadDocumentsSendingAction.ShouldSend = !(ZBool)uploadDocumentsSendingAction.ShouldSendInfo.Value;

			additionalInfoSendingObject.Verify(v => v.ValidateAllAndRefreshBinding(It.IsAny<object>(), It.IsAny<EventArgs>()), Times.Never);

			Assert("Needed to avoid 'Empty test' error", true);
		}

		protected override void SetUp()
		{
			base.SetUp();
			testBizObjs = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);
			requestedDocument = testBizObjs.entryHeaderWrapper.EntryHeader.EntryInstruction.RequestedDocuments.AddNew();
			requestedDocument.CSI_Code = "ADD1";
			requestedDocument.CSI_Status = EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestOpened;
			parentObject = new UploadDocumentsSendingAction(testBizObjs.entryHeaderWrapper.EntryHeader);
		}
		EU.Business.RequestedDocument requestedDocument;
		(EntryHeaderWrapper entryHeaderWrapper, EntryLineWrapper entryLineWrapper) testBizObjs;
		UploadDocumentsSendingAction parentObject;
	}
}
