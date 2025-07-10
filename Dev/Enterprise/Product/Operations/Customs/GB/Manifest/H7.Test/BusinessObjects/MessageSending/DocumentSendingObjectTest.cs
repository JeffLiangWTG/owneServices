using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.GB.H7.Business.Testing
{
	[TestedType(typeof(DocumentSendingObject))]
	class DocumentSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFileName()
		{
			var bill = CreateBillWithoutAnyDocuments();
			var eDoc = bill.DocManagerInfo().AddFileOrDocument(new byte[1], "AddInfo1.pdf", "CIV");

			var sendingObject = GetDocumentSendingObject(bill);
			AssertNullOrEmpty("FileName with No EDoc", sendingObject.FileName);

			sendingObject.EDoc = eDoc.UniqueKey;
			AssertEquals("FileName", "AddInfo1.pdf", sendingObject.FileName);
		}

		public void TestFileDescription()
		{
			var bill = CreateBillWithoutAnyDocuments();
			var eDoc = bill.DocManagerInfo().AddFileOrDocument(new byte[1], "AddInfo1.pdf", "CIV");
			var sendingObject = GetDocumentSendingObject(bill);
			sendingObject.EDoc = eDoc.UniqueKey;
			AssertEquals("Initial FileDescription", "Commercial Invoice", sendingObject.FileDescription);

			sendingObject.FileDescription = "Something different";
			var eDoc2 = bill.DocManagerInfo().AddFileOrDocument(new byte[1], "AddInfo2.pdf", Core.Constants.RefDocTypes.MiscellaneousDocument, description: "I am a description");
			sendingObject.EDoc = eDoc2.UniqueKey;
			AssertEquals("New FileDescription", "I am a description", sendingObject.FileDescription);
		}

		public void TestFileSizeInKB()
		{
			var bill = CreateBillWithoutAnyDocuments();
			var eDoc = bill.DocManagerInfo().AddFileOrDocument(new byte[1], "AddInfo1.pdf", "CIV");
			var sendingObject = GetDocumentSendingObject(bill);

			AssertEquals("FileSizeInKB with No EDoc", ZDecimal.Zero, sendingObject.FileSizeInKB);

			sendingObject.EDoc = eDoc.UniqueKey;
			AssertEquals("FileSizeInKB", 1m, sendingObject.FileSizeInKB);
		}

		public void TestDocType()
		{
			var bill = CreateBillWithoutAnyDocuments();
			var eDoc = bill.DocManagerInfo().AddFileOrDocument(new byte[1], "AddInfo1.pdf", "CIV");
			var sendingObject = GetDocumentSendingObject(bill);
			sendingObject.EDoc = eDoc.UniqueKey;
			AssertEquals("Initial Document Type is empty", "", sendingObject.DocumentType);

			sendingObject.DocumentType = "DC44I";
			AssertEquals("Document Type can be assigned", "DC44I", sendingObject.DocumentType);
		}

		public void TestCaptions()
		{
			var sendingObject = GetDocumentSendingObject();
			CombineAssertions(() =>
			{
				AssertEquals("FileName Caption", "Name", DataBoundResourceStrings.GetDataForProperty(sendingObject.FileNameInfo)?.Caption);
				AssertEquals("FileSizeInKB Caption", "File Size", DataBoundResourceStrings.GetDataForProperty(sendingObject.FileSizeInKBInfo)?.Caption);
				AssertEquals("FileDescription Caption", "Description", DataBoundResourceStrings.GetDataForProperty(sendingObject.FileDescriptionInfo)?.Caption);
			});
		}

		public void TestAvailableEDocsList_UpdatesWhenEDocAddedToBill()
		{
			var bill = CreateBillWithoutAnyDocuments();
			Factory.Save();

			var sendingObject = GetDocumentSendingObject(bill);
			AssertEquals(0, sendingObject.AvailableEDocs.Count);
			AssertNull("Does not create StorageMain if it does not exist", bill.DocManagerInfo().MasterFactory.GetStorageMainForPK(bill.PK));

			var newFactory = NewFactory();
			var billInNewFactory = newFactory.Load<AsycudaBill>(bill.PK);
			billInNewFactory.DocManagerInfo().AddFileOrDocument(new byte[1], "AddInfo1.pdf", "CIV");
			billInNewFactory.DocManagerInfo().Save();

			AssertEquals(1, sendingObject.AvailableEDocs.Count);
		}

		public void TestLocalReferenceNumber()
		{
			var bill = CreateBillWithoutAnyDocuments();
			Factory.Save();

			AssertNotNullOrEmpty("Precondition: LocalReferenceNumber has been set", bill.LocalReferenceNumber);

			var sendingObject = GetDocumentSendingObject(bill);
			AssertEquals("Should be bill's LRN", bill.LocalReferenceNumber, sendingObject.LocalReferenceNumber);

			bill.MovementReferenceNumber = "MRN001";
			sendingObject = GetDocumentSendingObject(bill);

			AssertEquals("Should be bill's MRN", "MRN001", sendingObject.LocalReferenceNumber);
		}

		public void TestShouldSend()
		{
			var bill = CreateBillWithoutAnyDocuments();
			var uploadDocumentsSendingAction = new UploadDocumentsSendingAction(bill);
			uploadDocumentsSendingAction.ShouldSend = true;
			var sendingObject = new DocumentSendingObject(uploadDocumentsSendingAction);

			AssertEquals("ShouldSend should be true when SendingAction's ShouldSend is true", true, sendingObject.ShouldSend);

			uploadDocumentsSendingAction.ShouldSend = false;
			AssertEquals("ShouldSend should be false when SendingAction's ShouldSend is false", false, sendingObject.ShouldSend);
		}

		#region ISupportingDocumentMessageDataProvider Members

		public void TestBusinessObject()
		{
			var bill = CreateBillWithoutAnyDocuments();
			ISupportingDocumentMessageDataProvider sendingObject = GetDocumentSendingObject(bill);
			AssertEquals(bill, sendingObject.BusinessObject);
		}

		public void TestContextType()
		{
			var bill = CreateBillWithoutAnyDocuments();
			ISupportingDocumentMessageDataProvider sendingObject = GetDocumentSendingObject(bill);
			AssertEquals(DataContextType.AsycudaBill, sendingObject.ContextType);
		}

		public void TestContextReference()
		{
			var bill = CreateBillWithoutAnyDocuments();
			bill.ABL_BillNumber = "1234";
			ISupportingDocumentMessageDataProvider sendingObject = GetDocumentSendingObject(bill);
			AssertEquals("1234", sendingObject.ContextReference);
		}

		#endregion

		AsycudaBill CreateBillWithoutAnyDocuments()
		{
			var bill = Factory.New<AsycudaManifestHeader>().Bills.AddNew();
			return bill;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetDocumentSendingObject();
		}

		DocumentSendingObject GetDocumentSendingObject(AsycudaBill bill = null)
		{
			if (bill == null)
			{
				bill = CreateBillWithoutAnyDocuments();
			}
			var uploadDocumentsSendingAction = new UploadDocumentsSendingAction(bill);
			var sendingObject = new DocumentSendingObject(uploadDocumentsSendingAction);

			return sendingObject;
		}
	}
}
