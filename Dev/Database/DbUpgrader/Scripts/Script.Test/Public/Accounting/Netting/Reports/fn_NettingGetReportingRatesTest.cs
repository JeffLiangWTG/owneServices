using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Netting.Reports;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Netting.Reports.Testing
{
	[TestedType(typeof(fn_NettingGetReportingRates))]
	class fn_NettingGetReportingRatesTest : DbCreateScriptTest
	{
		public void TestGetReportingRates()
		{
			Guid nsp_pk = new Guid("06A96D0C-C9F2-47EA-82C0-3A2D8A7BC0C5");
			PrepareData(nsp_pk);

			var testHelper = new TestDbHelperBase(TestConnection);

			var resultBeforeFinalizingCycle = RunScript(nsp_pk, false);
			var expectedResult = @"
NER_RX_NKCurrency NER_Rate
----------------- ---------------------------------------
AUD               1.000000
EUR               0.687543
JPY               79.523000
USD               0.000000
";

			testHelper.AssertTableAsTextFromSQLServerManagenentStudio("Before finalizing", resultBeforeFinalizingCycle, expectedResult, new List<string>() { "NER_Rate" });

			var resultAfterFinalizingCycle = RunScript(nsp_pk, true);
			expectedResult = @"
NER_RX_NKCurrency NER_Rate
----------------- ---------------------------------------
AUD               1.000000
EUR               0.690000
JPY               0.000000
USD               0.753210
";

			testHelper.AssertTableAsTextFromSQLServerManagenentStudio("After finalizing", resultAfterFinalizingCycle, expectedResult, new List<string>() { "NER_Rate" });
		}

		void PrepareData(Guid nsp_pk)
		{
			var ns_pk = "257913DA-C48F-45B5-B7BC-4FA69A1594EE";
			var ns_sql = $@"INSERT INTO dbo.NettingSystem([NS_PK], [NS_Code], [NS_Description], [NS_GC]) VALUES ('{ns_pk}', 'TESTNS', 'TEST NETTING SYSTEM', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')";
			TestConnection.ExecuteNonQuery(ns_sql);

			var nsp_sql = $@"INSERT INTO dbo.NettingSystemPeriod([NSP_PK],[NSP_NS_NettingSystem],[NSP_Period],[NSP_EarliestInvoiceDateUtc],[NSP_LatestInvoiceDateUtc],[NSP_LatestUploadDateUtc],[NSP_LatestFXOfferDateUtc],[NSP_LatestApprovalDateUtc],[NSP_NettingExecutionDateUtc],[NSP_IsComplete],[NSP_Description],[NSP_ValueDate],[NSP_OfferPrepaymentDate]) 
							VALUES('{nsp_pk}', '{ns_pk}', '062016', '2016-06-01', '2016-06-30', '2016-07-11', '2016-07-12', '2016-07-13', '2016-07-16', 0, 'June 2016', '2016-06-30', '2016-07-14')";
			TestConnection.ExecuteNonQuery(nsp_sql);

			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.NettingSystemExchangeRate([NER_PK],[NER_NS_NettingSystem],[NER_RX_NKCurrency],[NER_RateType],[NER_Rate],[NER_NSP_Period]) VALUES ('{Guid.NewGuid()}','{ns_pk}','EUR','IND',0.687543,'{nsp_pk}')");
			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.NettingSystemExchangeRate([NER_PK],[NER_NS_NettingSystem],[NER_RX_NKCurrency],[NER_RateType],[NER_Rate],[NER_NSP_Period]) VALUES ('{Guid.NewGuid()}','{ns_pk}','EUR','NET',0.69,'{nsp_pk}')");

			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.NettingSystemExchangeRate([NER_PK],[NER_NS_NettingSystem],[NER_RX_NKCurrency],[NER_RateType],[NER_Rate],[NER_NSP_Period]) VALUES ('{Guid.NewGuid()}','{ns_pk}','USD','NET',0.75321,'{nsp_pk}')");

			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.NettingSystemExchangeRate([NER_PK],[NER_NS_NettingSystem],[NER_RX_NKCurrency],[NER_RateType],[NER_Rate],[NER_NSP_Period]) VALUES ('{Guid.NewGuid()}','{ns_pk}','JPY','IND',79.523,'{nsp_pk}')");
		}

		DataTable RunScript(Guid nsp_pk, bool isFinal)
		{
			var sql = string.Format(@"select * from fn_NettingGetReportingRates('{0}', {1}) order by NER_RX_NKCurrency", nsp_pk, isFinal ? 1 : 0);
			return DataUtils.GetDataTableFromQuery(TestConnection, sql);
		}
	}
}

