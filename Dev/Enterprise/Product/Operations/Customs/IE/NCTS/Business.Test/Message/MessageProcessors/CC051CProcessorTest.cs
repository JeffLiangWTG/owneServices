using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC051C;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC051CProcessor))]
	class CC051CProcessorTest : NCTSDepartureMessageProcessorAbstractTest<CC051CProcessor, NCTSInboundEDIMessage, NCTSOutboundEDIMessage, CC051CProvider>
	{
		protected override void AssertProcessResultCore(NctsDepartureMovementHeader movementHeader, NCTSInboundEDIMessage incomingMessage)
		{
			AssertEquals("BM_MessageStatus", LogicalStatusList.Codes.Accepted, movementHeader.BM_MessageStatus);
			AssertEquals("BM_CustomsStatus", NCTS5DepartureCustomsStatusList.Codes.NotReleasedForTransit, movementHeader.BM_CustomsStatus);
			AssertMessageInterpretation(incomingMessage, @"A No Release for Transit (IE051) Message has been received for Job B00001000.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>19AA12345678901230</td></tr><tr><td>Declaration Submission Date And Time</td><td>17-Feb-23 00:00</td></tr><tr><td>No Release Motivation Code</td><td>CA</td></tr><tr><td>No Release Motivation Text</td><td>No Release</td></tr></table>");
			MessageProcessorNotificationTestHelper.AssertEmail(
				incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A No Release for Transit (IE051) Message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}

		protected override ZString MessageFriendlyName => "CC051C: NO RELEASE FOR TRANSIT";

		protected override CC051CProcessor Processor => new CC051CProcessor(logger, typeof(Cc051CType));

		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE051;

		protected override ZString MessageText => InterchangeProcessorTestHelper.GetStandardCC051CText();
	}
}
