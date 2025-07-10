using Enterprise.Customs.BE.Business.CusTempStorage;
using Enterprise.Customs.EU.Business.CusTempStorage.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(TemporaryStorageMessageSendingConfiguration))]
sealed class TemporaryStorageMessageSendingConfigurationTest : TemporaryStorageMessageSendingConfigurationAbstractTest<TemporaryStorageMessageSendingConfiguration>
{
	public override void TestGetNewMessageSendingObject()
	{
		AssertType<TemporaryStorageMessageSendingObject>(configuration.GetNewMessageSendingObject(Factory.New<TemporaryStorageHeader>()));
	}

	public override void TestGetNewMessageSendingObjectParent()
	{
		AssertType<TemporaryStorageMessageSendingObjectParent>(configuration.GetNewMessageSendingObjectParent(Factory.New<TemporaryStorageHeader>()));
	}
}
