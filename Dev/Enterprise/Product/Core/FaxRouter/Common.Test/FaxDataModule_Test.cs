using System;
using System.Configuration;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.FaxRouter
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClassesAnalyzer", Justification = "This assembly has no need to change SqlConnection")]
	sealed class FaxDataModule_Test : TestCaseWithFactory
	{
		public void TestIsValidGuidForValid()
		{
			AssertEquals(true, FaxDataModule.IsValidGuid("91D558F1-607D-4E69-84C6-90798EF57C92"));
		}

		public void TestIsValidGuidForInvalid()
		{
			AssertEquals(false, FaxDataModule.IsValidGuid("XF1D558F1-607D-4E69-84C6-90798EF57C92"));
		}

		public void TestIsValidFaxRecipientJobForInvalidGuid()
		{
			AssertEquals(false, FaxDataModule.IsValidFaxRecipientJob("XF1D558F1-607D-4E69-84C6-90798EF57C92"));
		}

		[DeveloperOnlyTest]
		public void TestTiffFileDataTransform()
		{
			try
			{
				InsertTestFaxJobs();

				using (SqlConnection conn = BaseDataModule.GetEDIFaxDBConnection())
				{
					string columnType = GetColumnType(conn, "FaxJobs", "TiffFile");
					AssertEquals("Precondition:", "image", columnType);

					var rowCount = GetRowCount(conn);
					AssertEquals("Precondition:", 5, rowCount);
				}

				var transformer = new FaxDataModule();
				transformer.TiffFileDataTransform();

				using (SqlConnection conn = BaseDataModule.GetEDIFaxDBConnection())
				{
					string columnType = GetColumnType(conn, "FaxJobs", "TiffFile");
					AssertEquals("FaxJobs.TiffFile columnType should be varbinary after data transform", "varbinary", columnType);

					var rowCount = GetRowCount(conn);
					AssertEquals("FaxJobs rowCount should be still 5 after data transform", 5, rowCount);
				}

				transformer.TiffFileDataTransform();

				using (SqlConnection conn = BaseDataModule.GetEDIFaxDBConnection())
				{
					int rowsAffected = GetRowsAffected(conn);
					AssertEquals("FaxJobs rowsAffected should be 0 when executing data transform the second time", 0, rowsAffected);
				}
			}
			finally
			{
				faxDbTestHelper.DropFaxDb();
			}
		}

		void InsertTestFaxJobs()
		{
			using (SqlConnection conn = BaseDataModule.GetEDIFaxDBConnection())
			{
				using (var sqlCmd = new SqlCommand(@"INSERT INTO FaxJobs
(FaxJobId, ReceivedDateTime, PageCount, EnterpriseCode, CompanyCode, ServerCode) VALUES
(@FaxJobId, @ReceivedDateTime, 1, 'UPE', 'SYD', 'SYD')", conn))
				{
					for (int i = 0; i < 5; i++)
					{
						sqlCmd.Parameters.Clear();
						sqlCmd.Parameters.AddWithValue("FaxJobId", Guid.NewGuid());
						sqlCmd.Parameters.AddWithValue("ReceivedDateTime", DateTime.Now);
						sqlCmd.ExecuteNonQuery();
					}
				}
			}
		}

		string GetColumnType(SqlConnection connection, string tableName, string columnName)
		{
			string columnTypeQuery = $"SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}' AND COLUMN_NAME = '{columnName}'";
			string columnType = null;
			using (SqlCommand typeCmd = new SqlCommand(columnTypeQuery, connection))
			{
				columnType = (string)typeCmd.ExecuteScalar();
			}
			return columnType;
		}

		int GetRowCount(SqlConnection conn)
		{
			string getDataCountQuery = "SELECT COUNT(*) FROM FaxJobs";
			SqlCommand countCmd = new SqlCommand(getDataCountQuery, conn);
			int rowCount = (int)countCmd.ExecuteScalar();
			return rowCount;
		}

		int GetRowsAffected(SqlConnection conn)
		{
			string query = "SELECT @@ROWCOUNT";
			SqlCommand command = new SqlCommand(query, conn);
			int rowsAffected = (int)command.ExecuteScalar();
			return rowsAffected;
		}

		protected override void SetUp()
		{
			base.SetUp();

			faxDbTestHelper = new FaxDbTestHelper();
			faxDbTestHelper.RunFaxDbCreateScripts();

			ConfigurationSettings.AppSettings["MailDBConnectionString"] = String.Format(@"Data Source=localhost; Integrated Security=SSPI; Initial Catalog={0}; Application Name=FaxRouter; Connect Timeout=60; Pooling=false", Db.DatabaseName);
			ConfigurationSettings.AppSettings["EDIFaxDBConnectionString"] = String.Format(@"Data Source=localhost; Integrated Security=SSPI; Initial Catalog={0}; Application Name=FaxRouter; Pooling=false", FaxDbTestHelper.FaxDbName);
			ConfigurationSettings.AppSettings["ENTERPRISE_DATABASE_NAME"] = Db.DatabaseName;
			ConfigurationSettings.AppSettings["AUTO_START"] = "1";

			ConfigurationSettings.AppSettings["FAX_GATEWAY_ADMINISTRATOR_EMAIL"] = "test1@edi.com.au;test2@cargowise.com";
			ConfigurationSettings.AppSettings["FAX_ACK_SENDER"] = "test_sender@edi.com.au";
			ConfigurationSettings.AppSettings["FAX_GATEWAY_POLLING_INTERVAL"] = "30000";
			ConfigurationSettings.AppSettings["FAX_VIEWER_PAGE_HIEGHT"] = "950";
			ConfigurationSettings.AppSettings["FAX_VIEWER_PAGE_WIDTH"] = "750";
			ConfigurationSettings.AppSettings["FAX_GATEWAY_TEMP_FILE_DIRECTORY"] = @"C:\Temp\";
			ConfigurationSettings.AppSettings["FAX_GATEWAY_LOG"] = @"EventLog.txt";

			ConfigurationSettings.AppSettings["ACK_FORMAT"] = "TNZ";
			ConfigurationSettings.AppSettings["BLACKLIST"] = @"djtest@edi.com.au:\+61290251194|0421944394|294|299";
			ConfigurationSettings.AppSettings["DAILY_REPORT_DISCREPANCY_THRESHOLD"] = "0";
			ConfigurationSettings.AppSettings["NOREPLY_EMAIL"] = "noreply@cargowise.com";
		}

		protected override void TearDown()
		{
			faxDbTestHelper.DropFaxDb();
			base.TearDown();
		}

		FaxDbTestHelper faxDbTestHelper;

		public class FaxDbTestHelper
		{
			public const string FaxDbName = "EDIFaxDBTest";

			public void RunFaxDbCreateScripts()
			{
				using (AdminConnection adminCon = Db.NewAdminConnection())
				{
					DoSql(adminCon, @"IF  EXISTS (SELECT name FROM sys.databases WHERE name = N'" + FaxDbName + "') DROP DATABASE [" + FaxDbName + "]");
					adminCon.CreateDatabase(FaxDbName);
					adminCon.CloseConnection();
				}

				using (DbConnection faxCon = Db.NewAdminConnection(FaxDbName))
				{
					DoSql(faxCon,
	@"CREATE TABLE [dbo].[FaxJobs](
	[FaxJobId] [uniqueidentifier] NOT NULL,
	[ReceivedDateTime] [datetime] NULL,
	[Sender] [varchar](128) NULL,
	[TiffFile] [image] NULL,
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
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];
");
					faxCon.CloseConnection();
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
		}
	}
}
