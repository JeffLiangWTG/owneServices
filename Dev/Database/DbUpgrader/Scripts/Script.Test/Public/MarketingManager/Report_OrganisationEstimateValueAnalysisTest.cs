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
	[TestedType(typeof(Report_OrganisationEstimateValueAnalysis))]
	class Report_OrganisationEstimateValueAnalysisTest : DbCreateScriptTest
	{
		public void TestRun()
		{
			CreateAccountingPeriod();

			var org1 = Guid.NewGuid();
			CreateOrg(org1, "TSTAAA");

			CreateStaff("AAA");
			CreateStaff("BBB");

			var opp1 = Guid.NewGuid();
			var opp2 = Guid.NewGuid();
			CreateOpportunity(opp1, "O00003001", org1, "AAA", new DateTime(2017, 9, 13));
			CreateOpportunity(opp2, "O00003002", org1, "BBB", new DateTime(2018, 1, 1));

			var detail1a = CreateProspectValue(org1, "SHP", "AUSYD", "NZAKL", "SUC", "MTH", "AUD", 1000m, new DateTime(2017, 10, 1), 6, false);
			var detail1b = CreateProspectValue(org1, "SHP", "AUMEL", "NZAKL", "SUC", "MTH", "AUD", 2000m, new DateTime(2018, 2, 1), 3, true);
			var detail2a = CreateProspectValue(org1, "SHP", "CNSHA", "AUBNE", "SUC", "MTH", "AUD", 3000m, new DateTime(2018, 1, 1), 6, false);
			var detail2b = CreateProspectValue(org1, "SHP", "CNSHA", "AUBNE", "SUC", "MTH", "AUD", 1500m, new DateTime(2018, 3, 1), 2, true);
			var detail2c = CreateProspectValue(org1, "SHP", "CNSHA", "AUBNE", "ACT", "MTH", "AUD", 500m, new DateTime(2018, 1, 1), 1, false);
			var detail2d = CreateProspectValue(org1, "SHP", "CNSHA", "AUBNE", "UNS", "MTH", "AUD", 2000m, new DateTime(2018, 1, 1), 1, false);

			CreateAssociation(opp1, detail1a);
			CreateAssociation(opp1, detail1b);
			CreateAssociation(opp2, detail2a);
			CreateAssociation(opp2, detail2b);
			CreateAssociation(opp2, detail2c);
			CreateAssociation(opp2, detail2d);

			var result = RunReportFunction(2018, isGroupedBySalesRep: true).OrderBy(x => x.ClientCode).ThenBy(x => x.SalesRepCode).ThenBy(x => x.ValueTypeOrder).ToList();
			AssertEquals(4, result.Count);
			AssertReportPeriodData(result[0], "TSTAAA", "AAA", "Committed and Forecast", 12000m, 0m, 0m, 0m, 1000m, 1000m, 1000m, 1000m, 3000m, 3000m, 2000m, 0m, 0m);
			AssertReportPeriodData(result[1], "TSTAAA", "AAA", "Committed", 6000m, 0m, 0m, 0m, 1000m, 1000m, 1000m, 1000m, 1000m, 1000m, 0m, 0m, 0m);
			AssertReportPeriodData(result[2], "TSTAAA", "BBB", "Committed and Forecast", 21000m, 0m, 0m, 0m, 0m, 0m, 0m, 3000m, 3000m, 4500m, 4500m, 3000m, 3000m);
			AssertReportPeriodData(result[3], "TSTAAA", "BBB", "Committed", 18000m, 0m, 0m, 0m, 0m, 0m, 0m, 3000m, 3000m, 3000m, 3000m, 3000m, 3000m);

			AssertReportOrgData(result[0], "TSTAAA", "AAA", 6000m, 12000m, 0m, 0m);
			AssertReportOrgData(result[1], "TSTAAA", "AAA", 6000m, 12000m, 0m, 0m);
			AssertReportOrgData(result[2], "TSTAAA", "BBB", 18000m, 21000m, 6000m, 24000m);
			AssertReportOrgData(result[3], "TSTAAA", "BBB", 18000m, 21000m, 6000m, 24000m);

			result = RunReportFunction(2018, isGroupedBySalesRep: false).OrderBy(x => x.ClientCode).ThenBy(x => x.ValueTypeOrder).ToList();
			AssertEquals(2, result.Count);
			AssertReportPeriodData(result[0], "TSTAAA", "", "Committed and Forecast", 33000m, 0m, 0m, 0m, 1000m, 1000m, 1000m, 4000m, 6000m, 7500m, 6500m, 3000m, 3000m);
			AssertReportPeriodData(result[1], "TSTAAA", "", "Committed", 24000m, 0m, 0m, 0m, 1000m, 1000m, 1000m, 4000m, 4000m, 4000m, 3000m, 3000m, 3000m);
			AssertReportOrgData(result[0], "TSTAAA", "", 24000m, 33000m, 6000m, 24000m);
			AssertReportOrgData(result[1], "TSTAAA", "", 24000m, 33000m, 6000m, 24000m);

			var detail3 = CreateProspectValue(org1, "SHP", "AUSYD", "NZAKL", "SUC", "MTH", "AUD", 1000m, new DateTime(2017, 1, 1), 3, false);
			CreateAssociation(opp2, detail3);
			result = RunReportFunction(2017, isGroupedBySalesRep: false).OrderBy(x => x.ClientCode).ThenBy(x => x.SalesRepCode).ThenBy(x => x.ValueTypeOrder).ToList();
			AssertEquals(2, result.Count);
			AssertReportPeriodData(result[0], "TSTAAA", "", "Committed and Forecast", 3000m, 0m, 0m, 0m, 0m, 0m, 0m, 1000m, 1000m, 1000m, 0m, 0m, 0m);
			AssertReportPeriodData(result[1], "TSTAAA", "", "Committed", 3000m, 0m, 0m, 0m, 0m, 0m, 0m, 1000m, 1000m, 1000m, 0m, 0m, 0m);
			AssertEquals("CommittedYTD", 3000m, result[0].CommittedYTD);
			AssertEquals("Pipeline", 0m, result[0].Pipeline);
			AssertEquals("Unsuccessful", 0m, result[0].Unsuccessful);
			AssertEquals("CommittedYTD", 3000m, result[1].CommittedYTD);
			AssertEquals("Pipeline", 0m, result[1].Pipeline);
			AssertEquals("Unsuccessful", 0m, result[1].Unsuccessful);
		}

		static void AssertReportOrgData(ReportData data, string clientCode, string salesRepCode, decimal committedCFY, decimal committedAndForecastCFY, decimal pipeline, decimal unsuccessful)
		{
			CombineAssertions(() =>
			{
				AssertEquals("ClientCode", clientCode, data.ClientCode);
				AssertEquals("SalesRepCode", salesRepCode, data.SalesRepCode);
				AssertEquals("CommittedCFY", committedCFY, data.CommittedCFY);
				AssertEquals("CommittedAndForecastCFY", committedAndForecastCFY, data.CommittedAndForecastCFY);
				AssertEquals("Pipeline", pipeline, data.Pipeline);
				AssertEquals("Unsuccessful", unsuccessful, data.Unsuccessful);
			});
		}

		static void AssertReportPeriodData(ReportData data, string clientCode, string salesRepCode, string valueType, decimal totalValue,
			decimal month1Value, decimal month2Value, decimal month3Value, decimal month4Value, decimal month5Value, decimal month6Value,
			decimal month7Value, decimal month8Value, decimal month9Value, decimal month10Value, decimal month11Value, decimal month12Value)
		{
			CombineAssertions(() =>
			{
				AssertEquals("ClientCode", clientCode, data.ClientCode);
				AssertEquals("SalesRepCode", salesRepCode, data.SalesRepCode);
				AssertEquals("ValueType", valueType, data.ValueType);
				AssertEquals("Total12MonthsValue", totalValue, data.Total12MonthsValue);
				AssertEquals("Month1Value", month1Value, data.Month1Value);
				AssertEquals("Month2Value", month2Value, data.Month2Value);
				AssertEquals("Month3Value", month3Value, data.Month3Value);
				AssertEquals("Month4Value", month4Value, data.Month4Value);
				AssertEquals("Month5Value", month5Value, data.Month5Value);
				AssertEquals("Month6Value", month6Value, data.Month6Value);
				AssertEquals("Month7Value", month7Value, data.Month7Value);
				AssertEquals("Month8Value", month8Value, data.Month8Value);
				AssertEquals("Month9Value", month9Value, data.Month9Value);
				AssertEquals("Month10Value", month10Value, data.Month10Value);
				AssertEquals("Month11Value", month11Value, data.Month11Value);
				AssertEquals("Month12Value", month12Value, data.Month12Value);
			});
		}

		void CreateAccountingPeriod()
		{
			var sql =
@"
INSERT INTO dbo.AccPeriodManagement
	(AM_PK, AM_Period, AM_Year, AM_StartDate, AM_EndDate, AM_GC_Company) 
VALUES
	(NEWID(), '201801', 2018, '2017-07-01 00:00:00.000', '2017-07-31 23:59:00.000', @Company),
	(NEWID(), '201802', 2018, '2017-08-01 00:00:00.000', '2017-08-31 23:59:00.000', @Company),
	(NEWID(), '201803', 2018, '2017-09-01 00:00:00.000', '2017-09-30 23:59:00.000', @Company),
	(NEWID(), '201804', 2018, '2017-10-01 00:00:00.000', '2017-10-31 23:59:00.000', @Company),
	(NEWID(), '201805', 2018, '2017-11-01 00:00:00.000', '2017-11-30 23:59:00.000', @Company),
	(NEWID(), '201806', 2018, '2017-12-01 00:00:00.000', '2017-12-31 23:59:00.000', @Company),
	(NEWID(), '201807', 2018, '2018-01-01 00:00:00.000', '2018-01-31 23:59:00.000', @Company),
	(NEWID(), '201808', 2018, '2018-02-01 00:00:00.000', '2018-02-28 23:59:00.000', @Company),
	(NEWID(), '201809', 2018, '2018-03-01 00:00:00.000', '2018-03-31 23:59:00.000', @Company),
	(NEWID(), '201810', 2018, '2018-04-01 00:00:00.000', '2018-04-30 23:59:00.000', @Company),
	(NEWID(), '201811', 2018, '2018-05-01 00:00:00.000', '2018-05-31 23:59:00.000', @Company),
	(NEWID(), '201812', 2018, '2018-06-01 00:00:00.000', '2018-06-30 23:59:00.000', @Company)
";
			using (var command = TestConnection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@Company", TestDbHelper.DefaultCompanyPK, AccPeriodManagementSchema.AM_GC_Company);
				command.ExecuteNonQuery();
			}
		}

		void CreateStaff(string staffCode)
		{
			var sql = "INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES (NEWID(), @StaffCode, @StaffCode, GETUTCDATE(), 'E', GETUTCDATE(), 'E')";
			using (var command = TestConnection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@StaffCode", staffCode, GlbStaffSchema.GS_Code);
				command.ExecuteNonQuery();
			}
		}

		void CreateOrg(Guid orgPk, string orgCode)
		{
			var sql = @"INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@OH_PK, @OH_Code)";
			using (var command = TestConnection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@OH_PK", orgPk, OrgHeaderSchema.PK);
				command.AddParameterBasedOnDbColumn("@OH_Code", orgCode, OrgHeaderSchema.OH_Code);
				command.ExecuteNonQuery();
			}
		}

		void CreateOpportunity(Guid oppPk, string oppId, Guid orgPk, string salesPerson, DateTime createTimeUtc)
		{
			var sql = @"INSERT INTO dbo.OrgOpportunity (P8_PK, P8_OpportunityID, P8_OH, P8_GC, P8_GS_NKPrimarySalesPerson, P8_SystemCreateTimeUtc, P8_SystemCreateUser, P8_SystemLastEditTimeUtc, P8_SystemLastEditUser)
VALUES (@P8_PK, @P8_OpportunityID, @P8_OH, @P8_GC, @P8_GS_NKPrimarySalesPerson, @P8_SystemCreateTimeUtc, 'E', GetUtcDate(), 'E')";
			using (var command = TestConnection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@P8_PK", oppPk, OrgOpportunitySchema.PK);
				command.AddParameterBasedOnDbColumn("@P8_OpportunityID", oppId, OrgOpportunitySchema.P8_OpportunityID);
				command.AddParameterBasedOnDbColumn("@P8_OH", orgPk, OrgOpportunitySchema.P8_OH);
				command.AddParameterBasedOnDbColumn("@P8_GC", TestDbHelper.DefaultCompanyPK, OrgOpportunitySchema.P8_GC);
				command.AddParameterBasedOnDbColumn("@P8_GS_NKPrimarySalesPerson", salesPerson, OrgOpportunitySchema.P8_GS_NKPrimarySalesPerson);
				command.AddParameterBasedOnDbColumn("@P8_SystemCreateTimeUtc", createTimeUtc, OrgOpportunitySchema.P8_SystemCreateTimeUtc);
				command.ExecuteNonQuery();
			}
		}

		Guid CreateProspectValue(Guid orgPk, string product, string origin, string destination,
			string status, string recurrenceType,
			string currency, decimal estimatedProfit, DateTime periodStart, int numOfPeriods, bool isForecast)
		{
			var detailPk = Guid.NewGuid();

			var sql =
@"
DECLARE @OriginPk UNIQUEIDENTIFIER = (SELECT TOP 1 RL_PK FROM dbo.RefUNLOCO WHERE RL_Code = @Origin)
DECLARE @DestinationPk UNIQUEIDENTIFIER = (SELECT TOP 1 RL_PK FROM dbo.RefUNLOCO WHERE RL_Code = @Destination)
DECLARE @ProductPk UNIQUEIDENTIFIER = (SELECT TOP 1 MP_PK FROM dbo.OrgSalesProduct WHERE MP_Code = @Product)
DECLARE @OW_PK UNIQUEIDENTIFIER = (SELECT NEWID())

INSERT INTO dbo.OrgSales (OW_PK, OW_MP_Product, OW_OriginID, OW_OriginTableCode, OW_DestinationID, OW_DestinationTableCode, OW_OH_Primary, OW_IsTraded, OW_SystemCreateTimeUtc, OW_SystemCreateUser, OW_SystemLastEditTimeUtc, OW_SystemLastEditUser) 
VALUES (@OW_PK, @ProductPk, @OriginPk, 'RL', @DestinationPk, 'RL', @OH_PK, 0, GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.OrgTradeDetail(PA_PK, PA_OW, PA_Status, PA_SystemCreateTimeUtc, PA_SystemCreateUser, PA_SystemLastEditTimeUtc, PA_SystemLastEditUser)
VALUES (@PA_PK, @OW_PK, @PA_Status, GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.OrgTradeProspect(PAP_PK, PAP_PA, PAP_RecurrenceType, PAP_SystemCreateTimeUtc, PAP_SystemCreateUser, PAP_SystemLastEditTimeUtc, PAP_SystemLastEditUser)
VALUES (NEWID(), @PA_PK, @PAP_RecurrenceType, GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.OrgTradePeriod(PAS_PK, PAS_PA, PAS_OH_Client, PAS_IsTraded, PAS_EstimatedProfit, PAS_RX_NKCurrency, PAS_IsForecast, PAS_Period, PAS_SystemCreateTimeUtc, PAS_SystemCreateUser, PAS_SystemLastEditTimeUtc, PAS_SystemLastEditUser)
SELECT
	NEWID(), @PA_PK, @OH_PK, 0, @PAS_EstimatedProfit, @PAS_RX_NKCurrency, @PAS_IsForecast,
	PAS_Period = CASE WHEN @NumOfPeriods > 1 THEN DATEADD(MM, PeriodMonth.n, @PeriodStart) ELSE NULL END,
	GetUtcDate(), 'E', GetUtcDate(), 'E'
FROM
	(
		SELECT n FROM (VALUES(0),(1),(2),(3),(4),(5),(6),(7),(8),(9),(10),(11)) months(n)
	) PeriodMonth
WHERE
	n < @NumOfPeriods
";
			using (var command = TestConnection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@OH_PK", orgPk, OrgHeaderSchema.PK);
				command.AddParameter("@Origin", SqlDbType.VarChar, origin);
				command.AddParameter("@Destination", SqlDbType.VarChar, destination);
				command.AddParameter("@Product", SqlDbType.VarChar, product);
				command.AddParameterBasedOnDbColumn("@PA_PK", detailPk, OrgTradeDetailSchema.PK);
				command.AddParameterBasedOnDbColumn("@PA_Status", status, OrgTradeDetailSchema.PA_Status);
				command.AddParameterBasedOnDbColumn("@PAP_RecurrenceType", recurrenceType, OrgTradeProspectSchema.PAP_RecurrenceType);
				command.AddParameterBasedOnDbColumn("@PAS_EstimatedProfit", estimatedProfit, OrgTradePeriodSchema.PAS_EstimatedProfit);
				command.AddParameterBasedOnDbColumn("@PAS_RX_NKCurrency", currency, OrgTradePeriodSchema.PAS_RX_NKCurrency);
				command.AddParameterBasedOnDbColumn("@PAS_IsForecast", isForecast, OrgTradePeriodSchema.PAS_IsForecast);
				command.AddParameter("@PeriodStart", SqlDbType.SmallDateTime, periodStart);
				command.AddParameter("@NumOfPeriods", SqlDbType.Int, numOfPeriods);
				command.ExecuteNonQuery();
			}

			return detailPk;
		}

		void CreateAssociation(Guid oppPk, Guid detailPk)
		{
			var sql =
@"
INSERT INTO dbo.OrgSalesValueAssociationPivot (SVP_PK, SVP_ActivityId, SVP_ActivityTableCode, SVP_TradeId, SVP_TradeTableCode, SVP_SystemCreateTimeUtc, SVP_SystemCreateUser, SVP_SystemLastEditTimeUtc, SVP_SystemLastEditUser)
VALUES (NEWID(), @SVP_ActivityId, 'P8', @SVP_TradeId, 'PA', GetUtcDate(), 'E', GetUtcDate(), 'E')
";
			using (var command = TestConnection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@SVP_ActivityId", oppPk, OrgSalesValueAssociationPivotSchema.SVP_ActivityId);
				command.AddParameterBasedOnDbColumn("@SVP_TradeId", detailPk, OrgSalesValueAssociationPivotSchema.SVP_TradeId);
				command.ExecuteNonQuery();
			}
		}

		List<ReportData> RunReportFunction(int year, bool isGroupedBySalesRep)
		{
			var result = new List<ReportData>();

			var sql =
@"SELECT
	[ClientCode], [SalesPersonCode], [ValueType], [ValueTypeOrder], 
	[Total12MonthsValue], 
	[Month1Value], [Month2Value], [Month3Value], [Month4Value], [Month5Value], [Month6Value], 
	[Month7Value], [Month8Value], [Month9Value], [Month10Value], [Month11Value], [Month12Value],
	[CommittedYTD], [CommittedCFY], [CommittedAndForecastCFY], [CommittedNFY], [CommittedAndForecastNFY],
	[Pipeline], [Unsuccessful]
FROM 
	Report_OrganisationEstimateValueAnalysis(@CompanyPk, @OrgPks, 1, @SaleRepPks, 1, @CompanyPk, @FinancialYear, 'Y', 'Y', @GroupByOption)
";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameter("@CompanyPk", SqlDbType.UniqueIdentifier, TestDbHelper.DefaultCompanyPK);
				command.AddParameter("@FinancialYear", SqlDbType.Int, year);
				command.AddParameter("@GroupByOption", SqlDbType.VarChar, isGroupedBySalesRep ? "Sales Person" : "Organization");

				var orgPks = new TVPParamInfo("@OrgPks", "dbo.TVP_UNIQUEIDENTIFIER", typeof(Guid), Array.Empty<object>());
				orgPks.AddTVPParameters(command);
				var salesRepPks = new TVPParamInfo("@SaleRepPks", "dbo.TVP_UNIQUEIDENTIFIER", typeof(Guid), Array.Empty<object>());
				salesRepPks.AddTVPParameters(command);

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
						ClientCode = GetStringValue(reader, "ClientCode"),
						SalesRepCode = GetStringValue(reader, "SalesPersonCode"),
						ValueType = GetStringValue(reader, "ValueType"),
						ValueTypeOrder = (int)reader["ValueTypeOrder"],
						Total12MonthsValue = GetDecimalValue(reader, "Total12MonthsValue"),
						Month1Value = GetDecimalValue(reader, "Month1Value"),
						Month2Value = GetDecimalValue(reader, "Month2Value"),
						Month3Value = GetDecimalValue(reader, "Month3Value"),
						Month4Value = GetDecimalValue(reader, "Month4Value"),
						Month5Value = GetDecimalValue(reader, "Month5Value"),
						Month6Value = GetDecimalValue(reader, "Month6Value"),
						Month7Value = GetDecimalValue(reader, "Month7Value"),
						Month8Value = GetDecimalValue(reader, "Month8Value"),
						Month9Value = GetDecimalValue(reader, "Month9Value"),
						Month10Value = GetDecimalValue(reader, "Month10Value"),
						Month11Value = GetDecimalValue(reader, "Month11Value"),
						Month12Value = GetDecimalValue(reader, "Month12Value"),
						CommittedYTD = GetDecimalValue(reader, "CommittedYTD"),
						CommittedCFY = GetDecimalValue(reader, "CommittedCFY"),
						CommittedAndForecastCFY = GetDecimalValue(reader, "CommittedAndForecastCFY"),
						CommittedNFY = GetDecimalValue(reader, "CommittedNFY"),
						CommittedAndForecastNFY = GetDecimalValue(reader, "CommittedAndForecastNFY"),
						Pipeline = GetDecimalValue(reader, "Pipeline"),
						Unsuccessful = GetDecimalValue(reader, "Unsuccessful"),
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

		struct ReportData
		{
			public string ClientCode;
			public string SalesRepCode;
			public string ValueType;
			public int ValueTypeOrder;
			public decimal Total12MonthsValue;
			public decimal Month1Value;
			public decimal Month2Value;
			public decimal Month3Value;
			public decimal Month4Value;
			public decimal Month5Value;
			public decimal Month6Value;
			public decimal Month7Value;
			public decimal Month8Value;
			public decimal Month9Value;
			public decimal Month10Value;
			public decimal Month11Value;
			public decimal Month12Value;
			public decimal CommittedYTD;
			public decimal CommittedCFY;
			public decimal CommittedAndForecastCFY;
			public decimal CommittedNFY;
			public decimal CommittedAndForecastNFY;
			public decimal Pipeline;
			public decimal Unsuccessful;
		}
	}
}

