using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(NE131ResponseMessageProcessor))]
sealed class NE131ResponseMessageProcessorTest : BaseGetMessageInboundMessageProcessorTest
{
	protected override string ApplicationCode => ApplicationCodeList.Codes.CHCustomsPassar;

protected override string MessageSubType => MessageSubTypeCodeList.Codes.PassarExportDeclarationActivationResponse;

protected override string GetResponseMessage() => TestingData.GetNE131(decision: PassarMessagingConstants.DecisionCodes.Accepted);

protected override string ExpectedFriendlyName => "NE131 - Export Declaration Activation Response";

public void TestProcessMessage_Accepted() => AssertProcessMessage(PassarMessagingConstants.DecisionCodes.Accepted, CHLogicalStatusList.Codes.Accepted, Events.CustomsEntryStatus, expectEntryStatusUpdated: true);

public void TestProcessMessage_Rejected() => AssertProcessMessage(PassarMessagingConstants.DecisionCodes.Rejected, CHLogicalStatusList.Codes.Invalid, Events.DeclarationActivationRejected);

public void AssertProcessMessage(string decision, string expectedEntryHeaderStatus, Event expectedEvent, bool expectEntryStatusUpdated = false)
{
	var gdrn = "21CHA6F0XKQGN8AMN0";
	var gdrnState = SwissCustomsConstants.CustomsStatusCodes.ShipmentRelease;

	CusEntryHeader entryHeader = null;

	AssertProcessMessage(
		(correlationIdentifier) =>
		{
			var declaration = Factory.New<JobDeclaration>();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter(gdrn + ".1");
			entryHeader.CH_EntryStatus = "XXX";
			var sentEdiMessage = MessageProcessorTestHelper.CreateSentEdiMessage(Factory, applicationReference: correlationIdentifier);
			entryHeader.Messages.Add(sentEdiMessage);
			return entryHeader;
		},
		(correlationIdentifier) => TestingData.GetNE131(gdrnNumber: gdrn, gdrnState: gdrnState, gdrnVersion: "1", decision: decision),
		(ediMessage) =>
		{
			AssertEquals("CH_Status", expectedEntryHeaderStatus, entryHeader.CH_Status);
			AssertEquals("CH_EntryStatus", expectEntryStatusUpdated ? gdrnState : "XXX", entryHeader.CH_EntryStatus);
			EventsTestHelper.AssertEventAdded(entryHeader, expectedEvent);
			EventsTestHelper.AssertEventAdded(entryHeader, Events.MessageStatusChange);
		},
		expectedMessageStatus: EDIMessageStatusList.Codes.ProcessedOK);
}

protected override BaseGetMessageInboundMessageProcessor CreateMessageProcessor() => new NE131ResponseMessageProcessor(Logger);
}