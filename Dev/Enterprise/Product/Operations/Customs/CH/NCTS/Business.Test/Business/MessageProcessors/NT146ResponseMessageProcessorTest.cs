using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NT146ResponseMessageProcessor))]
sealed class NT146ResponseMessageProcessorTest : BasePassarNctsMSGMessageProcessorTest
{
	protected override string ExpectedFriendlyName => "NT146 - Response Information about non-arrived movement";

	protected override string MessageSubType => MessageSubTypeCodeList.Codes.PassarNonArrivedTransitMovementInformation;

		protected override string TestedMovementType => Common.EU.NctsMoveHeaderType.Codes.Departure;

	protected override BaseGetMessageInboundMessageProcessor CreateMessageProcessor() => new NT146ResponseMessageProcessor(Logger);

	protected override string GetResponseMessage() => TestingData.GetNT146();

	public void TestProcessMessageAcceptance()
	{
		AssertMessage(DecisionStateList.Codes.Accepted, CHLogicalStatusList.Codes.Accepted);
	}

	public void TestProcessMessageRejection()
	{
		AssertMessage(DecisionStateList.Codes.Rejected, CHLogicalStatusList.Codes.Invalid);
	}

	public void TestProcessMessageReceived()
	{
		AssertMessage(DecisionStateList.Codes.Received, CHLogicalStatusList.Codes.Invalid);
	}

	void AssertMessage(string decision, string expectedMessageStatus)
	{
		NctsHeader nctsHeader = null;

		AssertProcessMessage(
			(correlationIdentifier) =>
			{
				nctsHeader = Helper.CreateNctsHeaderAndSentMessage(correlationIdentifier).nctsHeader;
				nctsHeader.MovementHeader.BM_CustomsStatus = "YYY";
				nctsHeader.MovementHeader.BM_EntryDate = new ZDateTime(2023, 12, 19);
				return nctsHeader.MovementHeader;
			},
			(correlationIdentifier) => TestingData.GetNT146(correlationId: correlationIdentifier, decision: decision),
			(ediMessage) =>
			{
				AssertEquals("BM_MessageStatus", expectedMessageStatus, nctsHeader.MovementHeader.BM_MessageStatus);
				AssertEquals("BM_CustomsStatus (unchanged)", "YYY", nctsHeader.MovementHeader.BM_CustomsStatus);
				AssertEquals("BM_EntryDate (unchanged)", new ZDateTime(2023, 12, 19), nctsHeader.MovementHeader.BM_EntryDate);
				Helper.AssertEvent("Event", nctsHeader.MovementHeader.Logs, Events.MessageStatusChange, expectedMessageStatus);
			}, expectedMessageStatus: EDIMessageStatusList.Codes.ProcessedOK);
	}
}
