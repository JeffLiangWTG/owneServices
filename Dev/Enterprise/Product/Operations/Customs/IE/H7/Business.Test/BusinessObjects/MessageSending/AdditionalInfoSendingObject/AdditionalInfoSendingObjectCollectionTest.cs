using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	[TestedType(typeof(AdditionalInfoSendingObjectCollection))]
	class AdditionalInfoSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AdditionalInfoSendingObjectCollection>
	{
		public void TestLoadElements()
		{
			CombineAssertions(() =>
			{
				Collection.LoadElements();
				AssertEquals("Count", 1, Collection.Count);
				AssertType<AdditionalInfoSendingObject>("Expected type", Collection[0]);
				AssertEquals("DocumentType", "ADD1", Collection[0].DocumentType);
			});
		}

		protected override AdditionalInfoSendingObjectCollection GetCollectionToTest() =>
			new AdditionalInfoSendingObjectCollection(testBizObj, parentObject);

		protected override BusinessObject GetNewElementToAddToTheCollection() =>
			new AdditionalInfoSendingObject(testBizObj, null, requestedDocument);

		protected override void SetUp()
		{
			testBizObj = Factory.New<AsycudaBill>();
			requestedDocument = testBizObj.RequestedDocuments.AddNew();
			requestedDocument.CSI_Code = "ADD1";
			requestedDocument.CSI_Status =
				EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestOpened;
			parentObject = new UploadDocumentsSendingAction(testBizObj);
		}

		RequestedDocument requestedDocument;
		AsycudaBill testBizObj;
		UploadDocumentsSendingAction parentObject;
	}
}
