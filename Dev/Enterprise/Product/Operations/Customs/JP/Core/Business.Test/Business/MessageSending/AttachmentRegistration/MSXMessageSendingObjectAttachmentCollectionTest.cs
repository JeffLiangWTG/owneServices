using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(MSXMessageSendingObjectAttachmentCollection))]
	sealed class MSXMessageSendingObjectAttachmentCollectionTest : NonPersistentBusinessObjectCollectionTestCase<MSXMessageSendingObjectAttachmentCollection>
	{
		protected override MSXMessageSendingObjectAttachmentCollection GetCollectionToTest()
		{
			var header = Factory.New<CusEntryHeader>();
			var parent = new MSXMessageSendingObject(header);
			return parent.Attachments;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var header = Factory.New<CusEntryHeader>();
			var parent = new MSXMessageSendingObject(header);
			return new MSXMessageSendingObjectAttachment(parent);
		}

		public void TestMaxCountValidation()
		{
			var header = Factory.New<CusEntryHeader>();
			var parent = new MSXMessageSendingObject(header);
			var attachments = parent.Attachments;

			for (var i = 0; i < 10; i++)
			{
				attachments.AddNew();
			}

			var expectedErrorMessage = "You are not allowed to attach more than 10 documents to one single message";

			AssertNoRowError(attachments.Last(), expectedErrorMessage);

			attachments.AddNew();
			AssertHasRowError(attachments.Last(), expectedErrorMessage);
		}

		public void TestAddNewMaxCount()
		{
			const int maxRowCount = 10;
			var testCollection = GetCollectionToTest();
			AssertEquals(maxRowCount, testCollection.MaxCount);

			for (var i = 0; i < maxRowCount - 1; i++)
			{
				testCollection.AddNew();
			}
			Assert($"Currently, collection has {testCollection.Count} elements", testCollection.AllowNew);
			testCollection.AddNew();
			Assert($"Currently, collection has {testCollection.Count} elements", !testCollection.AllowNew);
		}
	}
}
