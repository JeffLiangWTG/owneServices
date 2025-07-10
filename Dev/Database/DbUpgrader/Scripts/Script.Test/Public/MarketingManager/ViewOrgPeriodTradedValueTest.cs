using System;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MarketingManager;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MarketingManager.Testing
{
	[TestedType(typeof(ViewOrgPeriodTradedValue))]
	class ViewOrgPeriodTradedValueTest : DbCreateScriptTest
	{
		public void TestViewOrgPeriodTradedValue()
		{
			var orgPk = Guid.NewGuid();

			CreateOrg(orgPk);
			CreateTradedValue(orgPk, "SHP", "AUSYD", "AUMEL", new DateTime(2018, 1, 1), "AUD", 1000m, 200m);
			CreateTradedValue(orgPk, "SHP", "AUBNE", "NZAKL", new DateTime(2018, 1, 1), "AUD", 500m, 150m);
			CreateTradedValue(orgPk, "SHP", "NZAKL", "AUSYD", new DateTime(2018, 1, 1), "NZD", 2500m, 600m);
			CreateTradedValue(orgPk, "SHP", "NZAKL", "AUSYD", new DateTime(2018, 2, 1), "NZD", 2000m, 400m);

			using (var command = TestConnection.Command("SELECT * FROM dbo.ViewOrgPeriodTradedValue ORDER BY VPT_Period, VPT_RX_NKCurrency"))
			{
				var result = new DataTable();
				result.Load(command.ExecuteReader());
				AssertEquals("Pre-condition: should return a result", 3, result.Rows.Count);

				AssertDataRow(result.Rows[0], new DateTime(2018, 1, 1), "AUD", 1500m, 350m);
				AssertDataRow(result.Rows[1], new DateTime(2018, 1, 1), "NZD", 2500m, 600m);
				AssertDataRow(result.Rows[2], new DateTime(2018, 2, 1), "NZD", 2000m, 400m);
			}
		}

		void AssertDataRow(DataRow row, DateTime expectedPeriod, string expectedCurrency, decimal expectedRevenue, decimal expectedCost)
		{
			AssertEquals("Period", expectedPeriod, (DateTime)row["VPT_Period"]);
			AssertEquals("Currency", expectedCurrency, (string)row["VPT_RX_NKCurrency"]);
			AssertEquals("Revenue", expectedRevenue, (decimal)row["VPT_Revenue"]);
			AssertEquals("Cost", expectedCost, (decimal)row["VPT_Cost"]);
		}

		void CreateOrg(Guid orgPk)
		{
			var sql = @"INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@OH_PK, 'TESTORGAAA')";
			using (var command = TestConnection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@OH_PK", orgPk, OrgHeaderSchema.PK);
				command.ExecuteNonQuery();
			}
		}

		void CreateTradedValue(Guid orgPk, string product, string origin, string destination, DateTime period, string currency, decimal revenue, decimal cost)
		{
			var sql =
@"
DECLARE @OriginPk UNIQUEIDENTIFIER = (SELECT TOP 1 RL_PK FROM dbo.RefUNLOCO WHERE RL_Code = @Origin)
DECLARE @DestinationPk UNIQUEIDENTIFIER = (SELECT TOP 1 RL_PK FROM dbo.RefUNLOCO WHERE RL_Code = @Destination)
DECLARE @ProductPk UNIQUEIDENTIFIER = (SELECT TOP 1 MP_PK FROM dbo.OrgSalesProduct WHERE MP_Code = @Product)

INSERT INTO dbo.OrgSales (OW_PK, OW_MP_Product, OW_OriginID, OW_OriginTableCode, OW_DestinationID, OW_DestinationTableCode, OW_OH_Primary, OW_IsTraded, OW_SystemCreateTimeUtc, OW_SystemCreateUser, OW_SystemLastEditTimeUtc, OW_SystemLastEditUser) 
VALUES (@OW_PK, @ProductPk, @OriginPk, 'RL', @DestinationPk, 'RL', @OH_PK, 1, GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.OrgTradeDetail(PA_PK, PA_OW, PA_SystemCreateTimeUtc, PA_SystemCreateUser, PA_SystemLastEditTimeUtc, PA_SystemLastEditUser)
VALUES (@PA_PK, @OW_PK, GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.OrgTradePeriod(PAS_PK, PAS_PA, PAS_OH_Client, PAS_IsTraded, PAS_Period, PAS_SystemCreateTimeUtc, PAS_SystemCreateUser, PAS_SystemLastEditTimeUtc, PAS_SystemLastEditUser)
VALUES (@PAS_PK, @PA_PK, @OH_PK, 1, @PAS_Period, GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.OrgTradeValue(PAV_PK, PAV_PAS, PAV_RX_NKCurrency, PAV_Revenue, PAV_Cost, PAV_GC, PAV_SystemCreateTimeUtc, PAV_SystemCreateUser, PAV_SystemLastEditTimeUtc, PAV_SystemLastEditUser)
VALUES (NEWID(), @PAS_PK, @PAV_RX_NKCurrency, @PAV_Revenue, @PAV_Cost, @PAV_GC, GetUtcDate(), 'E', GetUtcDate(), 'E')
";
			using (var command = TestConnection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@OW_PK", Guid.NewGuid(), OrgSalesSchema.PK);
				command.AddParameterBasedOnDbColumn("@OH_PK", orgPk, OrgHeaderSchema.PK);
				command.AddParameter("@Origin", SqlDbType.VarChar, origin);
				command.AddParameter("@Destination", SqlDbType.VarChar, destination);
				command.AddParameter("@Product", SqlDbType.VarChar, product);
				command.AddParameterBasedOnDbColumn("@PA_PK", Guid.NewGuid(), OrgTradeDetailSchema.PK);
				command.AddParameterBasedOnDbColumn("@PAS_PK", Guid.NewGuid(), OrgTradePeriodSchema.PK);
				command.AddParameterBasedOnDbColumn("@PAS_Period", period, OrgTradePeriodSchema.PAS_Period);
				command.AddParameterBasedOnDbColumn("@PAV_RX_NKCurrency", currency, OrgTradeValueSchema.PAV_RX_NKCurrency);
				command.AddParameterBasedOnDbColumn("@PAV_GC", TestDbHelper.DefaultCompanyPK, OrgTradeValueSchema.PAV_GC);
				command.AddParameterBasedOnDbColumn("@PAV_Revenue", revenue, OrgTradeValueSchema.PAV_Revenue);
				command.AddParameterBasedOnDbColumn("@PAV_Cost", cost, OrgTradeValueSchema.PAV_Cost);
				command.ExecuteNonQuery();
			}
		}
	}
}
