using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.CusTempStorage.Testing;

[TestedType(typeof(TemporaryStorageMessageSendingObjectCollection))]
sealed class TemporaryStorageMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TemporaryStorageMessageSendingObjectCollection>
{
	protected override TemporaryStorageMessageSendingObjectCollection GetCollectionToTest() => new TemporaryStorageMessageSendingObjectCollection(Factory.New<TemporaryStorageHeader>());

	protected override BusinessObject GetNewElementToAddToTheCollection() => new TemporaryStorageMessageSendingObject(Factory.New<TemporaryStorageHeader>());

	public void TestAllowNew() => Assert(!GetCollectionToTest().AllowNew);

	public void TestAllowRemove() => Assert(!GetCollectionToTest().AllowRemove);
}
