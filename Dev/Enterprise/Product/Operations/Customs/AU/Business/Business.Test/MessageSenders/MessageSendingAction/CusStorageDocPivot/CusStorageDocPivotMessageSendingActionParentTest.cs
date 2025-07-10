using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusStorageDocPivotMessageSendingActionParent))]
	sealed class CusStorageDocPivotMessageSendingActionParentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSendingObjectsCollection()
		{
			var colsHeader = Factory.New<QuarantineColsHeader>();
			var doc1 = colsHeader.EDocPivotCollection.AddNew();
			var doc2 = colsHeader.EDocPivotCollection.AddNew();
			doc2.CSD_MessageStatus = COLSDocumentStatusList.Codes.AwaitingDocumentSentResponse;
			var doc3 = colsHeader.EDocPivotCollection.AddNew();
			doc3.CSD_MessageStatus = COLSDocumentStatusList.Codes.Discarded;

			var messageSendingActionParent = new CusStorageDocPivotMessageSendingActionParent(colsHeader);
			var sendingObjectsCollection = messageSendingActionParent.SendingObjectsCollection;
			CombineAssertions(() =>
			{
				AssertType<CusStorageDocPivotMessageSendingActionCollection>("Collection type", sendingObjectsCollection);
				AssertEquals("Number of objects in collection", 3, sendingObjectsCollection.Count);
			});
		}

		public void TestShouldAllowUsersToSelectSendingObjects() => CombineAssertions(() =>
		{
			var colsHeader = Factory.New<QuarantineColsHeader>();
			var messageSendingActionParent = new CusStorageDocPivotMessageSendingActionParent(colsHeader);
			AssertEquals("ShouldAllowUsersToSelectSendingObjects = false when there's no documents to be sent", false, messageSendingActionParent.ShouldAllowUsersToSelectSendingObjects);

			var doc1 = colsHeader.EDocPivotCollection.AddNew();
			var doc2 = colsHeader.EDocPivotCollection.AddNew();
			messageSendingActionParent = new CusStorageDocPivotMessageSendingActionParent(colsHeader);
			AssertEquals("ShouldAllowUsersToSelectSendingObjects = false when all documents have empty CSD_MessageStatus", false, messageSendingActionParent.ShouldAllowUsersToSelectSendingObjects);

			doc2.CSD_MessageStatus = COLSDocumentStatusList.Codes.Discarded;
			messageSendingActionParent = new CusStorageDocPivotMessageSendingActionParent(colsHeader);
			AssertEquals("ShouldAllowUsersToSelectSendingObjects = true when one or more documents have non-empty CSD_MessageStatus", true, messageSendingActionParent.ShouldAllowUsersToSelectSendingObjects);
		});

		public void TestSelectedDocuments() => CombineAssertions(() =>
		{
			var colsHeader = Factory.New<QuarantineColsHeader>();
			var doc1 = colsHeader.EDocPivotCollection.AddNew();
			doc1.CSD_MessageStatus = COLSDocumentStatusList.Codes.Discarded;
			var doc2 = colsHeader.EDocPivotCollection.AddNew();

			var messageSendingActionParent = new CusStorageDocPivotMessageSendingActionParent(colsHeader);
			AssertContainsExactElementsInAnyOrder("Only 1 doc is selected", new[] { doc2 }, messageSendingActionParent.SelectedDocuments);

			messageSendingActionParent.SendingObjectsCollection.Cast<CusStorageDocPivotMessageSendingAction>().ForEach(x => x.ShouldSend = true);
			AssertContainsExactElementsInAnyOrder("Both docs are selected", new[] { doc1, doc2 }, messageSendingActionParent.SelectedDocuments);
		});

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CusStorageDocPivotMessageSendingActionParent(Factory.New<QuarantineColsHeader>());
		}
	}
}
