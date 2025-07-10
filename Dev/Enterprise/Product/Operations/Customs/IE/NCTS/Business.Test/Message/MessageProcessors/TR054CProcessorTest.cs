using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR054C;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(TR054CProcessor))]
	class TR054CProcessorTest : NCTSDepartureMessageProcessorAbstractTest<TR054CProcessor, NCTSInboundEDIMessage, NCTSOutboundEDIMessage, TR054CProvider>
	{
		protected override void AssertProcessResultCore(NctsDepartureMovementHeader movementHeader, NCTSInboundEDIMessage incomingMessage)
		{
			AssertEquals("BM_MessageStatus", LogicalStatusList.Codes.Accepted, movementHeader.BM_MessageStatus);
			AssertEquals("BM_CustomsStatus", NCTS5DepartureCustomsStatusList.Codes.RequestForAdvice, movementHeader.BM_CustomsStatus);
			MessageProcessorNotificationTestHelper.AssertEmail(
				incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Request for Advice (TR054) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
			AssertMessageInterpretation(incomingMessage, $@"A Request for Advice (TR054) message has been received for Job B00001000.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDU4EX144268149</td></tr><tr><td>Advice Requested</td><td>Y</td></tr><tr><td>Advice Request Date &amp; Time</td><td>01-Jan-23 10:15</td></tr></table>");
		}

		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.TR054C;

		protected override ZString MessageText => InterchangeProcessorTestHelper.GetStandardTR054CText();

		protected override ZString MessageFriendlyName => "TR054C: REQUEST FOR ADVICE";

		protected override TR054CProcessor Processor => new TR054CProcessor(logger, typeof(Tr054C));
	}
}
