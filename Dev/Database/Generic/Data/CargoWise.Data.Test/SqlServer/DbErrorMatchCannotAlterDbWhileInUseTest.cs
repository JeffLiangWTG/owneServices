using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class DbErrorMatchCannotAlterDbWhileInUseTest : TestCase
	{
		public void TestSqlErrorNumbers()
		{
			foreach (var sqlErrorNumber in new[] { 5052, 5061, 5069, 5070 })
			{
				var dbErrorMatch = new DbErrorMatch(SqlExceptionBuilder.CreateSqlException(sqlErrorNumber, "DbErrorType.CannotAlterDbWhileInUse"));
				Assert(dbErrorMatch.ExceptionType == DbErrorType.CannotAlterDbWhileInUse);
			}
		}
	}
}
