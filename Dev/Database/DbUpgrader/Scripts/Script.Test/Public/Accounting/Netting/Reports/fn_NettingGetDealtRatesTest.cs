using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Netting.Reports;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Netting.Reports.Testing
{
	[TestedType(typeof(fn_NettingGetDealtRates))]
	class fn_NettingGetDealtRatesTest : DbCreateScriptTest
	{
		[TestDate(2016, 06, 01)]
		public void TestGetDealtRates()
		{
			Guid nsp_pk = new Guid("06A96D0C-C9F2-47EA-82C0-3A2D8A7BC0C5");
			PrepareData(nsp_pk);

			var resultBeforeFinalizingCycle = RunScript(nsp_pk, false);
			var expectedResult = @"
Currency          Rate
----------------- ---------------------------------------
AUD               1.000000
EUR               0.690000
JPY               80.000000
SGD               1.030000
USD               0.753210
";
			var testHelper = new TestDbHelperBase(TestConnection);
			testHelper.AssertTableAsTextFromSQLServerManagenentStudio("Before finalizing", resultBeforeFinalizingCycle, expectedResult, new List<string>() { "Rate" });

			var resultAfterFinalizingCycle = RunScript(nsp_pk, true);
			expectedResult = @"
Currency          Rate
----------------- ---------------------------------------
AUD               1.000000
EUR               0.700000
JPY               100.500000
USD               0.750000
";

			testHelper.AssertTableAsTextFromSQLServerManagenentStudio("After finalizing", resultAfterFinalizingCycle, expectedResult, new List<string>() { "Rate" });
		}

		void PrepareData(Guid nsp_pk)
		{
			var ns_pk = "257913DA-C48F-45B5-B7BC-4FA69A1594EE";
			var ns_sql = $@"INSERT INTO dbo.NettingSystem([NS_PK], [NS_Code], [NS_Description], [NS_GC]) VALUES ('{ns_pk}', 'TESTNS', 'TEST NETTING SYSTEM', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')";
			TestConnection.ExecuteNonQuery(ns_sql);

			var nsp_sql = $@"INSERT INTO dbo.NettingSystemPeriod([NSP_PK],[NSP_NS_NettingSystem],[NSP_Period],[NSP_EarliestInvoiceDateUtc],[NSP_LatestInvoiceDateUtc],[NSP_LatestUploadDateUtc],[NSP_LatestFXOfferDateUtc],[NSP_LatestApprovalDateUtc],[NSP_NettingExecutionDateUtc],[NSP_IsComplete],[NSP_Description],[NSP_ValueDate],[NSP_OfferPrepaymentDate]) 
							VALUES('{nsp_pk}', '{ns_pk}', '062016', '2016-06-01', '2016-06-30', '2016-07-11', '2016-07-12', '2016-07-13', '2016-07-16', 0, 'June 2016', '2016-06-30', '2016-07-14')";
			TestConnection.ExecuteNonQuery(nsp_sql);

			var ab_pk1 = "D1EB0381-A332-43FD-B6AD-6E68B8A856EC";
			TestConnection.ExecuteNonQuery(string.Format($@"INSERT INTO dbo.AccBankAccount (AB_PK, AB_Code, AB_RX_NKAccountCurrency, AB_AG, AB_GC, AB_BSB) VALUES ('{ab_pk1}', 'BANKCDE1', 'AUD', 'A303D530-B8D8-4CBF-A29F-F22F9D05BC7B', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '123')"));

			var ab_pk2 = "6D4C4E4C-1636-4E3A-8894-47D1D193469F";
			TestConnection.ExecuteNonQuery(string.Format($@"INSERT INTO dbo.AccBankAccount (AB_PK, AB_Code, AB_RX_NKAccountCurrency, AB_AG, AB_GC, AB_BSB) VALUES ('{ab_pk2}', 'BANKCDE2', 'AUD', '6ABAD13B-B575-4DF2-9F7C-F25236ED6F83', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '124')"));

			var sql = $@"INSERT INTO dbo.NettingFXDeal([NFD_PK],[NFD_NSP_Period],[NFD_NS_System],[NFD_RX_NKCurrencyBuy],[NFD_BuyAmount],[NFD_RX_NKCurrencySell],[NFD_SellAmount],[NFD_CrossRate],[NFD_ValueDate],[NFD_QuoteNumber],[NFD_DealNumber],[NFD_DealStatus],[NFD_AB_SellAccount],[NFD_AB_BuyAccount],[NFD_OH_Bank]) 
							VALUES('{Guid.NewGuid()}', '{nsp_pk}', '{ns_pk}', 'AUD', 900.89, 'EUR', 630.62, 0.70, '2016-06-30', 'q1', '','QUO','{ab_pk1}','{ab_pk2}','6020460D-26FE-4E0E-8B6E-C2B88084A56D')";
			TestConnection.ExecuteNonQuery(sql);

			sql = $@"INSERT INTO dbo.NettingFXDeal([NFD_PK],[NFD_NSP_Period],[NFD_NS_System],[NFD_RX_NKCurrencyBuy],[NFD_BuyAmount],[NFD_RX_NKCurrencySell],[NFD_SellAmount],[NFD_CrossRate],[NFD_ValueDate],[NFD_QuoteNumber],[NFD_DealNumber],[NFD_DealStatus],[NFD_AB_SellAccount],[NFD_AB_BuyAccount],[NFD_OH_Bank]) 
							VALUES('{Guid.NewGuid()}', '{nsp_pk}', '{ns_pk}', 'AUD', 900.89, 'EUR', 630.62, 0.70, '2016-06-30', 'q1', 'd1','ACC','{ab_pk1}','{ab_pk2}','6020460D-26FE-4E0E-8B6E-C2B88084A56D')";
			TestConnection.ExecuteNonQuery(sql);

			sql = $@"INSERT INTO dbo.NettingFXDeal([NFD_PK],[NFD_NSP_Period],[NFD_NS_System],[NFD_RX_NKCurrencyBuy],[NFD_BuyAmount],[NFD_RX_NKCurrencySell],[NFD_SellAmount],[NFD_CrossRate],[NFD_ValueDate],[NFD_QuoteNumber],[NFD_DealNumber],[NFD_DealStatus],[NFD_AB_SellAccount],[NFD_AB_BuyAccount],[NFD_OH_Bank]) 
							VALUES('{Guid.NewGuid()}', '{nsp_pk}', '{ns_pk}', 'USD', 1000, 'AUD', 750, 0.75, '2016-06-30', 'q2', '','QUO','{ab_pk2}','{ab_pk1}','6020460D-26FE-4E0E-8B6E-C2B88084A56D')";
			TestConnection.ExecuteNonQuery(sql);

			sql = $@"INSERT INTO dbo.NettingFXDeal([NFD_PK],[NFD_NSP_Period],[NFD_NS_System],[NFD_RX_NKCurrencyBuy],[NFD_BuyAmount],[NFD_RX_NKCurrencySell],[NFD_SellAmount],[NFD_CrossRate],[NFD_ValueDate],[NFD_QuoteNumber],[NFD_DealNumber],[NFD_DealStatus],[NFD_AB_SellAccount],[NFD_AB_BuyAccount],[NFD_OH_Bank]) 
							VALUES('{Guid.NewGuid()}', '{nsp_pk}', '{ns_pk}', 'USD', 1000, 'AUD', 750, 0.75, '2016-06-30', 'q2', 'd2','ACC','{ab_pk2}','{ab_pk1}','6020460D-26FE-4E0E-8B6E-C2B88084A56D')";
			TestConnection.ExecuteNonQuery(sql);

			sql = $@"INSERT INTO dbo.NettingFXDeal([NFD_PK],[NFD_NSP_Period],[NFD_NS_System],[NFD_RX_NKCurrencyBuy],[NFD_BuyAmount],[NFD_RX_NKCurrencySell],[NFD_SellAmount],[NFD_CrossRate],[NFD_ValueDate],[NFD_QuoteNumber],[NFD_DealNumber],[NFD_DealStatus],[NFD_AB_SellAccount],[NFD_AB_BuyAccount],[NFD_OH_Bank]) 
							VALUES('{Guid.NewGuid()}', '{nsp_pk}', '{ns_pk}', 'AUD', 100, 'JPY', 750, 10050, '2016-06-30', 'q3', '','QUO','{ab_pk1}','{ab_pk2}','6020460D-26FE-4E0E-8B6E-C2B88084A56D')";
			TestConnection.ExecuteNonQuery(sql);

			sql = $@"INSERT INTO dbo.NettingFXDeal([NFD_PK],[NFD_NSP_Period],[NFD_NS_System],[NFD_RX_NKCurrencyBuy],[NFD_BuyAmount],[NFD_RX_NKCurrencySell],[NFD_SellAmount],[NFD_CrossRate],[NFD_ValueDate],[NFD_QuoteNumber],[NFD_DealNumber],[NFD_DealStatus],[NFD_AB_SellAccount],[NFD_AB_BuyAccount],[NFD_OH_Bank]) 
							VALUES('{Guid.NewGuid()}', '{nsp_pk}', '{ns_pk}', 'AUD', 100, 'JPY', 10050, 100.50, '2016-06-30', 'q3', 'd3','ACC','{ab_pk1}','{ab_pk2}','6020460D-26FE-4E0E-8B6E-C2B88084A56D')";
			TestConnection.ExecuteNonQuery(sql);

			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.NettingSystemExchangeRate([NER_PK],[NER_NS_NettingSystem],[NER_RX_NKCurrency],[NER_RateType],[NER_Rate],[NER_NSP_Period]) VALUES ('{Guid.NewGuid()}','{ns_pk}','EUR','IND',0.687543,'{nsp_pk}')");
			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.NettingSystemExchangeRate([NER_PK],[NER_NS_NettingSystem],[NER_RX_NKCurrency],[NER_RateType],[NER_Rate],[NER_NSP_Period]) VALUES ('{Guid.NewGuid()}','{ns_pk}','EUR','NET',0.69,'{nsp_pk}')");

			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.NettingSystemExchangeRate([NER_PK],[NER_NS_NettingSystem],[NER_RX_NKCurrency],[NER_RateType],[NER_Rate],[NER_NSP_Period]) VALUES ('{Guid.NewGuid()}','{ns_pk}','USD','IND',0.76101,'{nsp_pk}')");
			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.NettingSystemExchangeRate([NER_PK],[NER_NS_NettingSystem],[NER_RX_NKCurrency],[NER_RateType],[NER_Rate],[NER_NSP_Period]) VALUES ('{Guid.NewGuid()}','{ns_pk}','USD','NET',0.75321,'{nsp_pk}')");

			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.NettingSystemExchangeRate([NER_PK],[NER_NS_NettingSystem],[NER_RX_NKCurrency],[NER_RateType],[NER_Rate],[NER_NSP_Period]) VALUES ('{Guid.NewGuid()}','{ns_pk}','JPY','IND',79.523,'{nsp_pk}')");
			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.NettingSystemExchangeRate([NER_PK],[NER_NS_NettingSystem],[NER_RX_NKCurrency],[NER_RateType],[NER_Rate],[NER_NSP_Period]) VALUES ('{Guid.NewGuid()}','{ns_pk}','JPY','NET',80,'{nsp_pk}')");

			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.NettingSystemExchangeRate([NER_PK],[NER_NS_NettingSystem],[NER_RX_NKCurrency],[NER_RateType],[NER_Rate],[NER_NSP_Period]) VALUES ('{Guid.NewGuid()}','{ns_pk}','SGD','IND',1.02,'{nsp_pk}')");
			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.NettingSystemExchangeRate([NER_PK],[NER_NS_NettingSystem],[NER_RX_NKCurrency],[NER_RateType],[NER_Rate],[NER_NSP_Period]) VALUES ('{Guid.NewGuid()}','{ns_pk}','SGD','NET',1.03,'{nsp_pk}')");
		}

		DataTable RunScript(Guid nsp_pk, bool isFinal)
		{
			var sql = string.Format(@"select * from fn_NettingGetDealtRates('{0}', {1}) order by Currency", nsp_pk, isFinal ? 1 : 0);
			return DataUtils.GetDataTableFromQuery(TestConnection, sql);
		}
	}
}

