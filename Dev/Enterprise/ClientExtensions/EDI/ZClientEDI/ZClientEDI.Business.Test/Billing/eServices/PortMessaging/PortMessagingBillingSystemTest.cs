using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Licencing.Business;

namespace Enterprise.Client.EDI.Billing.Test
{
	public class PortMessagingBillingSystemTest : EServicesBillingSystemTestCase
	{
		public void TestSystemCode()
		{
			var billingSystem = new PortMessagingBillingSystem();
			AssertEquals(BillingConstants.BillingSystem.PortMessaging, billingSystem.SystemCode);
		}

		public void TestCreateSystemBill()
		{
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.PortMessaging, "PM1", new ZDateTime(2015, 4, 30), ZGuid.Empty, 10);
			Factory.Save();

			var billingSystem = new PortMessagingBillingSystem();
			var context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2015, 4, 30));

			var bill = billingSystem.LoadSystemBills(context).First() as PortMessagingBill;
			AssertEquals(BillingConstants.BillingSystem.PortMessaging, bill.SystemCode);
		}

		public void TestLoadOdplRawUsage()
		{
			var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var org2 = BillingTestHelper.CreateOrganisation(Factory, "FFF", "III", "BRN");

			var db1 = org1.LicCompany.ActiveOrAllLicDatabases[0];
			db1.LD_DatabaseNumber = 342;
			var clientCompany1 = ClientCompany.FindOrCreate(Factory, "ABC", db1.PK, org1.PK, "", "");

			var db2 = org2.LicCompany.ActiveOrAllLicDatabases[0];
			db2.LD_DatabaseNumber = 5555;
			var clientCompany2 = ClientCompany.FindOrCreate(Factory, "ABC", db2.PK, org2.PK, "", "");

			Factory.Save();

			var clientNumber1 = db1.DatabaseId + ".ABC";
			var clientNumber2 = db2.DatabaseId + ".III";

			var infoList1 = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList1.Add(new EServicesBillingTestHelper.RawUsageInfo("PMG", "PM1", new ZDateTime(2015, 7, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "B00003001", "C00000001", "", "", null));
			infoList1.Add(new EServicesBillingTestHelper.RawUsageInfo("PMG", "PM1", new ZDateTime(2015, 6, 5, 12, 0, 0), "FFFIIIBRN", clientNumber2, db1.DatabaseId, clientCompany1.PK, "A00001001", "C00000001", "", "", null));

			var infoList2 = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList2.Add(new EServicesBillingTestHelper.RawUsageInfo("PMG", "PM2", new ZDateTime(2015, 7, 2, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "S00003001", "C00000023", "", "", null));
			infoList2.Add(new EServicesBillingTestHelper.RawUsageInfo("PMG", "PM2", new ZDateTime(2015, 7, 3, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "E00003001", "C00000055", "", "", null));
			infoList2.Add(new EServicesBillingTestHelper.RawUsageInfo("PMG", "PM2", new ZDateTime(2015, 7, 1, 10, 0, 0), "FFFIIIBRN", clientNumber2, db2.DatabaseId, clientCompany2.PK, "B00003001", "C00000085", "", "", null));
			infoList2.Add(new EServicesBillingTestHelper.RawUsageInfo("PMG", "PM2", new ZDateTime(2015, 7, 3, 10, 0, 0), "FFFIIIBRN", clientNumber2, db2.DatabaseId, clientCompany2.PK, "B00003001", "C00000085", "", "", null));
			infoList2.Add(new EServicesBillingTestHelper.RawUsageInfo("PMG", "PM2", new ZDateTime(2015, 7, 30, 0, 0, 0), "FFFIIIBRN", null, db2.DatabaseId, clientCompany2.PK, "", null, null, null, DateTime.Now, "HUB", 1));

			var infoList3 = new List<EServicesBillingTestHelper.RawUsageInfo>();
			var infoPM3 = new EServicesBillingTestHelper.RawUsageInfo("PMG", "PM3", new ZDateTime(2015, 7, 4, 4, 0, 0), "DDDABCSYD", clientNumber2, db1.DatabaseId, clientCompany1.PK, "JOB000001", "ref2", "ref3", "ref4", null);
			infoPM3.Reference5 = "Sender1";
			infoList3.Add(infoPM3);
			infoPM3 = new EServicesBillingTestHelper.RawUsageInfo("PMG", "PM3", new ZDateTime(2015, 6, 5, 12, 0, 0), "FFFIIIBRN", clientNumber2, db1.DatabaseId, clientCompany1.PK, "JOB000002", "ref2", "ref3", "ref4", null);
			infoPM3.Reference5 = "Sender2";
			infoList3.Add(infoPM3);
			infoPM3 = new EServicesBillingTestHelper.RawUsageInfo("PMG", "PM3", new ZDateTime(2015, 7, 5, 11, 0, 0), "FFFIIIBRN", clientNumber2, db2.DatabaseId, clientCompany2.PK, "JOB000003", "ref2", "ref3", "ref4", null);
			infoPM3.Reference5 = "Sender3";
			infoList3.Add(infoPM3);

			var infoPOZList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			var infoPOZ = new EServicesBillingTestHelper.RawUsageInfo("PMG", "POZ", new ZDateTime(2015, 7, 4, 4, 0, 0), "DDDABCSYD", clientNumber2, db1.DatabaseId, clientCompany1.PK, "Consol@POZ1", "MessageType@POZ1", "SubType@POZ1", "MsgTrackID@POZ1", null);
			infoPOZList.Add(infoPOZ);

			var infoPSZList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			var infoPSZ = new EServicesBillingTestHelper.RawUsageInfo("PMG", "PSZ", new ZDateTime(2015, 7, 4, 4, 0, 0), "DDDABCSYD", clientNumber2, db1.DatabaseId, clientCompany1.PK, "Consol@PSZ1", "EventType@PSZ1", "MessageType@PSZ1", "Order@PSZ1", null);
			infoPSZ.Reference5 = "MsgTrackID@PSZ1";
			infoPSZList.Add(infoPSZ);

			var infoPMNList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			var infoPMN = new EServicesBillingTestHelper.RawUsageInfo("PMG", "PMN", new ZDateTime(2015, 7, 4, 4, 0, 0), "DDDABCSYD", clientNumber2, db1.DatabaseId, clientCompany1.PK, "Consol@PMN1", "MRN@PMN1", "Recipient@PMN1", null);
			infoPMNList.Add(infoPMN);

			var infoPSNList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			var infoPSN = new EServicesBillingTestHelper.RawUsageInfo("PMG", "PSN", new ZDateTime(2015, 7, 4, 4, 0, 0), "DDDABCSYD", clientNumber2, db1.DatabaseId, clientCompany1.PK, "Consol@PSN1", "MRN@PSN1", "Sender@PSN1", "StatusCode@PSN1", null);
			infoPSNList.Add(infoPSN);

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList1.Concat(infoList2).Concat(infoList3)
				.Concat(infoPOZList).Concat(infoPSZList).Concat(infoPMNList).Concat(infoPSNList));
			EServicesBillingTestHelper.AddTransactions(infoList1);
			EServicesBillingTestHelper.AddTransactions(infoList2);
			EServicesBillingTestHelper.AddTransactions(infoList3);
			EServicesBillingTestHelper.AddTransactions(infoPOZList);
			EServicesBillingTestHelper.AddTransactions(infoPSZList);
			EServicesBillingTestHelper.AddTransactions(infoPMNList);
			EServicesBillingTestHelper.AddTransactions(infoPSNList);

			var billingSystem = new PortMessagingBillingSystem();
			var licence1 = org1.LicCompany.LicHeadersForAllDatabases[0];
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2015, 7, 1), org1.PK, clientCompany1.PK, licence1.Company.PK, licence1.Database.PK);
			var rawUsage = billingSystem.LoadOdplRawUsage(context) as SystemCodeRawUsage;
			var summarySections = rawUsage.GetRawUsageSummarySections();
			AssertEquals(7, summarySections.Length);
			var summaryPM1 = summarySections[0];
			var summaryPM2 = summarySections[1];
			var summaryPM3 = summarySections[2];
			var summaryPMN = summarySections[3];
			var summaryPOZ = summarySections[4];
			var summaryPSN = summarySections[5];
			var summaryPSZ = summarySections[6];
			AssertEquals(1, summaryPM1.Lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("Port Messaging Usage Summary - Port Order with HDS", summaryPM1.Header.TopLevelDescription);
				AssertOdplRawUsage(summaryPM1.Lines[0], "DDDABCSYD", "", "B00003001", "C00000001");
			});
			AssertEquals(2, summaryPM2.Lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("Port Messaging Usage Summary - Other Message", summaryPM2.Header.TopLevelDescription);
				AssertOdplRawUsage(summaryPM2.Lines[0], "DDDABCSYD", "", "S00003001", "C00000023");
				AssertOdplRawUsage(summaryPM2.Lines[1], "DDDABCSYD", "", "E00003001", "C00000055");
			});

			AssertEquals(1, summaryPM3.Lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("Port Messaging Usage Summary - Status Message", summaryPM3.Header.TopLevelDescription);
				AssertOdplRawUsage(summaryPM3.Lines[0], "DDDABCSYD", "ref4", "JOB000001", "ref2");
				AssertEquals(summaryPM3.Lines[0].Column5, "Sender1");
			});
			AssertEquals(1, summaryPOZ.Lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("Port Messaging Usage Summary - Data Record / Original Entry", summaryPOZ.Header.TopLevelDescription);
				AssertSummaryLine(summaryPOZ.Header, "Client ID|Consol|Message Type|Submission Type|Message ID|Message Time (UTC)|||");
				AssertSummaryLine(summaryPOZ.Lines[0], "DDDABCSYD|Consol@POZ1|MessageType@POZ1|SubType@POZ1|MsgTrackID@POZ1|04-Jul-15 04:00|||");
			});
			AssertEquals(1, summaryPSZ.Lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("Port Messaging Usage Summary - Port Status", summaryPSZ.Header.TopLevelDescription);
				AssertSummaryLine(summaryPSZ.Header, "Client ID|Consol|Event Type|Message Type|Order|Message ID|Message Time (UTC)||");
				AssertSummaryLine(summaryPSZ.Lines[0], "DDDABCSYD|Consol@PSZ1|EventType@PSZ1|MessageType@PSZ1|Order@PSZ1|MsgTrackID@PSZ1|04-Jul-15 04:00||");
			});
			AssertEquals(1, summaryPMN.Lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("Port Messaging Usage Summary - Data Record / Original Entry", summaryPMN.Header.TopLevelDescription);
				AssertSummaryLine(summaryPMN.Header, "Client ID|Consol|MRN|Service Provider/Recipient|Message Time (UTC)||||");
				AssertSummaryLine(summaryPMN.Lines[0], "DDDABCSYD|Consol@PMN1|MRN@PMN1|Recipient@PMN1|04-Jul-15 04:00||||");
			});
			AssertEquals(1, summaryPSN.Lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("Port Messaging Usage Summary - Port Status", summaryPSN.Header.TopLevelDescription);
				AssertSummaryLine(summaryPSN.Header, "Client ID|Consol|MRN|Service Provider/Sender|Status Code|Message Time (UTC)|||");
				AssertSummaryLine(summaryPSN.Lines[0], "DDDABCSYD|Consol@PSN1|MRN@PSN1|Sender@PSN1|StatusCode@PSN1|04-Jul-15 04:00|||");
			});

			var licence2 = org2.LicCompany.LicHeadersForAllDatabases[0];
			context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2015, 7, 1), org2.PK, clientCompany2.PK, licence2.Company.PK, licence2.Database.PK);
			rawUsage = billingSystem.LoadOdplRawUsage(context) as SystemCodeRawUsage;
			summarySections = rawUsage.GetRawUsageSummarySections();
			AssertEquals(2, summarySections.Length);
			summaryPM2 = summarySections[0];
			summaryPM3 = summarySections[1];
			AssertEquals(3, summaryPM2.Lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("Port Messaging Usage Summary - Other Message", summaryPM2.Header.TopLevelDescription);
				AssertOdplRawUsage(summaryPM2.Lines[0], "FFFIIIBRN", "", "B00003001", "C00000085");
				AssertOdplRawUsage(summaryPM2.Lines[1], "FFFIIIBRN", "", "B00003001", "C00000085");
				AssertOdplRawUsage(summaryPM2.Lines[2], "FFFIIIBRN", "", "", "");
			});
			AssertEquals(1, summaryPM3.Lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("Port Messaging Usage Summary - Status Message", summaryPM3.Header.TopLevelDescription);
				AssertOdplRawUsage(summaryPM3.Lines[0], "FFFIIIBRN", "ref4", "JOB000003", "ref2");
				AssertEquals(summaryPM3.Lines[0].Column5, "Sender3");
			});

			string expectedCsvResult =
