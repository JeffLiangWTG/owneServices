using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	[TestedType(typeof(MessageSendingObjectCollection<MessageSendingObject>))]
	sealed class MessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<MessageSendingObjectCollection<MessageSendingObject>>
	{
		public void TestAllowNew()
		{
			var collection = new MessageSendingObjectCollection<MessageSendingObject>(Factory);

			CombineAssertions("Collection should not allow adding new element", () =>
			{
				AssertEquals(false, collection.AllowNew);
				AssertExceptionThrown<NotImplementedException>(() => collection.AddNew());
			});
		}

		protected override MessageSendingObjectCollection<MessageSendingObject> GetCollectionToTest() => new MessageSendingObjectCollection<MessageSendingObject>(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			return new MessageSendingObject(bill);
		}
	}
}
