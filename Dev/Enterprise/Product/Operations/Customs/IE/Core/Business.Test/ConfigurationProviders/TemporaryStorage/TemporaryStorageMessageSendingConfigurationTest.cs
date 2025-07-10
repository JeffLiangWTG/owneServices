using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageMessageSendingConfiguration))]
	sealed class TemporaryStorageMessageSendingConfigurationTest : EU.Business.CusTempStorage.Testing.TemporaryStorageMessageSendingConfigurationAbstractTest<TemporaryStorageMessageSendingConfiguration>
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
}
