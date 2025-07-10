using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC045C;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC045CProcessor))]
	class CC045CProcessorTest : NCTSDepartureWithGuaranteeMessageProcessorAbstractTest<CC045CProcessor, NCTSInboundEDIMessage, NCTSOutboundEDIMessage, CC045CProvider>
	{
		protected override void AssertProcessResultCore(NctsDepartureMovementHeader movementHeader, NCTSInboundEDIMessage incomingMessage)
		{
			AssertEquals("BM_CustomsStatus", NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed, movementHeader.BM_CustomsStatus);
			AssertEquals("BM_MessageStatus", LogicalStatusList.Codes.Accepted, movementHeader.BM_MessageStatus);
			AssertTransaction(movementHeader, 3, 25000m, 0m, Customs.Business.PermitTransactionStatusList.Codes.Confirmed);
			MessageProcessorNotificationTestHelper.AssertEmail(
				incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Write-Off Notification (IE045) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
			AssertMessageInterpretation(incomingMessage, $@"A Write-Off Notification (IE045) message has been received for Job B00001000.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Write-off Date</td><td>18-Sep-71</td></tr></table>");
		}

		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE045;

		protected override ZString MessageText => InterchangeProcessorTestHelper.GetStandardCC045CText("21IEDUB11A782454R2");

		protected override ZString MessageFriendlyName => "CC045C: WRITE-OFF NOTIFICATION";

		protected override CC045CProcessor Processor => new CC045CProcessor(logger, typeof(Cc045CType));
	}
}
