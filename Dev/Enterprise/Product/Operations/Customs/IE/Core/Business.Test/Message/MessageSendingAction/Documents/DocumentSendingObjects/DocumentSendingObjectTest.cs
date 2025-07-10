using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(DocumentSendingObject))]
	sealed class DocumentSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFileName()
		{
			AssertEquals("FileName", "AddInfo1.pdf", sendingObject.FileName);

			var documentSendingObject = new DocumentSendingObject(CreateEntryHeaderWithoutAnyDocuments(), null);
			AssertNullOrEmpty("FileName with No EDoc", documentSendingObject.FileName);
		}

		public void TestFileDescription()
		{
			AssertEquals("Initial FileDescription", "Commercial Invoice", sendingObject.FileDescription);

			sendingObject.FileDescription = "Something different";
			var eDoc = testBizObjs.entryHeaderWrapper.Declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "AddInfo1.pdf", Core.Constants.RefDocTypes.MiscellaneousDocument, description: "I am a description");
			sendingObject.EDoc = eDoc.UniqueKey;
			AssertEquals("New FileDescription", "I am a description", sendingObject.FileDescription);
		}

		public void TestFileSizeInKB()
		{
			AssertEquals("FileSizeInKB", 1m, sendingObject.FileSizeInKB);

			var documentSendingObject = new DocumentSendingObject(CreateEntryHeaderWithoutAnyDocuments(), null);
			AssertEquals("FileSizeInKB with No EDoc", ZDecimal.Zero, documentSendingObject.FileSizeInKB);
		}

		public void TestCaptions()
		{
			CombineAssertions(() =>
			{
				AssertEquals("FileName Caption", "Name", DataBoundResourceStrings.GetDataForProperty(sendingObject.FileNameInfo)?.Caption);
				AssertEquals("FileSizeInKB Caption", "File Size", DataBoundResourceStrings.GetDataForProperty(sendingObject.FileSizeInKBInfo)?.Caption);
				AssertEquals("FileDescription Caption", "Description", DataBoundResourceStrings.GetDataForProperty(sendingObject.FileDescriptionInfo)?.Caption);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			testBizObjs = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);
			var supDoc = testBizObjs.entryHeaderWrapper.RandomInvoiceHeader.SupportingDocuments.AddNew();
			supDoc.CSI_Code = "9002";
			supDoc.CSI_Description = "9002 Desc";

			var add1 = testBizObjs.entryHeaderWrapper.Declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "AddInfo1.pdf", "CIV");

			var requestedDocument = testBizObjs.entryHeaderWrapper.EntryHeader.EntryInstruction.RequestedDocuments.AddNew();
			requestedDocument.CSI_Code = "9002";
			requestedDocument.CSI_Description = "9002 Desc";

			sendingObject = new DocumentSendingObject(testBizObjs.entryHeaderWrapper.EntryHeader, new AdditionalInfoSendingObject(testBizObjs.entryHeaderWrapper.EntryHeader, null, requestedDocument));
			sendingObject.EDoc = add1.UniqueKey;
		}

		protected override BusinessObject GetNewBusinessObject() => sendingObject;

		DocumentSendingObject sendingObject;
		(EntryHeaderWrapper entryHeaderWrapper, EntryLineWrapper entryLineWrapper) testBizObjs;

		CusEntryHeader CreateEntryHeaderWithoutAnyDocuments()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			return entryHeader;
		}
	}
}
