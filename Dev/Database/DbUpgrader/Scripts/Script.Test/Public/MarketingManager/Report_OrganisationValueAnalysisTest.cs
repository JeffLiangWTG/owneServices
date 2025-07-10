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
	[TestedType(typeof(Report_OrganisationValueAnalysis))]
	class Report_OrganisationValueAnalysisTest : DbCreateScriptTest
	{
		public void TestRun()
		{
			CreateAccountingPeriod(TestDbHelper.DefaultCompanyPK);
			var org = CreateOrg("TSTAAA");
			var activeOpp2017 = CreateOpportunity("ATST2017", "CRT", org, TestDbHelper.DefaultCompanyPK, new DateTime(2017, 1, 1), DBNull.Value);
			var activeOpp2018 = CreateOpportunity("ATST2018", "CRT", org, TestDbHelper.DefaultCompanyPK, new DateTime(2018, 1, 1), DBNull.Value);
			var activeOpp2019 = CreateOpportunity("ATST2019", "CRT", org, TestDbHelper.DefaultCompanyPK, new DateTime(2019, 1, 1), DBNull.Value);
			var closedOpp2017 = CreateOpportunity("CTST2017", "WON", org, TestDbHelper.DefaultCompanyPK, new DateTime(2017, 1, 2), new DateTime(2017, 1, 3));
			var closedOpp2018 = CreateOpportunity("CTST2018", "WON", org, TestDbHelper.DefaultCompanyPK, new DateTime(2017, 1, 2), new DateTime(2018, 1, 3));
			var closedOpp2019 = CreateOpportunity("CTST2019", "LOS", org, TestDbHelper.DefaultCompanyPK, new DateTime(2017, 1, 2), new DateTime(2019, 1, 3));

			CreateProspectValue(org, activeOpp2017, "SHP", "CNSHA", "AUBNE", "ACT", "MTH", "AUD", 1200m, 10m, new DateTime(2017, 1, 1), 1, false);
			CreateProspectValue(org, activeOpp2018, "SHP", "CNSHA", "AUBNE", "ACT", "MTH", "AUD", 500m, 5m, new DateTime(2018, 1, 1), 1, false);
			CreateProspectValue(org, activeOpp2019, "SHP", "CNSHA", "AUBNE", "ACT", "MTH", "AUD", 1500m, 15m, new DateTime(2019, 1, 1), 1, false);
			CreateProspectValue(org, closedOpp2017, "SHP", "AUSYD", "NZAKL", "SUC", "MTH", "AUD", 1000m, 20m, new DateTime(2017, 1, 1), 3, false);
			CreateProspectValue(org, closedOpp2018, "SHP", "AUSYD", "NZAKL", "SUC", "MTH", "AUD", 1000m, 10m, new DateTime(2017, 10, 1), 6, false);
			CreateProspectValue(org, closedOpp2018, "SHP", "CNSHA", "AUBNE", "SUC", "MTH", "AUD", 3000m, 30m, new DateTime(2018, 1, 1), 6, true);
			CreateProspectValue(org, closedOpp2017, "SHP", "CNSHA", "AUBNE", "UNS", "MTH", "AUD", 2500m, 22m, new DateTime(2017, 1, 1), 1, false);
			CreateProspectValue(org, closedOpp2018, "SHP", "CNSHA", "AUBNE", "UNS", "MTH", "AUD", 2000m, 20m, new DateTime(2018, 1, 1), 1, false);
			CreateProspectValue(org, closedOpp2019, "SHP", "CNSHA", "AUBNE", "UNS", "MTH", "AUD", 3000m, 25m, new DateTime(2019, 1, 1), 1, false);

			CreateTradedValue(org, TestDbHelper.DefaultCompanyPK, "SHP", "AUSYD", "NZAKL", false, "AUD", 2000m, 20m, 500m, new DateTime(2018, 1, 1));
			CreateTradedValue(org, TestDbHelper.DefaultCompanyPK, "SHP", "CNSHA", "AUBNE", false, "AUD", 6000m, 30m, 2500m, new DateTime(2018, 3, 1));
			CreateTradedValue(org, TestDbHelper.DefaultCompanyPK, "SHP", "CNSHA", "AUSYD", true, "AUD", 6000m, 40m, 2500m, new DateTime(2018, 3, 1));

			var result = RunReportFunctionAndSort(2018, TestDbHelper.DefaultCompanyPK);
			AssertEquals(9, result.Count);
			AssertReportPeriodData(result[0], "TSTAAA", "Committed and Forecast", 24000m, 0m, 0m, 0m, 1000m, 1000m, 1000m, 4000m, 4000m, 4000m, 3000m, 3000m, 3000m);
			AssertReportPeriodData(result[1], "TSTAAA", "Committed and Forecast TEU", 240m, 0m, 0m, 0m, 10m, 10m, 10m, 40m, 40m, 40m, 30m, 30m, 30m);
			AssertReportPeriodData(result[2], "TSTAAA", "Committed", 6000m, 0m, 0m, 0m, 1000m, 1000m, 1000m, 1000m, 1000m, 1000m, 0m, 0m, 0m);
			AssertReportPeriodData(result[3], "TSTAAA", "Committed TEU", 60m, 0m, 0m, 0m, 10m, 10m, 10m, 10m, 10m, 10m, 0m, 0m, 0m);
			AssertReportPeriodData(result[4], "TSTAAA", "Client Revenue", 8000m, 0m, 0m, 0m, 0m, 0m, 0m, 2000m, 0m, 6000m, 0m, 0m, 0m);
			AssertReportPeriodData(result[5], "TSTAAA", "Client Revenue TEU", 50m, 0m, 0m, 0m, 0m, 0m, 0m, 20m, 0m, 30m, 0m, 0m, 0m);
			AssertReportPeriodData(result[6], "TSTAAA", "Job Revenue", 6000m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 6000m, 0m, 0m, 0m);
			AssertReportPeriodData(result[7], "TSTAAA", "Job Revenue TEU", 40m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 40m, 0m, 0m, 0m);
			AssertReportPeriodData(result[8], "TSTAAA", "Job Profit", 3500m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 3500m, 0m, 0m, 0m);

			AssertReportOrgData(result[0], "TSTAAA", 8000m, 50m, 3500m, 6000m, 60m, 24000m, 240m, 20400m, 180m, 24000m, 240m);
			AssertReportOrgData(result[1], "TSTAAA", 8000m, 50m, 3500m, 6000m, 60m, 24000m, 240m, 20400m, 180m, 24000m, 240m);
			AssertReportOrgData(result[2], "TSTAAA", 8000m, 50m, 3500m, 6000m, 60m, 24000m, 240m, 20400m, 180m, 24000m, 240m);
			AssertReportOrgData(result[3], "TSTAAA", 8000m, 50m, 3500m, 6000m, 60m, 24000m, 240m, 20400m, 180m, 24000m, 240m);
			AssertReportOrgData(result[4], "TSTAAA", 8000m, 50m, 3500m, 6000m, 60m, 24000m, 240m, 20400m, 180m, 24000m, 240m);
			AssertReportOrgData(result[5], "TSTAAA", 8000m, 50m, 3500m, 6000m, 60m, 24000m, 240m, 20400m, 180m, 24000m, 240m);
			AssertReportOrgData(result[6], "TSTAAA", 8000m, 50m, 3500m, 6000m, 60m, 24000m, 240m, 20400m, 180m, 24000m, 240m);
			AssertReportOrgData(result[7], "TSTAAA", 8000m, 50m, 3500m, 6000m, 60m, 24000m, 240m, 20400m, 180m, 24000m, 240m);
			AssertReportOrgData(result[8], "TSTAAA", 8000m, 50m, 3500m, 6000m, 60m, 24000m, 240m, 20400m, 180m, 24000m, 240m);

			result = RunReportFunctionAndSort(2017, TestDbHelper.DefaultCompanyPK);
			AssertEquals(4, result.Count);
			AssertReportOrgData(result[0], "TSTAAA", 0, 0m, 0m, 3000m, 60m, 3000m, 60m, 14400m, 120m, 30000m, 264m);

			AssertReportPeriodData(result[0], "TSTAAA", "Committed and Forecast", 3000m, 0m, 0m, 0m, 0m, 0m, 0m, 1000m, 1000m, 1000m, 0m, 0m, 0m);
			AssertReportPeriodData(result[1], "TSTAAA", "Committed and Forecast TEU", 60m, 0m, 0m, 0m, 0m, 0m, 0m, 20m, 20m, 20m, 0m, 0m, 0m);
			AssertReportPeriodData(result[2], "TSTAAA", "Committed", 3000m, 0m, 0m, 0m, 0m, 0m, 0m, 1000m, 1000m, 1000m, 0m, 0m, 0m);
			AssertReportPeriodData(result[3], "TSTAAA", "Committed TEU", 60m, 0m, 0m, 0m, 0m, 0m, 0m, 20m, 20m, 20m, 0m, 0m, 0m);
		}

		public void TestVerifyValuesAreNotDoubledWithDuplicatedExchangeRate()
		{
			CreateAccountingPeriod(TestDbHelper.DefaultCompanyPK);
			var org1 = CreateOrg("TSTAAA");
			var opp1 = CreateOpportunity("CTST2018", "WON", org1, TestDbHelper.DefaultCompanyPK, new DateTime(2018, 2, 2), new DateTime(2018, 2, 3));

			CreateProspectValue(org1, opp1, "SHP", "AUSYD", "NZAKL", "SUC", "MTH", "AUD", 1000m, 20m, new DateTime(2017, 10, 1), 6, false);
			CreateProspectValue(org1, opp1, "SHP", "CNSHA", "AUBNE", "SUC", "MTH", "AUD", 3000m, 20m, new DateTime(2018, 1, 1), 6, true);
			CreateProspectValue(org1, opp1, "SHP", "CNSHA", "AUBNE", "ACT", "MTH", "AUD", 500m, 20m, new DateTime(2018, 1, 1), 1, false);
			CreateProspectValue(org1, opp1, "SHP", "CNSHA", "AUBNE", "UNS", "MTH", "AUD", 2000m, 20m, new DateTime(2018, 1, 1), 1, false);

			CreateTradedValue(org1, TestDbHelper.DefaultCompanyPK, "SHP", "AUSYD", "NZAKL", false, "AUD", 2000m, 20m, 500m, new DateTime(2018, 1, 1));
			CreateTradedValue(org1, TestDbHelper.DefaultCompanyPK, "SHP", "CNSHA", "AUBNE", false, "AUD", 6000m, 30m, 2500m, new DateTime(2018, 3, 1));
			CreateTradedValue(org1, TestDbHelper.DefaultCompanyPK, "SHP", "CNSHA", "AUSYD", true, "AUD", 6000m, 40m, 2500m, new DateTime(2018, 3, 1));

			CreateDuplicateExchangeRateForDefaultCompany(org1);

			var result = RunReportFunctionAndSort(2018, TestDbHelper.DefaultCompanyPK);
			AssertReportOrgData(result[0], "TSTAAA", 8000m, 50m, 3500m, 6000m, 120m, 24000m, 240m, 6000m, 240m, 24000m, 240m);
			AssertReportOrgData(result[1], "TSTAAA", 8000m, 50m, 3500m, 6000m, 120m, 24000m, 240m, 6000m, 240m, 24000m, 240m);
			AssertReportOrgData(result[2], "TSTAAA", 8000m, 50m, 3500m, 6000m, 120m, 24000m, 240m, 6000m, 240m, 24000m, 240m);
			AssertReportOrgData(result[3], "TSTAAA", 8000m, 50m, 3500m, 6000m, 120m, 24000m, 240m, 6000m, 240m, 24000m, 240m);
			AssertReportOrgData(result[4], "TSTAAA", 8000m, 50m, 3500m, 6000m, 120m, 24000m, 240m, 6000m, 240m, 24000m, 240m);
			AssertReportOrgData(result[5], "TSTAAA", 8000m, 50m, 3500m, 6000m, 120m, 24000m, 240m, 6000m, 240m, 24000m, 240m);
			AssertReportOrgData(result[6], "TSTAAA", 8000m, 50m, 3500m, 6000m, 120m, 24000m, 240m, 6000m, 240m, 24000m, 240m);
			AssertReportOrgData(result[7], "TSTAAA", 8000m, 50m, 3500m, 6000m, 120m, 24000m, 240m, 6000m, 240m, 24000m, 240m);
			AssertReportOrgData(result[8], "TSTAAA", 8000m, 50m, 3500m, 6000m, 120m, 24000m, 240m, 6000m, 240m, 24000m, 240m);
		}

		public void TestValuesForDifferentExchangeRate_Reciprocal()
		{
			TestValuesForDifferentExchangeRate(isReciprocal: true);
		}

		public void TestValuesForDifferentExchangeRate_NotReciprocal()
		{
			TestValuesForDifferentExchangeRate(isReciprocal: false);
		}

		void TestValuesForDifferentExchangeRate(bool isReciprocal)
		{
			var helper = new TestDbHelper(TestConnection);
			var company = helper.InsertCompany("TC1", "Company", "AUD", "AU", isReciprocal, isGSTRegistered: true);
			CreateAccountingPeriod(company);

			var org = CreateOrg("OH1");
			var opp = CreateOpportunity("CTST2018", "WON", org, company, new DateTime(2018, 2, 2), new DateTime(2018, 2, 3));

			CreateProspectValue(org, opp, "SHP", "NZAKL", "AUSYD", "SUC", "MTH", "NZD", 1000m, 10m, new DateTime(2017, 10, 1), 6, false);
			CreateProspectValue(org, opp, "SHP", "CNSHA", "AUBNE", "SUC", "MTH", "CNY", 3000m, 30m, new DateTime(2018, 1, 1), 6, true);
			CreateProspectValue(org, opp, "SHP", "CNSHA", "AUBNE", "ACT", "MTH", "CNY", 500m, 5m, new DateTime(2018, 1, 1), 1, false);
			CreateProspectValue(org, opp, "SHP", "CNSHA", "AUBNE", "UNS", "MTH", "CNY", 2000m, 20m, new DateTime(2018, 1, 1), 1, false);

			CreateTradedValue(org, company, "SHP", "NZAKL", "AUSYD", false, "NZD", 2000m, 20m, 500m, new DateTime(2018, 1, 1));
			CreateTradedValue(org, company, "SHP", "CNSHA", "AUBNE", false, "CNY", 6000m, 30m, 2500m, new DateTime(2018, 3, 1));
			CreateTradedValue(org, company, "SHP", "CNSHA", "AUSYD", true, "CNY", 6000m, 40m, 2500m, new DateTime(2018, 3, 1));

			CreateExchangeRate(org, company, "AUD", 1m);
			CreateExchangeRate(org, company, "NZD", isReciprocal ? 0.5m : 2m);
			CreateExchangeRate(org, company, "CNY", isReciprocal ? 1.25m : 0.8m);

			var result = RunReportFunctionAndSort(2018, company);
			AssertReportOrgData(result[0], "OH1", 8500m, 50m, 4375m, 3000m, 60m, 25500m, 240m, 7500m, 60m, 30000m, 240m);

			AssertEquals(9, result.Count);
			AssertReportPeriodData(result[0], "OH1", "Committed and Forecast", 25500m, 0m, 0m, 0m, 500m, 500m, 500m, 4250m, 4250m, 4250m, 3750m, 3750m, 3750m);
			AssertReportPeriodData(result[1], "OH1", "Committed and Forecast TEU", 240m, 0m, 0m, 0m, 10m, 10m, 10m, 40m, 40m, 40m, 30m, 30m, 30m);
			AssertReportPeriodData(result[2], "OH1", "Committed", 3000m, 0m, 0m, 0m, 500m, 500m, 500m, 500m, 500m, 500m, 0m, 0m, 0m);
			AssertReportPeriodData(result[3], "OH1", "Committed TEU", 60m, 0m, 0m, 0m, 10m, 10m, 10m, 10m, 10m, 10m, 0m, 0m, 0m);
			AssertReportPeriodData(result[4], "OH1", "Client Revenue", 8500m, 0m, 0m, 0m, 0m, 0m, 0m, 1000m, 0m, 7500m, 0m, 0m, 0m);
			AssertReportPeriodData(result[5], "OH1", "Client Revenue TEU", 50m, 0m, 0m, 0m, 0m, 0m, 0m, 20m, 0m, 30m, 0m, 0m, 0m);
			AssertReportPeriodData(result[6], "OH1", "Job Revenue", 7500m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 7500m, 0m, 0m, 0m);
			AssertReportPeriodData(result[7], "OH1", "Job Revenue TEU", 40m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 40m, 0m, 0m, 0m);
			AssertReportPeriodData(result[8], "OH1", "Job Profit", 4375m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 4375m, 0m, 0m, 0m);
		}

		static void AssertReportOrgData(ReportData data, string clientCode, decimal clientRevenue, decimal clientRevenueTEU, decimal jobProfit, decimal committedCFY, decimal committedCFYTEU, decimal committedAndForecastCFY, decimal committedAndForecastCFYTEU, decimal pipeline, decimal pipelineTEU, decimal unsuccessful, decimal unsuccessfulTEU)
		{
			CombineAssertions(() =>
			{
				AssertEquals("ClientCode", clientCode, data.ClientCode);
				AssertEquals("ClientRevenue", clientRevenue, data.ClientRevenueCFY);
				AssertEquals("TEUClientRevenue", clientRevenueTEU, data.TEUClientRevenueCFY);
				AssertEquals("JobProfit", jobProfit, data.JobProfitCFY);
				AssertEquals("CommittedCFY", committedCFY, data.CommittedCFY);
				AssertEquals("TEUCommittedCFY", committedCFYTEU, data.TEUCommittedCFY);
				AssertEquals("CommittedAndForecastCFY", committedAndForecastCFY, data.CommittedAndForecastCFY);
				AssertEquals("TEUCommittedAndForecastCFY", committedAndForecastCFYTEU, data.TEUCommittedAndForecastCFY);
				AssertEquals("Pipeline", pipeline, data.Pipeline);
				AssertEquals("TEUPipeline", pipelineTEU, data.TEUPipeline);
				AssertEquals("Unsuccessful", unsuccessful, data.Unsuccessful);
				AssertEquals("TEUUnsuccessful", unsuccessfulTEU, data.TEUUnsuccessful);
			});
		}

		static void AssertReportPeriodData(ReportData data, string clientCode, string valueType, decimal totalValue,
			decimal month1Value, decimal month2Value, decimal month3Value, decimal month4Value, decimal month5Value, decimal month6Value,
			decimal month7Value, decimal month8Value, decimal month9Value, decimal month10Value, decimal month11Value, decimal month12Value)
		{
			CombineAssertions(() =>
			{
				AssertEquals("ClientCode", clientCode, data.ClientCode);
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

		void CreateAccountingPeriod(Guid companyPk)
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
				command.AddParameterBasedOnDbColumn("@Company", companyPk, AccPeriodManagementSchema.AM_GC_Company);
				command.ExecuteNonQuery();
			}
		}

		void CreateDuplicateExchangeRateForDefaultCompany(Guid orgPk)
		{
			CreateExchangeRate(orgPk, TestDbHelper.DefaultCompanyPK, "AUD", 1m);
		}

		void CreateExchangeRate(Guid orgPk, Guid companyPk, string currency, decimal rate)
		{
			using (var command = TestConnection.Command(@"INSERT INTO dbo.RefExchangeRate(RE_PK, RE_GC, RE_OH_Client, RE_RX_NKExCurrency, RE_ExRateType, RE_StartDate, RE_ExpiryDate, RE_SellRate, RE_IsSystem, RE_AsPublished) VALUES
		(@PK, @CompanyPk, @Client, @Currency, @RateType, @StartDate, @ExpiryDate, @Rate, @IsSystem, @AsPublished)"))
			{
				command.AddParameterBasedOnDbColumn("@PK", Guid.NewGuid(), RefExchangeRateSchema.PK);
				command.AddParameterBasedOnDbColumn("@CompanyPk", companyPk, RefExchangeRateSchema.RE_GC);
				command.AddParameterBasedOnDbColumn("@Client", orgPk, RefExchangeRateSchema.RE_OH_Client);
				command.AddParameterBasedOnDbColumn("@Currency", currency, RefExchangeRateSchema.RE_RX_NKExCurrency);
				command.AddParameterBasedOnDbColumn("@RateType", "SEL", RefExchangeRateSchema.RE_ExRateType);
				command.AddParameterBasedOnDbColumn("@StartDate", DateTime.Today.AddMonths(-1), RefExchangeRateSchema.RE_StartDate);
				command.AddParameterBasedOnDbColumn("@ExpiryDate", DateTime.Today.AddMonths(1), RefExchangeRateSchema.RE_ExpiryDate);
				command.AddParameterBasedOnDbColumn("@Rate", rate, RefExchangeRateSchema.RE_SellRate);
				command.AddParameterBasedOnDbColumn("@IsSystem", true, RefExchangeRateSchema.RE_IsSystem);
				command.AddParameterBasedOnDbColumn("@AsPublished", string.Empty, RefExchangeRateSchema.RE_AsPublished);
				command.ExecuteNonQuery();
			}
		}

		Guid CreateOrg(string orgCode)
		{
			var orgPk = Guid.NewGuid();
			var sql = @"INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@OH_PK, @OH_Code)";
			using (var command = TestConnection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@OH_PK", orgPk, OrgHeaderSchema.PK);
				command.AddParameterBasedOnDbColumn("@OH_Code", orgCode, OrgHeaderSchema.OH_Code);
				command.ExecuteNonQuery();
			}
			return orgPk;
		}

		Guid CreateOpportunity(string oppId, string status, Guid orgPk, Guid companyPk, DateTime createTimeUtc, object closedDate)
		{
			var oppPk = Guid.NewGuid();
			var sql = @"INSERT INTO dbo.OrgOpportunity (P8_PK, P8_OpportunityID, P8_Status, P8_OH, P8_GC, P8_ClosedDate, P8_SystemCreateTimeUtc, P8_SystemCreateUser, P8_SystemLastEditTimeUtc, P8_SystemLastEditUser)
VALUES (@P8_PK, @P8_OpportunityID, @P8_Status, @P8_OH, @P8_GC, @P8_ClosedDate, @P8_SystemCreateTimeUtc, 'E', GetUtcDate(), 'E')";
			using (var command = TestConnection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@P8_PK", oppPk, OrgOpportunitySchema.PK);
				command.AddParameterBasedOnDbColumn("@P8_OpportunityID", oppId, OrgOpportunitySchema.P8_OpportunityID);
				command.AddParameterBasedOnDbColumn("@P8_Status", status, OrgOpportunitySchema.P8_Status);
				command.AddParameterBasedOnDbColumn("@P8_OH", orgPk, OrgOpportunitySchema.P8_OH);
				command.AddParameterBasedOnDbColumn("@P8_GC", companyPk, OrgOpportunitySchema.P8_GC);
				command.AddParameterBasedOnDbColumn("@P8_ClosedDate", closedDate, OrgOpportunitySchema.P8_ClosedDate);
				command.AddParameterBasedOnDbColumn("@P8_SystemCreateTimeUtc", createTimeUtc, OrgOpportunitySchema.P8_SystemCreateTimeUtc);
				command.ExecuteNonQuery();
			}
			return oppPk;
		}

		void CreateProspectValue(Guid orgPk, Guid oppPk, string product, string origin, string destination,
			string status, string recurrenceType,
			string currency, decimal estimatedProfit, decimal teuQuantity, DateTime periodStart, int numOfPeriods, bool isForecast)
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

INSERT INTO dbo.OrgSalesValueAssociationPivot (SVP_PK, SVP_ActivityId, SVP_ActivityTableCode, SVP_TradeId, SVP_TradeTableCode, SVP_SystemCreateTimeUtc, SVP_SystemCreateUser, SVP_SystemLastEditTimeUtc, SVP_SystemLastEditUser) 
VALUES (NEWID(), @P8_PK, 'P8', @PA_PK, 'PA', GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.OrgTradeDetail(PA_PK, PA_OW, PA_Status, PA_SystemCreateTimeUtc, PA_SystemCreateUser, PA_SystemLastEditTimeUtc, PA_SystemLastEditUser)
VALUES (@PA_PK, @OW_PK, @PA_Status, GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.OrgTradeProspect(PAP_PK, PAP_PA, PAP_RecurrenceType, PAP_SystemCreateTimeUtc, PAP_SystemCreateUser, PAP_SystemLastEditTimeUtc, PAP_SystemLastEditUser)
VALUES (NEWID(), @PA_PK, @PAP_RecurrenceType, GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.OrgTradePeriod(PAS_PK, PAS_PA, PAS_OH_Client, PAS_IsTraded, PAS_EstimatedProfit, PAS_TEUQuantity, PAS_RX_NKCurrency, PAS_IsForecast, PAS_Period, PAS_SystemCreateTimeUtc, PAS_SystemCreateUser, PAS_SystemLastEditTimeUtc, PAS_SystemLastEditUser)
SELECT
	NEWID(), @PA_PK, @OH_PK, 0, @PAS_EstimatedProfit, @PAS_TEUQuantity, @PAS_RX_NKCurrency, @PAS_IsForecast,
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
				command.AddParameterBasedOnDbColumn("@P8_PK", oppPk, OrgOpportunitySchema.PK);
				command.AddParameter("@Origin", SqlDbType.VarChar, origin);
				command.AddParameter("@Destination", SqlDbType.VarChar, destination);
				command.AddParameter("@Product", SqlDbType.VarChar, product);
				command.AddParameterBasedOnDbColumn("@PA_PK", detailPk, OrgTradeDetailSchema.PK);
				command.AddParameterBasedOnDbColumn("@PA_Status", status, OrgTradeDetailSchema.PA_Status);
				command.AddParameterBasedOnDbColumn("@PAP_RecurrenceType", recurrenceType, OrgTradeProspectSchema.PAP_RecurrenceType);
				command.AddParameterBasedOnDbColumn("@PAS_EstimatedProfit", estimatedProfit, OrgTradePeriodSchema.PAS_EstimatedProfit);
				command.AddParameterBasedOnDbColumn("@PAS_TEUQuantity", teuQuantity, OrgTradePeriodSchema.PAS_TEUQuantity);
				command.AddParameterBasedOnDbColumn("@PAS_RX_NKCurrency", currency, OrgTradePeriodSchema.PAS_RX_NKCurrency);
				command.AddParameterBasedOnDbColumn("@PAS_IsForecast", isForecast, OrgTradePeriodSchema.PAS_IsForecast);
				command.AddParameter("@PeriodStart", SqlDbType.SmallDateTime, periodStart);
				command.AddParameter("@NumOfPeriods", SqlDbType.Int, numOfPeriods);
				command.ExecuteNonQuery();
			}
		}

		void CreateTradedValue(Guid orgPk, Guid companyPk, string product, string origin, string destination,
			bool isJobValue, string currency, decimal revenue, decimal teuQuantity, decimal cost, DateTime period)
		{
			var sql =
@"
DECLARE @OriginPk UNIQUEIDENTIFIER = (SELECT TOP 1 RL_PK FROM dbo.RefUNLOCO WHERE RL_Code = @Origin)
DECLARE @DestinationPk UNIQUEIDENTIFIER = (SELECT TOP 1 RL_PK FROM dbo.RefUNLOCO WHERE RL_Code = @Destination)
DECLARE @ProductPk UNIQUEIDENTIFIER = (SELECT TOP 1 MP_PK FROM dbo.OrgSalesProduct WHERE MP_Code = @Product)
DECLARE @OW_PK UNIQUEIDENTIFIER = (SELECT NEWID())
DECLARE @PA_PK UNIQUEIDENTIFIER = (SELECT NEWID())
DECLARE @PAS_PK UNIQUEIDENTIFIER = (SELECT NEWID())

INSERT INTO dbo.OrgSales (OW_PK, OW_MP_Product, OW_OriginID, OW_OriginTableCode, OW_DestinationID, OW_DestinationTableCode, OW_OH_Buyer, OW_IsTraded, OW_SystemCreateTimeUtc, OW_SystemCreateUser, OW_SystemLastEditTimeUtc, OW_SystemLastEditUser)
VALUES (@OW_PK, @ProductPk, @OriginPk, 'RL', @DestinationPk, 'RL', @OH_PK, 1, GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.OrgTradeDetail(PA_PK, PA_OW, PA_SystemCreateTimeUtc, PA_SystemCreateUser, PA_SystemLastEditTimeUtc, PA_SystemLastEditUser)
VALUES (@PA_PK, @OW_PK, GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.OrgTradePeriod(PAS_PK, PAS_PA, PAS_OH_Client, PAS_IsTraded, PAS_Period, PAS_IsJobValue, PAS_TEUQuantity, PAS_SystemCreateTimeUtc, PAS_SystemCreateUser, PAS_SystemLastEditTimeUtc, PAS_SystemLastEditUser)
VALUES (@PAS_PK, @PA_PK, @OH_PK, 1, @PAS_Period, @PAS_IsJobValue, @PAS_TEUQuantity, GetUtcDate(), 'E', GetUtcDate(), 'E')

INSERT INTO dbo.OrgTradeValue(PAV_PK, PAV_PAS, PAV_GC, PAV_RX_NKCurrency, PAV_Revenue, PAV_Cost, PAV_SystemCreateTimeUtc, PAV_SystemCreateUser, PAV_SystemLastEditTimeUtc, PAV_SystemLastEditUser)
VALUES (NEWID(), @PAS_PK, @PAV_GC, @PAV_RX_NKCurrency, @PAV_Revenue, @PAV_Cost, GetUtcDate(), 'E', GetUtcDate(), 'E')
";
			using (var command = TestConnection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@OH_PK", orgPk, OrgHeaderSchema.PK);
				command.AddParameter("@Origin", SqlDbType.VarChar, origin);
				command.AddParameter("@Destination", SqlDbType.VarChar, destination);
				command.AddParameter("@Product", SqlDbType.VarChar, product);
				command.AddParameterBasedOnDbColumn("@PAS_Period", period, OrgTradePeriodSchema.PAS_Period);
				command.AddParameterBasedOnDbColumn("@PAS_IsJobValue", isJobValue, OrgTradePeriodSchema.PAS_IsJobValue);
				command.AddParameterBasedOnDbColumn("@PAS_TEUQuantity", teuQuantity, OrgTradePeriodSchema.PAS_TEUQuantity);
				command.AddParameterBasedOnDbColumn("@PAV_GC", companyPk, OrgTradeValueSchema.PAV_GC);
				command.AddParameterBasedOnDbColumn("@PAV_RX_NKCurrency", currency, OrgTradeValueSchema.PAV_RX_NKCurrency);
				command.AddParameterBasedOnDbColumn("@PAV_Revenue", revenue, OrgTradeValueSchema.PAV_Revenue);
				command.AddParameterBasedOnDbColumn("@PAV_Cost", cost, OrgTradeValueSchema.PAV_Cost);
				command.ExecuteNonQuery();
			}
		}

		List<ReportData> RunReportFunctionAndSort(int year, Guid companyPk)
		{
			var result = new List<ReportData>();

			var sql =
@"SELECT
	[ClientCode], [ValueType], [ValueTypeOrder], 
	[Total12MonthsValue], 
	[Month1Value], [Month2Value], [Month3Value], [Month4Value], [Month5Value], [Month6Value], 
	[Month7Value], [Month8Value], [Month9Value], [Month10Value], [Month11Value], [Month12Value],
	[ClientRevenueCFY], [TEUClientRevenueCFY], [JobProfitCFY], [CommittedYTD], [TEUCommittedYTD], [CommittedCFY], [TEUCommittedCFY], [CommittedAndForecastCFY], [TEUCommittedAndForecastCFY], 
	[CommittedNFY], [TEUCommittedNFY], [CommittedAndForecastNFY], [TEUCommittedAndForecastNFY], [Pipeline], [TEUPipeline], [Unsuccessful], [TEUUnsuccessful]
FROM 
	Report_OrganisationValueAnalysis(@CompanyPk, @FinancialYear, @OrgPks, 1, @SaleRepPks, 1, NULL, NULL, 'Y', 'Y', 'Y', 'Y', 'Y', 'Y')
";

			using (var command = TestConnection.Command(sql))
			{
				command.AddParameter("@CompanyPk", SqlDbType.UniqueIdentifier, companyPk);
				command.AddParameter("@FinancialYear", SqlDbType.Int, year);

				var orgPks = new TVPParamInfo("@OrgPks", "dbo.TVP_UNIQUEIDENTIFIER", typeof(Guid), Array.Empty<object>());
				orgPks.AddTVPParameters(command);
				var salesRepPks = new TVPParamInfo("@SaleRepPks", "dbo.TVP_UNIQUEIDENTIFIER", typeof(Guid), Array.Empty<object>());
				salesRepPks.AddTVPParameters(command);

				ReadData(result, command);
			}

			return result.OrderBy(x => x.ClientCode).ThenBy(x => x.ValueTypeOrder).ToList();
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
						ClientRevenueCFY = GetDecimalValue(reader, "ClientRevenueCFY"),
						TEUClientRevenueCFY = GetDecimalValue(reader, "TEUClientRevenueCFY"),
						JobProfitCFY = GetDecimalValue(reader, "JobProfitCFY"),
						CommittedYTD = GetDecimalValue(reader, "CommittedYTD"),
						TEUCommittedYTD = GetDecimalValue(reader, "TEUCommittedYTD"),
						CommittedCFY = GetDecimalValue(reader, "CommittedCFY"),
						TEUCommittedCFY = GetDecimalValue(reader, "TEUCommittedCFY"),
						CommittedAndForecastCFY = GetDecimalValue(reader, "CommittedAndForecastCFY"),
						TEUCommittedAndForecastCFY = GetDecimalValue(reader, "TEUCommittedAndForecastCFY"),
						CommittedNFY = GetDecimalValue(reader, "CommittedNFY"),
						TEUCommittedNFY = GetDecimalValue(reader, "TEUCommittedNFY"),
						CommittedAndForecastNFY = GetDecimalValue(reader, "CommittedAndForecastNFY"),
						TEUCommittedAndForecastNFY = GetDecimalValue(reader, "TEUCommittedAndForecastNFY"),
						Pipeline = GetDecimalValue(reader, "Pipeline"),
						TEUPipeline = GetDecimalValue(reader, "TEUPipeline"),
						Unsuccessful = GetDecimalValue(reader, "Unsuccessful"),
						TEUUnsuccessful = GetDecimalValue(reader, "TEUUnsuccessful"),
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
			public decimal ClientRevenueCFY;
			public decimal TEUClientRevenueCFY;
			public decimal JobProfitCFY;
			public decimal CommittedYTD;
			public decimal TEUCommittedYTD;
			public decimal CommittedCFY;
			public decimal TEUCommittedCFY;
			public decimal CommittedAndForecastCFY;
			public decimal TEUCommittedAndForecastCFY;
			public decimal CommittedNFY;
			public decimal TEUCommittedNFY;
			public decimal CommittedAndForecastNFY;
			public decimal TEUCommittedAndForecastNFY;
			public decimal Pipeline;
			public decimal TEUPipeline;
			public decimal Unsuccessful;
			public decimal TEUUnsuccessful;
		}
	}
}

