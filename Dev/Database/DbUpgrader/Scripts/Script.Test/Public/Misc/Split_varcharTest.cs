using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Misc;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Misc
{
	[TestedType(typeof(Split_varchar))]
	class Split_varcharTest : DbCreateScriptTest
	{
		public void TestSplit()
		{
			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "select SNS_Number, v = '[' + Value + ']' from dbo.Split_varchar('AB'  , ',')");
			AssertEquals("1 row should be found", 1, result.Rows.Count);
			AssertEquals("[AB]", result.Rows[0]["v"].ToString());
			result = DataUtils.GetDataTableFromQuery(TestConnection, "select SNS_Number, v = '[' + Value + ']' from dbo.Split_varchar('A,B' , ',')");
			AssertEquals("2 rows should be found", 2, result.Rows.Count);
			AssertEquals("[A]", result.Rows[0]["v"].ToString());
			AssertEquals("[B]", result.Rows[1]["v"].ToString());
			result = DataUtils.GetDataTableFromQuery(TestConnection, "select SNS_Number, v = '[' + Value + ']' from dbo.Split_varchar(',AB' , ',')");
			AssertEquals("2 rows should be found", 2, result.Rows.Count);
			AssertEquals("[]", result.Rows[0]["v"].ToString());
			AssertEquals("[AB]", result.Rows[1]["v"].ToString());
			result = DataUtils.GetDataTableFromQuery(TestConnection, "select SNS_Number, v = '[' + Value + ']' from dbo.Split_varchar(',A,B', ',')");
			AssertEquals("3 rows should be found", 3, result.Rows.Count);
			AssertEquals("[]", result.Rows[0]["v"].ToString());
			AssertEquals("[A]", result.Rows[1]["v"].ToString());
			AssertEquals("[B]", result.Rows[2]["v"].ToString());

			result = DataUtils.GetDataTableFromQuery(TestConnection, "select SNS_Number, v = '[' + Value + ']' from dbo.Split_varchar('AB'  , ' ')");
			AssertEquals("1 row should be found", 1, result.Rows.Count);
			AssertEquals("[AB]", result.Rows[0]["v"].ToString());
			result = DataUtils.GetDataTableFromQuery(TestConnection, "select SNS_Number, v = '[' + Value + ']' from dbo.Split_varchar('A B' , ' ')");
			AssertEquals("2 rows should be found", 2, result.Rows.Count);
			AssertEquals("[A]", result.Rows[0]["v"].ToString());
			AssertEquals("[B]", result.Rows[1]["v"].ToString());
			result = DataUtils.GetDataTableFromQuery(TestConnection, "select SNS_Number, v = '[' + Value + ']' from dbo.Split_varchar(' AB' , ' ')");
			AssertEquals("2 rows should be found", 2, result.Rows.Count);
			AssertEquals("[]", result.Rows[0]["v"].ToString());
			AssertEquals("[AB]", result.Rows[1]["v"].ToString());
			result = DataUtils.GetDataTableFromQuery(TestConnection, "select SNS_Number, v = '[' + Value + ']' from dbo.Split_varchar(' A B', ' ')");
			AssertEquals("3 rows should be found", 3, result.Rows.Count);
			AssertEquals("[]", result.Rows[0]["v"].ToString());
			AssertEquals("[A]", result.Rows[1]["v"].ToString());
			AssertEquals("[B]", result.Rows[2]["v"].ToString());

			result = DataUtils.GetDataTableFromQuery(TestConnection, "select SNS_Number, v = '[' + Value + ']' from dbo.Split_varchar('AB' , ', ')");
			AssertEquals("1 row should be found", 1, result.Rows.Count);
			AssertEquals("[AB]", result.Rows[0]["v"].ToString());
			result = DataUtils.GetDataTableFromQuery(TestConnection, "select SNS_Number, v = '[' + Value + ']' from dbo.Split_varchar('A, B' , ', ')");
			AssertEquals("2 rows should be found", 2, result.Rows.Count);
			AssertEquals("[A]", result.Rows[0]["v"].ToString());
			AssertEquals("[B]", result.Rows[1]["v"].ToString());
			result = DataUtils.GetDataTableFromQuery(TestConnection, "select SNS_Number, v = '[' + Value + ']' from dbo.Split_varchar(', AB' , ', ')");
			AssertEquals("2 rows should be found", 2, result.Rows.Count);
			AssertEquals("[]", result.Rows[0]["v"].ToString());
			AssertEquals("[AB]", result.Rows[1]["v"].ToString());
			result = DataUtils.GetDataTableFromQuery(TestConnection, "select SNS_Number, v = '[' + Value + ']' from dbo.Split_varchar(', A, B', ', ')");
			AssertEquals("3 rows should be found", 3, result.Rows.Count);
			AssertEquals("[]", result.Rows[0]["v"].ToString());
			AssertEquals("[A]", result.Rows[1]["v"].ToString());
			AssertEquals("[B]", result.Rows[2]["v"].ToString());

			result = DataUtils.GetDataTableFromQuery(TestConnection, "select SNS_Number, v = '[' + Value + ']' from dbo.Split_varchar('A and B', 'and') ");
			AssertEquals("2 rows should be found", 2, result.Rows.Count);
			AssertEquals("[A ]", result.Rows[0]["v"].ToString());
			AssertEquals("[ B]", result.Rows[1]["v"].ToString());
		}

		public void TestDuplicatedCodes()
		{
			var addInfoValue = "*test=1**test=2*";
			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT * FROM Split_varchar ('{0}','*')", addInfoValue));
			AssertEquals("5 rows should be found", 5, result.Rows.Count);
			AssertEquals("", result.Rows[0]["Value"].ToString());
			AssertEquals("test=1", result.Rows[1]["Value"].ToString());
			AssertEquals("", result.Rows[2]["Value"].ToString());
			AssertEquals("test=2", result.Rows[3]["Value"].ToString());
			AssertEquals("", result.Rows[4]["Value"].ToString());

			addInfoValue = "***";
			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT * FROM Split_varchar ('{0}','*')", addInfoValue));
			AssertEquals("4 rows should be found", 4, result.Rows.Count);
			AssertEquals("", result.Rows[0]["Value"].ToString());
			AssertEquals("", result.Rows[1]["Value"].ToString());
			AssertEquals("", result.Rows[2]["Value"].ToString());
			AssertEquals("", result.Rows[3]["Value"].ToString());

			addInfoValue = "*Description=SCREW*IntendedUseCode=081.001*ProdCountry=US*IntendedUseCode=081.002*ProductCode=87M--NI*ProdCountry=DE*";
			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT * FROM Split_varchar ('{0}','*')", addInfoValue));
			AssertEquals("8 rows should be found", 8, result.Rows.Count);
			AssertEquals("", result.Rows[0]["Value"].ToString());
			AssertEquals("Description=SCREW", result.Rows[1]["Value"].ToString());
			AssertEquals("IntendedUseCode=081.001", result.Rows[2]["Value"].ToString());
			AssertEquals("ProdCountry=US", result.Rows[3]["Value"].ToString());
			AssertEquals("IntendedUseCode=081.002", result.Rows[4]["Value"].ToString());
			AssertEquals("ProductCode=87M--NI", result.Rows[5]["Value"].ToString());
			AssertEquals("ProdCountry=DE", result.Rows[6]["Value"].ToString());
			AssertEquals("", result.Rows[7]["Value"].ToString());
		}
	}
}
