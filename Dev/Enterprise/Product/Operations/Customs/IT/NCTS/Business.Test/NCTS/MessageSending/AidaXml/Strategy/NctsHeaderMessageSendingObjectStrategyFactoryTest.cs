using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.NCTS.Business.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class NctsHeaderMessageSendingObjectStrategyFactoryTest : TestCaseWithFactory
{
	public void TestCreateStrategy()
	{
		var nctsHeaderMessageSendingObject = new NctsHeaderMessageSendingObject(nctsHeader);

		CombineAssertions("Strategy creation test for different message types", () =>
		{
			nctsHeaderMessageSendingObject.MessageType = "AMD";
			AssertType<NctsHeaderAmendmentMessageSendingObjectStrategy>("MessageType is 'AMD'", CreateStrategy());

			nctsHeaderMessageSendingObject.MessageType = "CAN";
			AssertType<NctsHeaderCancellationMessageSendingObjectStrategy>("MessageType is 'CAN'", CreateStrategy());

			nctsHeaderMessageSendingObject.MessageType = "NEW";
			AssertType<NctsHeaderNewDeclarationMessageSendingObjectStrategy>("MessageType is 'NEW'", CreateStrategy());

			nctsHeaderMessageSendingObject.MessageType = string.Empty;
			AssertExceptionThrown<InvalidOperationException>("MessageType is empty", () => CreateStrategy());

			nctsHeaderMessageSendingObject = null;
			AssertExceptionThrown<ArgumentNullException>("Object is null", () => CreateStrategy());
		});

		return;

		INctsHeaderMessageSendingObjectStrategy CreateStrategy() => NctsHeaderMessageSendingObjectStrategyFactory.CreateStrategy(nctsHeaderMessageSendingObject);
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.NewDepartureNctsHeader();
	}

	NctsHeader nctsHeader;
}
