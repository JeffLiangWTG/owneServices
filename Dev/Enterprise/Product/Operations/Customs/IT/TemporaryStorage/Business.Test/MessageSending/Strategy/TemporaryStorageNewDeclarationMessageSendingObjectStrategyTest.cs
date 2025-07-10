using System.Collections.Generic;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

sealed class TemporaryStorageNewDeclarationMessageSendingObjectStrategyTest : TemporaryStorageMessageSendingObjectStrategyAbstractTest<TemporaryStorageNewDeclarationMessageSendingObjectStrategy>
{
	protected override string GetMessageType() => "NEW";

	protected override List<(string CustomsStatus, string MessageStatus, bool AllowsSendingExpected)> GetTestCases() =>
	[
		(CustomsStatus: null, MessageStatus: "SNT", AllowsSendingExpected: false),

		(CustomsStatus: string.Empty, MessageStatus: string.Empty, AllowsSendingExpected: true),
		(CustomsStatus: string.Empty, MessageStatus: "SNT", AllowsSendingExpected: false),
		(CustomsStatus: string.Empty, MessageStatus: "FAL", AllowsSendingExpected: true),
		(CustomsStatus: string.Empty, MessageStatus: "REJ", AllowsSendingExpected: true),
		(CustomsStatus: string.Empty, MessageStatus: "ACK", AllowsSendingExpected: false),

		(CustomsStatus: "TSA", MessageStatus: "SNT", AllowsSendingExpected: false),
		(CustomsStatus: "TSA", MessageStatus: "FAL", AllowsSendingExpected: false),
		(CustomsStatus: "TSA", MessageStatus: "REJ", AllowsSendingExpected: false),
		(CustomsStatus: "TSA", MessageStatus: "ACK", AllowsSendingExpected: false),

		(CustomsStatus: "TPA", MessageStatus: "SNT", AllowsSendingExpected: false),
		(CustomsStatus: "TPA", MessageStatus: "FAL", AllowsSendingExpected: true),
		(CustomsStatus: "TPA", MessageStatus: "REJ", AllowsSendingExpected: true),
		(CustomsStatus: "TPA", MessageStatus: "ACK", AllowsSendingExpected: true),
	];
}
