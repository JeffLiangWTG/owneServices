using Enterprise.BatchProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(NC124ResponseMessageProcessor))]
public class NC124ResponseMessageProcessorTest : BaseGetMessageInboundMessageProcessorTest
{
	public void TestProcessV1ExportDeclarationAccepted() => TestProcessMessage("1", MessageTypeCodeList.Codes.Export, PassarMessagingConstants.DecisionCodes.Accepted, CHLogicalStatusList.Codes.Accepted, AdditionalCHEntryStatusList.Codes.Active, Events.CustomsEntryStatus);
	public void TestProcessV2ExportDeclarationAccepted() => TestProcessMessage("2", MessageTypeCodeList.Codes.Export, PassarMessagingConstants.DecisionCodes.Accepted, CHLogicalStatusList.Codes.Accepted, AdditionalCHEntryStatusList.Codes.Active, Events.CustomsEntryStatus);
	public void TestProcessV2ImportDeclarationAccepted() => TestProcessMessage("2", MessageTypeCodeList.Codes.Import, PassarMessagingConstants.DecisionCodes.Accepted, CHLogicalStatusList.Codes.Accepted, AdditionalCHEntryStatusList.Codes.Active, Events.CustomsEntryStatus);

	public void TestProcessV1ExportDeclarationRejected() => TestProcessMessage("1", MessageTypeCodeList.Codes.Export, PassarMessagingConstants.DecisionCodes.Rejected, CHLogicalStatusList.Codes.Invalid, null, Events.DeclarationActivationRejected);
	public void TestProcessV2ExportDeclarationRejected() => TestProcessMessage("2", MessageTypeCodeList.Codes.Export, PassarMessagingConstants.DecisionCodes.Rejected, CHLogicalStatusList.Codes.Invalid, null, Events.DeclarationActivationRejected);
	public void TestProcessV2ImportDeclarationRejected() => TestProcessMessage("2", MessageTypeCodeList.Codes.Import, PassarMessagingConstants.DecisionCodes.Rejected, CHLogicalStatusList.Codes.Invalid, null, Events.DeclarationActivationRejected);

	protected override string ExpectedFriendlyName => "NC124 - Activation Response Message Processor";

	protected override string ApplicationCode => ApplicationCodes.CHCustomsPassar;

	protected override string MessageSubType => MessageSubTypeCodeList.Codes.PassarActivationResponse;

	protected override BaseGetMessageInboundMessageProcessor CreateMessageProcessor() => new NC124ResponseMessageProcessorForTesting(Logger);

	protected override string GetResponseMessage() => TestingData.GetNC124();

	void TestProcessMessage(string version, string operation, string decision, string expectedStatus, string expectedEntryStatus, Event expectedEvent)
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
			(correlationIdentifier) => GetTestingData(version, operation, correlationIdentifier, decision),
			(ediMessage) =>
			{
				AssertEquals("CH_Status", expectedStatus, entryHeader.CH_Status);
				if (expectedEntryStatus != null)
				{
					AssertEquals("CH_EntryStatus", expectedEntryStatus, entryHeader.CH_EntryStatus);
				}
				EventsTestHelper.AssertEventAdded(entryHeader, expectedEvent);
				EventsTestHelper.AssertEventAdded(entryHeader, Events.MessageStatusChange);
			},
			expectedMessageStatus: EDIMessageStatusList.Codes.ProcessedOK);

		string GetTestingData(string version, string operation, string correlationIdentifier, string decision)
		{
			if (version == "2")
			{
				if (operation == MessageTypeCodeList.Codes.Import)
				{
					return TestingData.GetNC124V2_Import(correlationId: correlationIdentifier, decision: decision);
				}
				else
				{
					return TestingData.GetNC124V2_Export(correlationId: correlationIdentifier, decision: decision);
				}
			}
			else
			{
				return TestingData.GetNC124(correlationId: correlationIdentifier, decision: decision);
			}
		}
	}

	class NC124ResponseMessageProcessorForTesting : NC124ResponseMessageProcessor
	{
		public NC124ResponseMessageProcessorForTesting(LoggingInformation logger) : base(logger)
		{
		}
	}
}
