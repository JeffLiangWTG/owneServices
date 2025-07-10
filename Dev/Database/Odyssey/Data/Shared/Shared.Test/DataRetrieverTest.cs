using System.Data;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class DataRetrieverTest : TransactionedTestCase
	{
		public void TestLoadDataFromDatabase()
		{
			string sqlText = @"
				CREATE TABLE TempTableForTest777 (Id uniqueidentifier not null, CreateDate datetime)
				INSERT TempTableForTest777 VALUES (newid(), '2007-07-26 10:00:00')

				CREATE TABLE TempTableForTest888 (Id uniqueidentifier not null, CreateDate datetime)
				INSERT TempTableForTest888 VALUES (newid(), '2007-07-26 10:00:00')

				CREATE TABLE TempTableForTest999 (Id uniqueidentifier not null, CreateDate datetime)
				INSERT TempTableForTest999 VALUES (newid(), '2007-07-26 10:00:00')";
			Db.Connection.ExecuteNonQuery(sqlText);

			var dataSet = new DataSet();
			DataRetriever.LoadDataFromDatabase(dataSet, "SELECT * FROM TempTableForTest777 ORDER BY 1;SELECT * FROM TempTableForTest888 ORDER BY 1;", new[] { "TempTableForTest777", "TempTableForTest888" });
			AssertEquals(dataSet.Tables.Count, 2);
			AssertNotNull(dataSet.Tables["TempTableForTest777"]);
			AssertNotNull(dataSet.Tables["TempTableForTest888"]);

			DataRetriever.LoadDataFromDatabase(dataSet, "SELECT * FROM TempTableForTest999 ORDER BY 1;", new[] { "TempTableForTest999" });
			AssertEquals(dataSet.Tables.Count, 3);
			AssertNotNull(dataSet.Tables["TempTableForTest777"]);
			AssertNotNull(dataSet.Tables["TempTableForTest888"]);
			AssertNotNull(dataSet.Tables["TempTableForTest999"]);
		}
	}
}
