using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Test
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
			var bill = Factory.NewWithValidTestData<AsycudaManifestHeader>().Bills.AddNew();
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

		AsycudaBill CreateBillWithoutAnyDocuments()
		{
			var bill = Factory.New<AsycudaBill>();
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
			var additionalInfoSendingObj = new AdditionalInfoSendingObject(bill, null, null);
			var sendingObject = new DocumentSendingObject(additionalInfoSendingObj);

			return sendingObject;
		}
	}
}
