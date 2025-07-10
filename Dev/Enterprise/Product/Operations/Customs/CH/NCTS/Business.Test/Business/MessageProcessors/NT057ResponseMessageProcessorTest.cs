using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NT057ResponseMessageProcessor))]
sealed class NT057ResponseMessageProcessorTest : BasePassarNctsMSGMessageProcessorTest
{
	protected override string ExpectedFriendlyName => "NT057 - Unloading Remarks Response Processor";

	protected override string MessageSubType => MessageSubTypeCodeList.Codes.PassarUnloadingRemarksResponse;

	protected override string TestedMovementType => NctsMovementType.Codes.Arrival;

	protected override BaseGetMessageInboundMessageProcessor CreateMessageProcessor() => new NT057ResponseMessageProcessor(Logger);

	protected override string GetResponseMessage() => TestingData.GetNT057();

	public void TestProcessMessageAccepted()
	{
		NctsHeader nctsHeader = null;

		AssertProcessMessage(
			(correlationIdentifier) => nctsHeader = Helper.CreateNctsHeaderAndSentMessage(correlationIdentifier, movementType: TestedMovementType, prepareNctsHeader: PrepareNctsHeader).nctsHeader,
			(correlationIdentifier) => TestingData.GetNT057(correlationId: correlationIdentifier, decision: ACCEPTED),
			(ediMessage) =>
			{
				AssertEquals("EffectiveMessageStatus", CHLogicalStatusList.Codes.Accepted, nctsHeader.EffectiveMessageStatus);
				Helper.AssertEvent("", nctsHeader.Logs, Events.MessageStatusChange, expectedReference: CHLogicalStatusList.Codes.Accepted);
			},
			expectedMessageStatus: EDIMessage.Status.ProcessedOK);

		void PrepareNctsHeader(NctsHeader header)
		{
			header.ArrivalMovementHeader.BM_CustomsStatus = "SSS";
		}
	}

	public void TestProcessMessageRejected()
	{
		NctsHeader nctsHeader = null;

		AssertProcessMessage(
			(correlationIdentifier) => nctsHeader = Helper.CreateNctsHeaderAndSentMessage(correlationIdentifier, movementType: TestedMovementType).nctsHeader,
			(correlationIdentifier) => TestingData.GetNT057(correlationId: correlationIdentifier, decision: REJECTED),
			(ediMessage) =>
			{
				AssertEquals("EffectiveMessageStatus", CHLogicalStatusList.Codes.Invalid, nctsHeader.EffectiveMessageStatus);
				Helper.AssertEvent("", nctsHeader.Logs, Events.DeclarationRejected);
			},
			expectedMessageStatus: EDIMessage.Status.ProcessedOK);
	}

	public void TestMasterCustomsStatusUpdated()
	{
		var masterNctsHeader = CreateMasterMovement();
		AddChildMovement(masterNctsHeader, null, "MRN1", NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks, CHLogicalStatusList.Codes.Accepted);
		AddChildMovement(masterNctsHeader, null, "MRN2", NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease, ZString.Empty);

		AssertProcessMessage(
			(correlationIdentifier) => Helper.CreateNctsHeaderAndSentMessage(correlationIdentifier, movementType: TestedMovementType, prepareNctsHeader: PrepareNctsHeader).nctsHeader,
			(correlationIdentifier) => TestingData.GetNT057(correlationId: correlationIdentifier, decision: ACCEPTED),
			(ediMessage) =>
			{
				AssertEquals("master.BM_CustomsStatus", NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks, masterNctsHeader.ArrivalMovementHeader.BM_CustomsStatus);
				AssertEquals("master.BM_Phase", NCTS5ArrivalPhaseList.Codes.UnloadingRemarks, masterNctsHeader.ArrivalMovementHeader.BM_Phase);
			},
			expectedMessageStatus: EDIMessage.Status.ProcessedOK, assertEDIMessageLinked: false);

		void PrepareNctsHeader(NctsHeader childNctsHeader)
		{
			AddChildMovement(masterNctsHeader, childNctsHeader, "MRN3", NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks, CHLogicalStatusList.Codes.Accepted);
		}
	}

