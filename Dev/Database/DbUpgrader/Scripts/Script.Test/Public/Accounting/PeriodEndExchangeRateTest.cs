using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	[TestedType(typeof(PeriodEndExchangeRate))]
	class PeriodEndExchangeRateTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_StartDate, AM_EndDate, AM_GC_Company) VALUES (NEWID(), 201401, '2014-01-01 00:00:00','2014-01-31 23:59:00','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_StartDate, AM_EndDate, AM_GC_Company) VALUES (NEWID(), 201402, '2014-02-01 00:00:00','2014-02-28 23:59:00','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_StartDate, AM_EndDate, AM_GC_Company) VALUES (NEWID(), 201403, '2014-03-01 00:00:00','2014-03-31 23:59:00','878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')");

			TestConnection.ExecuteNonQuery("INSERT INTO dbo.ZZRefExchangeRate (RE_PK, RE_ExRateType, RE_StartDate, RE_ExpiryDate, RE_RX_NKExCurrency, RE_GC, RE_SellRate) VALUES (NEWID(), 'PER', '2014-02-01 00:00:00','2014-02-28 23:59:00','USD', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', 0.7)");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.ZZRefExchangeRate (RE_PK, RE_ExRateType, RE_StartDate, RE_ExpiryDate, RE_RX_NKExCurrency, RE_GC, RE_SellRate) VALUES (NEWID(), 'PER', '2014-02-01 00:00:00','2014-02-28 23:59:00','USD', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', 0.8)");
			TestConnection.ExecuteNonQuery("INSERT INTO dbo.ZZRefExchangeRate (RE_PK, RE_ExRateType, RE_StartDate, RE_ExpiryDate, RE_RX_NKExCurrency, RE_GC, RE_SellRate) VALUES (NEWID(), 'PER', '2014-03-01 00:00:00','2014-03-31 23:59:00','USD', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', 0.5)");

			var result = GetResultSet("USD", "201401", "878D7ACA-FFC3-49FC-9710-969CA0C0F2AC");
			AssertEquals("Should found 1 record", 1, result.Rows.Count);
			AssertEquals("No Matching exchange rate", 0.0m, result.Rows[0]["ExchangeRate"]);
			AssertEquals("No Matching exchange rate", new DateTime(2014, 01, 31), result.Rows[0]["MissingExchangeRateDate"]);

			result = GetResultSet("USD", "201402", "878D7ACA-FFC3-49FC-9710-969CA0C0F2AC");
			AssertEquals("Should found 1 record", 1, result.Rows.Count);
			AssertEquals("Exchange rate found", 0.8m, result.Rows[0]["ExchangeRate"]);
			AssertEquals("Exchange rate found", System.DBNull.Value, result.Rows[0]["MissingExchangeRateDate"]);

			result = GetResultSet("USD", "201403", "878D7ACA-FFC3-49FC-9710-969CA0C0F2AC");
			AssertEquals("Should found 1 record", 1, result.Rows.Count);
			AssertEquals("Exchange rate found", 0.5m, result.Rows[0]["ExchangeRate"]);
			AssertEquals("Exchange rate found", System.DBNull.Value, result.Rows[0]["MissingExchangeRateDate"]);

			result = GetResultSet("EUR", "201403", "878D7ACA-FFC3-49FC-9710-969CA0C0F2AC");
			AssertEquals("Should found 1 record", 1, result.Rows.Count);
			AssertEquals("No Matching exchange rate", 0.0m, result.Rows[0]["ExchangeRate"]);
			AssertEquals("No Matching exchange rate", new DateTime(2014, 03, 31), result.Rows[0]["MissingExchangeRateDate"]);
		}

		DataTable GetResultSet(string currency, string period, string companyPK)
		{
			var command = TestConnection.Command(string.Format(@" SELECT * FROM PeriodEndExchangeRate('{0}', {1}, '{2}')", currency, period, companyPK));
			return DataUtils.GetDataTableFromCommand(command);
		}
	}
}

