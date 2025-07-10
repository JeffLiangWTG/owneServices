using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NT025ResponseMessageProcessor))]
sealed class NT025ResponseMessageProcessorTest : BasePassarNctsMSGMessageProcessorTest
{
	protected override string ExpectedFriendlyName => "NT025 - Arrival Indication Message Processor";

	protected override string MessageSubType => MessageSubTypeCodeList.Codes.PassarArrivalIndication;

	protected override string TestedMovementType => NctsMovementType.Codes.Arrival;

	protected override bool FindLinkedObjectByMRNAnyVersion => true;

	protected override BaseGetMessageInboundMessageProcessor CreateMessageProcessor() => new NT025ResponseMessageProcessor(Logger);

	protected override string GetResponseMessage() => TestingData.GetNT025();

	protected override ZString GetResponseMessageWithMRN(string mrn, string mrnVersion) => TestingData.GetNT025(mrn, mrnVersion);

	public void TestProcessMessage()
	{
		const string MRN = "22CHVL2525YYN7IZJ7";
		const string MRNVersion = "1";
		var releaseDate = new ZDate(2024, 2, 3);

		NctsHeader nctsHeader = null;

		AssertProcessMessage(
			(correlationIdentifier) => nctsHeader = Helper.CreateNctsHeaderAndSentMessage(movementType: TestedMovementType, mrn: $"{MRN}.{MRNVersion}", prepareNctsHeader: PrepareNctsHeader).nctsHeader,
			(correlationIdentifier) => TestingData.GetNT025(mrn: MRN, mrnVersion: MRNVersion, releaseIndicator: FULL_RELEASE, releaseDate: releaseDate),
			(ediMessage) =>
			{
				AssertEquals("CE_IssueDate", releaseDate, nctsHeader.MovementReferenceEntryNumber.CE_IssueDate);
				AssertEquals("BM_CustomsStatus", NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease, nctsHeader.ArrivalMovementHeader.BM_CustomsStatus);
				Helper.AssertEvent("Event", nctsHeader.ArrivalMovementHeader.Logs, Events.CustomsEntryStatus, NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease);
			}, expectedMessageStatus: EDIMessageStatusList.Codes.ProcessedOK);

		AssertProcessMessage(
			(correlationIdentifier) => nctsHeader,
			(correlationIdentifier) => TestingData.GetNT025(mrn: MRN, mrnVersion: MRNVersion, releaseIndicator: PARTIAL_RELEASE),
			(ediMessage) =>
			{
				AssertEquals("BM_CustomsStatus", NCTS5ArrivalCustomsStatusList.Codes.ClosedPartialRelease, nctsHeader.ArrivalMovementHeader.BM_CustomsStatus);
				Helper.AssertEvent("Event", nctsHeader.ArrivalMovementHeader.Logs, Events.CustomsEntryStatus, NCTS5ArrivalCustomsStatusList.Codes.ClosedPartialRelease);
			}, expectedMessageStatus: EDIMessageStatusList.Codes.ProcessedOK);
		void PrepareNctsHeader(NctsHeader header)
		{
			header.MovementReferenceEntryNumber.CE_IssueDate = ZDateTime.Empty;
		}
	}

	public void TestMasterCustomsStatusUpdated_AllChildrenCL1()
	{
		var masterNctsHeader = CreateMasterMovement();
		AddChildMovement(masterNctsHeader, null, "MRN1", NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease);

		AssertProcessMessage(
			(correlationIdentifier) => Helper.CreateNctsHeaderAndSentMessage(correlationIdentifier, movementType: TestedMovementType, prepareNctsHeader: PrepareNctsHeader).nctsHeader,
			(correlationIdentifier) => TestingData.GetNT025(mrn: SampleMRN, mrnVersion: "1", releaseIndicator: FULL_RELEASE, releaseDate: new ZDate(2024, 5, 2)),
			(ediMessage) =>
			{
				AssertEquals("master.BM_CustomsStatus", NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease, masterNctsHeader.ArrivalMovementHeader.BM_CustomsStatus);
				AssertEquals("master.ARN.CE_ExpiryDate", new ZDateTime(2024, 5, 2), masterNctsHeader.ArrivalReferenceEntryNumber.CE_ExpiryDate);
			},
			expectedMessageStatus: EDIMessage.Status.ProcessedOK, assertEDIMessageLinked: false);

		void PrepareNctsHeader(NctsHeader childNctsHeader)
		{
			AddChildMovement(masterNctsHeader, childNctsHeader, SampleMRN + ".1", NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks);
		}
	}

