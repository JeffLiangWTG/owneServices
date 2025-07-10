using System.Collections.Generic;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

sealed class TemporaryStorageAmendmentMessageSendingObjectStrategyTest : TemporaryStorageMessageSendingObjectStrategyAbstractTest<TemporaryStorageAmendmentMessageSendingObjectStrategy>
{
	protected override string GetMessageType() => "AMD";

	protected override List<(string CustomsStatus, string MessageStatus, bool AllowsSendingExpected)> GetTestCases() =>
	[
		(CustomsStatus: "AMG", MessageStatus: "", AllowsSendingExpected: true),
		(CustomsStatus: "AMG", MessageStatus: "SNT", AllowsSendingExpected: false),
		(CustomsStatus: "AMG", MessageStatus: "FAL", AllowsSendingExpected: true),
		(CustomsStatus: "AMG", MessageStatus: "REJ", AllowsSendingExpected: true),
		(CustomsStatus: "AMG", MessageStatus: "ACS", AllowsSendingExpected: false),
	];
}
