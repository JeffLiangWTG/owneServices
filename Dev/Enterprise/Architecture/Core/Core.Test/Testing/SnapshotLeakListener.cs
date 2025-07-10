
using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	internal class SnapshotLeakListener : BaseTestListener
	{
		public override void StartAllTests(DateTime startTime)
		{
			if (TestingState.IsRunningOnDAT)
			{
				return;
			}

			try
			{
				using (var connection = Db.NewAdminConnection())
				{
					foreach (var databaseSuffix in DatabasesToCheckSuffixes)
					{
						var databaseName = Db.DatabaseName + databaseSuffix;
						var snapshotName = UseSnapshotProtectionAttribute.GetSnapshotName(databaseName);
						if (DoesSnapshotExist(connection, snapshotName))
						{
							SnapshotCreator.RestoreFromSnapshot(connection, () => Db.Connection.CloseConnection(), databaseName, snapshotName);
						}
					}
				}
			}
			catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.DatabaseCannotBeRevertedFromSnapshot)
			{
				// You cannot revert a database from a snapshot when there are multiple snapshots on the same database, sql server limitation
				// Fail the test run so the developer is notified that a snapshot was not restored.
				HasFailedToRestore = true;
			}
		}

		bool HasFailedToRestore;

		public override void AfterEachTest(DateTime endTime)
		{
			if (HasFailedToRestore)
			{
				Assertion.Fail("A database snapshot used for a snapshot protected test could not be restored because there are multiple snapshots on the same database, this may occur as a result of having additional snapshots on the database that is used for testing. Please drop the additional snapshot to allow snapshot protected tests to function properly.");
				HasFailedToRestore = false;
			}
		}

		bool DoesSnapshotExist(DbConnection connection, string snapshotName)
		{
			return connection.Exists($"FROM sys.databases where source_database_id is not null and name = '{snapshotName}'");
		}

		string[] DatabasesToCheckSuffixes => new[] { "", Db.EdwDatabaseSuffix, Db.AuditDatabaseSuffix };
	}
}
