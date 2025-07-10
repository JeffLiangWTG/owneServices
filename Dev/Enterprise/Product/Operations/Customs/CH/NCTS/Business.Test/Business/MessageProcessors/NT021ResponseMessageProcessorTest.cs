using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

abstract class NT021ResponseMessageProcessorTest : BasePassarNctsMSGMessageProcessorTest
{
	protected override string ExpectedFriendlyName => "NT021 - Payload request goods declaration response";

	protected override string ExpectedMovementType => null;

	protected override string MessageSubType => MessageSubTypeCodeList.Codes.PassarNctsPayloadRequestGoodsDeclarationResponse;

	protected override BaseGetMessageInboundMessageProcessor CreateMessageProcessor() => new NT021ResponseMessageProcessor(Logger);

	protected override string GetResponseMessage() => TestingData.GetNT021();

	public void TestMessageProcessor()
	{
		NctsHeader nctsHeader = null;

		AssertProcessMessage(
			correlationIdentifier =>
			{
				nctsHeader = Helper.CreateNctsHeaderAndSentMessage(correlationIdentifier, movementType: TestedMovementType).nctsHeader;
				nctsHeader.CommonMovementHeader.BM_CustomsStatus = "XXX";
				nctsHeader.CommonMovementHeader.BM_EntryDate = new ZDateTime(2021, 2, 3);
				return TestedMovementType == Common.EU.NctsMoveHeaderType.Codes.Departure ? nctsHeader.MovementHeader : nctsHeader;
			},
			correlationIdentifier => TestingData.GetNT021(correlationId: correlationIdentifier),
		(ediMessage) =>
		{
			AssertEquals("BM_CustomsStatus", CHLogicalStatusList.Codes.Accepted, nctsHeader.EffectiveMessageStatus);
			AssertEquals("BM_CustomsStatus not changed", "XXX", nctsHeader.CommonMovementHeader.BM_CustomsStatus);
			AssertEquals("BM_EntryDate not changed", new ZDateTime(2021, 2, 3), nctsHeader.CommonMovementHeader.BM_EntryDate);
			EventsTestHelper.AssertEventAdded(nctsHeader.CommonMovementHeader, Events.MessageStatusChange, expectedReference: CHLogicalStatusList.Codes.Accepted);
		}, expectedMessageStatus: EDIMessageStatusList.Codes.ProcessedOK);
	}
}