	public void TestMasterCustomsStatusNotUpdated_OtherCustomsStatus()
	{
		AssertMasterCustomsStatusNotUpdated(NCTS5ArrivalCustomsStatusList.Codes.ClosedPartialRelease, FULL_RELEASE);
	}

	public void TestMasterCustomsStatusNotUpdated_NotFullRelease()
	{
		AssertMasterCustomsStatusNotUpdated(NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease, PARTIAL_RELEASE);
	}

	void AssertMasterCustomsStatusNotUpdated(string otherChildCustomsStatus, string releaseIndicator)
	{
		var masterNctsHeader = CreateMasterMovement(arnExpireDate: new ZDateTime(2024, 1, 1));
		AddChildMovement(masterNctsHeader, null, "MRN1", otherChildCustomsStatus);

		AssertProcessMessage(
			(correlationIdentifier) => Helper.CreateNctsHeaderAndSentMessage(correlationIdentifier, movementType: TestedMovementType, prepareNctsHeader: PrepareNctsHeader).nctsHeader,
			(correlationIdentifier) => TestingData.GetNT025(mrn: SampleMRN, mrnVersion: "1", releaseIndicator: releaseIndicator, releaseDate: new ZDate(2024, 5, 2)),
			(ediMessage) =>
			{
				AssertEquals("master.BM_CustomsStatus", ZString.Empty, masterNctsHeader.ArrivalMovementHeader.BM_CustomsStatus);
				AssertEquals("master.ARN.CE_ExpiryDate not changed", new ZDateTime(2024, 1, 1), masterNctsHeader.ArrivalReferenceEntryNumber.CE_ExpiryDate);
			},
			expectedMessageStatus: EDIMessage.Status.ProcessedOK, assertEDIMessageLinked: false);

		void PrepareNctsHeader(NctsHeader childNctsHeader)
		{
			AddChildMovement(masterNctsHeader, childNctsHeader, SampleMRN + ".1", NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks);
		}
	}

		NctsHeader CreateMasterMovement(ZDateTime? arnExpireDate = null)
		{
			var masterNctsHeader = Factory.New<NctsHeader>();
			masterNctsHeader.SetMovementType(Common.EU.NctsMoveHeaderType.Codes.Arrival);
			masterNctsHeader.ArrivalMovementHeader.MultipleMRNIndicator = ZBool.True;
			if (arnExpireDate != null)
			{
				masterNctsHeader.ArrivalReferenceEntryNumber.CE_ExpiryDate = arnExpireDate.Value;
			}
			return masterNctsHeader;
		}

		void AddChildMovement(NctsHeader masterNctsHeader, NctsHeader childNctsHeader, string mrn, string customsStatus)
		{
			if (childNctsHeader == null)
			{
				childNctsHeader = Factory.New<NctsHeader>();
				childNctsHeader.SetMovementType(Common.EU.NctsMoveHeaderType.Codes.Arrival);
			}
			childNctsHeader.ArrivalMovementHeader.MultipleMRNIndicator = ZBool.False;
			childNctsHeader.ArrivalMovementHeader.BM_CustomsStatus = customsStatus;
			childNctsHeader.MovementReferenceNumberSetter(mrn);
			masterNctsHeader.ArrivalMovementHeader.MovementReferenceNumbers.AddNew().CSI_ReferenceNumber = mrn;
			masterNctsHeader.ArrivalMovementHeader.RelatedArrivalMovements.AddPivotFor(childNctsHeader.ArrivalMovementHeader);
		}

	const string SampleMRN = "22DE16137320157570";
	const string FULL_RELEASE = "FULL_RELEASE";
	const string PARTIAL_RELEASE = "PARTIAL_RELEASE";
}
