using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	[TestedType(typeof(RF415MessageSendingObjectCollection))]
	sealed class RF415MessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<RF415MessageSendingObjectCollection>
	{
		public void TestNotAllowNew()
		{
			var collection = new RF415MessageSendingObjectCollection(Factory);

			CombineAssertions("Collection should not allow adding new element", () =>
			{
				AssertEquals(false, collection.AllowNew);
				AssertExceptionThrown<NotImplementedException>(() => collection.AddNew());
			});
		}

		protected override RF415MessageSendingObjectCollection GetCollectionToTest()
		{
			return new RF415MessageSendingObjectCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			return new RF415MessageSendingObject(bill);
		}
	}
}
