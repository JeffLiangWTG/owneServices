using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.RefExchangeRate;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.RefExchangeRate.Testing
{
	[TestedType(typeof(fn_GetExchangeRate))]
	class fn_GetExchangeRateTest : DbCreateScriptTest
	{
		public void Testfn_GetExchangeRate()
		{
			var auCompanyPK = TestDataCreator.CreateCompany("AUS", "AU", "AUD");
			var nzSellExchangeRate = RefExchangeRateTest.InsertZZRefExchangeRate(TestConnection, Guid.NewGuid(), auCompanyPK, "NZ", "NZD", "SEL", string.Empty, new DateTime(2018, 1, 1), new DateTime(2018, 1, 31), 0.90m, Guid.Empty);
			var nzBuyExchangeRate = RefExchangeRateTest.InsertZZRefExchangeRate(TestConnection, Guid.NewGuid(), auCompanyPK, "NZ", "NZD", "BUY", string.Empty, new DateTime(2018, 1, 1), new DateTime(2018, 1, 31), 0.80m, Guid.Empty);

			var sqlQuery = "SELECT * FROM fn_GetExchangeRate(@CompanyPK, @Currency, @RateType, @DateTime)";
			var command = Db.Connection.Command(sqlQuery);
			command.AddParameter("@CompanyPK", System.Data.SqlDbType.UniqueIdentifier, auCompanyPK);
			command.AddParameter("@Currency", System.Data.SqlDbType.VarChar, "AUD");
			command.AddParameter("@RateType", System.Data.SqlDbType.VarChar, "BUY");
			command.AddParameter("@DateTime", System.Data.SqlDbType.DateTime, new DateTime(2018, 1, 15));

			var result = DataUtils.GetDataTableFromCommand(command);
			AssertEquals(1, result.Rows.Count);
			AssertEquals("Since currency is local currency exchange rate returned should be 1", 1M, result.Rows[0]["Rate"]);

			command = Db.Connection.Command(sqlQuery);
			command.AddParameter("@CompanyPK", System.Data.SqlDbType.UniqueIdentifier, auCompanyPK);
			command.AddParameter("@Currency", System.Data.SqlDbType.VarChar, "USD");
			command.AddParameter("@RateType", System.Data.SqlDbType.VarChar, "BUY");
			command.AddParameter("@DateTime", System.Data.SqlDbType.DateTime, new DateTime(2018, 1, 15));

			result = DataUtils.GetDataTableFromCommand(command);
			AssertEquals("No exchange rate created for USD", 0, result.Rows.Count);

			command = Db.Connection.Command(sqlQuery);
			command.AddParameter("@CompanyPK", System.Data.SqlDbType.UniqueIdentifier, auCompanyPK);
			command.AddParameter("@Currency", System.Data.SqlDbType.VarChar, "NZD");
			command.AddParameter("@RateType", System.Data.SqlDbType.VarChar, "BUY");
			command.AddParameter("@DateTime", System.Data.SqlDbType.DateTime, new DateTime(2018, 1, 15));

			result = DataUtils.GetDataTableFromCommand(command);
			AssertEquals("BUY exchange rate for NZD should be found", 1, result.Rows.Count);
			AssertEquals(nzBuyExchangeRate.Rate, result.Rows[0]["Rate"]);

			command = Db.Connection.Command(sqlQuery);
			command.AddParameter("@CompanyPK", System.Data.SqlDbType.UniqueIdentifier, auCompanyPK);
			command.AddParameter("@Currency", System.Data.SqlDbType.VarChar, "NZD");
			command.AddParameter("@RateType", System.Data.SqlDbType.VarChar, "BUY");
			command.AddParameter("@DateTime", System.Data.SqlDbType.DateTime, new DateTime(2018, 2, 15));

			result = DataUtils.GetDataTableFromCommand(command);
			AssertEquals("No exchange rate created for this date", 0, result.Rows.Count);

			command = Db.Connection.Command(sqlQuery);
			command.AddParameter("@CompanyPK", System.Data.SqlDbType.UniqueIdentifier, auCompanyPK);
			command.AddParameter("@Currency", System.Data.SqlDbType.VarChar, "NZD");
			command.AddParameter("@RateType", System.Data.SqlDbType.VarChar, "SEL");
			command.AddParameter("@DateTime", System.Data.SqlDbType.DateTime, new DateTime(2018, 1, 15));

			result = DataUtils.GetDataTableFromCommand(command);
			AssertEquals("SEL exchange rate for NZD should be found", 1, result.Rows.Count);
			AssertEquals(nzSellExchangeRate.Rate, result.Rows[0]["Rate"]);
		}
	}
}

