using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class ColumnSynchroniserComputedColumnsNoMockTest : TransactionedTestCase
	{
		public void TestComputedColumnIsNotAllowedToBeUsedInAnotherComputedColumn()
		{
			var ex = AssertExceptionThrown<SqlException>(() =>
			{
				TestConnection.ExecuteNonQuery(@"
CREATE TABLE dbo.TestTableName
(
	A int,
	B AS A + 1,
	C AS B + 2,
)

");
			});

			AssertEquals("Computed column 'B' in table 'TestTableName' is not allowed to be used in another computed-column definition.", ex.Message);

			TestConnection.ExecuteNonQuery(@"
CREATE TABLE dbo.TestTableName
(
	A int,
	B AS A + 1,
)

");

			ex = AssertExceptionThrown<SqlException>(() =>
			{
				TestConnection.ExecuteNonQuery(@"
ALTER TABLE dbo.TestTableName ADD C AS B + 2

");
			});

			AssertEquals("Computed column 'B' in table 'TestTableName' is not allowed to be used in another computed-column definition.", ex.Message);
		}
	}
}
