using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BE.Business.CusTempStorage.Testing;

sealed class TemporaryStorageMessageBuilderTest : EU.Business.CusTempStorage.Testing.TemporaryStorageMessageBuilderTest<TemporaryStorageMessageBuilder>
{
	public void TestGetIETS115Message()
	{
		AssertMessageCanBePopulated<CombinedTSDMessageFunction>("IETS115");
	}

	public void TestGetIETS215Message()
	{
		AssertMessageCanBePopulated<DeconsolidationNotificationTSDMessageFunction>("IETS215");
	}

	protected override ZString ApplicationCode => EDIInterchange.ApplicationCodes.BECustoms;

	protected override TemporaryStorageMessageBuilder GetMessageBuilder(EU.Business.CusTempStorage.TemporaryStorageHeader header, TemporaryStorageMessageFunction function) => new TemporaryStorageMessageBuilder(new TemporaryStorageMessageSendingObject((TemporaryStorageHeader)header), function);
}
