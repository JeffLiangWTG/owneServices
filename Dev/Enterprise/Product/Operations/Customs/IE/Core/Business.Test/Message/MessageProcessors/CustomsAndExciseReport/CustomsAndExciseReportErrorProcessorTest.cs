using CargoWise.Customs.IE.MessageDefinitions.Common.MessageAcknowledgement;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing;

[TestedType(typeof(CustomsAndExciseReportErrorProcessor))]
sealed class CustomsAndExciseReportErrorProcessorTest : CustomsAndExciseReportInboundMessageProcessorTest<CustomsAndExciseReportErrorProcessor, CustomsAndExciseReportErrorProvider>
{
	protected override ZString MessageFriendlyName => "ROS Error";

	protected override CustomsAndExciseReportErrorProcessor Processor => new CustomsAndExciseReportErrorProcessor(logger, typeof(MessageAcknowledgement));

	protected override ZString MessageType => CustomsAndExciseReportTypeList.Codes.PSR;

	protected override ZString MessageText => CustomsAndExciseReportInterchangeProcessorTestHelper.CreateErrorText();

	protected override ZString ExpectedMessageInterpretation => CustomsAndExciseReportErrorInterpreterTest.GetExpectedInterpretation();

	protected override void AssertProcessResultCore(CustomsAndExciseReportOutboundMessage messageAttachee)
	{
		AssertEquals("LinkedObject should have been set to Failed.", EDIMessageStatusList.Codes.Failed, messageAttachee.EM_Status);
		MessageProcessorNotificationTestHelper.AssertEmail($"{MessageFriendlyName} ({MessageType})",
			new string[] { $"{MessageFriendlyName} ({MessageType}) Response (Failure) for IER00001000" },
			new string[] { "staff1@where.com" });
	}

	protected override (CustomsAndExciseReportOutboundMessage outgoingMessage, CustomsAndExciseReportInboundMessage incomingMessage) CreateSetupData(string incomingMessageText = null)
	{
		CustomsAndExciseReportErrorInterpreterTest.CreateRefDataForInterpreter(Factory);
		return base.CreateSetupData(incomingMessageText);
	}
}

