using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class SqlExceptionExtensionsTest : TestCase
	{
		public void TestIsDeadlock()
		{
			var deadlockSqlEx = CreateTestSqlException(1205);
			Assert("deadlockSqlEx.IsDeadlock()", deadlockSqlEx.IsDeadlock());
			var databaseNotExistSqlEx = CreateTestSqlException(5011);
			Assert("!databaseNotExistSqlEx.IsDeadlock()", !databaseNotExistSqlEx.IsDeadlock());
		}

		public void TestIsInnermostDeadlock()
		{
			var exContainingDeadlockSqlEx = CreateTestExceptionWithInnerSqlException(1205);
			Assert("exContainingDeadlockSqlEx.IsInnermostDeadlock()", exContainingDeadlockSqlEx.IsInnermostDeadlock());
			Assert(exContainingDeadlockSqlEx.ShouldReprocess());
			var exContainingDatabaseNotExistSqlEx = CreateTestExceptionWithInnerSqlException(5011);
			Assert("!exContainingDatabaseNotExistSqlEx.IsInnermostDeadlock()", !exContainingDatabaseNotExistSqlEx.IsInnermostDeadlock());
			Assert(!exContainingDatabaseNotExistSqlEx.ShouldReprocess());

			var exContainingNoSqlEx = new Exception("I am an exception", new Exception("I am a non-sql inner exception"));
			Assert("!exContainingNoSqlEx.IsInnermostDeadlock()", !exContainingNoSqlEx.IsInnermostDeadlock());
			Assert(!exContainingNoSqlEx.ShouldReprocess());
		}

		public void TestLockTimeoutExpired()
		{
			var lockTimeoutExpiredEx = CreateTestSqlException(5245);
			Assert("LockTimeoutExpiredSqlEx.LockTimeoutExpired()", lockTimeoutExpiredEx.IsLockTimeoutExpired());
			var databaseNotExistSqlEx = CreateTestSqlException(5011);
			Assert("!databaseNotExistSqlEx.LockTimeoutExpired()", !databaseNotExistSqlEx.IsLockTimeoutExpired());
		}

		public void TestIsInnermostLockTimeoutExpired()
		{
			var exContainingLockTimeoutExpiredSqlEx = CreateTestExceptionWithInnerSqlException(5245);
			Assert("exContainingLockTimeoutExpiredSqlEx.IsInnermostLockTimeoutExpired()", exContainingLockTimeoutExpiredSqlEx.IsInnermostLockTimeoutExpired());
			Assert(exContainingLockTimeoutExpiredSqlEx.ShouldReprocess());
			var exContainingDatabaseNotExistSqlEx = CreateTestExceptionWithInnerSqlException(5011);
			Assert("!exContainingDatabaseNotExistSqlEx.IsInnermostLockTimeoutExpired()", !exContainingDatabaseNotExistSqlEx.IsInnermostLockTimeoutExpired());
			Assert(!exContainingDatabaseNotExistSqlEx.ShouldReprocess());

			var exContainingNoSqlEx = new Exception("I am an exception", new Exception("I am a non-sql inner exception"));
			Assert("!exContainingNoSqlEx.IsInnermostLockTimeoutExpired()", !exContainingNoSqlEx.IsInnermostLockTimeoutExpired());
			Assert(!exContainingNoSqlEx.ShouldReprocess());
		}

		public void TestIsSevereError()
		{
			var severeErrorSqlEx = CreateTestSqlException(596);
			Assert("severeErrorSqlEx.IsSevereError()", severeErrorSqlEx.IsSevereError());
			Assert(severeErrorSqlEx.ShouldReprocess());
			var databaseNotExistSqlEx = CreateTestSqlException(5011);
			Assert("!databaseNotExistSqlEx.IsSevereError()", !databaseNotExistSqlEx.IsSevereError());
			Assert(!databaseNotExistSqlEx.ShouldReprocess());
		}

		SqlException CreateTestSqlException(int sqlErrorNumber)
		{
			var error = SqlExceptionBuilder.CreateSqlError(sqlErrorNumber, 1, 1, Db.ServerName, "", "", 0);
			var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
			return SqlExceptionBuilder.CreateSqlException(errors);
		}

		Exception CreateTestExceptionWithInnerSqlException(int sqlErrorNumber)
		{
			var error = SqlExceptionBuilder.CreateSqlError(sqlErrorNumber, 1, 1, Db.ServerName, "", "", 0);
			var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
			var sqlEx = SqlExceptionBuilder.CreateSqlException(errors);
			return new Exception("I am an exception", sqlEx);
		}
	}
}
