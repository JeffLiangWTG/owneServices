using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(NE060ResponseMessageProcessor))]
sealed class NE060ResponseMessageProcessorTest : BaseGetMessageInboundMessageProcessorTest
{
	protected override string ExpectedFriendlyName => "NE060 - Control Decision Notification";

	protected override string ApplicationCode => ApplicationCodes.CHCustomsPassar;

	string MessageType => MessageTypeCodeList.Codes.MSG;

	protected override string MessageSubType => MessageSubTypeCodeList.Codes.PassarExportControlDecisionNotification;

	protected override BaseGetMessageInboundMessageProcessor CreateMessageProcessor() => new NE060ResponseMessageProcessor(Logger);

	protected override string GetResponseMessage() => TestingData.GetNE060();

	public void TestProcessMessage()
	{
		var entryNum = "21CHA6F0XKQGN8AMN0.1";
		var responseMessage = UniversalEventTestDataHelper.CreateUniversalEventXml(responseMessage: TestingData.GetNE060());

		var factory = new BusinessObjectFactory();
		var (entryHeader, ediMessage) = MessageProcessorTestHelper.CreateHeaderAndMessageWithCusEntryNum(factory, MessageType, MessageSubType, responseMessage, entryNum);
		factory.Save();

		MessageProcessor.ProcessMessage(ediMessage);

		var messageDetail = ((CHEDIMessage)ediMessage).MessageDetail as INE060ResponseDetail;

		CombineAssertions(() =>
		{
			Assert("EDIMessage LinkTable", ediMessage.EM_LinkTable == nameof(CusEntryHeader));
			Assert("EDIMessage LinkUniqueID", ediMessage.EM_LinkUniqueID == entryHeader.PK);

			AssertEquals("EntryHeader CH_EntryStatus", AdditionalCHEntryStatusList.Codes.DecisionToControl, entryHeader.CH_EntryStatus);
			EventsTestHelper.AssertEventAdded(entryHeader, Events.CustomsEntryStatus, AdditionalCHEntryStatusList.Codes.DecisionToControl);

			AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, ediMessage.EM_Status);
		});
	}
}
