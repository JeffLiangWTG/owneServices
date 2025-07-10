using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Script.Test
{
	sealed class TableCodeNameMappingCreatorTest : TransactionedTestCase
	{
		public void TestRunWillReCreatesTableCodeNameMappingFunctionEveryTime()
		{
			var dropTableCodeNameFunction = @"DROP FUNCTION [dbo].[TableCodeNameMapping]";
			Db.Connection.ExecuteNonQuery(dropTableCodeNameFunction);
			var createTableCodeNameFunction = @"
CREATE FUNCTION[dbo].[TableCodeNameMapping]
			(
   @key    varchar(35),
	@isCode bit
)

RETURNS TABLE AS RETURN

WITH
	Map AS(SELECT * FROM(VALUES
			('XD', 'ExternalData'),
			('FXD', 'FakeExternalData')
		) AS t(Code, Name))
SELECT
	Code, Name
	, Result = IIF(@isCode = 1, Name, Code)
FROM
	Map
WHERE 1 = 2
	OR @isCode = 1 AND Code = @key
	OR @isCode = 0 AND Name = @key";

			Db.Connection.ExecuteNonQuery(createTableCodeNameFunction);
			string sqlText = string.Format(
				"IF EXISTS(SELECT null FROM [{0}].sys.objects WHERE name = '{1}' AND [type] = 'IF') SELECT 1 ELSE SELECT 0",
				Db.DatabaseName, "TableCodeNameMapping");
			bool actual = Convert.ToBoolean(Db.Connection.ExecuteScalar(sqlText));
			Assert(actual);

			AssertEquals("FakeExternalData", Db.Connection.ExecuteScalar<string>("SELECT Name FROM TableCodeNameMapping('FXD', 1)"));
			AssertEquals("FXD", Db.Connection.ExecuteScalar<string>("SELECT Code FROM TableCodeNameMapping('FXD', 1)"));
			AssertEquals("FakeExternalData", Db.Connection.ExecuteScalar<string>("SELECT Result FROM TableCodeNameMapping('FXD', 1)"));

			AssertEquals("FakeExternalData", Db.Connection.ExecuteScalar<string>("SELECT Name FROM TableCodeNameMapping('FakeExternalData', 0)"));
			AssertEquals("FXD", Db.Connection.ExecuteScalar<string>("SELECT Code FROM TableCodeNameMapping('FakeExternalData', 0)"));
			AssertEquals("FXD", Db.Connection.ExecuteScalar<string>("SELECT Result FROM TableCodeNameMapping('FakeExternalData', 0)"));

			AssertEquals(null, Db.Connection.ExecuteScalar("SELECT Name FROM TableCodeNameMapping('SD', 1)"));
			AssertEquals(null, Db.Connection.ExecuteScalar("SELECT Code FROM TableCodeNameMapping('SD', 1)"));
			AssertEquals(null, Db.Connection.ExecuteScalar("SELECT Result FROM TableCodeNameMapping('SD', 1)"));

			AssertEquals(null, Db.Connection.ExecuteScalar("SELECT Name FROM TableCodeNameMapping('StmData', 0)"));
			AssertEquals(null, Db.Connection.ExecuteScalar("SELECT Code FROM TableCodeNameMapping('StmData', 0)"));
			AssertEquals(null, Db.Connection.ExecuteScalar("SELECT Result FROM TableCodeNameMapping('StmData', 0)"));

			var manager = new DummyUpgradeManager();
			var tableCodeNameMappingCreator = new TableCodeNameMappingCreator(manager, Db.Connection);
			tableCodeNameMappingCreator.Run();
			Assert(actual);

			AssertEquals(null, Db.Connection.ExecuteScalar("SELECT Name FROM TableCodeNameMapping('FXD', 1)"));
			AssertEquals(null, Db.Connection.ExecuteScalar("SELECT Code FROM TableCodeNameMapping('FXD', 1)"));
			AssertEquals(null, Db.Connection.ExecuteScalar("SELECT Result FROM TableCodeNameMapping('FXD', 1)"));

			AssertEquals(null, Db.Connection.ExecuteScalar("SELECT Name FROM TableCodeNameMapping('FakeExternalData', 0)"));
			AssertEquals(null, Db.Connection.ExecuteScalar("SELECT Code FROM TableCodeNameMapping('FakeExternalData', 0)"));
			AssertEquals(null, Db.Connection.ExecuteScalar("SELECT Result FROM TableCodeNameMapping('FakeExternalData', 0)"));

			AssertEquals("StmData", Db.Connection.ExecuteScalar<string>("SELECT Name FROM TableCodeNameMapping('SD', 1)"));
			AssertEquals("SD", Db.Connection.ExecuteScalar<string>("SELECT Code FROM TableCodeNameMapping('SD', 1)"));
			AssertEquals("StmData", Db.Connection.ExecuteScalar<string>("SELECT Result FROM TableCodeNameMapping('SD', 1)"));

			AssertEquals("StmData", Db.Connection.ExecuteScalar<string>("SELECT Name FROM TableCodeNameMapping('StmData', 0)"));
			AssertEquals("SD", Db.Connection.ExecuteScalar<string>("SELECT Code FROM TableCodeNameMapping('StmData', 0)"));
			AssertEquals("SD", Db.Connection.ExecuteScalar<string>("SELECT Result FROM TableCodeNameMapping('StmData', 0)"));
		}
	}
}
