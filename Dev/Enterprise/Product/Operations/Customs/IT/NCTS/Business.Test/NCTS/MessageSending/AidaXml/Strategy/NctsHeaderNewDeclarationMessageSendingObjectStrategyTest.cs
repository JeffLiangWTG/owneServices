using System.Collections.Generic;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class NctsHeaderNewDeclarationMessageSendingObjectStrategyTest : NctsHeaderMessageSendingObjectStrategyAbstractTest<NctsHeaderNewDeclarationMessageSendingObjectStrategy>
{
	protected override string GetMessageType() => "NEW";

	protected override List<(string PhaseStatus, string MessageStatus, string DepartureStatus, bool AllowsSendingExpected)> GetTestCases() =>
	[
		(PhaseStatus: string.Empty, MessageStatus: "SNT", DepartureStatus: string.Empty, AllowsSendingExpected: false),
		(PhaseStatus: null, MessageStatus: "SNT", DepartureStatus: null, AllowsSendingExpected: false),

		(PhaseStatus: "013", MessageStatus: "FAL", DepartureStatus: "AMR", AllowsSendingExpected: false),
		(PhaseStatus: "013", MessageStatus: "FAL", DepartureStatus: "ACK", AllowsSendingExpected: false),
		(PhaseStatus: "013", MessageStatus: "FAL", DepartureStatus: "CAN", AllowsSendingExpected: false),
		(PhaseStatus: "013", MessageStatus: "FAL", DepartureStatus: string.Empty, AllowsSendingExpected: false),
		(PhaseStatus: "013", MessageStatus: "FAL", DepartureStatus: null, AllowsSendingExpected: false),

		(PhaseStatus: "013", MessageStatus: "ERR", DepartureStatus: "AMR", AllowsSendingExpected: false),
		(PhaseStatus: "013", MessageStatus: "ERR", DepartureStatus: "ACK", AllowsSendingExpected: false),
		(PhaseStatus: "013", MessageStatus: "ERR", DepartureStatus: "CAN", AllowsSendingExpected: false),
		(PhaseStatus: "013", MessageStatus: "ERR", DepartureStatus: string.Empty, AllowsSendingExpected: false),
		(PhaseStatus: "013", MessageStatus: "ERR", DepartureStatus: null, AllowsSendingExpected: false),

		(PhaseStatus: "015", MessageStatus: "FAL", DepartureStatus: "AMR", AllowsSendingExpected: true),
		(PhaseStatus: "015", MessageStatus: "FAL", DepartureStatus: "ACK", AllowsSendingExpected: true),
		(PhaseStatus: "015", MessageStatus: "FAL", DepartureStatus: "CAN", AllowsSendingExpected: true),
		(PhaseStatus: "015", MessageStatus: "FAL", DepartureStatus: "MRN", AllowsSendingExpected: true),
		(PhaseStatus: "015", MessageStatus: "FAL", DepartureStatus: "REL", AllowsSendingExpected: true),
		(PhaseStatus: "015", MessageStatus: "FAL", DepartureStatus: string.Empty, AllowsSendingExpected: true),
		(PhaseStatus: "015", MessageStatus: "FAL", DepartureStatus: null, AllowsSendingExpected: true),

		(PhaseStatus: "015", MessageStatus: "ERR", DepartureStatus: "AMR", AllowsSendingExpected: true),
		(PhaseStatus: "015", MessageStatus: "ERR", DepartureStatus: "ACK", AllowsSendingExpected: true),
		(PhaseStatus: "015", MessageStatus: "ERR", DepartureStatus: "CAN", AllowsSendingExpected: true),
		(PhaseStatus: "015", MessageStatus: "ERR", DepartureStatus: string.Empty, AllowsSendingExpected: true),
		(PhaseStatus: "015", MessageStatus: "ERR", DepartureStatus: null, AllowsSendingExpected: true),

		(PhaseStatus: "015", MessageStatus: "SNT", DepartureStatus: "ACK", AllowsSendingExpected: false),
		(PhaseStatus: "015", MessageStatus: "ACK", DepartureStatus: "MRN", AllowsSendingExpected: false),
		(PhaseStatus: "015", MessageStatus: "INV", DepartureStatus: "REL", AllowsSendingExpected: false),
		(PhaseStatus: "015", MessageStatus: "ACC", DepartureStatus: string.Empty, AllowsSendingExpected: false),
	];
}
