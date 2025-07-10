using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

sealed class TemporaryStorageMessageSendingObjectStrategyFactoryTest : TestCaseWithFactory
{
	public void TestCreateStrategy()
	{
		var tempStorageMessageSendingObject = new TemporaryStorageMessageSendingObject(tempHeader);

		CombineAssertions("Strategy creation test for different message types", () =>
		{
			tempStorageMessageSendingObject.MessageType = "AMD";
			AssertType<TemporaryStorageAmendmentMessageSendingObjectStrategy>("MessageType is 'AMD'", CreateStrategy());

			tempStorageMessageSendingObject.MessageType = "NEW";
			AssertType<TemporaryStorageNewDeclarationMessageSendingObjectStrategy>("MessageType is 'NEW'", CreateStrategy());

			tempStorageMessageSendingObject.MessageType = "CAN";
			AssertType<TemporaryStorageDefaultMessageSendingObjectStrategy>("MessageType is 'CAN'", CreateStrategy());

			tempStorageMessageSendingObject = null;
			AssertExceptionThrown<ArgumentNullException>("Object is null", () => CreateStrategy());
		});

		return;

		ITemporaryStorageMessageSendingObjectStrategy CreateStrategy() => TemporaryStorageMessageSendingObjectStrategyFactory.CreateStrategy(tempStorageMessageSendingObject);
	}

	protected override void SetUp()
	{
		base.SetUp();
		tempHeader = Factory.New<TemporaryStorageHeader>();
	}

	TemporaryStorageHeader tempHeader;
}
