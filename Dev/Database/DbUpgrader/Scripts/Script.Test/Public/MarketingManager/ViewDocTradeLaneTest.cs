using System;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MarketingManager;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MarketingManager.Testing
{
	[TestedType(typeof(ViewDocTradeLane))]
	class ViewDocTradeLaneTest : DbCreateScriptTest
	{
		public void TestViewDocTradeLane()
		{
			var orgPk = Guid.NewGuid();

			CreateOrg(orgPk);

			CreateProspectValue(orgPk, "SHP", "AUSYD", "AUMEL", "AIR", "LSE", 100m, "KG", 15m, "M3", 1, 0, "YR", "AUD", 50m, new DateTime(2018, 1, 5));
			CreateProspectValue(orgPk, "SHP", "AUSYD", "AUMEL", "AIR", "LSE", 200m, "KG", 20m, "M3", 1, 2, "MTH", "AUD", 120m, new DateTime(2018, 2, 6));
			CreateProspectValue(orgPk, "SHP", "AUSYD", "NZAKL", "SEA", "FCL", 12m, "KG", 3m, "M3", 1, 3, "WK", "AUD", 30m, new DateTime(2018, 1, 14));

			CreateTradedValue(orgPk, "SHP", "AUSYD", "NZAKL", "SEA", "FCL", new DateTime(2018, 1, 1), 0.3m, "T", 50m, "M3", 1);
			CreateTradedValue(orgPk, "SHP", "AUSYD", "NZAKL", "SEA", "FCL", new DateTime(2018, 2, 1), 0.5m, "T", 60m, "M3", 3);
			CreateTradedValue(orgPk, "TRN", "AUMEL", "AUBNE", "PTR", "FCL", new DateTime(2018, 2, 1), 0.4m, "T", 6m, "M3", 2);

			using (var command = TestConnection.Command("SELECT * FROM dbo.ViewDocTradeLane ORDER BY [IsTraded], [ProductName], [OriginCode], [DestinationCode], [TradeMode], [TradeType]"))
			{
				var result = new DataTable();
				result.Load(command.ExecuteReader());
				AssertEquals("Pre-condition: should return a result", 4, result.Rows.Count);

				AssertDataRow(result.Rows[0], new DateTime(2018, 2, 6), 4900m, 495m, 25m, 2930m);
				AssertDataRow(result.Rows[1], new DateTime(2018, 1, 14), 1872m, 468m, 156m, 4680m);
				AssertDataRow(result.Rows[2], new DateTime(2018, 2, 1), 0.8m, 110m, 4m, 0m);
				AssertDataRow(result.Rows[3], new DateTime(2018, 2, 1), 0.4m, 6m, 2m, 0m);
			}
		}

		void AssertDataRow(DataRow row, DateTime expectedActivityDate, decimal expectedWeight, decimal expectedVolume, decimal expectedTeu, decimal expectedRevenue)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Period", expectedActivityDate, (DateTime)row["ActivityDate"]);
				AssertEquals("Weight", expectedWeight, (decimal)row["Weight"]);
				AssertEquals("Volume", expectedVolume, (decimal)row["Volume"]);
				AssertEquals("TEU", expectedTeu, (decimal)row["TEU"]);
				AssertEquals("EstimatedRevenue", expectedRevenue, (decimal)row["EstimatedRevenue"]);
			});
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

		void CreateProspectValue(Guid orgPk, string product, string origin, string destination, string tradeMode, string tradeType, decimal weight, string weightUQ, decimal volume, string volumnUQ, decimal teu, int jobCount, string recurrenceType, string currency, decimal revenue, DateTime latestProspectDate)
		{
			var sql =
@"
DECLARE @OriginPk UNIQUEIDENTIFIER = (SELECT TOP 1 RL_PK FROM dbo.RefUNLOCO WHERE RL_Code = @Origin)
DECLARE @DestinationPk UNIQUEIDENTIFIER = (SELECT TOP 1 RL_PK FROM dbo.RefUNLOCO WHERE RL_Code = @Destination)
DECLARE @ProductPk UNIQUEIDENTIFIER = (SELECT TOP 1 MP_PK FROM dbo.OrgSalesProduct WHERE MP_Code = @Product)

INSERT INTO dbo.OrgSales (OW_PK, OW_MP_Product, OW_OriginID, OW_OriginTableCode, OW_DestinationID, OW_DestinationTableCode, OW_OH_Primary, OW_IsTraded, OW_LatestProspectDate, OW_SystemCreateTimeUtc, OW_SystemCreateUser, OW_SystemLastEditTimeUtc, OW_SystemLastEditUser) 
VALUES (@OW_PK, @ProductPk, @OriginPk, 'RL', @DestinationPk, 'RL', @OH_PK, 0, @OW_LatestProspectDate, GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.OrgTradeDetail(PA_PK, PA_OW, PA_SystemCreateTimeUtc, PA_SystemCreateUser, PA_SystemLastEditTimeUtc, PA_SystemLastEditUser)
VALUES (@PA_PK, @OW_PK, GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.OrgTradePeriod(PAS_PK, PAS_PA, PAS_OH_Client, PAS_IsTraded, PAS_Period, PAS_RepeatsMnth, PAS_Weight, PAS_WeightUQ, PAS_Volume, PAS_VolumeUQ, PAS_TEUQuantity, PAS_RX_NKCurrency, PAS_EstimatedProfit, PAS_SystemCreateTimeUtc, PAS_SystemCreateUser, PAS_SystemLastEditTimeUtc, PAS_SystemLastEditUser)
VALUES (NEWID(), @PA_PK, @OH_PK, 0, NULL, @PAS_RepeatsMnth, @PAS_Weight, @PAS_WeightUQ, @PAS_Volume, @PAS_VolumeUQ, @PAS_TEUQuantity, @PAS_RX_NKCurrency, @PAS_EstimatedProfit, GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.OrgTradeProspect(PAP_PK, PAP_PA, PAP_RecurrenceType, PAP_SystemCreateTimeUtc, PAP_SystemCreateUser, PAP_SystemLastEditTimeUtc, PAP_SystemLastEditUser)
VALUES (NEWID(), @PA_PK, @PAP_RecurrenceType, GetUtcDate(), 'E', GetUtcDate(), 'E')
";
			using (var command = TestConnection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@OW_PK", Guid.NewGuid(), OrgSalesSchema.PK);
				command.AddParameterBasedOnDbColumn("@OH_PK", orgPk, OrgHeaderSchema.PK);
				command.AddParameter("@Origin", SqlDbType.VarChar, origin);
				command.AddParameter("@Destination", SqlDbType.VarChar, destination);
				command.AddParameter("@Product", SqlDbType.VarChar, product);
				command.AddParameterBasedOnDbColumn("@OW_LatestProspectDate", latestProspectDate, OrgSalesSchema.OW_LatestProspectDate);
				command.AddParameterBasedOnDbColumn("@PA_PK", Guid.NewGuid(), OrgTradeDetailSchema.PK);
				command.AddParameterBasedOnDbColumn("@PAS_RepeatsMnth", jobCount, OrgTradePeriodSchema.PAS_RepeatsMnth);
				command.AddParameterBasedOnDbColumn("@PAS_Weight", weight, OrgTradePeriodSchema.PAS_Weight);
				command.AddParameterBasedOnDbColumn("@PAS_WeightUQ", weightUQ, OrgTradePeriodSchema.PAS_WeightUQ);
				command.AddParameterBasedOnDbColumn("@PAS_Volume", volume, OrgTradePeriodSchema.PAS_Volume);
				command.AddParameterBasedOnDbColumn("@PAS_VolumeUQ", volumnUQ, OrgTradePeriodSchema.PAS_VolumeUQ);
				command.AddParameterBasedOnDbColumn("@PAS_TEUQuantity", teu, OrgTradePeriodSchema.PAS_TEUQuantity);
				command.AddParameterBasedOnDbColumn("@PAS_RX_NKCurrency", currency, OrgTradePeriodSchema.PAS_RX_NKCurrency);
				command.AddParameterBasedOnDbColumn("@PAS_EstimatedProfit", revenue, OrgTradePeriodSchema.PAS_EstimatedProfit);
				command.AddParameterBasedOnDbColumn("@PAP_RecurrenceType", recurrenceType, OrgTradeProspectSchema.PAP_RecurrenceType);
				command.ExecuteNonQuery();
			}
		}

		void CreateTradedValue(Guid orgPk, string product, string origin, string destination, string tradeMode, string tradeType, DateTime period, decimal weight, string weightUQ, decimal volume, string volumnUQ, decimal teu)
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

INSERT INTO dbo.OrgTradePeriod(PAS_PK, PAS_PA, PAS_OH_Client, PAS_IsTraded, PAS_Period, PAS_Weight, PAS_WeightUQ, PAS_Volume, PAS_VolumeUQ, PAS_TEUQuantity, PAS_SystemCreateTimeUtc, PAS_SystemCreateUser, PAS_SystemLastEditTimeUtc, PAS_SystemLastEditUser)
VALUES (@PAS_PK, @PA_PK, @OH_PK, 1, @PAS_Period, @PAS_Weight, @PAS_WeightUQ, @PAS_Volume, @PAS_VolumeUQ, @PAS_TEUQuantity, GetUtcDate(), 'E', GetUtcDate(), 'E')
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
				command.AddParameterBasedOnDbColumn("@PAS_Weight", weight, OrgTradePeriodSchema.PAS_Weight);
				command.AddParameterBasedOnDbColumn("@PAS_WeightUQ", weightUQ, OrgTradePeriodSchema.PAS_WeightUQ);
				command.AddParameterBasedOnDbColumn("@PAS_Volume", volume, OrgTradePeriodSchema.PAS_Volume);
				command.AddParameterBasedOnDbColumn("@PAS_VolumeUQ", volumnUQ, OrgTradePeriodSchema.PAS_VolumeUQ);
				command.AddParameterBasedOnDbColumn("@PAS_TEUQuantity", teu, OrgTradePeriodSchema.PAS_TEUQuantity);
				command.ExecuteNonQuery();
			}
		}
	}
}
