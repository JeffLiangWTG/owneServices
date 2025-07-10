using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.Business.Testing;

sealed class NE009ResponseMessageProcessorTest : PassarDecisionGetMessageInboundMessageProcessorTest
{
	protected override string ExpectedFriendlyName => "NE009 - Passar Export Declaration Withdrawal Response";

	protected override string MessageSubType => MessageSubTypeCodeList.Codes.PassarExportWithdrawalResponseRejected;

	public void TestProcessMessage_Accepted() => TestProcessMessage(PassarMessagingConstants.DecisionCodes.Accepted, CHLogicalStatusList.Codes.Accepted, Common.Shared.EntryStatusList.Codes.Cancelled, Events.CustomsEntryStatus, true);

	public void TestProcessMessage_Rejected() => TestProcessMessage(PassarMessagingConstants.DecisionCodes.Rejected, CHLogicalStatusList.Codes.Invalid, null, Events.DeclarationCancellationRejected, false);

	protected override string GetResponseMessage_FindLinkedObject_NotInitiatedByCustoms(string correlationIdentifier) => TestingData.GetNE009(correlationId: correlationIdentifier, decision: PassarMessagingConstants.DecisionCodes.Accepted, initiatedByCustoms: "false");

	protected override string GetResponseMessage_FindLinkedObject_InitiatedByCustoms(string gdrnNumber, string gdrnVersion) => TestingData.GetNE009(gdrnNumber: gdrnNumber, gdrnVersion: gdrnVersion, decision: PassarMessagingConstants.DecisionCodes.Accepted, initiatedByCustoms: "true");

	protected override string GetResponseMessage_ProcessMessage(string correlationIdentifier, string gdrn, string gdrnVersion, ZDateTime issueDateTimeUTC, string decision) => TestingData.GetNE009(correlationIdentifier, gdrn, gdrnVersion, issueDateTimeUTC, decision);

	protected override string GetResponseMessage() => TestingData.GetNE009();

	protected override BaseGetMessageInboundMessageProcessor CreateMessageProcessor() => new NE009ResponseMessageProcessor(Logger);
}
