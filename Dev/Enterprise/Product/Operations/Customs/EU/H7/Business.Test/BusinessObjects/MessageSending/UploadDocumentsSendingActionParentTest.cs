using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Test
{
	[TestedType(typeof(UploadDocumentsSendingActionParent<UploadDocumentsSendingAction>))]
	class UploadDocumentsSendingActionParentTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new UploadDocumentsSendingActionParent<UploadDocumentsSendingAction>(Factory.New<AsycudaManifestHeader>());
		}

		public void TestMessageSendingObjectProperties()
		{
			var sendingObjectParent = GetNewBusinessObject() as BaseMessageSendingObjectParent;
			var sendingObjectPropertyNames = sendingObjectParent.MessageSendingObjectProperties.Select(x => x.PropertyName).ToArray();
			AssertArrayEqualsByElements("MessageSendingObjectProperties", new ZString[] { "BillNumber", "MovementReferenceNumber", "LocalReferenceNumber", "EntryStatus", "Action" }, sendingObjectPropertyNames);
		}

		public void TestSendingObjectsCollection()
		{
			var parent = UploadDocumentsSendingActionParent();
			var sendingObjectCollection = parent.SendingObjectsCollection;

			AssertEquals("SendingObjectsCollection contains 1 entry for each bill open requested documents", 1, sendingObjectCollection.Count);
			var sendingObject = sendingObjectCollection[0];
			AssertType<UploadDocumentsSendingActionForTest>(sendingObject);
			AssertEquals("Bill1", sendingObject.BillNumber);
		}

		public void TestSendingObjectsCollectionView()
		{
			var parent = UploadDocumentsSendingActionParent();
			var sendingActionFilteredCollection = parent.UploadDocumentsSendingActionFilteredCollection;

			AssertEquals("SendingObjectsCollection contains 1 entry for each bill open requested documents", 1, sendingActionFilteredCollection.Count);
			var sendingObject = sendingActionFilteredCollection[0];
			AssertType<UploadDocumentsSendingActionForTest>(sendingObject);
			AssertEquals("Bill1", sendingObject.BillNumber);

			var filter = new ZQuery(AsycudaBillSchema.ABL_BillNumber, "###");
			sendingActionFilteredCollection.Load(filter);

			AssertEquals("SendingObjectsCollection contains no entry after applying the filter", 0, sendingActionFilteredCollection.Count);
		}

		UploadDocumentsSendingActionParent<UploadDocumentsSendingActionForTest> UploadDocumentsSendingActionParent()
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

			var parent = new UploadDocumentsSendingActionParent<UploadDocumentsSendingActionForTest>(header);
			return parent;
		}

		class UploadDocumentsSendingActionForTest : UploadDocumentsSendingAction
		{
			public UploadDocumentsSendingActionForTest(AsycudaBill bill) : base(bill)
			{
			}
		}
	}
}
