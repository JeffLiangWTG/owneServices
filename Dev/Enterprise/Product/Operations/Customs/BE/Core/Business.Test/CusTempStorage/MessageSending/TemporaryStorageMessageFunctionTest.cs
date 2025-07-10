using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.BE.Business.CusTempStorage.Testing;

sealed class TemporaryStorageMessageFunctionTest : TestCaseWithFactory
{
	public void TestCombinedTSDMessageFunction()
	{
		var combinedTSDMessageFunction = new CombinedTSDMessageFunction();

		CombineAssertions("Check CombinedTSDMessageFunction", () =>
		{
			AssertEquals(PNTSEntryTypeList.Codes.CombinedTemporaryStorage, combinedTSDMessageFunction.MessageType);
			AssertEquals(PNTSMessageStatusList.Codes.Sent, combinedTSDMessageFunction.SentMessageStatus);
			AssertEquals(ZString.Empty, combinedTSDMessageFunction.SentCustomsStatus);
		});
	}

	public void TestDeconsolidationNotificationTSDMessageFunction()
	{
		var deconsolidationNotificationTSDMessageFunction = new DeconsolidationNotificationTSDMessageFunction();

		CombineAssertions("Check DeconsolidationNotificationTSDMessageFunction", () =>
		{
			AssertEquals(PNTSEntryTypeList.Codes.DeconsolidationNotification, deconsolidationNotificationTSDMessageFunction.MessageType);
			AssertEquals(PNTSMessageStatusList.Codes.Sent, deconsolidationNotificationTSDMessageFunction.SentMessageStatus);
			AssertEquals(ZString.Empty, deconsolidationNotificationTSDMessageFunction.SentCustomsStatus);
		});
	}
}
