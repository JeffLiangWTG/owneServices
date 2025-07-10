using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Misc;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Misc
{
	[TestedType(typeof(SplitStringToTable))]
	class SplitStringToTableTest : DbCreateScriptTest
	{
		public void TestSplitMultipleElementString()
		{
			var query = "SELECT value FROM dbo.SplitStringToTable('A1,B12, ,D,E12345,', ',')";

			using (var reader = TestConnection.Command(query).ExecuteReader())
			{
				AssertEquals("Reading first row in the table", true, reader.Read());
				AssertEquals("A1", reader[0]);
				AssertEquals("Reading second row in the table", true, reader.Read());
				AssertEquals("B12", reader[0]);
				AssertEquals("Reading third row in the table", true, reader.Read());
				AssertEquals("", reader[0]);
				AssertEquals("Reading forth row in the table", true, reader.Read());
				AssertEquals("D", reader[0]);
				AssertEquals("Reading fifth row in the table", true, reader.Read());
				AssertEquals("E12345", reader[0]);
				AssertEquals("Reading sixth row in the table", true, reader.Read());
				AssertEquals("", reader[0]);
				AssertEquals("There should be no more rows in the table", false, reader.Read());
			}
		}

		public void TestSplitSingleElementString()
		{
			var query = "SELECT value FROM dbo.SplitStringToTable('Highlander', ',')";

			using (var reader = TestConnection.Command(query).ExecuteReader())
			{
				AssertEquals("Reading first row in the table", true, reader.Read());
				AssertEquals("Highlander", reader[0]);
				AssertEquals("There should be no more rows in the table", false, reader.Read());
			}
		}

		public void TestSplitEmptyString()
		{
			var query = "SELECT value FROM dbo.SplitStringToTable('', ',')";

			using (var reader = TestConnection.Command(query).ExecuteReader())
			{
				AssertEquals("Reading first row in the table", true, reader.Read());
				AssertEquals("", reader[0]);
				AssertEquals("There should be no more rows in the table", false, reader.Read());
			}
		}

		public void TestSplitNullString()
		{
			var query = "SELECT value FROM dbo.SplitStringToTable(null, ',')";

			using (var reader = TestConnection.Command(query).ExecuteReader())
			{
				AssertEquals("Reading first row in the table", true, reader.Read());
				AssertEquals("", reader[0]);
				AssertEquals("There should be no more rows in the table", false, reader.Read());
			}
		}

		public void TestSplitDelimiterOnlyString()
		{
			var query = "SELECT value FROM dbo.SplitStringToTable('|||', '|')";

			using (var reader = TestConnection.Command(query).ExecuteReader())
			{
				AssertEquals("Reading first row in the table", true, reader.Read());
				AssertEquals("", reader[0]);
				AssertEquals("Reading second row in the table", true, reader.Read());
				AssertEquals("", reader[0]);
				AssertEquals("Reading third row in the table", true, reader.Read());
				AssertEquals("", reader[0]);
				AssertEquals("Reading forth row in the table", true, reader.Read());
				AssertEquals("", reader[0]);
				AssertEquals("There should be no more rows in the table", false, reader.Read());
			}
		}
	}

	[TestedType(typeof(SplitStringToTable))]
	class SplitStringToTableEdwTest : SplitStringToTableTest
	{
		protected override DbConnection TestConnection => edwConnection;

		readonly DbConnection edwConnection = Db.NewExtraConnectionToMainDbWithReaderCredentials(Db.EdwDatabaseName);
	}
}
