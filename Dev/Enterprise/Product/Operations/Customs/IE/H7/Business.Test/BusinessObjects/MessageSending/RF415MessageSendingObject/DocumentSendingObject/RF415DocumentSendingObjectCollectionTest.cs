using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	[TestedType(typeof(RF415DocumentSendingObjectCollection))]
	sealed class RF415DocumentSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<RF415DocumentSendingObjectCollection>
	{
		public void TestNotAllowNewWhenMessageSendingObjectIsNotTickedForShouldSend()
		{
			var bill = Factory.New<AsycudaBill>();
			var messageSendingObject = new RF415MessageSendingObject(bill);
			var collection = new RF415DocumentSendingObjectCollection(messageSendingObject);

			Assert("Pre-condition:	RF415MessageSendingObject is defuault to Should Send.", messageSendingObject.ShouldSend);
			Assert(collection.AllowNew);

			messageSendingObject.ShouldSend = ZBool.False;
			Assert(!collection.AllowNew);
		}

		public void TestNotAllowToAddRF415DocumentSendingObjectFromDifferentMessageSendingObject()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			var messageSendingObjectForBill1 = new RF415MessageSendingObject(bill1);
			var messageSendingObjectForBill2 = new RF415MessageSendingObject(bill2);

			var collection = new RF415DocumentSendingObjectCollection(messageSendingObjectForBill1);
			AssertEquals("Pre-condition: No documentSendingObject", 0, collection.Count);

			var documentSendingObjectForBill2 = new RF415DocumentSendingObject(messageSendingObjectForBill2);
			collection.Add(documentSendingObjectForBill2);
			AssertEquals("Should not allow to add document sending object from another message sending object", 0, collection.Count);

			var documentSendingObjectForBill1 = new RF415DocumentSendingObject(messageSendingObjectForBill1);
			collection.Add(documentSendingObjectForBill1);
			AssertEquals("Should allow to add document sending object from the same message sending object", 1, collection.Count);
		}

		protected override RF415DocumentSendingObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			var messageSendingObject = new RF415MessageSendingObject(bill);

			var test = new RF415DocumentSendingObjectCollection(messageSendingObject);

			return new RF415DocumentSendingObjectCollection(messageSendingObject);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new RF415DocumentSendingObject(Collection.MessageSendingObject);
		}
	}
}
