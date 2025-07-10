using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using static System.FormattableString;

namespace Enterprise.DbHealth.Check
{
	class PhysicalFileLocationChecker : IChecker
	{
		public PhysicalFileLocationChecker()
		{
		}

		public void Check(DbConnection connection, DbHealthWarningList warningList, ILogger logger)
		{
			var dbNames = GetDatabases(connection);
			DoCheckDataAndLogFileLocation(dbNames, connection, warningList);
		}

		string IChecker.Description
		{
			get { return "Check database DATA and LOG files are not in the same disk volume"; }
		}

		/// <summary>
		/// For each operation database it checks if its data and log files are located in separate disks.
		/// Adds a warning for each DB where data and log files are located in the same disk.
		/// </summary>
		/// <returns>Warning List</returns>
		protected void DoCheckDataAndLogFileLocation(IEnumerable<string> dbNames, DbConnection connection, DbHealthWarningList warningList)
		{
			if (ObjectFactory.Get<IEntityFrameworkSettings>().SuppressDbFilesHealthCheckNotificationsForHostedSystems && EnvProxy.IsHostedWithCargowise)
			{
				return;
			}

			foreach (string dbName in dbNames)
			{
				string[] dataFiles = connection.GetDBDataFiles(dbName);
				string[] logFiles = connection.GetDBLogFiles(dbName);

				List<string> dataDisks = GetDbDbFileDrives(dataFiles);
				List<string> logDisks = GetDbDbFileDrives(logFiles);

				foreach (string dataDisk in dataDisks)
				{
					if (logDisks.Contains(dataDisk))
					{
						string description =
							Invariant($"The database [{dbName}] has data and log files located in the same disk volume.")
							+ "\r\nthis reduces database performance and puts its recoverability at risk in case the shared volume crashes."
							+ "\r\n\r\nData File(s):\r\n" + String.Join("\r\n", dataFiles)
							+ "\r\nLog File(s):\r\n" + String.Join("\r\n", logFiles);

						string action =
							"Move the database files so data and log are located in 2 separate physical disks or RAIDs."
							+ "\r\n\r\nWhilst a RAID volume is more reliable, it is not uncommon to have a problem that "
							+ "damages the RAID volume and requires the data to be restored from tape. "
							+ "\r\n\r\nIf the log suffers the same fate as the data when the disk, storage unit or RAID crashes "
							+ "then the restore will cause irretrievable loss of data. "
							+ "\r\n\r\nIf the log file is on a separate physical disk or RAID which has not been damaged "
							+ "then it can be reapplied (rolled forward) to the last moment of data entry and no data will be lost.";

						DatabaseWarning warning = new DatabaseWarning(dbName, DatabaseWarning.FileLocationWarning, description, action);
						warningList.Add(warning);

						break;
					}
				}
			}
		}

		List<string> GetDbDbFileDrives(string[] dbFiles)
		{
			List<string> result = new List<string>();

			foreach (string dbFile in dbFiles)
			{
				string driveLetter = dbFile.Trim().Substring(0, 2);

				if (!result.Contains(driveLetter))
				{
					result.Add(driveLetter);
				}
			}

			return result;
		}

		static IEnumerable<string> GetDatabases(DbConnection connection)
		{
			return (RefDbTableNameResolver.ShouldUseSharedAvailabilityGroupDatabases(connection))
				? connection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW)
				: connection.GetDatabases(DatabaseType.Operational | DatabaseType.UserRepository | DatabaseType.Audit);
		}
	}
}
