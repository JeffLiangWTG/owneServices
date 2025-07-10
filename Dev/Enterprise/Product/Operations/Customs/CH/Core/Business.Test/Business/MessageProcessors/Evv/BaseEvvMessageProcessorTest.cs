using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.Business.Testing;

abstract class BaseEvvMessageProcessorTest : TestCaseWithFactory
{
	public void TestMessageFriendlyName() => AssertEquals(MessageFriendlyName, GetMessageProcessor().MessageFriendlyName);

	protected abstract ApplicationTypeMessageProcessor GetMessageProcessor();

	protected abstract string MessageFriendlyName { get; }

	protected LoggingInformationForTesting Logger => logger ??= new LoggingInformationForTesting();
	LoggingInformationForTesting logger;

	internal void AssertMessageLinked(string messageSubType, string messageContent) => CombineAssertions(() =>
	{
		var factory = new BusinessObjectFactory();

		var (entryHeader, ediMessage) = MessageProcessorTestHelper.CreateHeaderMessagesAndInterchanges(factory, ApplicationCodeList.Codes.CHCustomsEdec, MessageTypeCodeList.Codes.EVV, string.Empty, messageSubType, messageContent);

		MessageProcessorTestHelper.ProcessMessage(GetMessageProcessor(), ediMessage);

		AssertEquals($"{messageSubType}: EM_LinkTable", CusEntryHeader.Schema.TableName, ediMessage.EM_LinkTable);
		AssertEquals($"{messageSubType}: EM_LinkUniqueID", entryHeader.PK, ediMessage.EM_LinkUniqueID);
	});

	internal void AssertEvent(string incommingMessageSubType, string outgoingMessageSubType, string incommingMessageContent, string expectedSLReference)
	{
		var factory = new BusinessObjectFactory();

		var (entryHeader, ediMessage) = MessageProcessorTestHelper.CreateHeaderMessagesAndInterchanges(factory, ApplicationCodeList.Codes.CHCustomsEdec, MessageTypeCodeList.Codes.EVV, outgoingMessageSubType, incommingMessageSubType, incommingMessageContent);

		MessageProcessorTestHelper.ProcessMessage(GetMessageProcessor(), ediMessage);

		var logEntry = entryHeader.Logs.MostRecentLogByEventTime(Events.ElectronicAssessmentDecisionStatus);
		AssertEquals($"{incommingMessageSubType} for {outgoingMessageSubType}:", expectedSLReference, logEntry.SL_Reference);
	}

	internal void AssertApplicationReference(string messageSubType, string messageContent, string expectedApplicationReference)
	{
		var factory = new BusinessObjectFactory();

		var (entryHeader, ediMessage) = MessageProcessorTestHelper.CreateHeaderMessagesAndInterchanges(factory, ApplicationCodeList.Codes.CHCustomsEdec, MessageTypeCodeList.Codes.EVV, string.Empty, messageSubType, messageContent);
		ediMessage.EM_ApplicationReference = "XXX";

		MessageProcessorTestHelper.ProcessMessage(GetMessageProcessor(), ediMessage);

		AssertEquals($"SubType {messageSubType}", expectedApplicationReference, ediMessage.EM_ApplicationReference);
	}

	internal void AssertProcessing(string messageSubType, string messageText, string documentType, string eventStu, string applicationReference = "")
	{
		var (shipment, ediMessage) = MessageProcessorTestHelper.CreateShipmentMessagesAndInterchanges(Factory, ApplicationCodeList.Codes.CHCustomsEdec, MessageTypeCodeList.Codes.EVV, messageSubType, messageSubType, messageText);

		MessageProcessorTestHelper.ProcessMessage(GetMessageProcessor(), ediMessage);

		AssertEquals($"{messageSubType}: EM_LinkTable", shipment.TableName, ediMessage.EM_LinkTable);
		AssertEquals($"{messageSubType}: EM_LinkUniqueID", shipment.PK, ediMessage.EM_LinkUniqueID);
		AssertEquals($"{messageSubType}: EM_MessageSubType", messageSubType, ediMessage.EM_MessageSubType);
		AssertEquals($"{messageSubType}: EM_ApplicationReference", applicationReference, ediMessage.EM_ApplicationReference);
		EventsTestHelper.AssertEventAdded(shipment, Events.ElectronicAssessmentDecisionStatus, $"|STU=Received-{eventStu}|TYP={documentType}", message: messageSubType);
	}

	internal void AssertStatementLineStatus(string expectedStatementLineStatus, string messageSubType, string mrnIncludingVersion, string messageText)
	{
		var (_, ediMessage) = MessageProcessorTestHelper.CreateHeaderMessagesAndInterchanges(Factory, ApplicationCodeList.Codes.CHCustomsEdec, MessageTypeCodeList.Codes.EVV, messageSubType, messageSubType, messageText, mrnIncludingVersion);

		var summaryHeader = Factory.New<CustomsSummaryHeader>();
		summaryHeader.B2_GC = GlbCompany.CurrentCompany.PK;
		var summaryLine = summaryHeader.SummaryLines.AddNew();
		summaryLine.B3_EntryNum = mrnIncludingVersion;
		summaryLine.LineCharge.B4_ChargeType = BordereauChargeTypeList.GetChargeType(messageSubType);

		Factory.Save();

		MessageProcessorTestHelper.ProcessMessage(GetMessageProcessor(), ediMessage);

		AssertEquals(expectedStatementLineStatus, summaryLine.B3_Status);
	}
}
