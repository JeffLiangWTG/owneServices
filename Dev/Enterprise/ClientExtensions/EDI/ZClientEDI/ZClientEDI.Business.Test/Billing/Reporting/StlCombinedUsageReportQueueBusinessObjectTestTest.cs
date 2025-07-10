using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Test;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Environment;
using Enterprise.MailManager.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(StlCombinedUsageReportQueue))]
	public class StlCombinedUsageReportQueueBusinessObjectTestTest : EnterpriseBusinessObjectTestCase
	{
	}

	public class StlCombinedUsageReportQueueTest : EServicesBillingSystemTestCase
	{
		public void TestRequestReport()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var org = BillingTestHelper.CreateOrganisation(Factory, "DDD", "EDF", "SYD");
			var contact = org.Contacts[0];
			Factory.Save();

			var result = StlCombinedUsageReportQueue.RequestReport(ZGuid.Empty, ZGuid.Empty, ZGuid.Empty, 0, "");
			AssertEquals(false, result);

			result = StlCombinedUsageReportQueue.RequestReport(org.PK, contact.PK, lic.Database.PK, 201701, "");
			AssertEquals(true, result);

			result = StlCombinedUsageReportQueue.RequestReport(org.PK, contact.PK, lic.Database.PK, 201701, "");
			AssertEquals(true, result);

			result = StlCombinedUsageReportQueue.RequestReport(org.PK, contact.PK, lic.Database.PK, 201701, "U01");
			AssertEquals(true, result);

			result = StlCombinedUsageReportQueue.RequestReport(org.PK, contact.PK, lic.Database.PK, 201701, "U01");
			AssertEquals(true, result);

			var queues = Factory.Load<StlCombinedUsageReportQueue>(new ZQuery());
			AssertEquals(2, queues.Length);
		}

		public void TestGenerateReport()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var org = BillingTestHelper.CreateOrganisation(Factory, "DDD", "EDF", "SYD");
			var contact = org.Contacts[0];
			Factory.Save();

			var db = org.LicCompany.ActiveOrAllLicDatabases[0];
			db.LD_DatabaseNumber = 4809;
			var clientCompany = ClientCompany.FindOrCreate(Factory, "ABC", db.PK, org.PK, "", "");
			Factory.Save();

			var clientNumber = db.DatabaseId + ".ABC";

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("E2E", "E2E", new ZDateTime(2015, 11, 1, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "ref1a", "ref2a", "ref3a", "ref4a", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("E2E", "E2E", new ZDateTime(2015, 11, 2, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "ref1b", "ref2b", "ref3b", "ref4b", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("E2E", "E2E", new ZDateTime(2015, 11, 3, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "ref1c", "ref2c", "ref3c", "ref4c", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("E2E", "E2E", new ZDateTime(2015, 11, 30, 0, 0, 0), "DDDABCSYD", null, db.DatabaseId, clientCompany.PK, "", null, null, null, DateTime.Now, "HUB", 1));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var result = StlCombinedUsageReportQueue.RequestReport(org.PK, contact.PK, lic.Database.PK, 201511, "");
			AssertEquals(true, result);

			var queue = Factory.LoadTop1<StlCombinedUsageReportQueueForTest>(new ZQuery());

			using (var tempDir = new TempDirectory())
			{
				AssertEquals("NEW", queue.ERQ_Status);

				EDIDataRegistry.Instance.MyAccountReportsShareFolderPath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tempDir.DirectoryName);
				queue.GenerateReport();

				AssertEquals("PCD", queue.ERQ_Status);
				AssertEquals("201511_SYD_StlCombinedUsageReport", queue.ERQ_ReportName);
				AssertEquals(Path.Combine(tempDir.DirectoryName, queue.PK.ToString() + ".zip"), queue.ERQ_ReportFileFullName);
				AssertEquals(true, File.Exists(queue.ERQ_ReportFileFullName));

				var mailItems = new StandardMailItemCollection(Factory);
				mailItems.Load();

				AssertEquals("One email should be sent.", 1, mailItems.Count);
				AssertEquals("Email.MI_Status", MailManager.MailStatus.Queued, mailItems[0].MI_Status);
				AssertEquals("Email.MI_Subject", "CargoWise My Account - Report Download Notification", mailItems[0].MI_Subject);
				AssertEquals("Email.MI_Body", $"The Report is ready for download.<br/>Download Link:<a href='https://myaccount-portal.cargowise.com/myaccount/Download.aspx?report={queue.PK.ToString()}'>{queue.ERQ_ReportName}</a>", mailItems[0].MI_Body);

				AssertEquals("Email should have 1 recipients.", 1, mailItems[0].MailRecipients.Count);
				AssertEquals("First Recipient Address", "DDD@test.com", mailItems[0].MailRecipients[0].EmailAddress);
			}

			using (var ms = new MemoryStream())
			using (var sw = new StreamWriter(ms))
			{
				queue.GenerateReport_Exposed(sw);
				sw.Flush();
				AssertEquals(@"Usage Time(UTC),Company,Branch,Staff,Reference,Price Code,Price Item Description,Unit Count,Adjusted Unit Count
""01-Nov-21 10:01:01"",""CO1"",""BR1"",""US1"",""Ref#1"",""USR"",""User Price Desc"",""99"","""",""""
""02-Nov-21 11:01:01"",""CO2"",""BR2"",""US2"",""Ref#2"",""WTU"",""Transit Warehouse"",""1"",""0.5"",""""
""02-Nov-21 11:01:01"",""CO2"",""BR2"",""US2"",""Ref#2"",""SMF"",""SMF Desc"",""1"","""",""TID:123""
", Encoding.UTF8.GetString(ms.ToArray()));
			}
		}

		public void TestGenerateReport_ExcludedSystemCodes()
		{
			var licence1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "PRD");
			var db1 = licence1.Database;
			var company1 = licence1.Company;
			var org1 = company1.Header;
			var contact = org1.Contacts[0];

			var prices = BillingTestHelper.CreatePriceList(company1);
			prices.L6_SystemCode = "STL";
			var priceItem = prices.Items.AddNew();
			priceItem.L7_Code = "#PD";
			priceItem.L7_Category = "STL";
			priceItem.L7_FeeType = "TRA";

			var mapping1 = BillingTestHelper.AddUsageMap(prices, "STL", "#PD", "ASC");
			mapping1.PUM_UsageCategory = "ASC";

			var mapping2 = BillingTestHelper.AddUsageMap(prices, "STL", "#PD", "AM2");
			mapping2.PUM_UsageCategory = "STL";

			BillingTestHelper.CreatePriceLink(db1, prices, new ZDateTime(2023, 1, 1));
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "AM2", new ZDateTime(2023, 6, 1), licence1.ClientCompany, 10);
			BillingTestHelper.CreateChargeableUsage(Factory, "ASC", "ASC", new ZDateTime(2023, 6, 1), licence1.ClientCompany, 20);

			Factory.Save();

			var clientNumber = db1.DatabaseId + ".ABC";
			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ASC", "ASC", new ZDateTime(2023, 6, 1, 0, 0, 0), "DDDABCPRD", clientNumber, db1.DatabaseId, ZGuid.Empty, "ASC_REF_1", "", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "AM2", new ZDateTime(2023, 6, 1, 0, 0, 0), "DDDABCPRD", clientNumber, db1.DatabaseId, ZGuid.Empty, "STL_AM2_REF1", "", "", "", null));
			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			StlCombinedUsageReportQueue.RequestReport(org1.PK, contact.PK, db1.PK, 202306, "");

			var queue = Factory.LoadTop1<StlCombinedUsageReportQueueForTest>(new ZQuery());
			queue.ShouldWriteDummyUsages = false;
			using (var ms = new MemoryStream())
			using (var sw = new StreamWriter(ms))
			{
				queue.GenerateReport_Exposed(sw);
				sw.Flush();
				AssertEquals(@"Usage Time(UTC),Company,Branch,Staff,Reference,Price Code,Price Item Description,Unit Count,Adjusted Unit Count
""01-Jun-23 00:00:00"",""   "",""B10"",""S10"",""ASC_REF_1"",""#PD"","""",""1"","""",""""
""01-Jun-23 00:00:00"",""   "",""B11"",""S11"",""STL_AM2_REF1"",""AM2"","""",""1"","""",""""
", Encoding.UTF8.GetString(ms.ToArray()));
			}
		}

		public void TestGenerateReport_GenericUsage()
		{
			var mappings = EDIDataRegistry.Instance.SystemProductMappings.Value;
			mappings.AddNew("ABC", "ABC Product", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);

			var settings = new UsageBillingSettings();
			var priceLists = settings.PriceLists;
			var priceList1 = priceLists.AddNew();
			priceList1.ProductCode = "ABC";
			priceList1.RawUsageCategory = "SAT";
			priceList1.PriceListCode = "DEF";
			priceList1.Description = "ABC - Price List #0";
			EDIDataRegistry.Instance.UsageBillingSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

			var lic = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			lic.LA_LicenceAdvStdOth = "STL";
			lic.Database.LD_Product = "ABC";
			BillingTestHelper.SetInvoicing(lic, Env.CurrentBranch.PK, "AUD");
			var org = BillingTestHelper.CreateOrganisation(Factory, "DDD", "EDF", "SYD");
			var contact = org.Contacts[0];
			Factory.Save();

			var db = org.LicCompany.ActiveOrAllLicDatabases[0];
			db.LD_DatabaseNumber = 4809;
			var clientCompany = ClientCompany.FindOrCreate(Factory, "ABC", db.PK, org.PK, "", "");
			Factory.Save();

			var clientNumber = db.DatabaseId + ".ABC";

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABC", "P01", new ZDateTime(2015, 11, 1, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "ref1a", "ref2a", "ref3a", "ref4a", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABC", "P01", new ZDateTime(2015, 11, 2, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "ref1b", "ref2b", "ref3b", "ref4b", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABC", "P01", new ZDateTime(2015, 11, 3, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "ref1c", "ref2c", "ref3c", "ref4c", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABC", "P01", new ZDateTime(2015, 11, 30, 0, 0, 0), "DDDABCSYD", null, db.DatabaseId, clientCompany.PK, "", null, null, null, DateTime.Now, "HUB", 1));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var result = StlCombinedUsageReportQueue.RequestReport(org.PK, contact.PK, lic.Database.PK, 201511, "");
			AssertEquals(true, result);

			var queue = Factory.LoadTop1<StlCombinedUsageReportQueueForTest>(new ZQuery());

			using (var tempDir = new TempDirectory())
			{
				AssertEquals("NEW", queue.ERQ_Status);

				EDIDataRegistry.Instance.MyAccountReportsShareFolderPath.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tempDir.DirectoryName);
				queue.GenerateReport();

				AssertEquals("PCD", queue.ERQ_Status);
				AssertEquals("201511_SYD_StlCombinedUsageReport", queue.ERQ_ReportName);
				AssertEquals(Path.Combine(tempDir.DirectoryName, queue.PK.ToString() + ".zip"), queue.ERQ_ReportFileFullName);
				AssertEquals(true, File.Exists(queue.ERQ_ReportFileFullName));

				var mailItems = new StandardMailItemCollection(Factory);
				mailItems.Load();

				AssertEquals("One email should be sent.", 1, mailItems.Count);
				AssertEquals("Email.MI_Status", MailManager.MailStatus.Queued, mailItems[0].MI_Status);
				AssertEquals("Email.MI_Subject", "CargoWise My Account - Report Download Notification", mailItems[0].MI_Subject);
				AssertEquals("Email.MI_Body", $"The Report is ready for download.<br/>Download Link:<a href='https://myaccount-portal.cargowise.com/myaccount/Download.aspx?report={queue.PK.ToString()}'>{queue.ERQ_ReportName}</a>", mailItems[0].MI_Body);

				AssertEquals("Email should have 1 recipients.", 1, mailItems[0].MailRecipients.Count);
				AssertEquals("First Recipient Address", "DDD@test.com", mailItems[0].MailRecipients[0].EmailAddress);
			}

			using (var ms = new MemoryStream())
			using (var sw = new StreamWriter(ms))
			{
				queue.GenerateReport_Exposed(sw);
				sw.Flush();
				AssertEquals(@"Usage Time(UTC),Company,Branch,Staff,Reference,Price Code,Price Item Description,Unit Count,Adjusted Unit Count
""01-Nov-21 10:01:01"",""CO1"",""BR1"",""US1"",""Ref#1"",""USR"",""User Price Desc"",""99"","""",""""
""02-Nov-21 11:01:01"",""CO2"",""BR2"",""US2"",""Ref#2"",""WTU"",""Transit Warehouse"",""1"",""0.5"",""""
""02-Nov-21 11:01:01"",""CO2"",""BR2"",""US2"",""Ref#2"",""SMF"",""SMF Desc"",""1"","""",""TID:123""
", Encoding.UTF8.GetString(ms.ToArray()));
			}
		}

		[TestDate(2024, 4, 1)]
		public void TestGenerateReport_FailSafe()
		{
			EDIDataRegistry.Instance.EDIERouterUsageDBServerName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "localhost");
			EDIDataRegistry.Instance.EDIERouterUsageDBName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "~~!@#");
			var lic = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var org = BillingTestHelper.CreateOrganisation(Factory, "DDD", "EDF", "SYD");
			var contact = org.Contacts[0];
			var db = org.LicCompany.ActiveOrAllLicDatabases[0];
			db.LD_DatabaseNumber = 4809;
			Factory.Save();

			Db.Connection.ExecuteNonQuery("DROP FUNCTION EdiGetStlUsageSummary;");

			Db.Connection.ExecuteNonQuery($@"
CREATE FUNCTION EdiGetStlUsageSummary(@OrgPk UNIQUEIDENTIFIER, @PeriodStart DATETIME)  
RETURNS @Result TABLE  
(  
 LD_PK UNIQUEIDENTIFIER NOT NULL,  
 LD_ServerCode VARCHAR(3) NOT NULL,  
 LE_EnterpriseCode VARCHAR(3) NOT NULL,  
 L7_PK UNIQUEIDENTIFIER NOT NULL,  
 L7_Description NVARCHAR(500) NOT NULL,  
 L7_Order SMALLINT NOT NULL,  
 LCC_PK UNIQUEIDENTIFIER,  
 LCC_Code VARCHAR(3) NOT NULL,  
 U1_Code VARCHAR(3) NOT NULL,  
 U1_UnitCount INT NOT NULL  
)  
BEGIN  
	INSERT INTO @Result VALUES
	('{db.PK}', 'SYD', 'ENT', 'c128bad9-e6fa-4b01-81aa-61e08457335f', 'DESC', 0, 'c128bad9-e6fa-4b01-81aa-61e08457335f', 'CO1', 'IQM', 1);
	RETURN;
END

");
			var result = StlCombinedUsageReportQueue.RequestReport(org.PK, contact.PK, lic.Database.PK, 201511, "");
			AssertEquals(true, result);

			var queue = Factory.LoadTop1<StlCombinedUsageReportQueueForTest>(new ZQuery());

			AssertExceptionThrown("the caller should retry after exception in 1 hour", typeof(InvalidOperationException),
				"You have tried to establish a DB Connection using a generic machine name", () =>
			{
				using (var ms = new MemoryStream())
				using (var sw = new StreamWriter(ms))
				{
					queue.GenerateReport_Exposed(sw);
				}
			}, assertStartsWith: true);

			//just log the error after 1 hour
			queue.ERQ_CreateTimeUtc = ZDateTime.UtcNow.AddHours(-1).AddMinutes(-5);
			Factory.Save();
			using (var ms = new MemoryStream())
			using (var sw = new StreamWriter(ms))
			{
				queue.GenerateReport_Exposed(sw);
				sw.Flush();
				AssertEquals(@"Usage Time(UTC),Company,Branch,Staff,Reference,Price Code,Price Item Description,Unit Count,Adjusted Unit Count
""01-Apr-24 00:00:00"","""","""","""",""The report generation process has encountered errors, potentially resulting in incomplete data within the report. Please reach out to customer service for further information."","""","""",""0"","""",""""
""01-Nov-21 10:01:01"",""CO1"",""BR1"",""US1"",""Ref#1"",""USR"",""User Price Desc"",""99"","""",""""
""02-Nov-21 11:01:01"",""CO2"",""BR2"",""US2"",""Ref#2"",""WTU"",""Transit Warehouse"",""1"",""0.5"",""""
""02-Nov-21 11:01:01"",""CO2"",""BR2"",""US2"",""Ref#2"",""SMF"",""SMF Desc"",""1"","""",""TID:123""
", Encoding.UTF8.GetString(ms.ToArray()));
			}
			AssertEquals("StlCombinedUsageReportQueue.GenerateReportCore", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		class StlCombinedUsageReportQueueForTest : StlCombinedUsageReportQueue
		{
			public StlCombinedUsageReportQueueForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public void GenerateReport_Exposed(StreamWriter streamWriter)
			{
				base.GenerateReportCore(streamWriter);
				if (ShouldWriteDummyUsages)
				{
					var reporter = new Reporter(streamWriter);
					reporter.WriteCsvUsageReport(new ZDateTime(2021, 11, 1, 10, 1, 1), "CO1", "BR1", "US1", "Ref#1", "USR", "User Price Desc", 99);
					reporter.WriteCsvUsageReport(new ZDateTime(2021, 11, 2, 11, 1, 1), "CO2", "BR2", "US2", "Ref#2", "WTU", "Transit Warehouse", 1, 0.5);
					reporter.WriteCsvUsageReport(new ZDateTime(2021, 11, 2, 11, 1, 1), "CO2", "BR2", "US2", "Ref#2", "SMF", "SMF Desc", 1, null, "TID:123");
				}
			}

			class Reporter : ReportWriter
			{
				public Reporter(StreamWriter streamWriter) : base(streamWriter)
				{
				}
			}

			public bool ShouldWriteDummyUsages = true;
		}
	}
}
