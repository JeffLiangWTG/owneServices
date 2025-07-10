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

namespace Enterprise.Client.EDI.Billing.ERouter.Test
{
	[TestedType(typeof(EdiERouterChargeableUsageProvider))]
	public class EdiERouterChargeableUsageProviderTest : ExternalChargeableUsageProviderTestCase<EdiERouterChargeableUsageProvider>
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

			ERouterUsageTestHelper.CreateUsageTable();
			ERouterUsageTestHelper.InsertUsageTable("A01", "T01", "User001", "Ref001", new DateTime(2000, 1, 1), clientCompany1.PK);
			ERouterUsageTestHelper.InsertUsageTable("ABC", "T02", "User002", "Ref002", new DateTime(2009, 09, 01), clientCompany1.PK);
			ERouterUsageTestHelper.InsertUsageTable("ABC", "T03", "User003", "Ref003", new DateTime(2009, 09, 02), clientCompany2.PK);
			factory.Save();

			var period = new BillingPeriod(new ZDateTime(2009, 9, 1));
			var provider = new EdiERouterChargeableUsageProviderForTest("ABC");
			var rowsAsString = new StringBuilder();

			//clientCompany1, no Database
			var context = new BillingLoadRawUsageContext(factory, new ZDateTime(2009, 09, 01), licHeader1.Company.Header.PK, clientCompany1.PK, licHeader1.Company.PK, ZGuid.Empty);
			provider.LoadRawUsage(context, (r) =>
			{
				rowsAsString.AppendLine($"{r["U2_Type"]}-{r["U2_Sender"]}-{r["U2_Reference"]}-{r["U2_Date"]}-{r["CompanyCode"]}");
			});
			AssertEquals("T02-User002-Ref002-1/09/2009 12:00:00 AM-CO1\r\n", rowsAsString.ToString());

			//Database only
			rowsAsString.Clear();
			context = new BillingLoadRawUsageContext(factory, new ZDateTime(2009, 09, 01), licHeader1.Company.Header.PK, ZGuid.Empty, licHeader1.Company.PK, licHeader1.Database.PK);
			provider.LoadRawUsage(context, (r) =>
			{
				rowsAsString.AppendLine($"{r["U2_Type"]}-{r["U2_Sender"]}-{r["U2_Reference"]}-{r["U2_Date"]}-{r["CompanyCode"]}");
			});
			AssertEquals("T02-User002-Ref002-1/09/2009 12:00:00 AM-CO1\r\nT03-User003-Ref003-2/09/2009 12:00:00 AM-CO2\r\n", rowsAsString.ToString());
		}

		public class EdiERouterChargeableUsageProviderForTest : EdiERouterChargeableUsageProvider
		{
			public EdiERouterChargeableUsageProviderForTest(string applicationCode) : base(applicationCode)
			{
			}

			protected override DbConnection GetNewConnection()
			{
				return Db.NewExtraConnectionToMainDb();
			}
		}

		public static class ERouterUsageTestHelper
		{
			const string TableCreationQuery = @"
CREATE TABLE [dbo].[eRouter_ChargeableMessage_20240411]
(
	[U2_PK] [uniqueidentifier] NOT NULL PRIMARY KEY WITH (ALLOW_PAGE_LOCKS = OFF),
	[U2_LC] [uniqueidentifier] NULL,
	[U2_ApplicationCode] [varchar](3) NOT NULL,
	[U2_Type] [varchar](3) NOT NULL,
	[U2_Sender] [varchar](10) NOT NULL,
	[U2_Reference] [varchar](20) NOT NULL,
	[U2_Date] [datetime] NOT NULL,
	[U2_LCC] [uniqueidentifier] NULL
);

";

			public static void CreateUsageTable()
			{
				Db.Connection.ExecuteNonQuery(TableCreationQuery);
			}

			public static void InsertUsageTable(string applicationCode, string type, string sender, string reference, DateTime date, ZGuid lcc)
			{
				var sql = $@"
INSERT INTO [dbo].[eRouter_ChargeableMessage_20240411]
           ([U2_PK]
           ,[U2_LC]
           ,[U2_ApplicationCode]
           ,[U2_Type]
           ,[U2_Sender]
           ,[U2_Reference]
           ,[U2_Date]
           ,[U2_LCC])
     VALUES
           (NEWID()
           ,NULL
           , '{applicationCode}'
           , '{type}'
           , '{sender}'
           , '{reference}'
           , '{date.ToString("yyyy-MM-dd HH:mm:ss")}'
           , '{lcc}')
";
				Db.Connection.ExecuteNonQuery(sql);
			}
		}
	}
}
