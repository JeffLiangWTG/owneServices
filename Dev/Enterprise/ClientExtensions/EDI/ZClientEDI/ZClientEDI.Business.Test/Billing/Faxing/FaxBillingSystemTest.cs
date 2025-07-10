using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;

namespace Enterprise.Client.EDI.Billing.Fax.Test
{
	public class FaxBillingSystemTest : TestCaseWithFactory
	{
		public void TestSystemCode()
		{
			FaxBillingSystem billing = new FaxBillingSystem();
			AssertEquals(BillingConstants.BillingSystem.Fax, billing.SystemCode);
		}

		public void TestCreateSystemBill()
		{
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.Fax, new ZDateTime(2010, 10, 01), ZGuid.Empty, 10);
			Factory.Save();

			FaxBillingSystem faxBilling = new FaxBillingSystem();
			BillingRunContext context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2010, 10, 31));
			AssertEquals(true, faxBilling.LoadSystemBills(context).First() is FaxSystemBill);
		}

		public void TestLoadRawUsage()
		{
			var licHeader1 = BillingTestHelper.CreateLicence(Factory, "CCE", "CCO", "PRD");
			var clientCompany2 = BillingTestHelper.CreateClientCompany(licHeader1.Database, "CO2");

			faxDbHelper.InsertFaxJobs();
			faxDbHelper.InsertFaxJob(10, new DateTime(2009, 9, 1, 2, 3, 4), "CCC Inc. (AU) <mail.edi@ccc.test>", 0.08m, "12345678", true, true, true, "CCE", "CCO", "PRD");
			faxDbHelper.InsertFaxJob(5, new DateTime(2009, 9, 2, 2, 2, 2), "CCC Inc. (AU) <mail.edi@ccc.test>", 0.07m, "22222222", true, true, true, "CCE", "CO2", "PRD");

			Factory.Save();

			FaxBillingSystem billing = new FaxBillingSystemForTest();
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2009, 09, 01), licHeader1.Company.Header.PK, licHeader1.ClientCompany.PK, licHeader1.Company.PK, licHeader1.Database.PK);

			var rawUsage = (SystemCodeRawUsage)billing.LoadOdplRawUsage(context);
			AssertEquals(licHeader1.Company.Header.OH_Code, rawUsage.OrgCode);
			AssertEquals("Fax", rawUsage.Summary.Header.Column1);
			AssertEquals("Number Of Pages", rawUsage.Summary.Header.Column2);
			AssertEquals("Sent", rawUsage.Summary.Header.Column3);
			AssertEquals(1, rawUsage.Summary.Lines.Count);
			AssertEquals("12345678", rawUsage.Summary.Lines[0].Column1);
			AssertEquals("10", rawUsage.Summary.Lines[0].Column2);
			AssertEquals("01-Sep-2009 02:09", rawUsage.Summary.Lines[0].Column3);

			var stlRawUsage = billing.LoadStlRawUsage(context);
			AssertEquals("Company Code", stlRawUsage.Summary.Header.Column1);
			AssertEquals("Fax", stlRawUsage.Summary.Header.Column2);
			AssertEquals("Number Of Pages", stlRawUsage.Summary.Header.Column3);
			AssertEquals("Sent", stlRawUsage.Summary.Header.Column9);
			AssertEquals(1, stlRawUsage.Summary.Lines.Count);
			AssertEquals("CCO", stlRawUsage.Summary.Lines[0].Column1);
			AssertEquals("12345678", stlRawUsage.Summary.Lines[0].Column2);
			AssertEquals("10", stlRawUsage.Summary.Lines[0].Column3);
			AssertEquals("01-Sep-2009 02:09", stlRawUsage.Summary.Lines[0].Column9);

			string expectedCsvResult =
