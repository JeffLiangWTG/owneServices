using System;
using System.Text;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Billing.Test;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Hosting.Test
{
	[TestedType(typeof(EdiRemoteDevicesChargeableUsageProvider))]
	public class EdiRemoteDevicesChargeableUsageProviderTest : ExternalChargeableUsageProviderTestCase<EdiRemoteDevicesChargeableUsageProvider>
	{
		public override void TestBulkCopyUsages() => Assert(true);

		[UseSnapshotProtection]
		public override void TestLoadRawUsage()
		{
			TestCaseHelper.RunClientDbCreateScripts();

			var factory = new BusinessObjectFactory(Db.Connection);
			var licHeader1 = BillingTestHelper.CreateLicence(factory, "CCE", "CCO", "PRD");
			var clientCompany1 = BillingTestHelper.CreateClientCompany(licHeader1.Database, "CO1");
			var clientCompany2 = BillingTestHelper.CreateClientCompany(licHeader1.Database, "CO2");
			licHeader1.Database.LD_HostDBName = "OdysseyABCPRD";

			RemoteDevicesUsageTestHelper.CreateUsageTable();
			RemoteDevicesUsageTestHelper.InsertUsageTable(new DateTime(2000, 1, 1), "OdysseyCLYPRD", "DisplayName1","queueName1", "ServerName1");
			RemoteDevicesUsageTestHelper.InsertUsageTable(new DateTime(2000, 2, 1), "OdysseyABCPRD", "DisplayName2", "queueName2", "ServerName2");
			RemoteDevicesUsageTestHelper.InsertUsageTable(new DateTime(2000, 2, 1), "OdysseyCLYPRD", "DisplayName3", "queueName3", "ServerName3");
			factory.Save();

			var period = new BillingPeriod(new ZDateTime(2000, 2, 1));
			var provider = new EdiRemoteDevicesChargeableUsageProviderForTest();
			var rowsAsString = new StringBuilder();

			var context = new BillingLoadRawUsageContext(factory, new ZDateTime(2000, 2, 1), licHeader1.Company.Header.PK, ZGuid.Empty, licHeader1.Company.PK, licHeader1.Database.PK);
			provider.LoadRawUsage(context, (r) =>
			{
				rowsAsString.AppendLine($"{r["PrinterName"]}-{r["PrintServerName"]}-{r["DateCaptured"]}");
			});
			AssertEquals("DisplayName2-ServerName2-1/02/2000 12:00:00 AM\r\n", rowsAsString.ToString());
		}

		public class EdiRemoteDevicesChargeableUsageProviderForTest : EdiRemoteDevicesChargeableUsageProvider
		{
			protected override DbConnection GetNewConnection()
			{
				return Db.NewExtraConnectionToMainDb();
			}
		}

		public static class RemoteDevicesUsageTestHelper
		{
			const string TableCreationQuery = @"
CREATE TABLE [dbo].[WiseGridPrinters_202206](
	[DateCaptured] [smalldatetime] NULL,
	[DBName] [varchar](50) NOT NULL,
	[SQ_DisplayName] [nvarchar](128) NOT NULL,
	[SQ_QueueName] [nvarchar](250) NOT NULL,
	[SQ_ServerName] [varchar](128) NOT NULL,
	[Period] [int] NOT NULL
);

CREATE CLUSTERED INDEX [NR_RC__Period_DBName] ON [dbo].[WiseGridPrinters_202206]
(
	[Period] ASC,
	[DBName] ASC
) WITH (ALLOW_PAGE_LOCKS = OFF);

";
			public static void CreateUsageTable()
			{
				Db.Connection.ExecuteNonQuery(TableCreationQuery);
			}

			public static void InsertUsageTable(DateTime date, string dbName, string displayName, string queueName, string serverName)
			{
				var sql = $@"
INSERT INTO [dbo].[WiseGridPrinters_202206]
           ([DateCaptured]
           ,[DBName]
           ,[SQ_DisplayName]
           ,[SQ_QueueName]
           ,[SQ_ServerName]
		   ,[Period])
     VALUES
           ('{date.ToString("yyyy-MM-dd HH:mm:ss")}'
           ,'{dbName}'
           ,'{displayName}'
           ,'{queueName}'
           ,'{serverName}'
		   , {date.Year * 100 + date.Month})
";
				Db.Connection.ExecuteNonQuery(sql);
			}
		}
	}
}
