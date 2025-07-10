using Enterprise.BatchProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.CH.Business.Testing;

public class NC909ResponseMessageProcessorTest : BaseGetMessageInboundMessageProcessorTest
{
	protected override string ApplicationCode => ApplicationCodes.CHCustomsPassar;

	public void TestProcessExportDeclarationDeclaration() => TestProcessMessage(PassarDeclarationPhaseList.Codes.Declaration, Events.DeclarationRejected);

	public void TestProcessExportDeclarationAmendment() => TestProcessMessage(PassarDeclarationPhaseList.Codes.Amendment, Events.DeclarationAmendmentRejected);

	public void TestProcessExportDeclarationCancellation() => TestProcessMessage(PassarDeclarationPhaseList.Codes.Cancellation, Events.DeclarationCancellationRejected);

	public void TestProcessExportDeclarationActivation() => TestProcessMessage(PassarDeclarationPhaseList.Codes.Activation, Events.DeclarationActivationRejected);

	void TestProcessMessage(string initialPhaseStatus, Event expectedEvent = null)
	{
		CusEntryHeader entryHeader = null;

		AssertProcessMessage(
			(correlationIdentifier) =>
			{
				var declaration = Factory.New<JobDeclaration>();
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				var sentEdiMessage = MessageProcessorTestHelper.CreateSentEdiMessage(Factory, applicationReference: correlationIdentifier);
				entryHeader.Messages.Add(sentEdiMessage);
				entryHeader.CH_PhaseStatus = initialPhaseStatus;
				return entryHeader;
			},
			(correlationIdentifier) => TestingData.GetNC909(correlationIdentifier),
			(ediMessage) =>
			{
				AssertEquals("MessageStatus", CHLogicalStatusList.Codes.Invalid, entryHeader.CH_Status);
				if (expectedEvent != null)
				{
					EventsTestHelper.AssertEventAdded(entryHeader, expectedEvent);
				}
			},
			expectedMessageStatus: EDIMessageStatusList.Codes.ProcessedOK);
	}

	protected override string ExpectedFriendlyName => "NC909 - Technical Error Response Message Processor";

	protected override string MessageSubType => MessageSubTypeCodeList.Codes.PassarTechnicalError;

	protected override string GetResponseMessage() => TestingData.GetNC909();

	protected override BaseGetMessageInboundMessageProcessor CreateMessageProcessor() => new NC909ResponseMessageProcessorForTesting(Logger);

	class NC909ResponseMessageProcessorForTesting : NC909ResponseMessageProcessor
	{
		public NC909ResponseMessageProcessorForTesting(LoggingInformation logger) : base(logger)
		{
		}
	}
}
