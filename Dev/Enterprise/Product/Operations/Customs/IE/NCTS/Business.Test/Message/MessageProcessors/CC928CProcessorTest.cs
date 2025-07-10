using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC928C;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC928CProcessor))]
	class CC928CProcessorAdditionalDeclarationTypeDTest : NCTSDepartureMessageProcessorAbstractTest<CC928CProcessor, NCTSInboundEDIMessage, NCTSOutboundEDIMessage, CC928CProvider>
	{
		protected virtual string BM_AdditionalDeclarationType => NctsTypeOfAdditionalDeclarationList.Codes.D;

		protected virtual string ExpectedBM_CustomsStatus => NCTS5DepartureCustomsStatusList.Codes.PreLodged;

		protected override void AssertProcessResultCore(NctsDepartureMovementHeader movementHeader, NCTSInboundEDIMessage incomingMessage)
		{
			AssertEquals("BM_MessageStatus", LogicalStatusList.Codes.Accepted, movementHeader.BM_MessageStatus);
			AssertEquals("BM_CustomsStatus", ExpectedBM_CustomsStatus, movementHeader.BM_CustomsStatus);
			AssertMessageInterpretation(incomingMessage, $@"A Positive Acknowledgement message has been received from Customs for Job B00001000 through the IE928 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN123456789</td></tr><tr><td>Reference Number</td><td>RNALPHN8</td></tr></table>");
			MessageProcessorNotificationTestHelper.AssertEmail(
				incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Positive Acknowledgement message has been received from Customs for Job B00001000 through the IE928 message." },
				new string[] { "staff1@where.com" });
		}

		protected override ZString MessageFriendlyName => "CC928C: POSITIVE ACKNOWLEDGE";

		protected override CC928CProcessor Processor => new CC928CProcessor(logger, typeof(Cc928CType));

		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE928;

		protected override ZString MessageText => InterchangeProcessorTestHelper.GetStandardCC928CText("LRN123456789", "RNALPHN8");

		protected override (NctsHeader declaration, NctsDepartureMovementHeader messageAttachee, EDIMessage outgoingMessage, NCTSInboundEDIMessage incomingMessage) CreateSetupData()
		{
			var result = base.CreateSetupData();
			var header = result.messageAttachee;
			header.BM_AdditionalDeclarationType = BM_AdditionalDeclarationType;
			return result;
		}
	}

	[TestedType(typeof(CC928CProcessor))]
	class CC928CProcessorAdditionalDeclarationTypeATest : CC928CProcessorAdditionalDeclarationTypeDTest
	{
		protected override string BM_AdditionalDeclarationType => NctsTypeOfAdditionalDeclarationList.Codes.A;
		protected override string ExpectedBM_CustomsStatus => NCTS5DepartureCustomsStatusList.Codes.Acknowledged;
	}
}
