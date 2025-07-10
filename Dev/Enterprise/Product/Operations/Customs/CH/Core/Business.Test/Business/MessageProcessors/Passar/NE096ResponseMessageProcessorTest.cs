using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(NE096ResponseMessageProcessor))]
sealed class NE096ResponseMessageProcessorTest : PassarDecisionGetMessageInboundMessageProcessorTest
{
	protected override string ExpectedFriendlyName => "NE096 - Passar Export Declaration Rectification";

protected override string MessageSubType => MessageSubTypeCodeList.Codes.PassarExportDecisionRectification;

public void TestProcessMessage_Accepted() => TestProcessMessage(PassarMessagingConstants.DecisionCodes.Accepted, CHLogicalStatusList.Codes.Accepted, null, null, true);

public void TestProcessMessage_Rejected() => TestProcessMessage(PassarMessagingConstants.DecisionCodes.Rejected, CHLogicalStatusList.Codes.Invalid, null, Events.DeclarationAmendmentRejected, false);

protected override string GetResponseMessage_FindLinkedObject_NotInitiatedByCustoms(string correlationIdentifier) => TestingData.GetNE096(correlationId: correlationIdentifier, decision: PassarMessagingConstants.DecisionCodes.Accepted, initiatedByCustoms: "false");

protected override string GetResponseMessage_FindLinkedObject_InitiatedByCustoms(string gdrnNumber, string gdrnVersion) => TestingData.GetNE096(gdrnNumber: gdrnNumber, gdrnVersion: gdrnVersion, decision: PassarMessagingConstants.DecisionCodes.Accepted, initiatedByCustoms: "true");

protected override string GetResponseMessage_ProcessMessage(string correlationIdentifier, string gdrn, string gdrnVersion, ZDateTime issueDateTimeUTC, string decision) => TestingData.GetNE096(correlationIdentifier, gdrn, gdrnVersion, issueDateTimeUTC, decision);

protected override string GetResponseMessage() => TestingData.GetNE096();

protected override BaseGetMessageInboundMessageProcessor CreateMessageProcessor() => new NE096ResponseMessageProcessor(Logger);
}