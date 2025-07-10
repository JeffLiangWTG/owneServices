using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC019C;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC019CProcessor))]
	class CC019CProcessorTest : NCTSDepartureMessageProcessorAbstractTest<CC019CProcessor, NCTSInboundEDIMessage, NCTSOutboundEDIMessage, CC019CProvider>
	{
		protected override void AssertProcessResultCore(NctsDepartureMovementHeader messageAttachee, NCTSInboundEDIMessage incomingMessage)
		{
			AssertEquals("BM_MessageStatus", LogicalStatusList.Codes.Accepted, messageAttachee.BM_MessageStatus);
			AssertEquals("BM_CustomsStatus", EU.NCTS.Business.CodeDescriptionPairLists.NCTS5ArrivalCustomsStatusList.Codes.DiscrepanciesAtDestination, messageAttachee.BM_CustomsStatus);
			AssertMessageInterpretation(incomingMessage, @"A Discrepancy message has been received from Customs for Job B00001000 through the IE019 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Discrepancy Date</td><td>18-Sep-71</td></tr><tr><td>Discrepancy Notification Text</td><td>Test Discrepancies Notification</td></tr></table>");
			MessageProcessorNotificationTestHelper.AssertEmail(
				incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Discrepancy message has been received from Customs for Job B00001000 through the IE019 message." },
				new string[] { "staff1@where.com" });
		}

		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE019;

		protected override ZString MessageFriendlyName => "CC019C: DISCREPANCIES";

		protected override CC019CProcessor Processor => new CC019CProcessor(logger, typeof(Cc019CType));

		protected override ZString MessageText => InterchangeProcessorTestHelper.GetStandardCC019CText("21IEDUB11A782454R2");
	}
}
