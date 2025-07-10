using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Netting.Reports;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Netting.Reports.Testing
{
	[TestedType(typeof(fn_NettingGetSpreadsPerPeriodForReport))]
	class fn_NettingGetSpreadsPerPeriodForReportTest : DbCreateScriptTest
	{
		[TestDate(2016, 7, 15)]
		public void TestSpreadPerPeriodReportDoesNotBreakWithHugeAmount()
		{
			//the system is setup with wrong reciprocity for exchange rate, that is why when we expect it do to a value * exchange rate, the system is doing a value / exchange rate
			//what ever the setup the user has still it should show the report with the setting as it is. it should not throw out of range error
			var loginCompanyPk = new Guid("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC");
			var ns_pk = new Guid("257913DA-C48F-45B5-B7BC-4FA69A1594EE");
			var nsp_pk = new Guid("6F0F0655-D0EA-46F0-AA58-211F13781D02");
			var ns_sql = $@"INSERT INTO dbo.NettingSystem([NS_PK], [NS_Code], [NS_Description], [NS_GC]) VALUES ('{ns_pk}', 'TESTNS', 'TEST NETTING SYSTEM', '{loginCompanyPk}')";
			TestConnection.ExecuteNonQuery(ns_sql);

			var nsp_sql = $@"INSERT INTO dbo.NettingSystemPeriod([NSP_PK],[NSP_NS_NettingSystem],[NSP_Period],[NSP_EarliestInvoiceDateUtc],[NSP_LatestInvoiceDateUtc],[NSP_LatestUploadDateUtc],[NSP_LatestFXOfferDateUtc],[NSP_LatestApprovalDateUtc],[NSP_NettingExecutionDateUtc],[NSP_IsComplete],[NSP_Description],[NSP_ValueDate],[NSP_OfferPrepaymentDate]) 
							VALUES('{nsp_pk}', '{ns_pk}', '062016', '2016-06-01', '2016-06-30', '2016-07-11', '2016-07-12', '2016-07-13', '2016-07-16', 0, 'June 2016', '2016-06-30', '2016-07-14')";
			TestConnection.ExecuteNonQuery(nsp_sql);

			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.NettingSystemExchangeRate([NER_PK],[NER_NS_NettingSystem],[NER_RX_NKCurrency],[NER_RateType],[NER_Rate],[NER_NSP_Period]) VALUES ('{Guid.NewGuid()}','{ns_pk}','JPY','NET', 0.0000530,'{nsp_pk}')");

			var ab_pk1 = "D1EB0381-A332-43FD-B6AD-6E68B8A856EC";
			TestConnection.ExecuteNonQuery(string.Format($@"INSERT INTO dbo.AccBankAccount (AB_PK, AB_Code, AB_RX_NKAccountCurrency, AB_AG, AB_GC, AB_BSB) VALUES ('{ab_pk1}', 'BANKCDE1', 'AUD', 'A303D530-B8D8-4CBF-A29F-F22F9D05BC7B', '{loginCompanyPk}', '123')"));

			var ab_pk2 = "6D4C4E4C-1636-4E3A-8894-47D1D193469F";
			TestConnection.ExecuteNonQuery(string.Format($@"INSERT INTO dbo.AccBankAccount (AB_PK, AB_Code, AB_RX_NKAccountCurrency, AB_AG, AB_GC, AB_BSB) VALUES ('{ab_pk2}', 'BANKCDE2', 'AUD', '6ABAD13B-B575-4DF2-9F7C-F25236ED6F83', '{loginCompanyPk}', '124')"));

			var sql = $@"INSERT INTO dbo.NettingFXDeal([NFD_PK],[NFD_NSP_Period],[NFD_NS_System],[NFD_RX_NKCurrencyBuy],[NFD_BuyAmount],[NFD_RX_NKCurrencySell],[NFD_SellAmount],[NFD_CrossRate],[NFD_ValueDate],[NFD_QuoteNumber],[NFD_DealNumber],[NFD_DealStatus],[NFD_AB_SellAccount],[NFD_AB_BuyAccount],[NFD_OH_Bank]) 
							VALUES('{Guid.NewGuid()}', '{nsp_pk}', '{ns_pk}', 'AUD', 53000, 'JPY', 1000000000, 0.0000530, '2016-06-30', 'q1', 'D1','ACC','{ab_pk1}','{ab_pk2}','6020460D-26FE-4E0E-8B6E-C2B88084A56D')";
			TestConnection.ExecuteNonQuery(sql);

			AssertNoExceptionThrown("Exchange rates are entered assuming IsReciprocal value should be 1, but in db IsReciprocal value is set as 0, so will try to divide to get the actual value and this used to throw overflow exception"
				, () => RunScript(loginCompanyPk));
		}

		DataTable RunScript(Guid companyPk)
		{
			var sql = $@"select * from fn_NettingGetSpreadsPerPeriodForReport('{companyPk}', '2016-07-01', '2016-07-31')";
			return DataUtils.GetDataTableFromQuery(TestConnection, sql);
		}
	}
}

