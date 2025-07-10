using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Finance;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.Finance.Testing
{
	[TestedType(typeof(vw_CUS__ExchangeRatesForReportingBook))]
	internal class vw_CUS__ExchangeRatesForReportingBookTest : BiCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestColumns()
		{
			var columns = new[]
			{
				"CompanyKey",
				"ExchangeRatesForReportingBookKey",
				"PeriodManagementKey",
				"Currency",
				"ExRateType",
				"StartDate",
				"EndDate",
				"SellRate"
			};

			var result = SelectRows();
			foreach (string column in columns)
			{
				Assert($"{column} should be contained", result.Columns.Contains(column));
			}
			AssertEquals(columns.Length, result.Columns.Count);
		}

		DataTable SelectRows()
		{
			return DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM [{ScriptDbName}].[Finance].vw_CUS__ExchangeRatesForReportingBook");
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}
	}
}
