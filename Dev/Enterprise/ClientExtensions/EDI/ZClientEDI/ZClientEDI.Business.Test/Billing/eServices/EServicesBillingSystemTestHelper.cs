using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Licensing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Test
{
	public static class EServicesBillingTestHelper
	{
		public class RawUsageInfo
		{
			public RawUsageInfo(ZString category, ZString messageType, ZDateTime messageTimeUTC, ZString clientID, ZString reference1, ZString reference2, ZString reference3, ZString reference4)
				: this(category, messageType, messageTimeUTC, clientID, reference1, reference2, reference3, reference4, null, "HUB", 1)
			{ }

			public RawUsageInfo(ZString category, ZString priceItemCode, ZDateTime messageTimeUTC, ZString clientID, ZString reference1, ZString reference2, ZString reference3, ZString reference4, string reportingSource)
				: this(category, priceItemCode, messageTimeUTC, clientID, reference1, reference2, reference3, reference4, null, reportingSource, 1)
			{ }

			public RawUsageInfo(ZString category, ZString priceItemCode, ZDateTime messageTimeUTC, ZString clientID, ZString reference1, ZString reference2, ZString reference3, ZString reference4, int billableCount)
				: this(category, priceItemCode, messageTimeUTC, clientID, reference1, reference2, reference3, reference4, null, "HUB", billableCount)
			{ }

			public RawUsageInfo(ZString category, ZString priceItemCode, ZDateTime messageTimeUTC, ZString clientID, ZString clientNumber, ZString reference1, ZString reference2, ZString reference3, ZString reference4, ZString reference5)
				: this(category, priceItemCode, messageTimeUTC, clientID, reference1, reference2, reference3, reference4, null, "HUB", 1)
			{
				this.ClientNumber = clientNumber;
				this.Reference5 = reference5;
			}

			public RawUsageInfo(ZString category, ZString priceItemCode, ZDateTime messageTimeUTC, ZString clientID, ZString reference1, ZString reference2, ZString reference3, ZString reference4,
				 DateTime? systemCreateUTC = null, string reportingSource = "HUB", int billableCount = 1)
				: this(category, priceItemCode, messageTimeUTC, clientID, "", reference1, reference2, reference3, reference4, systemCreateUTC, reportingSource, billableCount)
			{
			}

			public RawUsageInfo(ZString category, ZString priceItemCode, ZDateTime messageTimeUTC, ZString clientID, string clientNumber, ZString reference1, string reference2, string reference3, string reference4,
				 DateTime? systemCreateUTC = null, string reportingSource = "HUB", int billableCount = 1)
			{
				this.Category = category;
				this.PriceItemCode = priceItemCode;
				this.MessageTimeUTC = messageTimeUTC;
				this.ClientID = clientID;
				this.ClientNumber = clientNumber;
				this.Reference1 = reference1;
				this.Reference2 = reference2;
				this.Reference3 = reference3;
				this.Reference4 = reference4;
				this.SystemCreateUTC = systemCreateUTC;
				this.ReportingSource = reportingSource;
				this.BillableCount = billableCount;

				if (!string.IsNullOrEmpty(clientNumber))
				{
					var dotIndex = clientNumber.IndexOf('.');
					this.SystemID = dotIndex >= 0 ? new ZString(clientNumber).Left(dotIndex) : new ZString(clientNumber);
					int dbId;
					Base27Encoding.TryDecode(SystemID, out dbId);
					this.DatabaseNumber = dbId;
				}

				this.Period = this.MessageTimeUTC.Year * 100 + this.MessageTimeUTC.Month;
			}

			public RawUsageInfo(ZString category, ZString priceItemCode, ZDateTime messageTimeUTC, ZString clientID, string clientNumber, ZString systemId, ZGuid companyPk, ZString reference1, string reference2, string reference3, string reference4,
				 DateTime? systemCreateUTC = null, string reportingSource = "HUB", int billableCount = 1)
			{
				this.Category = category;
				this.PriceItemCode = priceItemCode;
				this.MessageTimeUTC = messageTimeUTC;
				this.ClientID = clientID;
				this.ClientNumber = clientNumber;
				this.SystemID = systemId;
				this.CompanyPk = companyPk;
				this.Reference1 = reference1;
				this.Reference2 = reference2;
				this.Reference3 = reference3;
				this.Reference4 = reference4;
				this.SystemCreateUTC = systemCreateUTC;
				this.ReportingSource = reportingSource;
				this.BillableCount = billableCount;

				int dbId;
				Base27Encoding.TryDecode(SystemID, out dbId);
				this.DatabaseNumber = dbId;
				this.Period = this.MessageTimeUTC.Year * 100 + this.MessageTimeUTC.Month;
			}

			public RawUsageInfo(ZString category, ZString priceItemCode, ZDateTime messageTimeUTC, ZString clientID, string clientNumber, ZString systemId, ZGuid companyPk, ZString reference1, ZString reference2, ZString reference3, ZString reference4, ZString reference5,
				DateTime? systemCreateUTC = null, string reportingSource = "HUB", int billableCount = 1) : this(category, priceItemCode, messageTimeUTC, clientID, clientNumber, systemId, companyPk, reference1, reference2, reference3, reference4, systemCreateUTC, reportingSource, billableCount)
			{
				Reference5 = reference5;
			}

			public RawUsageInfo(ZString category, ZString priceItemCode, ZDateTime messageTimeUTC, LicenceHeader licHeader,
				ZString reference1, string reference2, string reference3, string reference4, string reference5 = null,
				 DateTime? systemCreateUTC = null, string reportingSource = "HUB", int billableCount = 1)
			{
				this.Category = category;
				this.PriceItemCode = priceItemCode;
				this.MessageTimeUTC = messageTimeUTC;
				this.ClientID = licHeader.LicenceCode;
				this.ClientNumber = licHeader.Database.DatabaseId + "." + licHeader.CompanyCode;
				this.SystemID = licHeader.Database.DatabaseId;
				this.DatabaseNumber = licHeader.Database.LD_DatabaseNumber;
				this.CompanyPk = licHeader.ClientCompany.PK;
				this.Reference1 = reference1;
				this.Reference2 = reference2;
				this.Reference3 = reference3;
				this.Reference4 = reference4;
				this.Reference5 = reference5;
				this.SystemCreateUTC = systemCreateUTC;
				this.ReportingSource = reportingSource;
				this.BillableCount = billableCount;

				this.Period = this.MessageTimeUTC.Year * 100 + this.MessageTimeUTC.Month;
			}

			public void SetBranchAndStaff(ZString branch, ZString staff)
			{
				BranchCode = branch;
				StaffCode = staff;
			}

			public static void FillBranchAndStaff(IEnumerable<RawUsageInfo> usageInfos, int startIndex = 10)
			{
				foreach (var usage in usageInfos)
				{
					usage.SetBranchAndStaff($"B{startIndex}", $"S{startIndex}");
					startIndex++;

					if (startIndex > 99)
					{
						startIndex = 10;
					}
				}
			}

			public ZInt Period { get; set; }
			public ZString PriceItemCode { get; set; }
			public ZDateTime MessageTimeUTC { get; set; }
			public ZString ClientID { get; set; }
			public string ClientNumber { get; set; }
			public ZInt DatabaseNumber { get; set; }
			public string Reference1 { get; set; }
			public string Reference2 { get; set; }
			public string Reference3 { get; set; }
			public string Reference4 { get; set; }
			public string Reference5 { get; set; }
			public DateTime? SystemCreateUTC { get; set; }
			public ZString ReportingSource { get; set; }
			public int BillableCount { get; set; }
			public ZString Category { get; set; }
			public ZString BranchCode { get; set; }
			public ZString SystemID { get; set; }
			public ZGuid CompanyPk { get; set; }
			public ZString StaffCode { get; set; }
		}

		public static EDIOrgHeader CreateClient(BusinessObjectFactory factory, ZString enterpriseCode, ZString companyCode, ZString databaseServerCode)
		{
			var clientOrg = factory.New<EDIOrgHeader>();
			clientOrg.OH_Code = enterpriseCode + companyCode + databaseServerCode;
			clientOrg.OH_FullName = clientOrg.OH_Code;

			var enterprise = factory.LoadTop1<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_EnterpriseCode, enterpriseCode));
			if (enterprise == null)
			{
				enterprise = factory.NewWithValidTestData<LicenceEnterprise>();
				enterprise.LE_EnterpriseCode = enterpriseCode;
				enterprise.LE_OH = clientOrg.PK;
			}

			var company = factory.New<LicenceCompany>();
			company.LC_CompanyCode = companyCode;
			company.LC_LE = enterprise.PK;
			company.LC_OH = clientOrg.PK;

			var licDatabaseQuery = new ZQuery(LicenceDatabaseSchema.LD_ServerCode, databaseServerCode);
			licDatabaseQuery.AddToFilter(LicenceDatabaseSchema.LD_LE, enterprise.PK);
			var database = factory.LoadTop1<LicenceDatabase>(licDatabaseQuery);
			if (database == null)
			{
				database = factory.New<LicenceDatabase>();
				database.LD_ServerCode = databaseServerCode;
				database.LD_LE = enterprise.PK;
			}

			var licenceHeader = factory.New<LicenceHeader>();
			licenceHeader.LA_LC = company.PK;
			licenceHeader.LA_LD = database.PK;

			return clientOrg;
		}

		public static void CreateTable()
		{
			CreateTableBillingTransaction();
		}

		public static void CreateTableBillingTransaction()
		{
			DoSql(
@"CREATE TABLE [dbo].[BillingTransaction](
    TX_ID                BIGINT           NOT NULL PRIMARY KEY IDENTITY,
    TX_Period            INT              NOT NULL,
    TX_Category          CHAR (3)         NOT NULL,
    TX_PriceItemCode     CHAR (3)         NOT NULL,
	TX_DatabaseNumber    INT              NOT NULL default(0),
    TX_BillableCount     INT              NOT NULL,
    TX_ReportingSource   VARCHAR (3)      NOT NULL,
    TX_ServiceOccuredUTC DATETIME2 (7)    NOT NULL,
    TX_ClientID          VARCHAR (9)      NOT NULL,
    TX_ClientNumber      VARCHAR (20)     NULL,
    TX_ClientStaffCode   VARCHAR (3)      NULL,
    TX_Reference1        VARCHAR (50)     NOT NULL,
    TX_Reference2        VARCHAR (50)     NULL,
    TX_Reference3        VARCHAR (50)     NULL,
    TX_Reference4        VARCHAR (50)     NULL,
    TX_Reference5        VARCHAR (50)     NULL,
    TX_Version           INT              DEFAULT ((0)) NOT NULL,
    TX_Branch            VARCHAR (3)      NULL,
    TX_SystemCreateUTC   DATETIME2 (0)    DEFAULT (sysutcdatetime()) NOT NULL,
    TX_SystemLastEditUTC DATETIME2 (0)    DEFAULT (sysutcdatetime()) NOT NULL,
    TX_CapturedUTC       DATETIME2 (0)    NOT NULL,
    TX_MessageTrackingID VARCHAR (36)     NULL,
    TX_SystemID          VARCHAR (7)      NULL,
    TX_LCC               UNIQUEIDENTIFIER NOT NULL,
)");

			DoSql(
@"CREATE VIEW [dbo].[BillingViewChargeable]
AS
SELECT * FROM [dbo].[BillingTransaction]
");
		}

		public static void CreateTableBillingTransactionStaging()
		{
			DoSql(
@"CREATE TABLE [dbo].[BillingTransactionStaging](
    TX_ID                BIGINT           NOT NULL IDENTITY,
    TX_Category          VARCHAR (3)      NOT NULL,
    TX_PriceItemCode     VARCHAR (3)      NOT NULL,
    TX_BillableCount     INT              NOT NULL,
    TX_ReportingSource   VARCHAR (3)      NOT NULL,
    TX_ServiceOccuredUTC DATETIME2 (7)    NOT NULL,
    TX_ClientID          VARCHAR (9)      NOT NULL,
    TX_ClientNumber      VARCHAR (20)     NULL,
    TX_ClientStaffCode   VARCHAR (3)      NULL,
    TX_Reference1        VARCHAR (50)     NOT NULL,
    TX_Reference2        VARCHAR (50)     NULL,
    TX_Reference3        VARCHAR (50)     NULL,
    TX_Reference4        VARCHAR (50)     NULL,
    TX_Reference5        VARCHAR (50)     NULL,
    TX_SystemCreateUTC   DATETIME2 (0)    DEFAULT (sysutcdatetime()) NOT NULL,
    TX_Version           INT              DEFAULT ((0)) NOT NULL,
    TX_Branch            VARCHAR (3)      NULL,
    TX_MessageTrackingID VARCHAR (36)     NULL,
    TX_Period            INT              NOT NULL DEFAULT 0,
    IsRefSwapped         BIT              NOT NULL default(0),
    ProcessingStatus     TINYINT          NOT NULL default(0),
    DatabaseNumber       INT              NOT NULL default(0),
    CompanyNumber        SMALLINT         NOT NULL default(0)
)");
		}

		public static void CreateTableClientMappingInterface()
		{
			DoSql(
@"CREATE TABLE dbo.BillingClientMappingInterface(
	Name varchar(50) not null,
	FirstCapturedUtc datetime2(0) not null
)");
		}

		public static void InsertClientMappingInterface(string name, DateTime firstCapturedUtc)
		{
			string sql = "insert BillingClientMappingInterface(Name, FirstCapturedUtc) VALUES(@name, @firstCapturedUtc)";
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@name", System.Data.SqlDbType.VarChar, name);
				cmd.AddParameter("@firstCapturedUtc", System.Data.SqlDbType.DateTime2, firstCapturedUtc);
				cmd.ExecuteNonQuery();
			}
		}

		public static void DropTable()
		{
			DoSql(TearDownSql);
		}

		public static void AddTransactions(IEnumerable<RawUsageInfo> infos)
		{
			foreach (var info in infos)
			{
				InsertTransaction(info);
			}
		}

		public static void InsertTransaction(RawUsageInfo info)
		{
			string sql =
@"INSERT INTO [BillingTransaction] (
TX_Period,
TX_PriceItemCode, TX_BillableCount, TX_ReportingSource, TX_ServiceOccuredUTC, TX_CapturedUTC,
TX_ClientID, TX_ClientNumber, TX_DatabaseNumber, TX_SystemID, TX_LCC,
TX_Reference1, TX_Reference2, TX_Reference3, TX_Reference4, TX_Reference5, TX_Category, TX_Branch, TX_ClientStaffCode" +
				(info.SystemCreateUTC != null ? ", TX_SystemCreateUTC" : "") +
				") VALUES ("
				+ "" + info.Period + ", "
				+ "'" + info.PriceItemCode + "', "
				+ info.BillableCount + ", "
				+ "'" + info.ReportingSource + "',"
				+ "'" + info.MessageTimeUTC.SqlFormat + "', "
				+ "'" + info.MessageTimeUTC.SqlFormat + "', "
				+ "'" + info.ClientID + "', "
				+ (info.ClientNumber != null ? ("'" + info.ClientNumber + "', ") : ("null, "))
				+ info.DatabaseNumber + ", "
				+ (info.SystemID.IsEmpty ? "null" : "'" + info.SystemID + "'") + ", "
				+ "'" + info.CompanyPk.ToString() + "', "
				+ "'" + info.Reference1 + "', "
				+ (info.Reference2 != null ? ("'" + info.Reference2 + "', ") : ("null, "))
				+ (info.Reference3 != null ? ("'" + info.Reference3 + "', ") : ("null, "))
				+ (info.Reference4 != null ? ("'" + info.Reference4 + "', ") : ("null, "))
				+ (info.Reference5 != null ? ("'" + info.Reference5 + "', ") : ("null, "))
				+ "'" + info.Category + "', "
				+ "'" + info.BranchCode + "', "
				+ "'" + info.StaffCode + "'"
				+ (info.SystemCreateUTC != null ? (", '" + new ZDateTime(info.SystemCreateUTC.Value).SqlFormat + "'") : "")
				+ ")";
			using (var command = Db.Connection.Command(sql))
			{
				command.ExecuteNonQuery();
			}
		}

		static void DoSql(string sql)
		{
			using (var command = Db.Connection.Command(sql))
			{
				command.ExecuteNonQuery();
			}
		}

		const string TearDownSql =
@"IF EXISTS (SELECT name FROM sys.tables WHERE name = N'BillingTransaction')
begin
	DROP VIEW BillingViewChargeable
	DROP TABLE [BillingTransaction];
end

IF EXISTS (SELECT name FROM sys.tables WHERE name = N'BillingTransactionStaging')
begin
	DROP TABLE [BillingTransactionStaging];
end
";
	}
}
