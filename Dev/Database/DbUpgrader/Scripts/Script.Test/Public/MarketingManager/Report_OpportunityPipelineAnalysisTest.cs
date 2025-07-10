using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MarketingManager;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MarketingManager.Testing
{
	[TestedType(typeof(Report_OpportunityPipelineAnalysis))]
	class Report_OpportunityPipelineAnalysisTest : DbCreateScriptTest
	{
		public void TestRun()
		{
			var org = Guid.NewGuid();
			CreateOrg(org);

			var opp1 = Guid.NewGuid();
			var opp2 = Guid.NewGuid();
			var opp3 = Guid.NewGuid();
			CreateOpportunity(opp1, "O0001", org, new DateTime(2018, 1, 2));
			CreateOpportunity(opp2, "O0002", org, new DateTime(2018, 1, 8));
			CreateOpportunity(opp3, "O0003", org, new DateTime(2018, 1, 20));

			CreateSalesProduct("MPP");

			var sales1 = Guid.NewGuid();
			var sales2 = Guid.NewGuid();
			var sales3 = Guid.NewGuid();
			var detail1 = Guid.NewGuid();
			var detail2 = Guid.NewGuid();
			var detail3 = Guid.NewGuid();
			CreateProspectValue(org, sales1, "MPP", "AUSYD", "NZAKL", detail1, "ACT", "MTH", 1000m, 100m);
			CreateProspectValue(org, sales2, "TRN", "AUSYD", "AUMEL", detail2, "SUC", "MTH", 2000m, 200m);
			CreateProspectValue(org, sales3, "SHP", "CNSHA", "AUBNE", detail3, "UNS", "MTH", 3000m, 300m);

			CreateProspectCommittedValue(detail2, org, 2000m, 200m, new DateTime(2018, 3, 1), false);
			CreateProspectCommittedValue(detail2, org, 2000m, 200m, new DateTime(2018, 4, 1), false);
			CreateProspectCommittedValue(detail2, org, 2000m, 200m, new DateTime(2018, 5, 1), false);
			CreateProspectCommittedValue(detail2, org, 2000m, 200m, new DateTime(2018, 6, 1), false);
			CreateProspectCommittedValue(detail2, org, 2000m, 200m, new DateTime(2018, 7, 1), true);
			CreateProspectCommittedValue(detail2, org, 2000m, 200m, new DateTime(2018, 8, 1), true);
			CreateProspectCommittedValue(detail2, org, 2000m, 200m, new DateTime(2018, 9, 1), true);
			CreateProspectCommittedValue(detail2, org, 2000m, 200m, new DateTime(2018, 10, 1), true);
			CreateProspectCommittedValue(detail2, org, 2000m, 200m, new DateTime(2018, 11, 1), true);
			CreateProspectCommittedValue(detail2, org, 2000m, 200m, new DateTime(2018, 12, 1), true);
			CreateProspectCommittedValue(detail2, org, 2000m, 200m, new DateTime(2019, 1, 1), true);
			CreateProspectCommittedValue(detail2, org, 2000m, 200m, new DateTime(2019, 2, 1), true);

			CreateAssociation(opp1, sales1, OrgSalesSchema.Constants.Prefix);
			CreateAssociation(opp2, detail2, OrgTradeDetailSchema.Constants.Prefix);
			CreateAssociation(opp3, detail3, OrgTradeDetailSchema.Constants.Prefix);

			var result = RunReportFunction().OrderBy(x => x.ID).ToList();
			AssertEquals(3, result.Count);
			AssertReportData(result[0], "O0001", "TESTORGAAA", "MPP", "AUSYD", "NZAKL", 12000m, 0m, 0m, 0m, 12000m, 1200m, 0m, 0m, Guid.Empty);
			AssertReportData(result[1], "O0002", "TESTORGAAA", "TRN", "AUSYD", "AUMEL", 24000m, 8000m, 800m, 16000m, 0m, 0m, 0m, 0m, Guid.Empty);
			AssertReportData(result[2], "O0003", "TESTORGAAA", "SHP", "CNSHA", "AUBNE", 0m, 0m, 0m, 0m, 0m, 0m, 36000m, 3600m, Guid.Empty);

			result = RunReportFunction(new DateTime(2018, 1, 1), new DateTime(2018, 1, 10)).OrderBy(x => x.ID).ToList();
			AssertEquals(2, result.Count);
			AssertReportData(result[0], "O0001", "TESTORGAAA", "MPP", "AUSYD", "NZAKL", 12000m, 0m, 0m, 0m, 12000m, 1200m, 0m, 0m, Guid.Empty);
			AssertReportData(result[1], "O0002", "TESTORGAAA", "TRN", "AUSYD", "AUMEL", 24000m, 8000m, 800m, 16000m, 0m, 0m, 0m, 0m, Guid.Empty);

			var ausyd = (Guid)TestConnection.ExecuteScalar("SELECT TOP 1 RL_PK FROM dbo.RefUNLOCO WHERE RL_Code = 'AUSYD'");
			var aumel = (Guid)TestConnection.ExecuteScalar("SELECT TOP 1 RL_PK FROM dbo.RefUNLOCO WHERE RL_Code = 'AUMEL'");
			var nzakl = (Guid)TestConnection.ExecuteScalar("SELECT TOP 1 RL_PK FROM dbo.RefUNLOCO WHERE RL_Code = 'NZAKL'");

			result = RunReportFunction($"'{ausyd.ToString()}'", $"'{nzakl.ToString()}','{aumel.ToString()}'", null, Guid.Empty).OrderBy(x => x.ID).ToList();
			AssertEquals(2, result.Count);
			AssertReportData(result[0], "O0001", "TESTORGAAA", "MPP", "AUSYD", "NZAKL", 12000m, 0m, 0m, 0m, 12000m, 1200m, 0m, 0m, Guid.Empty);
			AssertReportData(result[1], "O0002", "TESTORGAAA", "TRN", "AUSYD", "AUMEL", 24000m, 8000m, 800m, 16000m, 0m, 0m, 0m, 0m, Guid.Empty);

			var relatedActivityId = Guid.NewGuid();
			CreateRelatedActivity(opp1, relatedActivityId);
			result = RunReportFunction($"'{ausyd.ToString()}'", $"'{nzakl.ToString()}','{aumel.ToString()}'", "CAM", relatedActivityId).OrderBy(x => x.ID).ToList();
			AssertReportData(result[0], "O0001", "TESTORGAAA", "MPP", "AUSYD", "NZAKL", 12000m, 0m, 0m, 0m, 12000m, 1200m, 0m, 0m, Guid.Empty);
			AssertEquals(1, result.Count);
		}

		static void AssertReportData(ReportData data, string id, string clientCode, string salesProductCode, string locationColumn1, string locationColumn2,
			decimal totalEstimatedValue, decimal committedValue, decimal committedTEUValue, decimal forecastValue, decimal pipelineValue, decimal pipelineTEUValue, decimal unsuccessfulValue, decimal unsuccessfulTEUValue, Guid commodityPk)
		{
			CombineAssertions(() =>
			{
				AssertEquals("ID", id, data.ID);
				AssertEquals("ClientCode", clientCode, data.ClientCode);
				AssertEquals("SalesProductCode", salesProductCode, data.SalesProductCode);
				AssertEquals("LocationColumn1", locationColumn1, data.LocationColumn1);
				AssertEquals("LocationColumn2", locationColumn2, data.LocationColumn2);

				AssertEquals("AnnualEstimatedValue", totalEstimatedValue, data.TotalEstimateValue);
				AssertEquals("AnnualCommittedValue", committedValue, data.CommittedValue);
				AssertEquals("AnnualCommittedTEUValue", committedTEUValue, data.CommittedTEUValue);
				AssertEquals("AnnualForecastValue", forecastValue, data.ForecastValue);
				AssertEquals("AnnualPipelineValue", pipelineValue, data.PipelineValue);
				AssertEquals("AnnualPipelineTEUValue", pipelineTEUValue, data.PipelineTEUValue);
				AssertEquals("AnnualUnsuccessfulValue", unsuccessfulValue, data.UnsuccessfulValue);
				AssertEquals("AnnualUnsuccessfulTEUValue", unsuccessfulTEUValue, data.UnsuccessfulTEUValue);

				AssertEquals("CommodityPk", commodityPk, data.CommodityPk);
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

		void CreateOpportunity(Guid oppPk, string oppId, Guid orgPk, DateTime createTimeUtc)
		{
			var sql = @"INSERT INTO dbo.OrgOpportunity (P8_PK, P8_OpportunityID, P8_OH, P8_GC, P8_SystemCreateTimeUtc, P8_SystemCreateUser, P8_SystemLastEditTimeUtc, P8_SystemLastEditUser)
VALUES (@P8_PK, @P8_OpportunityID, @P8_OH, @P8_GC, @P8_SystemCreateTimeUtc, 'E', GetUtcDate(), 'E')";
			using (var command = TestConnection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@P8_PK", oppPk, OrgOpportunitySchema.PK);
				command.AddParameterBasedOnDbColumn("@P8_OpportunityID", oppId, OrgOpportunitySchema.P8_OpportunityID);
				command.AddParameterBasedOnDbColumn("@P8_OH", orgPk, OrgOpportunitySchema.P8_OH);
				command.AddParameterBasedOnDbColumn("@P8_GC", TestDbHelper.DefaultCompanyPK, OrgOpportunitySchema.P8_GC);
				command.AddParameterBasedOnDbColumn("@P8_SystemCreateTimeUtc", createTimeUtc, OrgOpportunitySchema.P8_SystemCreateTimeUtc);
				command.ExecuteNonQuery();
			}
		}

		void CreateSalesProduct(string productCode)
		{
			var sql =
@"
INSERT INTO dbo.OrgSalesProduct(MP_PK, MP_Code, MP_Name, MP_IsSystemDefined, MP_FormLayoutData, MP_SystemCreateTimeUtc, MP_SystemCreateUser, MP_SystemLastEditTimeUtc, MP_SystemLastEditUser) 
SELECT NEWID(), @MP_Code, 'Test Product', 0, MP_FormLayoutData, GETUTCDATE(), 'E', GETUTCDATE(), 'E'
FROM dbo.OrgSalesProduct
WHERE MP_Code = 'SHP'
";
			using (var command = TestConnection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@MP_Code", productCode, OrgSalesProductSchema.MP_Code);
				command.ExecuteNonQuery();
			}
		}

		void CreateProspectValue(Guid orgPk, Guid salesPk, string product, string origin, string destination, Guid detailPk, string detailStatus, string recurrenceType, decimal estimatedProfit, decimal teuQuantity)
		{
			var sql =
@"
DECLARE @OriginPk UNIQUEIDENTIFIER = (SELECT TOP 1 RL_PK FROM dbo.RefUNLOCO WHERE RL_Code = @Origin)
DECLARE @DestinationPk UNIQUEIDENTIFIER = (SELECT TOP 1 RL_PK FROM dbo.RefUNLOCO WHERE RL_Code = @Destination)
DECLARE @ProductPk UNIQUEIDENTIFIER = (SELECT TOP 1 MP_PK FROM dbo.OrgSalesProduct WHERE MP_Code = @Product)

INSERT INTO dbo.OrgSales (OW_PK, OW_MP_Product, OW_OriginID, OW_OriginTableCode, OW_DestinationID, OW_DestinationTableCode, OW_OH_Primary, OW_IsTraded, OW_SystemCreateTimeUtc, OW_SystemCreateUser, OW_SystemLastEditTimeUtc, OW_SystemLastEditUser) 
VALUES (@OW_PK, @ProductPk, @OriginPk, 'RL', @DestinationPk, 'RL', @OH_PK, 0, GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.OrgTradeDetail(PA_PK, PA_OW, PA_Status, PA_SystemCreateTimeUtc, PA_SystemCreateUser, PA_SystemLastEditTimeUtc, PA_SystemLastEditUser)
VALUES (@PA_PK, @OW_PK, @PA_Status, GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.OrgTradeProspect(PAP_PK, PAP_PA, PAP_RecurrenceType, PAP_SystemCreateTimeUtc, PAP_SystemCreateUser, PAP_SystemLastEditTimeUtc, PAP_SystemLastEditUser)
VALUES (NEWID(), @PA_PK, @PAP_RecurrenceType, GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.OrgTradePeriod(PAS_PK, PAS_PA, PAS_OH_Client, PAS_IsTraded, PAS_EstimatedProfit, PAS_TEUQuantity, PAS_SystemCreateTimeUtc, PAS_SystemCreateUser, PAS_SystemLastEditTimeUtc, PAS_SystemLastEditUser)
VALUES (NEWID(), @PA_PK, @OH_PK, 0, @PAS_EstimatedProfit, @PAS_TEUQuantity, GetUtcDate(), 'E', GetUtcDate(), 'E')
";
			using (var command = TestConnection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@OW_PK", salesPk, OrgSalesSchema.PK);
				command.AddParameterBasedOnDbColumn("@OH_PK", orgPk, OrgHeaderSchema.PK);
				command.AddParameter("@Origin", SqlDbType.VarChar, origin);
				command.AddParameter("@Destination", SqlDbType.VarChar, destination);
				command.AddParameter("@Product", SqlDbType.VarChar, product);
				command.AddParameterBasedOnDbColumn("@PA_PK", detailPk, OrgTradeDetailSchema.PK);
				command.AddParameterBasedOnDbColumn("@PA_Status", detailStatus, OrgTradeDetailSchema.PA_Status);
				command.AddParameterBasedOnDbColumn("@PAP_RecurrenceType", recurrenceType, OrgTradeProspectSchema.PAP_RecurrenceType);
				command.AddParameterBasedOnDbColumn("@PAS_EstimatedProfit", estimatedProfit, OrgTradePeriodSchema.PAS_EstimatedProfit);
				command.AddParameterBasedOnDbColumn("@PAS_TEUQuantity", teuQuantity, OrgTradePeriodSchema.PAS_TEUQuantity);
				command.ExecuteNonQuery();
			}
		}

		void CreateProspectCommittedValue(Guid detailPk, Guid orgPk, decimal estimatedProfit, decimal teuQuantity, DateTime period, bool isForecast)
		{
			var sql =
@"INSERT INTO dbo.OrgTradePeriod(PAS_PK, PAS_PA, PAS_OH_Client, PAS_Period, PAS_EstimatedProfit, PAS_TEUQuantity, PAS_IsForecast, PAS_SystemCreateTimeUtc, PAS_SystemCreateUser, PAS_SystemLastEditTimeUtc, PAS_SystemLastEditUser)
VALUES (NEWID(), @PA_PK, @OH_PK, @PAS_Period, @PAS_EstimatedProfit, @PAS_TEUQuantity, @PAS_IsForecast, GetUtcDate(), 'E', GetUtcDate(), 'E')
";
			using (var command = TestConnection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@OH_PK", orgPk, OrgHeaderSchema.PK);
				command.AddParameterBasedOnDbColumn("@PA_PK", detailPk, OrgTradeDetailSchema.PK);
				command.AddParameterBasedOnDbColumn("@PAS_Period", period, OrgTradePeriodSchema.PAS_Period);
				command.AddParameterBasedOnDbColumn("@PAS_EstimatedProfit", estimatedProfit, OrgTradePeriodSchema.PAS_EstimatedProfit);
				command.AddParameterBasedOnDbColumn("@PAS_TEUQuantity", teuQuantity, OrgTradePeriodSchema.PAS_TEUQuantity);
				command.AddParameterBasedOnDbColumn("@PAS_IsForecast", isForecast, OrgTradePeriodSchema.PAS_IsForecast);
				command.ExecuteNonQuery();
			}
		}

		void CreateAssociation(Guid oppPk, Guid salesOrDetailPk, string salesOrDetailTableCode)
		{
			var sql =
@"
INSERT INTO dbo.OrgSalesValueAssociationPivot (SVP_PK, SVP_ActivityId, SVP_ActivityTableCode, SVP_TradeId, SVP_TradeTableCode, SVP_SystemCreateTimeUtc, SVP_SystemCreateUser, SVP_SystemLastEditTimeUtc, SVP_SystemLastEditUser) 
VALUES (NEWID(), @SVP_ActivityId, 'P8', @SVP_TradeId, @SVP_TradeTableCode, GetUtcDate(), 'E', GetUtcDate(), 'E')
";
			using (var command = TestConnection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@SVP_ActivityId", oppPk, OrgSalesValueAssociationPivotSchema.SVP_ActivityId);
				command.AddParameterBasedOnDbColumn("@SVP_TradeId", salesOrDetailPk, OrgSalesValueAssociationPivotSchema.SVP_TradeId);
				command.AddParameterBasedOnDbColumn("@SVP_TradeTableCode", salesOrDetailTableCode, OrgSalesValueAssociationPivotSchema.SVP_TradeTableCode);
				command.ExecuteNonQuery();
			}
		}

		void CreateRelatedActivity(Guid oppPk, Guid relatedActivityId)
		{
			var sql =
@"
INSERT INTO dbo.RelatedActivityPivot
(RAP_PK, RAP_ParentActivityTableCode, RAP_ParentActivityID, RAP_ChildActivityTableCode, RAP_ChildActivityID, RAP_SalesRelationTreeID, RAP_SystemCreateTimeUtc, RAP_SystemCreateUser, RAP_SystemLastEditTimeUtc, RAP_SystemLastEditUser)
VALUES
(newid(), 'P8', @RAP_ParentActivityId, 'G0', @RAP_ChildActivityID, @RAP_SalesRelationTreeID, GetUtcDate(), 'E', GetUtcDate(), 'E')
";
			using (var command = TestConnection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@RAP_ParentActivityId", oppPk, RelatedActivityPivotSchema.RAP_ParentActivityID);
				command.AddParameterBasedOnDbColumn("@RAP_ChildActivityID", relatedActivityId, RelatedActivityPivotSchema.RAP_ChildActivityID);
				command.AddParameterBasedOnDbColumn("@RAP_SalesRelationTreeID", relatedActivityId, RelatedActivityPivotSchema.RAP_SalesRelationTreeID);
				command.ExecuteNonQuery();
			}
		}

		List<ReportData> RunReportFunction()
		{
			var result = new List<ReportData>();

			var sql =
@"SELECT
	[ID], [ClientCode], [SalesProductCode], [LocationColumn1], [LocationColumn2], [AnnualEstimatedValue], [AnnualCommittedValue], [AnnualCommittedTEUValue], [AnnualForecastValue], [AnnualPipelineValue], [AnnualPipelineTEUValue], [AnnualUnsuccessfulValue], [AnnualUnsuccessfulTEUValue], [CommodityPk]
FROM 
	Report_OpportunityPipelineAnalysis('', '', '', '', 'Y', 'Y', '', '', '', '', '', @CompanyPk, 'ANY', NULL, '', DEFAULT, DEFAULT, NULL)
";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameter("@CompanyPk", SqlDbType.UniqueIdentifier, TestDbHelper.DefaultCompanyPK);
				ReadData(result, command);
			}

			return result;
		}

		List<ReportData> RunReportFunction(DateTime createTimeFrom, DateTime createTimeTo)
		{
			var result = new List<ReportData>();

			var sql =
@"SELECT
	[ID], [ClientCode], [SalesProductCode], [LocationColumn1], [LocationColumn2], [AnnualEstimatedValue], [AnnualCommittedValue], [AnnualCommittedTEUValue], [AnnualForecastValue], [AnnualPipelineValue], [AnnualPipelineTEUValue], [AnnualUnsuccessfulValue], [AnnualUnsuccessfulTEUValue], [CommodityPk]
FROM 
	Report_OpportunityPipelineAnalysis('', '', '', '', 'Y', 'Y', '', '', '', '', '', @CompanyPk, 'ANY', NULL, '', @CreateTimeFrom, @CreateTimeTo, NULL)
";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameter("@CompanyPk", SqlDbType.UniqueIdentifier, TestDbHelper.DefaultCompanyPK);
				command.AddParameter("@CreateTimeFrom", SqlDbType.SmallDateTime, createTimeFrom);
				command.AddParameter("@CreateTimeTo", SqlDbType.SmallDateTime, createTimeTo);
				ReadData(result, command);
			}

			return result;
		}

		List<ReportData> RunReportFunction(string originPKs, string destinationPKs, string salesRelation, Guid relatedActivityId)
		{
			var result = new List<ReportData>();

			var sql =
@"SELECT
	[ID], [ClientCode], [SalesProductCode], [LocationColumn1], [LocationColumn2], [AnnualEstimatedValue], [AnnualCommittedValue], [AnnualCommittedTEUValue], [AnnualForecastValue], [AnnualPipelineValue], [AnnualPipelineTEUValue], [AnnualUnsuccessfulValue], [AnnualUnsuccessfulTEUValue], [CommodityPk]
FROM 
	Report_OpportunityPipelineAnalysis('', '', @OriginPKs, @DestinationPKs, 'Y', 'Y', '', '', '', '', '', @CompanyPk, 'ANY', @SalesRelation, '', DEFAULT, DEFAULT, @RelatedActivityID)
";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameter("@CompanyPk", SqlDbType.UniqueIdentifier, TestDbHelper.DefaultCompanyPK);
				command.AddParameter("@OriginPKs", SqlDbType.VarChar, originPKs);
				command.AddParameter("@DestinationPKs", SqlDbType.VarChar, destinationPKs);
				command.AddParameter("@SalesRelation", SqlDbType.VarChar, salesRelation ?? (object)DBNull.Value);
				command.AddParameter("@RelatedActivityID", SqlDbType.UniqueIdentifier, relatedActivityId);
				ReadData(result, command);
			}

			return result;
		}

		static void ReadData(List<ReportData> result, DbCommand command)
		{
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					var data = new ReportData()
					{
						ID = GetStringValue(reader, "ID"),
						ClientCode = GetStringValue(reader, "ClientCode"),
						SalesProductCode = GetStringValue(reader, "SalesProductCode"),
						LocationColumn1 = GetStringValue(reader, "LocationColumn1"),
						LocationColumn2 = GetStringValue(reader, "LocationColumn2"),
						TotalEstimateValue = GetDecimalValue(reader, "AnnualEstimatedValue"),
						CommittedValue = GetDecimalValue(reader, "AnnualCommittedValue"),
						CommittedTEUValue = GetDecimalValue(reader, "AnnualCommittedTEUValue"),
						ForecastValue = GetDecimalValue(reader, "AnnualForecastValue"),
						PipelineValue = GetDecimalValue(reader, "AnnualPipelineValue"),
						PipelineTEUValue = GetDecimalValue(reader, "AnnualPipelineTEUValue"),
						UnsuccessfulValue = GetDecimalValue(reader, "AnnualUnsuccessfulValue"),
						UnsuccessfulTEUValue = GetDecimalValue(reader, "AnnualUnsuccessfulTEUValue"),
						CommodityPk = GetGuidValue(reader, "CommodityPk"),
					};
					result.Add(data);
				}
			}
		}

		static string GetStringValue(IDataReader reader, string columnName)
		{
			var data = reader[columnName];
			return data == DBNull.Value ? string.Empty : (string)data;
		}
		static decimal GetDecimalValue(IDataReader reader, string columnName)
		{
			var data = reader[columnName];
			return data == DBNull.Value ? decimal.Zero : (decimal)data;
		}
		static Guid GetGuidValue(IDataReader reader, string columnName)
		{
			var data = reader[columnName];
			return data == DBNull.Value ? Guid.Empty : (Guid)data;
		}

		struct ReportData
		{
			public string ID;
			public string ClientCode;
			public string SalesProductCode;
			public string LocationColumn1;
			public string LocationColumn2;
			public decimal TotalEstimateValue;
			public decimal CommittedValue;
			public decimal CommittedTEUValue;
			public decimal ForecastValue;
			public decimal PipelineValue;
			public decimal PipelineTEUValue;
			public decimal UnsuccessfulValue;
			public decimal UnsuccessfulTEUValue;
			public Guid CommodityPk;
		}
	}
}
