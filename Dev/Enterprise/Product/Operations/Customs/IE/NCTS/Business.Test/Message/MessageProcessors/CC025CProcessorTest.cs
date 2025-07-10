using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC025C;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC025CProcessor))]
	class CC025CProcessorCL1Test : NCTSDepartureMessageProcessorAbstractTest<CC025CProcessor, NCTSInboundEDIMessage, NCTSOutboundEDIMessage, CC025CProvider>
	{
		protected override void AssertProcessResultCore(NctsDepartureMovementHeader messageAttachee, NCTSInboundEDIMessage incomingMessage)
		{
			AssertEquals("BM_MessageStatus", LogicalStatusList.Codes.Accepted, messageAttachee.BM_MessageStatus);
			AssertEquals("BM_CustomsStatus", ExpectedBM_CustomsStatus, messageAttachee.BM_CustomsStatus);
			AssertMessageInterpretation(incomingMessage, CC025CMessageInterpreterTest.GetExpectedInterpretationText(ReleaseIndicatorForTest));
			MessageProcessorNotificationTestHelper.AssertEmail(
				incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Goods Release Notification (IE025) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}

		protected virtual string ExpectedBM_CustomsStatus => "CL1";

		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE025;

		protected override ZString MessageText => InterchangeProcessorTestHelper.GetStandardCC025CText("21IEDUB11A782454R2", ReleaseIndicatorForTest);

		protected virtual string ReleaseIndicatorForTest => "1";

		protected override ZString MessageFriendlyName => "CC025C: GOODS RELEASE NOTIFICATION";

		protected override CC025CProcessor Processor => new CC025CProcessor(logger, typeof(Cc025CType));

		protected override void SetUp()
		{
			MessageTestHelper.SetupReleaseCodes(Factory);
			base.SetUp();
		}
	}

	[TestedType(typeof(CC025CProcessor))]
	class CC025CProcessorCD2Test : CC025CProcessorCL1Test
	{
		protected override string ExpectedBM_CustomsStatus => "CD2";
		protected override string ReleaseIndicatorForTest => "2";
	}

	[TestedType(typeof(CC025CProcessor))]
	class CC025CProcessorCL3Test : CC025CProcessorCL1Test
	{
		protected override string ExpectedBM_CustomsStatus => "CL3";
		protected override string ReleaseIndicatorForTest => "3";
	}

	[TestedType(typeof(CC025CProcessor))]
	class CC025CProcessorCD4Test : CC025CProcessorCL1Test
	{
		protected override string ExpectedBM_CustomsStatus => "CD4";
		protected override string ReleaseIndicatorForTest => "4";
	}
}
