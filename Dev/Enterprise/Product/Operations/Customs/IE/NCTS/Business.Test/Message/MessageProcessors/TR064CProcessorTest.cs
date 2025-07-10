using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR064C;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(TR064CProcessor))]
	class TR064CProcessorTest : NCTSDepartureMessageProcessorAbstractTest<TR064CProcessor, NCTSInboundEDIMessage, NCTSOutboundEDIMessage, TR064CProvider>
	{
		protected override void AssertProcessResultCore(NctsDepartureMovementHeader messageAttachee, NCTSInboundEDIMessage incomingMessage)
		{
			AssertEquals("BM_MessageStatus", LogicalStatusList.Codes.Accepted, messageAttachee.BM_MessageStatus);
			AssertEquals("BM_CustomsStatus", "CAR", messageAttachee.BM_CustomsStatus);
			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Request Declaration Invalidation (TR064) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}

		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.TR064C;

		protected override ZString MessageFriendlyName => "TR064C: REQUEST DECLARATION INVALIDATION";

		protected override ZString MessageText => InterchangeProcessorTestHelper.GetStandardTR064CText("21IEDU4EX144268149");

		protected override TR064CProcessor Processor => new TR064CProcessor(logger, typeof(Tr064C));
	}
}
