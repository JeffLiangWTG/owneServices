using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusStorageDocPivotMessageSendingActionCollection))]
	sealed class CusStorageDocPivotMessageSendingActionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CusStorageDocPivotMessageSendingActionCollection>
	{
		public void TestAllowNew()
		{
			var collection = new CusStorageDocPivotMessageSendingActionCollection(Factory.New<QuarantineColsHeader>());
			AssertEquals(false, collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			var collection = new CusStorageDocPivotMessageSendingActionCollection(Factory.New<QuarantineColsHeader>());
			AssertEquals(false, collection.AllowRemove);
		}

		public void TestPopulateElements()
		{
			var colsHeader = Factory.New<QuarantineColsHeader>();
			var doc1 = colsHeader.EDocPivotCollection.AddNew();
			var doc2 = colsHeader.EDocPivotCollection.AddNew();
			doc2.CSD_MessageStatus = COLSDocumentStatusList.Codes.AwaitingDocumentSentResponse;
			var doc3 = colsHeader.EDocPivotCollection.AddNew();
			doc3.CSD_MessageStatus = COLSDocumentStatusList.Codes.Discarded;
			AssertEquals(3, new CusStorageDocPivotMessageSendingActionCollection(colsHeader).Count);
		}

		protected override CusStorageDocPivotMessageSendingActionCollection GetCollectionToTest()
		{
			return new CusStorageDocPivotMessageSendingActionCollection(Factory.New<QuarantineColsHeader>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var colsHeader = Factory.New<QuarantineColsHeader>();
			var docPivot = colsHeader.EDocPivotCollection.AddNew();
			return new CusStorageDocPivotMessageSendingAction(docPivot);
		}
	}
}