@"""Fax"",""Number Of Pages"",""Sent""
""12345678"",""10"",""01-Sep-2009 02:09""
";
			var builder = new ZStringBuilder();
			billing.LoadRawUsageInCsv(context, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billing.LoadRawUsageInCsv(context, false, writer);
			AssertEquals(@"""01-Sep-09 02:09"","""","""","""",""12345678"","""","""",""10""
", writer.ToString());
		}

		#region Implementation

		FaxDbTestHelper faxDbHelper;

		#region SetUp

		protected override void OnBeforeBaseTestCaseRunBare()
		{
			base.OnBeforeBaseTestCaseRunBare();

			faxDbHelper = new FaxDbTestHelper();
			faxDbHelper.RunFaxDbCreateScripts();
		}

		#endregion

		#region TearDown

		public override void RunBare()
		{
			base.RunBare();
			faxDbHelper.DropFaxDb();
		}

		#endregion

		#endregion

		class FaxBillingSystemForTest : FaxBillingSystem
		{
			protected override EdiFaxChargeableUsageProvider GetUsageProvider()
			{
				return new EdiFaxChargeableUsageProviderForTest();
			}
		}
	}

	public class EdiFaxChargeableUsageProviderForTest : EdiFaxChargeableUsageProvider
	{
		protected override DbConnection GetNewConnection()
		{
			return Db.NewAdminConnection(FaxDbTestHelper.FaxDbName);
		}
	}

	public class FaxDbTestHelper
	{
		public const string FaxDbName = "EDIFaxDBTest";

		public void RunFaxDbCreateScripts()
		{
			using (AdminConnection adminCon = Db.NewAdminConnection())
			{
				DoSql(adminCon, @"IF  EXISTS (SELECT name FROM sys.databases WHERE name = N'" + FaxDbName + "') DROP DATABASE [" + FaxDbName + "]");
				adminCon.CreateDatabase(FaxDbName);
			}

			using (DbConnection faxCon = Db.NewAdminConnection(FaxDbName))
			{
				DoSql(faxCon,
@"CREATE TABLE [dbo].[EmailACL](
	[address] [varchar](128) NULL
);
---------------------------------------
CREATE TABLE [dbo].[EmailLicence](
	[Email] [varchar](128) NOT NULL,
	[LA] [uniqueidentifier] NULL,
PRIMARY KEY CLUSTERED 
(
	[Email] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS = OFF)
);
---------------------------------------
CREATE TABLE [dbo].[FaxJobs](
	[FaxJobId] [uniqueidentifier] NOT NULL,
	[ReceivedDateTime] [datetime] NULL,
	[Sender] [varchar](128) NULL,
	[TiffFile] [varbinary](max) NULL,
	[ChargeCode] [varchar](30) NULL,
	[PageCount] [int] NOT NULL,
	[SysId] [varchar](20) NULL,
	[SysFaxJobId] [uniqueidentifier] NULL,
	[ReportedLicenceHeader] [uniqueidentifier] NULL,
 	[EnterpriseCode] varchar(3) NOT NULL,
	[CompanyCode] varchar(3) NOT NULL,
	[ServerCode] varchar(3) NOT NULL
CONSTRAINT [PK_FaxJobs] PRIMARY KEY NONCLUSTERED 
(
	[FaxJobId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];
---------------------------------------
ALTER TABLE [dbo].[FaxJobs] ADD  CONSTRAINT [DF_FaxJobs_PageCount]  DEFAULT (0) FOR [PageCount];
ALTER TABLE [dbo].[FaxJobs] ADD  CONSTRAINT [DF_FaxJobs_EnterpriseCode]  DEFAULT ('') FOR [EnterpriseCode];
ALTER TABLE [dbo].[FaxJobs] ADD  CONSTRAINT [DF_FaxJobs_CompanyCode]  DEFAULT ('') FOR [CompanyCode];
ALTER TABLE [dbo].[FaxJobs] ADD  CONSTRAINT [DF_FaxJobs_ServerCode]  DEFAULT ('') FOR [ServerCode];
---------------------------------------
CREATE TABLE [dbo].[FaxRecipients](
	[FaxRecipientId] [uniqueidentifier] NOT NULL,
	[SentDateTime] [datetime] NULL,
	[AttentionName] [varchar](50) NOT NULL,
	[FaxNumber] [varchar](50) NOT NULL,
	[Company] [varchar](50) NOT NULL,
	[FaxJobId] [uniqueidentifier] NOT NULL,
	[IsAcknowledged] [int] NOT NULL,
	[AckDateTime] [datetime] NULL,
	[AckSuccess] [int] NULL,
	[IsAckWarningSent] [int] NOT NULL,
 CONSTRAINT [PK_FaxRecipients] PRIMARY KEY NONCLUSTERED 
(
	[FaxJobId] ASC,
	[FaxRecipientId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = OFF) ON [PRIMARY]
) ON [PRIMARY];
---------------------------------------
ALTER TABLE [dbo].[FaxRecipients] ADD  CONSTRAINT [DF_FaxRecipients_IsAckWarningSent]  DEFAULT (0) FOR [IsAckWarningSent];
---------------------------------------
CREATE TABLE [dbo].[FaxBilling](
	[ChargeCode] [uniqueidentifier] NOT NULL,
	[Pages] [int] NOT NULL,
	[Status] [bit] NOT NULL,
	[Price] [smallmoney] NOT NULL,
 CONSTRAINT [PK_FaxBilling] PRIMARY KEY CLUSTERED 
(
	[ChargeCode] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = OFF) ON [PRIMARY]
) ON [PRIMARY]
");
			}
		}

		const string TearDownSql =
@"IF  EXISTS (SELECT name FROM sys.databases WHERE name = N'" + FaxDbName + @"')
DROP DATABASE [" + FaxDbName + "]";

		public void DropFaxDb()
		{
			DoSql(TearDownSql);
		}

		void DoSql(DbConnection con, string sql)
		{
			con.ExecuteNonQuery(sql);
		}

		void DoSql(string sql)
		{
			using (DbConnection con = Db.NewAdminConnection())
			{
				con.ExecuteNonQuery(sql);
			}
		}

		class FaxJob
		{
			public FaxJob(string line)
			{
				string[] field = line.Split(new char[] { ',' });
				FaxJobId = field[0];
				ReceivedDateTime = field[1];
				Sender = field[2];
				PageCount = field[3];
				EnterpriseCode = field[4];
				CompanyCode = field[5];
				ServerCode = field[6];
			}

			public string FaxJobId { get; set; }
			public string ReceivedDateTime { get; set; }
			public string Sender { get; set; }
			public string PageCount { get; set; }
			public string EnterpriseCode { get; set; }
			public string CompanyCode { get; set; }
			public string ServerCode { get; set; }
		}

		class FaxRecipient
		{
			public FaxRecipient(FaxJob job, string line)
			{
				string[] field = line.Split(new char[] { ',' });
				FaxRecipientId = field[0];
				SentDateTime = field[1];
				FaxNumber = field[2];
				FaxJobId = job.FaxJobId;
				IsAcknowledged = field[3];
				AckSuccess = field[4];
			}

			public string FaxRecipientId { get; set; }
			public string SentDateTime { get; set; }
			public string FaxNumber { get; set; }
			public string FaxJobId { get; set; }
			public string IsAcknowledged { get; set; }
			public string AckSuccess { get; set; }
		}

		class FaxCdr
		{
			public FaxCdr(FaxRecipient recipient, string line)
			{
				string[] field = line.Split(new char[] { ',' });
				ChargeCode = recipient.FaxRecipientId;
				Pages = field[0];
				Status = field[1];
				Price = field[2];
			}

			public string ChargeCode { get; set; }
			public string Pages { get; set; }
			public string Status { get; set; }
			public string Price { get; set; }
		}

		public void InsertFaxJob(int pageCount, DateTime date, string sender, decimal price, string faxNumber, bool ackSuccess, bool isAcknowledged, bool status, string enterpriseCode, string companyCode, string serverCode)
		{
			Guid jobId = Guid.NewGuid();
			Guid recipientId = Guid.NewGuid();
			FaxJob job = new FaxJob(",,,,,,")
			{
				FaxJobId = jobId.ToString("D"),
				PageCount = pageCount.ToString(),
				ReceivedDateTime = date.ToString("yyyy-MM-dd HH:MM"),
				Sender = sender,
				EnterpriseCode = enterpriseCode,
				CompanyCode = companyCode,
				ServerCode = serverCode
			};
			FaxRecipient recipient = new FaxRecipient(job, ",,,,")
			{
				AckSuccess = ackSuccess ? "1" : "0",
				FaxNumber = faxNumber,
				FaxRecipientId = recipientId.ToString("D"),
				IsAcknowledged = isAcknowledged ? "1" : "0",
				SentDateTime = date.ToString()
			};
			FaxCdr cdr = new FaxCdr(recipient, ",,")
			{
				Pages = job.PageCount,
				Price = price.ToString(),
				Status = status ? "1" : "0"
			};

			using (var faxDbConnection = Db.NewAdminConnection(FaxDbName))
			{
				InsertFaxJob(faxDbConnection, job);
				InsertFaxRecipient(faxDbConnection, recipient);
				InsertFaxCdr(faxDbConnection, cdr);
			}
		}

		public void InsertFaxJobs()
		{
			string[] jobRecipientCdr = {
@"7D11E675-C240-4AE0-967E-290F472CCEAA,2009-09-01 01:55:49,AAA Pty Ltd <mail.edi@aaa.test>,4,AAE,ACO,PRD",
@"2B0E11EE-9365-4B5D-8E1D-0EF3F4CED82C,2009-09-01 01:56:19,0394294356,1,1",
@"4,1,0.08",

@"FF0B1B26-B9B1-49D3-8220-BE84BD193F61,2009-09-01 03:14:13,""AAA Pty Ltd"" <mail.edi@aaa.test>,5,AAE,ACO,PRD",
@"76E344E9-9F63-4FCF-B751-798F762007C4,2009-09-01 03:14:36,0393292452,1,1",
@"5,1,0.08",

@"4406986F-14C9-40F1-9C57-A01194E5ADE8,2009-09-01 03:14:13,AAA Pty Ltd <mail.edi@aaa.test>,4,AAE,ACO,PRD",
@"AF74EE30-88F1-4EFE-9EB0-7B94D65B7AA8,2009-09-01 03:14:38,0394279895,1,0",
@"4,0,0.50",

@"44589E98-F9E8-45FC-8A5F-C5688262037F,2009-09-01 07:01:49,AAA Pty Ltd <mail.edi@aaa.test>,4,AAE,ACO,PRD",
@"E8C7FE96-C41A-4967-A5A2-944B58F16499,2009-09-01 07:02:09,0299557800,1,1",
@"4,1,0.08",

@"AC56A67D-57D7-44D2-B1C4-F49706AEB82D,2009-09-01 07:32:20,AAA Pty Ltd <mail.edi@aaa.test>,4,AAE,ACO,PRD",
@"47A0D6EB-604E-4C0D-AA05-DC972C55AF7B,2009-09-01 07:32:35,0398535766,1,1",
@"4,1,0.09",

@"71F54577-839D-41BC-97D0-A8F2D2D02092,2009-09-01 07:33:20,AAA Pty Ltd <mail.edi@aaa.test>,5,AAE,ACO,PRD",
@"1A5C72D0-EE03-45C2-AF3C-574D062A3095,2009-09-01 07:33:38,0393354300,1,1",
@"5,1,0.09",

@"71C02A95-2FB4-4C0F-A707-8CE46F0619BE,2009-09-01 07:33:21,AAA Pty Ltd <mail.edi@aaa.test>,6,AAE,ACO,PRD",
@"BEAB926B-0655-4F5A-9A6B-2DA85B29DC9F,2009-09-01 07:33:41,0388031267,1,0",
@"6,0,0.00",

@"7A62903A-4409-4A08-8C12-1BC3EA5EEC26,2009-09-01 07:34:22,AAA Pty Ltd <mail.edi@aaa.test>,4,AAE,ACO,PRD",
@"6BB9CE51-0C60-4400-B1A4-D1BD8B4D0662,2009-09-01 07:34:45,0394279895,1,0",
@"4,0,0.00",

@"DFB10728-81CB-4C7D-A77F-705CD02FB30B,2009-09-01 07:34:22,AAA Pty Ltd <mail.edi@aaa.test>,4,AAE,ACO,PRD",
@"799B6279-3C5F-473A-A4F5-5BACD1D5E72F,2009-09-01 07:34:48,0394279895,1,0",
@"4,0,0.00",

@"AC226C5B-93EF-485E-85C1-A436DDCE220E,2009-09-01 07:34:22,AAA Pty Ltd <mail.edi@aaa.test>,4,AAE,ACO,PRD",
@"35E85019-848C-4289-A15B-FD8ED7597D31,2009-09-01 07:34:51,0394294356,1,1",
@"4,1,0.10",

@"4369DE30-83D2-403D-AEA6-07E01915DC6E,2009-09-01 00:34:26,BBB Inc. (US) <mail.edi@bbb.test>,1,BBE,BCO,PRD",
@"DD91660E-3568-4C08-9DFF-F61704ADA739,2009-09-01 00:36:00.890,+1 (919) 852-2221,1,1",
@"1,1,0.06",

@"5FF1F8E0-B434-42CE-B8C5-2827FE021B26,2009-09-01 00:34:26,""BBB Inc. (US)"" <mail.edi@bbb.test>,1,BBE,BCO,PRD",
@"39F0B22B-094F-4CEE-B872-C4ECA081F380,2009-09-01 00:36:04.017,+1 (630) 736-2567,1,1",
@"1,1,0.07",

@"D1F7F3CC-3FD2-47E6-92B9-4ED3E55FD814,2009-09-01 00:34:26,BBB Inc. (US) <mail.edi@bbb.test>,1,BBE,BCO,PRD",
@"B1D102A1-2F57-4BBD-A2F3-8BA67C33BA37,2009-09-01 00:35:03.810,+1 (614) 253-1330,1,1",
@"1,1,0.18",

@"2510FD4F-51E3-480E-ABDE-28C450A13D98,2009-09-01 00:34:27,BBB Inc. (US) <mail.edi@bbb.test>,1,BBE,BCO,PRD",
@"8E1DCCC8-982C-4168-A3CC-3CB3C5773126,2009-09-01 00:35:07.327,+1 (630) 736-2567,1,1",
@"1,1,0.08",

@"6112B8A8-2988-4CE4-8891-8C5A1A020072,2009-09-01 00:34:27,BBB Inc. (US) <mail.edi@bbb.test>,1,BBE,BCO,PRD",
@"8E3B928A-9309-491C-9FD8-D447B41EB43E,2009-09-01 00:36:08.547,+1 (630) 736-2567,1,1",
@"1,1,0.07",

@"B1BCD482-FFF3-40A2-BBAF-FEBEEA428C81,2009-09-01 00:34:27,BBB Inc. (US) <mail.edi@bbb.test>,1,BBE,BCO,PRD",
@"1163D423-F854-4518-B6D9-A0D8741B6EF9,2009-09-01 00:36:11.173,+1 (303) 322-4175,1,1",
@"1,1,0.08",

@"D497A01D-414F-4CE4-99AA-C9670EA97E99,2009-09-01 00:34:27,BBB Inc. (US) <mail.edi@bbb.test>,2,BBE,BCO,PRD",
@"A6371F4E-8408-42A5-9568-2841BCA1703B,2009-09-01 00:35:48.297,+1 (614) 253-1330,1,1",
@"2,1,0.09",

@"478A59CF-8AD6-4699-87D7-D894518337CE,2009-09-01 00:34:30,BBB Inc. (US) <mail.edi@bbb.test>,2,BBE,BCO,PRD",
@"F0165558-6B62-4622-BFB1-C858E77EB6A2,2009-09-01 00:35:13.500,+1 (505) 242-8854,1,1",
@"2,1,0.12",

@"8BD73173-E15B-4F4F-AE8D-3346C0BCA48C,2009-09-01 00:34:30,BBB Inc. (US) <mail.edi@bbb.test>,1,BBE,BCO,PRD",
@"CF47634C-76DB-423E-B02B-FE0607C84DC1,2009-09-01 00:35:18.250,+1 (614) 253-1330,1,1",
@"1,1,0.18",

@"9FDB68AB-8EAB-4AAB-80C3-DF2FEF2906A6,2009-09-01 00:34:31,BBB Inc. (US) <mail.edi@bbb.test>,1,BBE,BCO,PRD",
@"25A70A88-E6D0-4D11-8115-DC7598E8CD21,2009-09-01 00:35:23.187,+1 (614) 253-1330,1,1",
@"1,1,0.18",
};

			using (var faxDbConnection = Db.NewAdminConnection(FaxDbName))
			{
				for (int i = 0; i < jobRecipientCdr.Length / 3; ++i)
				{
					FaxJob job = new FaxJob(jobRecipientCdr[i * 3]);
					FaxRecipient recipient = new FaxRecipient(job, jobRecipientCdr[i * 3 + 1]);
					FaxCdr cdr = new FaxCdr(recipient, jobRecipientCdr[i * 3 + 2]);
					InsertFaxJob(faxDbConnection, job);
					InsertFaxRecipient(faxDbConnection, recipient);
					InsertFaxCdr(faxDbConnection, cdr);
				}
			}
		}

		void InsertFaxJob(DbConnection faxDbConnection, FaxJob job)
		{
			faxDbConnection.ExecuteNonQuery(@"INSERT INTO FaxJobs (FaxJobId, ReceivedDateTime, Sender, PageCount, EnterpriseCode, CompanyCode, ServerCode) VALUES ('"
				+ job.FaxJobId + "', '"
				+ job.ReceivedDateTime + "', '"
				+ job.Sender + "', "
				+ job.PageCount + ", '"
				+ job.EnterpriseCode + "', '"
				+ job.CompanyCode + "', '"
				+ job.ServerCode + "'"
				+ ")");
		}

		void InsertFaxRecipient(DbConnection faxDbConnection, FaxRecipient row)
		{
			faxDbConnection.ExecuteNonQuery(@"INSERT INTO FaxRecipients (" +
				@"FaxRecipientId, SentDateTime, FaxNumber, FaxJobId, IsAcknowledged, AckSuccess, AttentionName, Company, IsAckWarningSent) VALUES ('"
				+ row.FaxRecipientId + "', '"
				+ row.SentDateTime + "', '"
				+ row.FaxNumber + "', '"
				+ row.FaxJobId + "', "
				+ row.IsAcknowledged + ", "
				+ row.AckSuccess + ",'','',0)");
		}

		void InsertFaxCdr(DbConnection faxDbConnection, FaxCdr row)
		{
			faxDbConnection.ExecuteNonQuery(@"INSERT INTO FaxBilling (ChargeCode, Pages, Status, Price) VALUES ('"
				+ row.ChargeCode + "', "
				+ row.Pages + ", "
				+ row.Status + ", "
				+ row.Price + ")");
		}
	}
}
