using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Data;

namespace Enterprise.SqlSecurity
{
	static class Helper
	{
		internal static DatabaseType GetDatabaseType(string mainDatabaseName, string singleRefDbName, string databaseName)
		{
			if (databaseName.Equals(mainDatabaseName, StringComparison.OrdinalIgnoreCase))
			{
				return DatabaseType.Main;
			}

			if (databaseName.Equals(mainDatabaseName + Db.EdwDatabaseSuffix, StringComparison.OrdinalIgnoreCase))
			{
				return DatabaseType.EDW;
			}

			if (databaseName.Equals(mainDatabaseName + Db.AuditDatabaseSuffix, StringComparison.OrdinalIgnoreCase))
			{
				return DatabaseType.Audit;
			}

			if (databaseName.Equals(mainDatabaseName + DbUserRepository.RepositoryDbSuffix, StringComparison.OrdinalIgnoreCase))
			{
				return DatabaseType.UserRepository;
			}

			if (databaseName.StartsWith(mainDatabaseName + Db.SDDatabaseAffix, StringComparison.OrdinalIgnoreCase))
			{
				return DatabaseType.SD;
			}

			if (databaseName.Equals(singleRefDbName, StringComparison.OrdinalIgnoreCase))
			{
				return DatabaseType.SingleSharedRef;
			}

			if (RefDbTableNameResolver.IsExclusiveDatabase(mainDatabaseName, databaseName))
			{
				return DatabaseType.ExclusiveRef;
			}

			if (RefDbTableNameResolver.IsSharedDatabase(databaseName))
			{
				return DatabaseType.SharedRef;
			}

			throw new ArgumentException($"Database name '{databaseName}' does not match with any of the supported database types", nameof(databaseName));
		}

		internal static void PerformActionWithElevatedPermissions(Action action, CancellationToken cancellationToken)
		{
			var task = Task.Run(() =>
			{
				// We need to make sure that admin connection is used here.
				// If we do not do this, then trying to get information from registry
				// will cause another connection to be open using RestrictedWriterLogin.
				// If RestrictedWriterLogin does not exist,
				// the system will try to correct it, and potentially call this code that would result in an infinite loop.
				using (((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade())
				{
					action();
				}
			});

			task.Wait(cancellationToken);
		}

		internal static string DbValueToNullString(object dbValue)
		{
			if (dbValue is bool)
			{
				return (bool)dbValue ? "1" : "0";
			}

			if (dbValue is byte[])
			{
				return DataUtils.BytesToHexString((byte[])dbValue);
			}

			if (dbValue is string)
			{
				return (string)dbValue;
			}

			return "NULL";
		}
	}
}
