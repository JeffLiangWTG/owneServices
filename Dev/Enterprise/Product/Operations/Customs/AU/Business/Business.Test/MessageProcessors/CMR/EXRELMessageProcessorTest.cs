using System;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class EXRELMessageProcessorTest : CMRMessageResponseProcessorTest
	{
		public void TestProcessEXRELManifestHeader()
		{
			SetupHeader();
			Factory.Save();

			AssertEquals(0, header.Messages.Count);

			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("EXRELMessage.txt")).Replace("\r\n", "");
			processor.ProcessMessage(incomingMessage);

			ZString expectedResult =
@"Reference: K00001234
CAN: AAACYXYJP

Status: CLEAR
";
			AssertNotNull(header.Logs.AddedLog);
			AssertEquals(1, header.Messages.Count);
			AssertEquals("EXL", header.Messages[0].EM_MessageType);

			AssertContains("Report", expectedResult.Replace("\r\n", "<br>"), processor.SentReport.Body);
		}

		public void TestProcessEXRELMessage()
		{
			SetupConsolAndShipments();
			Factory.Save();

			AssertEquals(0, consol.Messages.Count);
			AssertEquals(0, shipment2.Messages.Count);

			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("EXRELMessage.txt")).Replace("\r\n", "");
			processor.ProcessMessage(incomingMessage);

			AssertNotNull(shipment2.Logs.AddedLog);
			AssertEquals(1, shipment2.Messages.Count);
			AssertEquals("EXL", shipment2.Messages[0].EM_MessageType);
			AssertEquals(0, consol.Messages.Count);
			AssertEquals("Subject", "Export Consignment Release Advice (EXREL) Message for S00001234 House Bill: 88326446A4 - CLEAR", processor.SentReport.Subject);
			AssertContains("Report - Status", "Status: CLEAR", processor.SentReport.Body);
			AssertContains("Report - message", "An Export Consignment Release Advice (EXREL) message has been received from the ACS.", processor.SentReport.Body);
		}

		protected override ZString GetExpectedMessageCode() => "EXL";

		protected override ZString GetExpectedMessageName() => "Export Consignment Release Advice (EXREL)";

		protected override CMRMessageResponseProcessor GetMessageProcessor() => processor;

		protected override void SetUp()
		{
			base.SetUp();
			processor = new EXRELMessageProcessorTestHelper(logger);
		}

		protected override Type IncomingMessageType => typeof(CMREXRELMessage);

		void SetupConsolAndShipments()
		{
			consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001208";

			AUCusEntryNumber entryNum = Factory.New<AUCusEntryNumber>();
			entryNum.CE_EntryType = "XLV";
			entryNum.CE_EntryNum = "AAACYXYJP";
			entryNum.CE_ParentID = consol.PK;
			entryNum.CE_ParentTable = consol.TableName;

			shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S00001235";

			shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "S00001234";
			shipment2.JS_HouseBill = "88326446A4";
			shipment2.CustomsEntryNumber = "AAACYXYJP";
			shipment2.CustomsEntryNumberType = "EXLV";
		}

		void SetupHeader()
		{
			header = Factory.New<ExportCustomsManifestHeader>();
			header.ED_BGMReference = "K00001234";
			header.ED_CAN = "AAACYXYJP";
		}

		ExportCustomsManifestHeader header;
		ForwardingConsol consol;
		CommonShipment shipment1;
		CommonShipment shipment2;
		EXRELMessageProcessorTestHelper processor;

		sealed class EXRELMessageProcessorTestHelper : EXRELMessageProcessor
		{
			public EXRELMessageProcessorTestHelper(LoggingInformation logger) : base(logger) { }
			protected override void SendReport(EmailDef email)
			{
				SentReport = email;
			}
			public EmailDef SentReport;
		}
	}
}
