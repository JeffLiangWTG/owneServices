using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

public class NC909ResponseMessageProcessorTest : CH.Business.Testing.NC909ResponseMessageProcessorTest
{
	public void TestProcessMessageDeclaration_Departure() => TestProcessMessage(NctsMovementType.Codes.Departure, NCTS5DeparturePhaseList.Codes.Declaration, Events.DeclarationRejected);

	public void TestProcessMessageDeclaration_Arrival() => TestProcessMessage(NctsMovementType.Codes.Arrival, NCTS5DeparturePhaseList.Codes.Declaration, Events.DeclarationRejected);

	public void TestProcessMessageAmendment_Departure() => TestProcessMessage(NctsMovementType.Codes.Departure, NCTS5DeparturePhaseList.Codes.Amendment, Events.DeclarationAmendmentRejected);

	public void TestProcessMessageAmendment_Arrival() => TestProcessMessage(NctsMovementType.Codes.Arrival, NCTS5DeparturePhaseList.Codes.Amendment, Events.DeclarationAmendmentRejected);

	public void TestProcessActivationRejection_Departure() => TestProcessMessage(NctsMovementType.Codes.Departure, DeparturePhaseList.Codes.Activation, Events.DeclarationActivationRejected);

	public void TestProcessActivationRejection_Arrival() => TestProcessMessage(NctsMovementType.Codes.Arrival, DeparturePhaseList.Codes.Activation, Events.DeclarationActivationRejected);

	public void TestDeclarationCancellationRejected_Departure() => TestProcessMessage(NctsMovementType.Codes.Departure, DeparturePhaseList.Codes.Cancellation, Events.DeclarationCancellationRejected);

	public void TestDeclarationCancellationRejected_Arrival() => TestProcessMessage(NctsMovementType.Codes.Arrival, DeparturePhaseList.Codes.Cancellation, Events.DeclarationCancellationRejected);

	public void TestInformationAboutNonArrivedMovement_Departure() => TestProcessMessage(NctsMovementType.Codes.Departure, DeparturePhaseList.Codes.NonArrivedMovement, unexpectedEvent: Events.DeclarationRejected);

	void TestProcessMessage(string movementType, string initialMovementPhase, Event expectedEvent = null, string expectedEventReference = null, Event unexpectedEvent = null)
	{
		NctsHeader nctsHeader = null;

		AssertProcessMessage(
			(correlationIdentifier) =>
			{
				nctsHeader = Helper.CreateNctsHeaderAndSentMessage(applicationReference: correlationIdentifier, movementType: movementType).nctsHeader;
				nctsHeader.CommonMovementHeader.BM_Phase = initialMovementPhase;
				nctsHeader.CommonMovementHeader.BM_CustomsStatus = "C00";
				nctsHeader.CommonMovementHeader.BM_EntryDate = new ZDateTime(2000, 1, 8);
				return movementType == NctsMovementType.Codes.Departure ? nctsHeader.MovementHeader : nctsHeader;
			},
			(correlationIdentifier) => TestingData.GetNC909(correlationIdentifier),
			(ediMessage) =>
			{
				AssertEquals("MessageStatus", CHLogicalStatusList.Codes.Invalid, nctsHeader.EffectiveMessageStatus);
				EventsTestHelper.AssertEventAdded(nctsHeader.CommonMovementHeader, Events.MessageStatusChange, CHLogicalStatusList.Codes.Invalid);
				if (expectedEvent != null)
				{
					EventsTestHelper.AssertEventAdded(nctsHeader, expectedEvent, expectedEventReference);
				}
				if (unexpectedEvent != null)
				{
					EventsTestHelper.AssertEventNotAdded(nctsHeader, unexpectedEvent);
				}
				AssertEquals("BM_CustomsStatus not changed", "C00", nctsHeader.CommonMovementHeader.BM_CustomsStatus);
				AssertEquals("BM_EntryDate not changed", new ZDateTime(2000, 1, 8), nctsHeader.CommonMovementHeader.BM_EntryDate);
			},
			expectedMessageStatus: EDIMessageStatusList.Codes.ProcessedOK);
	}

	protected override BaseGetMessageInboundMessageProcessor CreateMessageProcessor() => new NC909ResponseMessageProcessor(Logger);

	protected NctsMessageProcessorTestHelper Helper => helper ??= new NctsMessageProcessorTestHelper(Factory);
	NctsMessageProcessorTestHelper helper;
}
