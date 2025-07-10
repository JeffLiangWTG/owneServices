using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC029C;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC029CProcessor))]
	class CC029CProcessorTest : NCTSDepartureWithGuaranteeMessageProcessorAbstractTest<CC029CProcessor, NCTSInboundEDIMessage, NCTSOutboundEDIMessage, CC029CProvider>
	{
		protected override void AssertProcessResultCore(NctsDepartureMovementHeader messageAttachee, NCTSInboundEDIMessage incomingMessage)
		{
			AssertEquals("BM_MessageStatus", LogicalStatusList.Codes.Accepted, messageAttachee.BM_MessageStatus);
			AssertEquals("BM_CustomsStatus", NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit, messageAttachee.BM_CustomsStatus);
			AssertEquals("Guarantee count", 1, messageAttachee.Header.MovementHeader.Guarantees.Count);
			AssertTransaction(messageAttachee, 2, -25000m, -25000m, Customs.Business.PermitTransactionStatusList.Codes.Confirmed);
			AssertEquals("Release Date", new ZDateTime(2023, 7, 21), messageAttachee.Header.MovementReferenceEntryNumber.CE_IssueDate);
		}

		protected override ZString MessageFriendlyName => "CC029C: RELEASED FOR TRANSIT";

		protected override CC029CProcessor Processor => new CC029CProcessor(logger, typeof(Cc029CType));

		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE029;

		protected override ZString MessageText => InterchangeProcessorTestHelper.GetStandardCC029CText(mrn: "21IEDUB11A782454R2", lrn: "LRN123456789");
	}
}
