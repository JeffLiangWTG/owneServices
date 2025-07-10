using System;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NT061ResponseMessageProcessor))]
sealed class NT061ResponseMessageProcessorTest : BasePassarNctsMSGMessageProcessorTest
{
	protected override string TestedMovementType => NctsMovementType.Codes.Arrival;

	[TestDate(2024, 8, 29, 0, 0, 0)]
	public void TestProcessMessageFullReleased() => AssertProcessMessage(true, true, false, NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease);

	[TestDate(2024, 8, 29, 0, 0, 0)]
	public void TestProcessMessageNotFinal() => AssertProcessMessage(true, false, false , NCTS5ArrivalCustomsStatusList.Codes.CHClear);

	[TestDate(2024, 8, 29, 0, 0, 0)]
	public void TestProcessMessagehasMRN() => AssertProcessMessage(true, true, true, NCTS5ArrivalCustomsStatusList.Codes.CHClear);

	[TestDate(2024, 8, 29, 0, 0, 0)]
	public void TestProcessMessageIsNotClear() => AssertProcessMessage(false, true, false, NCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease);

	void AssertProcessMessage(bool isInspectionDecisionClear, bool isSelectionStatusIsFinal, bool hasMRN, string expCustomsStatus)
	{
		NctsHeader nctsHeader = null;
		const string arrivalReferenceNumber = "ARN123";

		AssertProcessMessage(
			(correlationIdentifier) =>
			{
				nctsHeader = Helper.CreateNctsHeaderAndSentMessage(applicationReference: correlationIdentifier, movementType: TestedMovementType).nctsHeader;
				nctsHeader.ArrivalReferenceEntryNumber.CE_EntryNum = arrivalReferenceNumber;
				nctsHeader.ArrivalMovementHeader.AdditionalTransitOperations.AddNew();
				nctsHeader.ArrivalMovementHeader.SupernumeraryGoods.AddNew();
				if (hasMRN)
				{
					nctsHeader.ArrivalMovementHeader.MovementReferenceNumbers.AddNew();
				}
				return nctsHeader;
			},
			(correlationIdentifier) => TestingData.GetNT061(correlationId: correlationIdentifier, arrivalReferenceNumber: arrivalReferenceNumber, inspectionDecision: isInspectionDecisionClear ? "CLEAR" : "INTERVENTION", selectionStatus: isSelectionStatusIsFinal ?  "FINAL" : "PRELIMINARY"),
			(ediMessage) =>
			{
				var customsStatus = nctsHeader.ArrivalMovementHeader.BM_CustomsStatus;
				AssertEquals("Customs Job Status", expCustomsStatus, customsStatus);
				var customsEntryStatusEvent = nctsHeader.ArrivalMovementHeader.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus);
				AssertNotNull("Customs Entry Status (CES) Event added", customsEntryStatusEvent);
				AssertEquals("CES Event -> SL_Referece", customsStatus, customsEntryStatusEvent.SL_Reference);
			},
			expectedMessageStatus: EDIMessage.Status.ProcessedOK);
	}

	public void TestFindLinkedObjectByOppositeInformationReferenceNumber()
	{
		var passarMessageProcessorTestHelper = new CustomsMessageProcessorTestHelper(Factory);
		var messageIdentification = Guid.NewGuid().ToString();
		var (nctsHeader, sentEdiMessage) = Helper.CreateNctsHeaderAndSentMessage(applicationReference: messageIdentification, movementType: TestedMovementType);
		var (company, ediMessage, transaction) = passarMessageProcessorTestHelper.CreateGetMessageResponseObjects(ApplicationCode, eventType: EventType, messageSubType: MessageSubType, responseMessage: TestingData.GetNT061(referenceNumber: messageIdentification));
		Factory.Save();

		MessageProcessor.ProcessMessage(ediMessage);
		AssertEquals("Linked Object Nctsheader", nctsHeader, ediMessage.EM_LinkedObject);
	}

	public void TestLinkedObjectNotFound()
	{
		var passarMessageProcessorTestHelper = new CustomsMessageProcessorTestHelper(Factory);
		var messageId = Guid.NewGuid().ToString();
		var (company, ediMessage, transaction) = passarMessageProcessorTestHelper.CreateGetMessageResponseObjects(ApplicationCode, eventType: EventType, messageSubType: MessageSubType, responseMessage: TestingData.GetNT061(arrivalReferenceNumber: "ARNXXX"), messageId: messageId);
		Factory.Save();
		MessageProcessor.ProcessMessage(ediMessage);
		AssertNull(ediMessage.EM_LinkedObject);
		AssertEquals($"Warning: Unable to link EDI Message '{ediMessage.EM_MessageNum}' to an existing business object.\r\n", Logger.AccumulatedLogMessages.ToString());
	}

	protected override string ExpectedFriendlyName => "NT061 - Control Decision Notification Message Processor";

	protected override string MessageSubType => MessageSubTypeCodeList.Codes.PassarControlDecisionNotification;

	protected override BaseGetMessageInboundMessageProcessor CreateMessageProcessor() => new NT061ResponseMessageProcessor(Logger);

	protected override string GetResponseMessage() => TestingData.GetNT061();
}
