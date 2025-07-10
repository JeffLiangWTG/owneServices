using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.Business.Testing;

class NE021ResponseMessageProcessorTest : BaseGetMessageInboundMessageProcessorTest
{
	protected override string ExpectedFriendlyName => "NE021 - Payload request goods declaration response";

	protected override string ApplicationCode => ApplicationCodeList.Codes.CHCustomsPassar;

	protected override string MessageSubType => MessageSubTypeCodeList.Codes.PassarExportPayloadRequestGoodsDeclarationResponse;

	protected override string GetResponseMessage() => TestingData.GetNE021();

	protected override BaseGetMessageInboundMessageProcessor CreateMessageProcessor() => new NE021ResponseMessageProcessor(Logger);

	public void TestProcessMessage()
	{
		CusEntryHeader entryHeader = null;

		AssertProcessMessage(
			(correlationIdentifier) =>
			{
				var declaration = Factory.New<JobDeclaration>();
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				var sentEdiMessage = MessageProcessorTestHelper.CreateSentEdiMessage(Factory, applicationReference: correlationIdentifier);
				entryHeader.Messages.Add(sentEdiMessage);
				return entryHeader;
			},
			(correlationIdentifier) => TestingData.GetNE021(correlationIdentifier),
			(ediMessage) =>
			{
				AssertEquals("MessageStatus", CHLogicalStatusList.Codes.Accepted, entryHeader.CH_Status);
				EventsTestHelper.AssertEventAdded(entryHeader, Events.MessageStatusChange, expectedReference: CHLogicalStatusList.Codes.Accepted);
			},
			expectedMessageStatus: EDIMessageStatusList.Codes.ProcessedOK);
	}
}
