using System;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC009C;
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
	[TestedType(typeof(CC009CProcessor))]
	class CC009CProcessorTest : NCTSDepartureWithGuaranteeMessageProcessorAbstractTest<CC009CProcessor, NCTSInboundEDIMessage, NCTSOutboundEDIMessage, CC009CProvider>
	{
		public void TestGuaranteeTransactionNewTransactionAdded()
		{
			var (nctsHeader, _, _, incomingMessage) = CreateSetupData();
			var trans = cusGuaranteeHeader.GetTransactions();
			trans.Last().CPL_TransactionStatus = Customs.Business.PermitTransactionStatusList.Codes.Confirmed;
			var movementHeader = nctsHeader.MovementHeader;
			AssertTransaction(movementHeader, 2, -25000m, -25000m, Customs.Business.PermitTransactionStatusList.Codes.Confirmed);

			using (incomingMessage.Factory.AddDisposableService())
			{
				ResponseDetail responseDetail = ResponseMessageDetails.GetResponseDetail(incomingMessage.EM_ApplicationCode, incomingMessage.EM_MessageType, incomingMessage.EM_MessageSubType);
				Type processorType = responseDetail.ProcessorType;
				var val2 = (CC009CProcessor)Activator.CreateInstance(processorType, logger, responseDetail.XmlObjectType);
				val2.PreProcessMessage(incomingMessage);
				val2.ProcessMessage(incomingMessage);
				AssertTransaction(movementHeader, 3, 25000m, -0m, Customs.Business.PermitTransactionStatusList.Codes.Confirmed);
			}
		}

		public void TestGuaranteeTransactionStatusNotUpdated()
		{
			var messageText = InterchangeProcessorTestHelper.GetMailboxItemText("5B625BFB-BF2A-491C-8C23-BDBB2DECA438", InterchangeProcessorTestHelper.GetStandardCC009CText(CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.tcl.Flag.Item0), "ce45c655-c780-43be-94f8-69ef936ea871", includeResponseWrap: false);
			var (nctsHeader, _, _, incomingMessage) = CreateSetupData(messageText);
			var movementHeader = nctsHeader.MovementHeader;
			using (incomingMessage.Factory.AddDisposableService())
			{
				ResponseDetail responseDetail = ResponseMessageDetails.GetResponseDetail(incomingMessage.EM_ApplicationCode, incomingMessage.EM_MessageType, incomingMessage.EM_MessageSubType);
				Type processorType = responseDetail.ProcessorType;
				var val2 = (CC009CProcessor)Activator.CreateInstance(processorType, logger, responseDetail.XmlObjectType);
				val2.PreProcessMessage(incomingMessage);
				val2.ProcessMessage(incomingMessage);
				AssertTransaction(movementHeader, 2, -25000m, -25000m, Customs.Business.PermitTransactionStatusList.Codes.Pending);
			}
		}

		protected override void AssertProcessResultCore(NctsDepartureMovementHeader movementHeader, NCTSInboundEDIMessage incomingMessage)
		{
			AssertEquals("BM_MessageStatus", LogicalStatusList.Codes.Accepted, movementHeader.BM_MessageStatus);
			AssertEquals("BM_CustomsStatus", NCTS5DepartureCustomsStatusList.Codes.Cancelled, movementHeader.BM_CustomsStatus);
			AssertTransaction(movementHeader, 2, -25000m, -25000m, Customs.Business.PermitTransactionStatusList.Codes.Deleted);
			AssertMessageInterpretation(incomingMessage, @"
				An Invalidation Decision Message (IE009) has been received for Job B00001000.<br />
				<br />
				<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
					<tr><td>MRN</td><td>19AA12345678901230</td></tr>
					<tr><td>Request Date &amp; Time</td><td>30-Jan-23 15:30</td></tr>
					<tr><td>Decision Date &amp; Time</td><td>30-Jan-23 16:00</td></tr>
					<tr><td>Decision</td><td>Invalidated</td></tr>
					<tr><td>Initiated by Customs</td><td>Y</td></tr>
					<tr><td>Justification</td><td>Reason for decision</td></tr>
				</table>");
			MessageProcessorNotificationTestHelper.AssertEmail(
				incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "An Invalidation Decision Message (IE009) has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}

		protected override ZString MessageFriendlyName => "CC009C: INVALIDATION DECISION";

		protected override CC009CProcessor Processor => new CC009CProcessor(logger, typeof(Cc009CType));

		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE009;

		protected override ZString MessageText => InterchangeProcessorTestHelper.GetStandardCC009CText();
	}
}
