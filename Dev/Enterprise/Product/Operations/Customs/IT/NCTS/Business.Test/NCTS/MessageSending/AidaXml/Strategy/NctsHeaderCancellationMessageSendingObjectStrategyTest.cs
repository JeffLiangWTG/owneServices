using System.Collections.Generic;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class NctsHeaderCancellationMessageSendingObjectStrategyTest : NctsHeaderMessageSendingObjectStrategyAbstractTest<NctsHeaderCancellationMessageSendingObjectStrategy>
{
	protected override string GetMessageType() => "CAN";

	protected override List<(string PhaseStatus, string MessageStatus, string DepartureStatus, bool AllowsSendingExpected)> GetTestCases() =>
	[
		(PhaseStatus: string.Empty, MessageStatus: "SNT", DepartureStatus: string.Empty, AllowsSendingExpected: true),
		(PhaseStatus: null, MessageStatus: "SNT", DepartureStatus: null, AllowsSendingExpected: true),

		(PhaseStatus: "013", MessageStatus: "FAL", DepartureStatus: "AMR", AllowsSendingExpected: true),
		(PhaseStatus: "013", MessageStatus: "FAL", DepartureStatus: "ACK", AllowsSendingExpected: true),
		(PhaseStatus: "013", MessageStatus: "FAL", DepartureStatus: "CAN", AllowsSendingExpected: true),
		(PhaseStatus: "013", MessageStatus: "FAL", DepartureStatus: "ACS", AllowsSendingExpected: true),
		(PhaseStatus: "013", MessageStatus: "FAL", DepartureStatus: string.Empty, AllowsSendingExpected: true),
		(PhaseStatus: "013", MessageStatus: "FAL", DepartureStatus: null, AllowsSendingExpected: true),

		(PhaseStatus: "013", MessageStatus: "ERR", DepartureStatus: "AMR", AllowsSendingExpected: true),
		(PhaseStatus: "013", MessageStatus: "ERR", DepartureStatus: "ACK", AllowsSendingExpected: true),
		(PhaseStatus: "013", MessageStatus: "ERR", DepartureStatus: "CAN", AllowsSendingExpected: true),
		(PhaseStatus: "013", MessageStatus: "ERR", DepartureStatus: string.Empty, AllowsSendingExpected: true),
		(PhaseStatus: "013", MessageStatus: "ERR", DepartureStatus: null, AllowsSendingExpected: true),

		(PhaseStatus: "015", MessageStatus: "FAL", DepartureStatus: "AMR", AllowsSendingExpected: true),
		(PhaseStatus: "015", MessageStatus: "FAL", DepartureStatus: "ACK", AllowsSendingExpected: true),
		(PhaseStatus: "015", MessageStatus: "FAL", DepartureStatus: "CAN", AllowsSendingExpected: true),
		(PhaseStatus: "015", MessageStatus: "FAL", DepartureStatus: string.Empty, AllowsSendingExpected: true),
		(PhaseStatus: "015", MessageStatus: "FAL", DepartureStatus: null, AllowsSendingExpected: true),

		(PhaseStatus: "015", MessageStatus: "ERR", DepartureStatus: "AMR", AllowsSendingExpected: true),
		(PhaseStatus: "015", MessageStatus: "ERR", DepartureStatus: "ACK", AllowsSendingExpected: true),
		(PhaseStatus: "015", MessageStatus: "ERR", DepartureStatus: "CAN", AllowsSendingExpected: true),
		(PhaseStatus: "015", MessageStatus: "ERR", DepartureStatus: string.Empty, AllowsSendingExpected: true),
		(PhaseStatus: "015", MessageStatus: "ERR", DepartureStatus: null, AllowsSendingExpected: true),
	];
}
