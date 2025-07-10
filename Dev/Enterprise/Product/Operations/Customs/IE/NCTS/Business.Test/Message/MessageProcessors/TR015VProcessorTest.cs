using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR015V;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(TR015VProcessor))]
	class TR015VProcessorTest : NCTSDepartureMessageProcessorAbstractTest<TR015VProcessor, NCTSInboundEDIMessage, NCTSOutboundEDIMessage, TR015VProvider>
	{
		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.TR015V;

		protected override ZString MessageText => InterchangeProcessorTestHelper.GetStandardTR015VText();

		protected override ZString MessageFriendlyName => "TR015V: TRANSIT PRE-LODGED DECLARATION ACKNOWLEDGMENT";

		protected override TR015VProcessor Processor => new TR015VProcessor(logger, typeof(Tr015V));

		protected override void AssertProcessResultCore(NctsDepartureMovementHeader movementHeader, NCTSInboundEDIMessage incomingMessage)
		{
			AssertEquals("BM_MessageStatus", LogicalStatusList.Codes.Accepted, movementHeader.BM_MessageStatus);
			AssertEquals("BM_CustomsStatus", NCTS5DepartureCustomsStatusList.Codes.PreLodged, movementHeader.BM_CustomsStatus);
			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000", new[] { "A Transit Pre-lodged Declaration Acknowledgment message (TR015V) has been received. Customs has acknowledged receipt of a Pre-Lodged Transit Declaration for Job B00001000" }, new string[] { "staff1@where.com" });
		}
	}
}
