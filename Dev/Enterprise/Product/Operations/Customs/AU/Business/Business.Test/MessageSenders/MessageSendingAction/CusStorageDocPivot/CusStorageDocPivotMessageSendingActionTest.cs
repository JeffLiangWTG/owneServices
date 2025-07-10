using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusStorageDocPivotMessageSendingAction))]
	sealed class CusStorageDocPivotMessageSendingActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaultShouldSend() => CombineAssertions(() =>
		{
			var docPivot = Factory.New<CusStorageDocPivot>();
			var messageSendingAction = new CusStorageDocPivotMessageSendingAction(docPivot);
			AssertEquals("ShouldSend defaults to True when CSD_MessageStatus is empty", true, messageSendingAction.ShouldSend);

			docPivot.CSD_MessageStatus = COLSDocumentStatusList.Codes.Discarded;
			messageSendingAction = new CusStorageDocPivotMessageSendingAction(docPivot);
			AssertEquals("ShouldSend defaults to False when CSD_MessageStatus is not empty", false, messageSendingAction.ShouldSend);
		});

		public void TestDocument()
		{
			var docPivot = Factory.New<CusStorageDocPivot>();
			var messageSendingAction = new CusStorageDocPivotMessageSendingAction(docPivot);
			AssertSame(docPivot, messageSendingAction.Document);
		}

		public void TestDocReference_Caption()
		{
			AssertEquals("eDoc", DataBoundResourceStrings.GetDataForProperty(typeof(CusStorageDocPivotMessageSendingAction), nameof(CusStorageDocPivotMessageSendingAction.DocReference)).Caption);
		}

		public void TestDocReference()
		{
			var declaration = Factory.New<JobDeclaration>();
			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;
			var docPivot = colsHeader.EDocPivotCollection.AddNew();
			docPivot.CSD_StorageDocReference = eDoc.UniqueKey;
			var messageSendingAction = new CusStorageDocPivotMessageSendingAction(docPivot);
			AssertEquals("Invoice.pdf", messageSendingAction.DocReference);
		}

		public void TestDocType_Caption()
		{
			AssertEquals("Document Type", DataBoundResourceStrings.GetDataForProperty(typeof(CusStorageDocPivotMessageSendingAction), nameof(CusStorageDocPivotMessageSendingAction.DocType)).Caption);
		}

		public void TestDocType()
		{
			var docPivot = Factory.New<CusStorageDocPivot>();
			docPivot.CSD_DocType = "INV";
			var messageSendingAction = new CusStorageDocPivotMessageSendingAction(docPivot);
			AssertEquals("INV", messageSendingAction.DocType);
		}

		public void TestDescription_Caption()
		{
			AssertEquals("Description", DataBoundResourceStrings.GetDataForProperty(typeof(CusStorageDocPivotMessageSendingAction), nameof(CusStorageDocPivotMessageSendingAction.Description)).Caption);
		}

		public void TestDescription()
		{
			var docPivot = Factory.New<CusStorageDocPivot>();
			docPivot.CSD_Description = "Some description";
			var messageSendingAction = new CusStorageDocPivotMessageSendingAction(docPivot);
			AssertEquals("Some description", messageSendingAction.Description);
		}

		public void TestMessageStatus_Caption()
		{
			AssertEquals("Status", DataBoundResourceStrings.GetDataForProperty(typeof(CusStorageDocPivotMessageSendingAction), nameof(CusStorageDocPivotMessageSendingAction.MessageStatus)).Caption);
		}

		public void TestMessageStatus()
		{
			var docPivot = Factory.New<CusStorageDocPivot>();
			docPivot.CSD_MessageStatus = COLSDocumentStatusList.Codes.Discarded;
			var messageSendingAction = new CusStorageDocPivotMessageSendingAction(docPivot);
			AssertEquals(COLSDocumentStatusList.Codes.Discarded, messageSendingAction.MessageStatus);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CusStorageDocPivotMessageSendingAction(Factory.New<CusStorageDocPivot>());
		}
	}
}
