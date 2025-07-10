using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC004C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC004CProcessor))]
	class CC004CProcessorTest : NCTSDepartureWithGuaranteeMessageProcessorAbstractTest<CC004CProcessor, NCTSInboundEDIMessage, NCTSOutboundEDIMessage, CC004CProvider>
	{
		public void TestGuaranteeTransactionStatusUpdated()
		{
			var (nctsHeader, _, _, incomingMessage) = CreateSetupData();
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested;
			using (incomingMessage.Factory.AddDisposableService())
			{
				ResponseDetail responseDetail = ResponseMessageDetails.GetResponseDetail(incomingMessage.EM_ApplicationCode, incomingMessage.EM_MessageType, incomingMessage.EM_MessageSubType);
				Type processorType = responseDetail.ProcessorType;
				var val2 = (CC004CProcessor)Activator.CreateInstance(processorType, logger, responseDetail.XmlObjectType);
				val2.PreProcessMessage(incomingMessage);
				val2.ProcessMessage(incomingMessage);
				AssertTransaction(movementHeader, 2, -25000m, -25000m, Customs.Business.PermitTransactionStatusList.Codes.Confirmed);
			}
		}

		protected override void AssertProcessResultCore(NctsDepartureMovementHeader messageAttachee, NCTSInboundEDIMessage incomingMessage)
		{
			AssertEquals("BM_MessageStatus", LogicalStatusList.Codes.Accepted, messageAttachee.BM_MessageStatus);
			AssertEquals("BM_CustomsStatus", NCTS5DepartureCustomsStatusList.Codes.MrnAllocated, messageAttachee.BM_CustomsStatus);
			AssertTransaction(messageAttachee, 2, -25000m, -25000m, Customs.Business.PermitTransactionStatusList.Codes.Pending);
			AssertMessageInterpretation(incomingMessage, @"An Amendment Acceptance message has been received from Customs for Job B00001000 through the IE004 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Amendment Submission Date and Time</td><td>18-Sep-71 00:00</td></tr><tr><td>Amendment Acceptance Date and Time</td><td>19-Sep-71 00:00</td></tr></table>");
			MessageProcessorNotificationTestHelper.AssertEmail(
				incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "An Amendment Acceptance message has been received from Customs for Job B00001000 through the IE004 message." },
				new string[] { "staff1@where.com" });
		}

		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE004;

		protected override ZString MessageFriendlyName => "CC004C: AMENDMENT ACCEPTANCE";

		protected override CC004CProcessor Processor => new CC004CProcessor(logger, typeof(Cc004CType));

		protected override ZString MessageText => InterchangeProcessorTestHelper.GetStandardCC004CText("LRN123456789", "21IEDUB11A782454R2");
	}
}
