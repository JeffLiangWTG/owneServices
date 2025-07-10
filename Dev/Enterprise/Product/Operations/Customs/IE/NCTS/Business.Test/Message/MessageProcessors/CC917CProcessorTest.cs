using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC917C;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC917CProcessor))]
	sealed class CC917CProcessorTest : NCTSDepartureMessageProcessorAbstractTest<CC917CProcessor, NCTSInboundEDIMessage, NCTSOutboundEDIMessage, CC917CProvider>
	{
		protected override void AssertProcessResultCore(NctsDepartureMovementHeader movementHeader, NCTSInboundEDIMessage incomingMessage)
		{
			AssertEquals("BM_MessageStatus", LogicalStatusList.Codes.Error, movementHeader.BM_MessageStatus);
			AssertMessageInterpretation(incomingMessage, @"An Syntax Error Notification (IE917) message has been received for Job B00001000.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>MRN</td><td>19MRNCC060C0123456</td></tr><tr><td>Error Line Number</td><td>1</td></tr><tr><td>Error Column Number</td><td>1</td></tr><tr><td>Error Pointer</td><td>Pointer1</td></tr><tr><td>Error Code</td><td>12</td></tr><tr><td>Error Text</td><td>ErrorText1</td></tr><tr><td>Error Line Number</td><td>2</td></tr><tr><td>Error Column Number</td><td>1</td></tr><tr><td>Error Pointer</td><td>Pointer2</td></tr><tr><td>Error Code</td><td>13</td></tr><tr><td>Error Text</td><td>ErrorText2</td></tr></table>");
			MessageProcessorNotificationTestHelper.AssertEmail(
				incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "An Syntax Error Notification (IE917) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}

		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE917;

		protected override ZString MessageText => InterchangeProcessorTestHelper.GetStandardCC917CText("21IEDUB11A782454R2", "19MRNCC060C0123456");

		protected override ZString MessageFriendlyName => "CC917C: XML NACK";

		protected override CC917CProcessor Processor => new CC917CProcessor(logger, typeof(Cc917CType));
	}
}
