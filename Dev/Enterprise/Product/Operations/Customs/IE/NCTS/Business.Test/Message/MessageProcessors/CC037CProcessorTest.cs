using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC037C;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC037CProcessor))]
	class CC037CProcessorTest : NCTSGuaranteeMessageProcessorAbstractTest<CC037CProcessor, NCTSInboundEDIMessage, NCTSOutboundEDIMessage, CC037CProvider>
	{
		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE037;

		protected override ZString MessageText => InterchangeProcessorTestHelper.GetStandardCC037CText();

		protected override ZString MessageFriendlyName => "CC037C: RESPONSE QUERY ON GUARANTEES";

		protected override CC037CProcessor Processor => new CC037CProcessor(logger, typeof(Cc037CType));

		protected override void AssertProcessResultCore(NctsDepartureMovementHeader movementHeader, NCTSInboundEDIMessage incomingMessage)
		{
			AssertEquals("BM_MessageStatus", LogicalStatusList.Codes.Accepted, movementHeader.BM_MessageStatus);
			AssertMessageInterpretation(incomingMessage, CC037CMessageInterpreterTest.GetExpectedInterpretationText());
			MessageProcessorNotificationTestHelper.AssertEmail(
				incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "Response Query on Guarantee Message (IE037) has been received. NCTS has sent the response on Query on Guarantee for Job B00001000." },
				new string[] { "staff1@where.com" });
		}
	}
}
