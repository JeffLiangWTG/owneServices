using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(NE083ResponseMessageProcessor))]
sealed class NE083ResponseMessageProcessorTest : BaseGetMessageInboundMessageProcessorTest
{
	protected override string ApplicationCode => ApplicationCodeList.Codes.CHCustomsPassar;

protected override string MessageSubType => MessageSubTypeCodeList.Codes.PassarExportDeclarationIssuanceOfAssessmentDecision;

protected override string ExpectedFriendlyName => "NE083 - Export declaration issuance of assessment decisions";

protected override string GetResponseMessage() => TestingData.GetNE083();

public void TestProcessMessage()
{
	var gdrn = "21CHA6F0XKQGN8AMN0";

	CusEntryHeader entryHeader = null;

	AssertProcessMessage(
		(correlationIdentifier) =>
			{
				var declaration = Factory.New<JobDeclaration>();
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.MovementReferenceNumberSetter(gdrn + ".1");
				var sentEdiMessage = MessageProcessorTestHelper.CreateSentEdiMessage(Factory, applicationReference: correlationIdentifier);
				entryHeader.Messages.Add(sentEdiMessage);
				return entryHeader;
			},
			(correlationIdentifier) => TestingData.GetNE083(gdrnNumber: gdrn, gdrnVersion: "2"),
			(ediMessage) =>
			{
				AssertEquals("EM_LinkedObject", entryHeader, ediMessage.EM_LinkedObject);
				AssertEquals("CH_EntryStatus", CHLogicalStatusList.Codes.EVV, entryHeader.CH_EntryStatus);
				EventsTestHelper.AssertEventAdded(entryHeader, Events.CustomsEntryStatus, CHLogicalStatusList.Codes.EVV);
			},
			expectedMessageStatus: EDIMessageStatusList.Codes.ProcessedOK);
}

protected override BaseGetMessageInboundMessageProcessor CreateMessageProcessor() => new NE083ResponseMessageProcessor(Logger);
}