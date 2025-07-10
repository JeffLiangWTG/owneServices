using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC529C;
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
	[TestedType(typeof(CC529CProcessor))]
	class CC529CProcessorTest : EntryHeaderMessageProcessorTest<CC529CProcessor, AESInboundEDIMessage, AESOutboundEDIMessage, CC529CProvider>
	{
		protected override void AssertProcessResultCore(CusEntryHeader entry, AESInboundEDIMessage incomingMessage)
		{
			AssertEquals("Release date should have been set.", ZDateTime.BrettsBirthday.AddDays(1), entry.CH_EntryReleaseDate);

			AssertEquals("Logical Status", LogicalStatusList.Codes.Accepted, entry.CH_Status);
			AssertEquals("Enry Status", AESEntryStatusList.Codes.ReleasedForExport, entry.CH_EntryStatus);
			var jobNumber = entry.Declaration.JE_DeclarationReference;
			var messageInterpretation = $@"A Release Response message has been received from Customs for Job {jobNumber} through the IE529 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Status</td><td>Goods Released for Export</td></tr><tr><td>LRN</td><td>LRN123456789</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Release Date</td><td>19-Sep-71</td></tr></table>";
			AssertMessageInterpretation(incomingMessage, messageInterpretation);
			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for " + jobNumber, new[] { messageInterpretation }, new string[] { "staff1@where.com" });
		}

		protected override ZString MessageFriendlyName => "CC529C: RELEASE FOR EXPORT";

		protected override CC529CProcessor Processor => new CC529CProcessor(logger, typeof(Cc529C));

		protected override ZString MessageType => AESIncomingMessageTypeList.Codes.IE529;

		protected override ZString MessageText => AESInterchangeProcessorTestHelper.GetStandardCC529CText("LRN123456789", "21IEDUB11A782454R2");

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
	}
}
