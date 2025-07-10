using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC055C;
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
	[TestedType(typeof(CC055CProcessor))]
	class CC055CProcessorTest : NCTSDepartureWithGuaranteeMessageProcessorAbstractTest<CC055CProcessor, NCTSInboundEDIMessage, NCTSOutboundEDIMessage, CC055CProvider>
	{
		public void TestGuaranteeTransactionStatus_G01()
		{
			TestGuaranteeTransactionStatus("G01", Customs.Business.PermitTransactionStatusList.Codes.Deleted);
		}

		public void TestGuaranteeTransactionStatus_G02() // tested in end to end
		{
			TestGuaranteeTransactionStatus("G02", Customs.Business.PermitTransactionStatusList.Codes.Deleted);
		}

		public void TestGuaranteeTransactionStatus_G03()
		{
			TestGuaranteeTransactionStatus("G03", Customs.Business.PermitTransactionStatusList.Codes.Pending);
		}

		public void TestGuaranteeTransactionStatus_G05()
		{
			TestGuaranteeTransactionStatus("G05", Customs.Business.PermitTransactionStatusList.Codes.Deleted);
		}

		public void TestGuaranteeTransactionStatus_G06()
		{
			TestGuaranteeTransactionStatus("G06", Customs.Business.PermitTransactionStatusList.Codes.Pending);
		}

		public void TestGuaranteeTransactionStatus_G08()
		{
			TestGuaranteeTransactionStatus("G08", Customs.Business.PermitTransactionStatusList.Codes.Pending);
		}

		public void TestGuaranteeTransactionStatus_G09()
		{
			TestGuaranteeTransactionStatus("G09", Customs.Business.PermitTransactionStatusList.Codes.Deleted);
		}

		public void TestGuaranteeTransactionStatus_G10()
		{
			TestGuaranteeTransactionStatus("G10", Customs.Business.PermitTransactionStatusList.Codes.Deleted);
		}

		public void TestGuaranteeTransactionStatus_G11()
		{
			TestGuaranteeTransactionStatus("G11", Customs.Business.PermitTransactionStatusList.Codes.Pending);
		}

		public void TestGuaranteeTransactionStatus_G12()
		{
			TestGuaranteeTransactionStatus("G12", Customs.Business.PermitTransactionStatusList.Codes.Pending);
		}

		void TestGuaranteeTransactionStatus(string code, string expectedTransactionStatus)
		{
			var messageText = InterchangeProcessorTestHelper.GetMailboxItemText("5B625BFB-BF2A-491C-8C23-BDBB2DECA438", InterchangeProcessorTestHelper.GetStandardCC055CText(invalidCode: code), "ce45c655-c780-43be-94f8-69ef936ea871", includeResponseWrap: false);
			var (nctsHeader, _, _, incomingMessage) = CreateSetupData(messageText);
			var movementHeader = nctsHeader.MovementHeader;
			using (incomingMessage.Factory.AddDisposableService())
			{
				ResponseDetail responseDetail = ResponseMessageDetails.GetResponseDetail(incomingMessage.EM_ApplicationCode, incomingMessage.EM_MessageType, incomingMessage.EM_MessageSubType);
				Type processorType = responseDetail.ProcessorType;
				var val2 = (CC055CProcessor)Activator.CreateInstance(processorType, logger, responseDetail.XmlObjectType);
				val2.PreProcessMessage(incomingMessage);
				val2.ProcessMessage(incomingMessage);
				AssertTransaction(movementHeader, 2, -25000m, -25000m, expectedTransactionStatus);
			}
		}

		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE055;

		protected override ZString MessageText => InterchangeProcessorTestHelper.GetStandardCC055CText();

		protected override ZString MessageFriendlyName => "CC055C: GUARANTEE NOT VALID";

		protected override CC055CProcessor Processor => new CC055CProcessor(logger, typeof(Cc055CType));

		protected override void AssertProcessResultCore(NctsDepartureMovementHeader movementHeader, NCTSInboundEDIMessage incomingMessage)
		{
			AssertEquals("BM_MessageStatus", LogicalStatusList.Codes.Accepted, movementHeader.BM_MessageStatus);
			AssertEquals("BM_CustomsStatus", NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid, movementHeader.BM_CustomsStatus);
			AssertTransaction(movementHeader, 2, -25000m, -25000m, Customs.Business.PermitTransactionStatusList.Codes.Deleted);
			AssertMessageInterpretation(incomingMessage, @"
			A Guarantee Not Valid Message (IE055) has been received for Job B00001000.<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>MRN</td><td>19MRNCC055C0123456</td></tr>
				<tr><td>Declaration Acceptance Date</td><td>31-Jan-23</td></tr>
			</table><br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>Sequence Number</td><td>1</td></tr>
				<tr><td>Guarantee Reference Number</td><td>12GRNCC055C012345A678901</td></tr>
				<tr><td>Invalid Guarantee Reason Sequence Number</td><td>1</td></tr>
				<tr><td>Invalid Guarantee Reason Code</td><td>G02</td></tr>
				<tr><td>Invalid Guarantee Reason Text</td><td>Guarantee exists, but not valid</td></tr>
				<tr><td>Invalid Guarantee Reason Sequence Number</td><td>2</td></tr>
				<tr><td>Invalid Guarantee Reason Code</td><td>G05</td></tr>
				<tr><td>Invalid Guarantee Reason Text</td><td>Guarantee error</td></tr>
			</table><br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>Sequence Number</td><td>2</td></tr>
				<tr><td>Guarantee Reference Number</td><td>12GRNCC055C012345A678901</td></tr>
				<tr><td>Invalid Guarantee Reason Sequence Number</td><td>1</td></tr>
				<tr><td>Invalid Guarantee Reason Code</td><td>G04</td></tr>
				<tr><td>Invalid Guarantee Reason Text</td><td>Holder of Guarantee is not equal to Holder of Transit procedure in declaration</td></tr>
			</table>");
			MessageProcessorNotificationTestHelper.AssertEmail(
				incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Guarantee Not Valid Message (IE055) has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}
	}
}
