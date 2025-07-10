using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR062C;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(TR062CProcessor))]
	class TR062CProcessorTest : NCTSDepartureMessageProcessorAbstractTest<TR062CProcessor, NCTSInboundEDIMessage, NCTSOutboundEDIMessage, TR062CProvider>
	{
		protected override void AssertProcessResultCore(NctsDepartureMovementHeader messageAttachee, NCTSInboundEDIMessage incomingMessage)
		{
			AssertEquals("BM_MessageStatus", LogicalStatusList.Codes.Accepted, messageAttachee.BM_MessageStatus);
			AssertEquals("BM_CustomsStatus", "AMR", messageAttachee.BM_CustomsStatus);
			MessageProcessorNotificationTestHelper.AssertEmail(
				incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Request Declaration Amendment (TR062) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
			AssertMessageInterpretation(incomingMessage, @"A Request Declaration Amendment (TR062) message has been received for Job B00001000.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDU4EX144268149</td></tr><tr><td>Case Id</td><td>ID123456</td></tr><tr><td>Remarks</td><td>Remarks</td></tr></table>");
		}

		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.TR062C;

		protected override ZString MessageFriendlyName => "TR062C: REQUEST DECLARATION AMENDMENT";

		protected override ZString MessageText => InterchangeProcessorTestHelper.GetStandardTR062CText("21IEDU4EX144268149");

		protected override TR062CProcessor Processor => new TR062CProcessor(logger, typeof(Tr062C));
	}
}