	public void TestMasterCustomsStatusNotUpdated_OtherCustomsStatus()
	{
		AssertMasterCustomsStatusNotUpdated(NCTS5ArrivalCustomsStatusList.Codes.ClosedPartialRelease, CHLogicalStatusList.Codes.Accepted, ACCEPTED);
	}

	public void TestMasterCustomsStatusNotUpdated_OtherMessageStatus()
	{
		AssertMasterCustomsStatusNotUpdated(NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks, CHLogicalStatusList.Codes.Invalid, ACCEPTED);
	}

	public void TestMasterCustomsStatusNotUpdated_Rejected()
	{
		AssertMasterCustomsStatusNotUpdated(NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks, CHLogicalStatusList.Codes.Accepted, REJECTED);
	}

	public void AssertMasterCustomsStatusNotUpdated(string otherChildCustomsStatus, string otherChildMessageStatus, string decision)
	{
		var masterNctsHeader = CreateMasterMovement();
		AddChildMovement(masterNctsHeader, null, "MRN1", otherChildCustomsStatus, otherChildMessageStatus);

		AssertProcessMessage(
			(correlationIdentifier) => Helper.CreateNctsHeaderAndSentMessage(correlationIdentifier, movementType: TestedMovementType, prepareNctsHeader: PrepareNctsHeader).nctsHeader,
			(correlationIdentifier) => TestingData.GetNT057(correlationId: correlationIdentifier, decision: decision),
			(ediMessage) =>
			{
				AssertEquals("master.BM_CustomsStatus", ZString.Empty, masterNctsHeader.ArrivalMovementHeader.BM_CustomsStatus);
				AssertEquals("master.BM_Phase", ZString.Empty, masterNctsHeader.ArrivalMovementHeader.BM_Phase);
			},
			expectedMessageStatus: EDIMessage.Status.ProcessedOK, assertEDIMessageLinked: false);

		void PrepareNctsHeader(NctsHeader childNctsHeader)
		{
			AddChildMovement(masterNctsHeader, childNctsHeader, "MRN3", NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks, CHLogicalStatusList.Codes.Accepted);
		}
	}

		NctsHeader CreateMasterMovement()
		{
			var masterNctsHeader = Factory.New<NctsHeader>();
			masterNctsHeader.SetMovementType(Common.EU.NctsMoveHeaderType.Codes.Arrival);
			masterNctsHeader.ArrivalMovementHeader.MultipleMRNIndicator = ZBool.True;
			return masterNctsHeader;
		}

		void AddChildMovement(NctsHeader masterNctsHeader, NctsHeader childNctsHeader, string mrn, string customsStatus, string messageStatus)
		{
			if (childNctsHeader == null)
			{
				childNctsHeader = Factory.New<NctsHeader>();
				childNctsHeader.SetMovementType(Common.EU.NctsMoveHeaderType.Codes.Arrival);
			}
			childNctsHeader.ArrivalMovementHeader.MultipleMRNIndicator = ZBool.False;
			childNctsHeader.EffectiveMessageStatus = messageStatus;
			childNctsHeader.ArrivalMovementHeader.BM_CustomsStatus = customsStatus;
			childNctsHeader.MovementReferenceNumberSetter(mrn);
			masterNctsHeader.ArrivalMovementHeader.MovementReferenceNumbers.AddNew().CSI_ReferenceNumber = mrn;
			masterNctsHeader.ArrivalMovementHeader.RelatedArrivalMovements.AddPivotFor(childNctsHeader.ArrivalMovementHeader);
		}

	const string ACCEPTED = "ACCEPTED";
	const string REJECTED = "REJECTED";
}
