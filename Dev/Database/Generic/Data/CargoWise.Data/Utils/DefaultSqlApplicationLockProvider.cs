using System;

namespace CargoWise.Data.Utils
{
	public class DefaultSqlApplicationLockProvider : ISqlApplicationLockProvider
	{
		public bool TryGetLock(string key, out ISqlApplicationLock appLock, string dbName = null)
		{
			var result = Db.Connection.TryGetLock(key, out var sqlLock, dbName);
			appLock = sqlLock;
			return result;
		}

		public bool TryGetLock(string key, TimeSpan lockTimeout, out ISqlApplicationLock appLock, string dbName = null)
		{
			var result = Db.Connection.TryGetLock(key, lockTimeout, out var sqlLock, dbName);
			appLock = sqlLock;
			return result;
		}
	}
}

#region Test
#if DEBUG

namespace CargoWise.Data.Utils.Tests
{
	public class TestLockProvider : ISqlApplicationLockProvider
	{
		public static TestLockProvider CreateDbLockProvider(DbConnection connection)
		{
			return new TestLockProvider(connection);
		}

		public static TestLockProvider CreateCustomLockProvider(Func<string, TimeSpan, string, ISqlApplicationLock> lockCreateFunc)
		{
			return new TestLockProvider(lockCreateFunc);
		}

		public Func<bool> PreFunc { get; set; } = () => true;
		public Action PostFunc { get; set; } = () => { };

		public Func<string, TimeSpan, string, ISqlApplicationLock> LockCreateFunc { get; set; }

		TestLockProvider(Func<string, TimeSpan, string, ISqlApplicationLock> lockCreateFunc)
		{
			LockCreateFunc = lockCreateFunc;
		}

		TestLockProvider(DbConnection connection)
		{
			LockCreateFunc = (key, lockTimeout, dbName) =>
			{
				var result = connection.TryGetLock(key, out var sqlLock, dbName);
				return result ? sqlLock : null;
			};
		}

		public bool TryGetLock(string key, out ISqlApplicationLock appLock, string dbName = null)
		{
			return TryGetLock(key, TimeSpan.Zero, out appLock, dbName);
		}

		public bool TryGetLock(string key, TimeSpan lockTimeout, out ISqlApplicationLock appLock, string dbName = null)
		{
			var result = false;
			appLock = null;

			if (PreFunc())
			{
				appLock = LockCreateFunc(key, lockTimeout, dbName);
				result = appLock != null;
			}

			PostFunc();
			return result;
		}
	}
}

#endif
#endregion