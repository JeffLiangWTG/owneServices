using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.DbHealth.Check.Checkers;
using Enterprise.Integration;

namespace Enterprise.DbHealth.Check
{
	public class DbCheckRunner
	{
		public DbHealthWarningList PerformMainChecks(string dbServer, string mainDbName, ILogger logger)
		{
			var checkers = new IChecker[]
			{
				new DbOptionsChecker(),
				new AlwaysOnGroupChecker(),
				new BackupChecker(),
				new RestoreChecker(),
				new GhostRecordsChecker(),
				new PhysicalFileLocationChecker(),
				new PhysicalFileSizeChecker(),
				new SnapshotIsolationChecker(),
				new UtcTimeChecker(),
				new AvailabilityGroupListenerChecker(),
				new TraceFlagChecker(),
				new InstantFileInitializationChecker(),
				new DbServerAddressChecker(),
				new CdcLatencyChecker(),
				new DbNameChecker(),
			};

			var list = this.PerformChecks(dbServer, mainDbName, logger, checkers);
			return list;
		}

		public DbHealthWarningList CheckDatabaseConsistency(string dbServer, string mainDbName, ILogger logger)
		{
			using (var conn = Db.NewAdminConnection(dbServer, mainDbName))
			{
				var list = PerformChecks(conn, mainDbName, logger, new IChecker[] { new DbConsistencyChecker(conn, mainDbName) });
				return list;
			}
		}

		public DbHealthWarningList CheckDatabaseConsistencyOnSecondaries(DbConnection primaryConnection, AdminConnection secondaryConnection, string mainDbName, ILogger logger)
		{
			return
				PerformChecks(secondaryConnection, mainDbName, logger, new IChecker[] { new DbConsistencyChecker(primaryConnection, secondaryConnection, mainDbName) });
		}

		/// <summary>
		/// Must use AdminConnection as most of the DBA checks performed required admin rights.
		/// </summary>
		protected DbHealthWarningList PerformChecks(string dbServer, string mainDbName, ILogger logger, IEnumerable<IChecker> checkers)
		{
			using (var conn = Db.NewAdminConnection(dbServer, mainDbName))
			{
				return PerformChecks(conn, mainDbName, logger, checkers);
			}
		}

		DbHealthWarningList PerformChecks(DbConnection conn, string mainDbName, ILogger logger, IEnumerable<IChecker> checkers)
		{
			DbHealthWarningList result = new DbHealthWarningList();

			result.DbWindowsServerName = conn.ServerNameWithoutInstance;
			result.DbServerAndInstance = conn.ServerNameReportedByDatabase;
			result.MainDbName = mainDbName;

			foreach (IChecker checker in checkers)
			{
				logger.Log(LogType.Information, checker.Description);
				checker.Check(conn, result, logger);
			}

			logger.Log(LogType.Information, "DbHealthCheck is completed");

			return result;
		}
	}
}
