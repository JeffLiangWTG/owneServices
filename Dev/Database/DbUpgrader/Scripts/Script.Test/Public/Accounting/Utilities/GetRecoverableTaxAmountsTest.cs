using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Utilities;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Utilities.Testing
{
	[TestedType(typeof(GetRecoverableTaxAmounts))]
	class GetRecoverableTaxAmountsTest : DbCreateScriptTest
	{
		public void TestAmounts()
		{
			var resultTable = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT GSTVATRecoverable, GSTVATNotRecoverable FROM GetRecoverableTaxAmounts(10.11, 0.777, 100)");
			AssertEquals("Rows.Count", 1, resultTable.Rows.Count);
			var row = resultTable.Rows[0];
			AssertEquals("GSTVATRecoverable should be rounded", 7.86m, row["GSTVATRecoverable"]);
			AssertEquals("GSTVATNotRecoverable", 2.25m, row["GSTVATNotRecoverable"]);

			resultTable = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT GSTVATRecoverable, GSTVATNotRecoverable FROM GetRecoverableTaxAmounts(10.1, 0.77, 10)");
			AssertEquals("Rows.Count", 1, resultTable.Rows.Count);
			row = resultTable.Rows[0];
			AssertEquals("GSTVATRecoverable should be rounded", 7.8m, row["GSTVATRecoverable"]);
			AssertEquals("GSTVATNotRecoverable", 2.3m, row["GSTVATNotRecoverable"]);

			resultTable = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT GSTVATRecoverable, GSTVATNotRecoverable FROM GetRecoverableTaxAmounts(10, 0.777, 1)");
			AssertEquals("Rows.Count", 1, resultTable.Rows.Count);
			row = resultTable.Rows[0];
			AssertEquals("GSTVATRecoverable should be rounded", 8m, row["GSTVATRecoverable"]);
			AssertEquals("GSTVATNotRecoverable", 2m, row["GSTVATNotRecoverable"]);

			resultTable = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT GSTVATRecoverable, GSTVATNotRecoverable FROM GetRecoverableTaxAmounts(0, 0.777, 1)");
			AssertEquals("Rows.Count", 1, resultTable.Rows.Count);
			row = resultTable.Rows[0];
			AssertEquals("GSTVATRecoverable should be rounded", 0m, row["GSTVATRecoverable"]);
			AssertEquals("GSTVATNotRecoverable", 0m, row["GSTVATNotRecoverable"]);

			resultTable = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT GSTVATRecoverable, GSTVATNotRecoverable FROM GetRecoverableTaxAmounts(10, 0, 1)");
			AssertEquals("Rows.Count", 1, resultTable.Rows.Count);
			row = resultTable.Rows[0];
			AssertEquals("GSTVATRecoverable should be rounded", 0m, row["GSTVATRecoverable"]);
			AssertEquals("GSTVATNotRecoverable", 10m, row["GSTVATNotRecoverable"]);

			resultTable = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT GSTVATRecoverable, GSTVATNotRecoverable FROM GetRecoverableTaxAmounts(0, 0, 1)");
			AssertEquals("Rows.Count", 1, resultTable.Rows.Count);
			row = resultTable.Rows[0];
			AssertEquals("GSTVATRecoverable should be rounded", 0m, row["GSTVATRecoverable"]);
			AssertEquals("GSTVATNotRecoverable", 0m, row["GSTVATNotRecoverable"]);

			resultTable = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT GSTVATRecoverable, GSTVATNotRecoverable FROM GetRecoverableTaxAmounts(10, 1, 1)");
			AssertEquals("Rows.Count", 1, resultTable.Rows.Count);
			row = resultTable.Rows[0];
			AssertEquals("GSTVATRecoverable should be rounded", 10m, row["GSTVATRecoverable"]);
			AssertEquals("GSTVATNotRecoverable", 0m, row["GSTVATNotRecoverable"]);

			resultTable = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT GSTVATRecoverable, GSTVATNotRecoverable FROM GetRecoverableTaxAmounts(1, 1, NULL)");
			AssertEquals("Rows.Count", 1, resultTable.Rows.Count);
			row = resultTable.Rows[0];
			AssertEquals("GSTVATRecoverable should be rounded", DBNull.Value, row["GSTVATRecoverable"]);
			AssertEquals("GSTVATNotRecoverable", DBNull.Value, row["GSTVATNotRecoverable"]);
		}
	}
}

