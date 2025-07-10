using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC509C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.ExitControl.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	[TestedType(typeof(CC509CProcessor))]
	class CC509CProcessorTest : EntryHeaderMessageProcessorTest<CC509CProcessor, AESInboundEDIMessage, AESOutboundEDIMessage, CC509CProvider>
	{
		public void TestProcessDeclarationMessageAfterExitReport()
		{
			var declaration = Factory.New<JobDeclaration>();
			var exitHeader = Factory.New<CusExitHeader>();
			exitHeader.Parent = declaration;
			declaration.RegisterEditableChildObject(exitHeader);
			var (entry, incomingMessage) = CreateSetupDataForExitReport(declaration, exitHeader);

			using (incomingMessage.Factory.AddDisposableService())
			{
				var processor = Processor;
				processor.PreProcessMessage(incomingMessage);
				AssertNoExceptionThrown(() => processor.ProcessMessage(incomingMessage));
				CombineAssertions("Process", () =>
				{
					AssertProcessResultCore(entry, incomingMessage);
				});
			}
		}

		public void TestProcessShipmentMessageAfterExitReport()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			var exitHeader = Factory.New<CusExitHeader>();
			exitHeader.Parent = shipment;
			var (entry, incomingMessage) = CreateSetupDataForExitReport(declaration, exitHeader);

			using (incomingMessage.Factory.AddDisposableService())
			{
				var processor = Processor;
				processor.PreProcessMessage(incomingMessage);
				AssertNoExceptionThrown(() => processor.ProcessMessage(incomingMessage));
				CombineAssertions("Process", () =>
				{
					AssertProcessResultCore(entry, incomingMessage);
				});
			}
		}

		public void TestProcessMessage_InvalidationInitiatedByCustoms()
		{
			(var _, var entry, var _, var incomingMessage) = CreateInvalidationInitiatedByCustomsData();
			using (incomingMessage.Factory.AddDisposableService())
			{
				var processor = Processor;
				processor.PreProcessMessage(incomingMessage);
				AssertNoExceptionThrown(() => processor.ProcessMessage(incomingMessage));
				CombineAssertions("Process", () =>
				{
					AssertProcessResult(entry, incomingMessage);
				});
			}
		}

		protected override void AssertProcessResultCore(CusEntryHeader entry, AESInboundEDIMessage incomingMessage)
		{
			AssertEquals("Accepted message, expected Entry Status: Cancelled", AESEntryStatusList.Codes.Cancelled, entry.CH_EntryStatus);
			AssertEquals("Accepted message, expected Logical Status: Accepted", LogicalStatusList.Codes.Accepted, entry.CH_Status);

			MessageProcessorNotificationTestHelper.AssertEmail(
				incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "An Invalidation Decision message has been received from Customs for Job B00001000 through the IE509 message." },
				new string[] { "staff1@where.com" });
		}

		protected override ZString MessageFriendlyName => "CC509C: EXPORT INVALIDATION DECISION";

		protected override CC509CProcessor Processor => new CC509CProcessor(logger, typeof(Cc509C));

		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE509;

		protected override ZString MessageText => AESInterchangeProcessorTestHelper.GetStandardCC509CText("LRN123456789", "21IEDUB11A782454R2", "0");

		(JobDeclaration declaration, CusEntryHeader entry, EDIMessage outgoingMessage, AESInboundEDIMessage incomingMessage) CreateInvalidationInitiatedByCustomsData()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = Branch.PK;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var incomingMessage = CreateAcceptedIncomingMessgae();
			var outgoingMessage = Factory.New<AESOutboundEDIMessage>();
			outgoingMessage.EM_ApplicationReference = TransactionID;
			outgoingMessage.EM_Status = EDIMessage.Status.Acknowledged;
			outgoingMessage.EM_GB = Branch.PK;
			entry.Messages.Add(outgoingMessage);
			var stuff = MessageProcessorNotificationTestHelper.SetupStaffData(Factory);
			outgoingMessage.EM_SystemCreateUser = stuff.GS_Code;
			Factory.Save();
			return (declaration, entry, outgoingMessage, incomingMessage);
		}

		AESInboundEDIMessage CreateAcceptedIncomingMessgae()
		{
			var message = Factory.New<AESInboundEDIMessage>();
			message.EM_ApplicationReference = TransactionID;
			message.EM_MessageType = MessageType;
			message.EM_MessageText = AESInterchangeProcessorTestHelper.GetStandardAESCC509CMailboxItemText(TransactionID, "1", includeResponseWrap: false);
			return message;
		}
	}
}