@"""Type"",""Client ID"",""Consol"",""Shipment"",""Ref 3"",""Message Id"",""Service Provider"",""Message Time (UTC)""
""Other Message"",""FFFIIIBRN"",""B00003001"",""C00000085"","""","""","""",""01-Jul-15 10:00""
""Other Message"",""FFFIIIBRN"",""B00003001"",""C00000085"","""","""","""",""03-Jul-15 10:00""
""Other Message"",""FFFIIIBRN"","""","""","""","""","""",""30-Jul-15 00:00""
""Type"",""Client ID"",""Job Number"",""Z or B-Number"",""Ref 3"",""Message Id"",""Sender"",""Message Time (UTC)""
""Status Message"",""FFFIIIBRN"",""JOB000003"",""ref2"",""ref3"",""ref4"",""Sender3"",""05-Jul-15 11:00""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, false, writer);
			AssertEquals(@"""01-Jul-15 10:00"",""FFFIIIBRN"",""B14"",""S14"",""Other Message B00003001 C00000085"",""PM2"","""",""1""
""03-Jul-15 10:00"",""FFFIIIBRN"",""B15"",""S15"",""Other Message B00003001 C00000085"",""PM2"","""",""1""
""30-Jul-15 00:00"",""FFFIIIBRN"",""B16"",""S16"",""Other Message"",""PM2"","""",""1""
""05-Jul-15 11:00"",""FFFIIIBRN"",""B19"",""S19"",""Status Message JOB000003 ref2 ref3 ref4 Sender3"",""PM3"","""",""1""
", writer.ToString());
		}

		void AssertOdplRawUsage(SummaryLine summaryLine, string clientId, string messageType, string consolId, string shipmentId)
		{
			AssertEquals("Client ID", clientId, summaryLine.Column1);
			AssertEquals("Consol ID", consolId, summaryLine.Column2);
			AssertEquals("Shipment ID", shipmentId, summaryLine.Column3);
			AssertEquals("Message Type", messageType, summaryLine.Column4);
		}

		void AssertSummaryLine(SummaryLine summaryLine, string expectedText)
		{
			var text = string.Join("|", new[] { summaryLine.Column1, summaryLine.Column2, summaryLine.Column3, summaryLine.Column4, summaryLine.Column5, summaryLine.Column6, summaryLine.Column7, summaryLine.Column8, summaryLine.Column9 });
			AssertEquals(expectedText, text);
		}
	}
}