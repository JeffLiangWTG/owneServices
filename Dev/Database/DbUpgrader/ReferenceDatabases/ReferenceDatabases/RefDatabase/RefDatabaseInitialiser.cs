using System;
using System.Globalization;
using CargoWise.Data;
using CargoWise.RefDbRepo.Client.Common;

namespace Enterprise.DbUpgrader.ReferenceDatabases
{
	public static class RefDatabaseInitialiser
	{
		public static void CreateRefDbIfNotExists(AdminConnection adminConnection, string refDBName)
		{
			RefServiceExceptionHandler.Execute(() =>
			CreateRefDbIfNotExistsCore(adminConnection, refDBName),
			RefServiceExceptionHandler.IgnoreOption.IgnoreTimeoutException |
			RefServiceExceptionHandler.IgnoreOption.IgnoreConflictOperationException |
			RefServiceExceptionHandler.IgnoreOption.IgnoreCreateDatabasePermissionDeniedException |
			RefServiceExceptionHandler.IgnoreOption.IgnoreDbVersionMismatchException);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		static void CreateRefDbIfNotExistsCore(AdminConnection adminConnection, string refDBName)
		{
			if (!adminConnection.DatabaseExists(refDBName))
			{
				adminConnection.DefaultCommandTimeOutInSeconds = DbCommand.Timeout.Infinite;
				adminConnection.CreateDatabase(
					databaseName: refDBName,
					dataPath: null,
					logPath: null,
					dataInitialSizeMb: null,
					logInitialSizeMb: RefDbLogFileIntialSizeMb,
					dataGrowthMb: null,
					logGrowthMb: RefDbLogFileGrowthMb,
					mapDbLogins: false,
					actionToPerformAfterCreatingDb: (dbName) =>
					{
						using (((ICurrentDbControl)adminConnection).UseDatabase(dbName))
						{
							GrantGuestUserPermission(adminConnection);
							DataUtils.SaveDbExtendedProperty(adminConnection, RefDbVersionProperty, "0");
						}
					},
					sqlLockTimeout: null,
					physicalFileSuffix: DateTime.UtcNow.ToString("yyyyMMddhhmmss", CultureInfo.InvariantCulture));
			}
		}

		public static void GrantGuestUserPermission(DbConnection connection)
		{
			connection.ExecuteNonQuery(EnableGuestUserQuery);
		}

		public const string RefDbVersionProperty = "RefDatabaseVersion";
		const int RefDbLogFileIntialSizeMb = 100;
		const int RefDbLogFileGrowthMb = 100;
		const string EnableGuestUserQuery = @"
IF ((SELECT COUNT(*) FROM sys.database_principals princ
INNER JOIN sys.database_permissions perm
ON princ.principal_id = perm.grantee_principal_id
WHERE name = 'guest' AND perm.permission_name IN ('CONNECT', 'SELECT', 'EXECUTE')) < 3) 
BEGIN
	GRANT CONNECT, SELECT, EXECUTE TO guest
END";
	}
}
