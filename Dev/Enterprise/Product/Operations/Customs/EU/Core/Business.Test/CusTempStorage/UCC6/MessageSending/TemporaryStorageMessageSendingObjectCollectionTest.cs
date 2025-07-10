using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageMessageSendingObjectCollection<TemporaryStorageMessageSendingObject, TemporaryStorageHeader>))]
	public class TemporaryStorageMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TemporaryStorageMessageSendingObjectCollection<TemporaryStorageMessageSendingObject, TemporaryStorageHeader>>
	{
		protected override TemporaryStorageMessageSendingObjectCollection<TemporaryStorageMessageSendingObject, TemporaryStorageHeader> GetCollectionToTest()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			return new TemporaryStorageMessageSendingObjectCollection<TemporaryStorageMessageSendingObject, TemporaryStorageHeader>(header);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var collection = GetCollectionToTest();
			return collection.AddNew();
		}

		public void TestAllowNew()
		{
			var collection = GetCollectionToTest();
			Assert(!collection.AllowNew);
		}
	}
}
