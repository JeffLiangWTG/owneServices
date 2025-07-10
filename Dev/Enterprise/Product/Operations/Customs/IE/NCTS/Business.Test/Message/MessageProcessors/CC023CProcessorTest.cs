using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC023C;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC023CProcessor))]
	sealed class CC023CProcessorTest : NCTSDepartureMessageProcessorAbstractTest<CC023CProcessor, NCTSInboundEDIMessage, NCTSOutboundEDIMessage, CC023CProvider>
	{
		protected override void AssertProcessResultCore(NctsDepartureMovementHeader messageAttachee, NCTSInboundEDIMessage incomingMessage)
		{
			AssertMessageInterpretation(incomingMessage, CC023CMessageInterpreterTest.GetExpectedInterpretationText());
			MessageProcessorNotificationTestHelper.AssertEmail(
				incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Guarantor Notification (IE023) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}

		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE023;

		protected override ZString MessageText => InterchangeProcessorTestHelper.GetStandardCC023CText("21IEDUB11A782454R2");

		protected override ZString MessageFriendlyName => "CC023C: GUARANTOR NOTIFICATION";

		protected override CC023CProcessor Processor => new CC023CProcessor(logger, typeof(Cc023CType));
	}
}
