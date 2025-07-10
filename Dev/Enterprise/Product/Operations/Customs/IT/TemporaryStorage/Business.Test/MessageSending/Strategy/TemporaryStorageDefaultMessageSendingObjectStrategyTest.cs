using System.Collections.Generic;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

sealed class TemporaryStorageDefaultMessageSendingObjectStrategyTest : TemporaryStorageMessageSendingObjectStrategyAbstractTest<TemporaryStorageDefaultMessageSendingObjectStrategy>
{
	protected override string GetMessageType() => "CAN";

	protected override List<(string CustomsStatus, string MessageStatus, bool AllowsSendingExpected)> GetTestCases() =>
	[
		(CustomsStatus: "CAN", MessageStatus: "", AllowsSendingExpected: true),
		(CustomsStatus: "CAN", MessageStatus: "SNT", AllowsSendingExpected: true),
		(CustomsStatus: "CAN", MessageStatus: "FAL", AllowsSendingExpected: true),
		(CustomsStatus: "CAN", MessageStatus: "REJ", AllowsSendingExpected: true),
		(CustomsStatus: "CAN", MessageStatus: "ACS", AllowsSendingExpected: true),
	];
}
