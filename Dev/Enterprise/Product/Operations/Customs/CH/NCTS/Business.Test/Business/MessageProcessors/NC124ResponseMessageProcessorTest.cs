using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NC124ResponseMessageProcessor))]
sealed class NC124ResponseMessageProcessorTest : BasePassarNctsMSGMessageProcessorTest
{
	protected override string ExpectedFriendlyName => "NC124 - Activation Response Message Processor";

	protected override string MessageSubType => MessageSubTypeCodeList.Codes.PassarActivationResponse;

	protected override string TestedMovementType => null;

	protected override BaseGetMessageInboundMessageProcessor CreateMessageProcessor() => new NC124ResponseMessageProcessor(Logger);

	protected override string GetResponseMessage() => TestingData.GetNC124();

	public void TestProcessMessage()
	{
		NctsHeader nctsHeader = null;

		AssertProcessMessage(
			(correlationIdentifier) =>
			{
				nctsHeader = Helper.CreateNctsHeaderAndSentMessage(applicationReference: correlationIdentifier, movementType: TestedMovementType).nctsHeader;
				return nctsHeader.MovementHeader;
			},
			(correlationIdentifier) => TestingData.GetNC124(correlationId: correlationIdentifier, decision: PassarMessagingConstants.DecisionCodes.Accepted),
			(ediMessage) =>
			{
				AssertEquals("EffectiveMessageStatus", CHLogicalStatusList.Codes.Accepted, nctsHeader.EffectiveMessageStatus);
				AssertEquals("BM_Phase", DeparturePhaseList.Codes.Activation, nctsHeader.MovementHeader.BM_Phase);
				AssertEquals("BM_EntryDate", new ZDateTime(2004, 02, 14, 20, 44, 14), nctsHeader.MovementHeader.BM_EntryDate);
				EventsTestHelper.AssertEventAdded(nctsHeader.MovementHeader, Events.CustomsEntryStatus);
			}, expectedMessageStatus: EDIMessageStatusList.Codes.ProcessedOK);

		AssertProcessMessage(
			(correlationIdentifier) =>
			{
				nctsHeader = Helper.CreateNctsHeaderAndSentMessage(applicationReference: correlationIdentifier, movementType: TestedMovementType).nctsHeader;
				return nctsHeader.MovementHeader;
			},
			(correlationIdentifier) => TestingData.GetNC124(correlationId: correlationIdentifier, decision: PassarMessagingConstants.DecisionCodes.Rejected),
			(ediMessage) =>
			{
				AssertEquals("EffectiveMessageStatus", CHLogicalStatusList.Codes.Invalid, nctsHeader.EffectiveMessageStatus);
				AssertEquals("BM_Phase", DeparturePhaseList.Codes.Declaration, nctsHeader.MovementHeader.BM_Phase);
				EventsTestHelper.AssertEventAdded(nctsHeader, Events.DeclarationActivationRejected);
			}, expectedMessageStatus: EDIMessageStatusList.Codes.ProcessedOK);

		AssertProcessMessage(
			(correlationIdentifier) =>
			{
				nctsHeader = Helper.CreateNctsHeaderAndSentMessage(applicationReference: correlationIdentifier, movementType: TestedMovementType).nctsHeader;
				nctsHeader.MovementHeader.BM_Phase = DeparturePhaseList.Codes.Activation;
				return nctsHeader.MovementHeader;
			},
			(correlationIdentifier) => TestingData.GetNC124(correlationId: correlationIdentifier, decision: PassarMessagingConstants.DecisionCodes.Received),
			(ediMessage) =>
			{
				AssertEquals("EffectiveMessageStatus", CHLogicalStatusList.Codes.Invalid, nctsHeader.EffectiveMessageStatus);
				AssertEquals("BM_Phase", DeparturePhaseList.Codes.Activation, nctsHeader.MovementHeader.BM_Phase);
			}, expectedMessageStatus: EDIMessageStatusList.Codes.ProcessedOK);
	}
}
