using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Test
{
	[TestedType(typeof(DocumentSendingObjectCollection))]
	class DocumentSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocumentSendingObjectCollection>
	{
		protected override DocumentSendingObjectCollection GetCollectionToTest() => new DocumentSendingObjectCollection(testBizObj, additionalInfoSendingObj);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new DocumentSendingObject(additionalInfoSendingObj);

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<AsycudaManifestHeader>();
			testBizObj = header.Bills.AddNew();
			requestedDocument = testBizObj.RequestedDocuments.AddNew();

			additionalInfoSendingObj = new AdditionalInfoSendingObject(testBizObj, null, requestedDocument);
		}

		AsycudaBill testBizObj;
		RequestedDocument requestedDocument;
		AdditionalInfoSendingObject additionalInfoSendingObj;
	}
}
