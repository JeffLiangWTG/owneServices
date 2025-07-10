using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR060C;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing;

[TestedType(typeof(TR060CProcessor))]
class TR060CProcessorTest : NCTSDepartureMessageProcessorAbstractTest<TR060CProcessor, NCTSInboundEDIMessage, NCTSOutboundEDIMessage, TR060CProvider>
{
	protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.TR060C;

	protected override ZString MessageText => InterchangeProcessorTestHelper.GetStandardTR060CText("19MRNCC060C0123456");

	protected override ZString MessageFriendlyName => "TR060C: CONTROL DECISION NOTIFICATION AT DESTINATION";

	protected override TR060CProcessor Processor => new TR060CProcessor(logger, typeof(Tr060C));

	protected override void AssertProcessResultCore(NctsDepartureMovementHeader messageAttachee, NCTSInboundEDIMessage incomingMessage)
	{
		AssertEquals("BM_MessageStatus", LogicalStatusList.Codes.Accepted, messageAttachee.BM_MessageStatus);
		AssertEquals("BM_CustomsStatus", "UCN", messageAttachee.BM_CustomsStatus);
		MessageProcessorNotificationTestHelper.AssertEmail(
			incomingMessage.MessageTypeWithDescription + " Response for B00001000",
			new[] { "A control message (TR060) from Office of Destination has been received for Job B00001000." },
			new string[] { "staff1@where.com" });
		AssertMessageInterpretation(incomingMessage, TR060CMessageInterpreterTest.ExpectedInterpretation);
	}

	protected override void SetUp()
	{
		base.SetUp();
		MessageTestHelper.SetupCL384Types(Factory);
	}
}
