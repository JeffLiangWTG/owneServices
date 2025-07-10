using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.H7.Business.Testing
{
	[TestedType(typeof(UploadDocumentsSendingActionParent))]
	class UploadDocumentsSendingActionParentTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new UploadDocumentsSendingActionParent(Factory.New<AsycudaManifestHeader>());
		}

		public void TestMessageSendingObjectProperties()
		{
			var sendingObjectParent = GetNewBusinessObject() as BaseMessageSendingObjectParent;
			var sendingObjectPropertyNames = sendingObjectParent.MessageSendingObjectProperties.Select(x => x.PropertyName).ToArray();
			AssertArrayEqualsByElements("MessageSendingObjectProperties", new ZString[] { "BillNumber", "MovementReferenceNumber", "LocalReferenceNumber", "EntryStatus", "Action" }, sendingObjectPropertyNames);
		}

		public void TestSendingObjectsCollection()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "Bill1";
			var requestedDocument = bill.RequestedDocuments.AddNew();
			requestedDocument.CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;
			var requestedDocument2 = bill.RequestedDocuments.AddNew();
			requestedDocument2.CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;

			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "Bill2";
			var requestedDocument3 = bill2.RequestedDocuments.AddNew();
			requestedDocument3.CSI_Status = RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived;

			var parent = new UploadDocumentsSendingActionParent(header);
			var sendingObjectCollection = parent.SendingObjectsCollection;

			AssertEquals("SendingObjectsCollection contains 2 entry for each bill regardless of open requested documents", 2, sendingObjectCollection.Count);

			var sendingObject1 = sendingObjectCollection[0];
			AssertType<UploadDocumentsSendingAction>(sendingObject1);
			AssertEquals("Bill1", sendingObject1.BillNumber);

			var sendingObject2 = sendingObjectCollection[1];
			AssertType<UploadDocumentsSendingAction>(sendingObject2);
			AssertEquals("Bill2", sendingObject2.BillNumber);
		}

		public void TestISupportingDocSendingObjectParent_SendingObjects()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "Bill1";
			var requestedDocument = bill.RequestedDocuments.AddNew();
			requestedDocument.CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;
			var eDoc1 = bill.DocManagerInfo().AddFileOrDocument(new byte[1], "AddInfo1.pdf", "CIV");
			var eDoc2 = bill.DocManagerInfo().AddFileOrDocument(new byte[1], "AddInfo2.pdf", "CIV");

			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "Bill2";
			var requestedDocument2 = bill2.RequestedDocuments.AddNew();
			requestedDocument2.CSI_Status = RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived;
			var eDoc3 = bill2.DocManagerInfo().AddFileOrDocument(new byte[1], "AddInfo3.pdf", "CIV");

			var parent = new UploadDocumentsSendingActionParent(header);
			var sendingEDoc1 = parent.SendingObjectsCollection[0].EDocsCollection.AddNew();
			sendingEDoc1.EDoc = eDoc1.UniqueKey;
			var sendingEDoc2 = parent.SendingObjectsCollection[0].EDocsCollection.AddNew();
			sendingEDoc2.EDoc = eDoc2.UniqueKey;

			var sendingEDoc3 = parent.SendingObjectsCollection[1].EDocsCollection.AddNew();
			sendingEDoc3.EDoc = eDoc3.UniqueKey;

			var sendingObjects = ((ISupportingDocSendingObjectParent)parent).SendingObjects;

			AssertEquals("SendingObjects contains 3 entry, 1 for each document", 3, sendingObjects.Count());
			AssertContainsExactElementsInAnyOrder(["AddInfo1.pdf", "AddInfo2.pdf", "AddInfo3.pdf"], sendingObjects.Select(x => x.Document.FileName));
			AssertContainsExactElementsInAnyOrder([bill, bill, bill2], sendingObjects.Select(x => x.BusinessObject));
		}
	}
}
