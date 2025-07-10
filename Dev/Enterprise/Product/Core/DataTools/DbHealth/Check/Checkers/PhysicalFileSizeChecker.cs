using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using CargoWise.Data;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using static System.FormattableString;

namespace Enterprise.DbHealth.Check
{
	class PhysicalFileSizeChecker : IChecker
	{
		public PhysicalFileSizeChecker()
		{
		}

		void IChecker.Check(DbConnection connection, DbHealthWarningList warningList, ILogger logger)
		{
			var dbNames = GetDatabases(connection);
			DoCheckDesktopExpressDataSize(connection, warningList);
			DoCheckDiskSpace(dbNames, connection, warningList);
		}

		string IChecker.Description
		{
			get { return "Check database storage disk devices are not running out of space"; }
		}

		/// <summary>
		/// Checks if the sum of the data file sizes of the main database in a SQL Server engine
		/// is getting closer to the limit imposed by these engines.
		/// It warns if total size > 75% of the limit.
		///   - 2008 or earlier  => Express max data size = 4GB (warns if total size > 3GB)
		///   - 2008 R2 or later => Express max data size = 10GB (warns if total size > 9GB)
		/// </summary>
		/// <returns>Warning List</returns>
		protected void DoCheckDesktopExpressDataSize(DbConnection connection, DbHealthWarningList warningList)
		{
			if (IsExpressSqlServerEdition(connection.ServerEdition))
			{
				string sqlText = Invariant($"SELECT sum(size) FROM {Db.DatabaseName}.sys.database_files WHERE [type] = 0");
				long totalPages = Convert.ToInt64(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture);

				if (totalPages > SqlExpressDataPagesThreshold)
				{
					string description = "Database size is getting close to the MS SQL Server Express Edition limit.";

					string action =
						"Upgrade to Standard/Enterprise SQL Server licence ASAP."
						+ "\r\n\r\nWhilst these databases are supplied free, "
						+ "SQL Server Express Editions are designed to hold only 10GB of data (4GB for SQL Server 2008 or earlier). "
						+ "These limits are set by Microsoft and are not adjustable. "
						+ "\r\n\r\nThe full SQL Server Standard Edition is recommended and "
						+ "CargoWise has special ISV supplied versions that are around 1/3 of the standard pricing. "
						+ "\r\n\r\nYou should consult our web site prior to purchasing any upgrade and examine the options "
						+ "for SQL Server Editions and x64 architecture in supporting a faster and more scalable long term solution.";

					DatabaseWarning warning = new DatabaseWarning(Db.DatabaseName, DatabaseWarning.ExpressDbSizeWarning, description, action);
					warningList.Add(warning);
				}
			}
		}

		/// <summary>
		/// Checks the amount of free space on each disk containing any operational databases files (data or log).
		/// A warning is returned for each disk where free space is less than the sum of all files in it.
		/// </summary>
		/// <returns>Warning List</returns>
		protected void DoCheckDiskSpace(IEnumerable<string> dbNames, DbConnection connection, DbHealthWarningList warningList)
		{
			string sqlText = GetAllDrivesQuery(dbNames);
			DataTable dbFileDrives = Utilities.GetDataTableFromQuery(connection, sqlText);

			foreach (DataRow driveRow in dbFileDrives.Rows)
			{
				string driveLetter = driveRow["drive"].ToString();
				long totalPages = Convert.ToInt64(driveRow["totalpages"], CultureInfo.InvariantCulture);
				long usedSpaceInMb = totalPages / 128; // 8 KB pages => 128 pages per MB
				long freeSpaceInMb = GetDiskFreeSpaceInMb(driveLetter, connection);

				if (freeSpaceInMb < usedSpaceInMb * FreeSpaceUsedSpaceMinRatio)
				{
					string description =
						Invariant($"The total free space on the disk {driveLetter} of your database server is {freeSpaceInMb:#,##0} MB.")
						+ Invariant($"\r\nThis is less than the total space used by your operational databases in this disk ({usedSpaceInMb:#,##0} MB).")
						+ "\r\n" + GetDiskDbFiles(driveLetter, dbNames, connection);
					string action = "Try freeing space on the existing disk or RAID or replace/upgrade the drive by a larger unit, RAID or storage unit.";

					DiskWarning warning = new DiskWarning(driveLetter, DiskWarning.DiskSpaceWarning, description, action);
					warningList.Add(warning);
				}
			}
		}

		string GetAllDrivesQuery(IEnumerable<string> dbNames)
		{
			StringBuilder allDbFilesQuery = new StringBuilder();

			foreach (string dbName in dbNames)
			{
				if (allDbFilesQuery.Length > 0)
				{
					allDbFilesQuery.Append(" UNION ALL ");
				}

				string dbFileQuery = Invariant($"SELECT left(ltrim(physical_name collate database_default), 1) drive, size FROM [{dbName}].sys.database_files");
				allDbFilesQuery.Append(dbFileQuery);
			}

			if (allDbFilesQuery.Length == 0)
			{
				return null;
			}

			return Invariant($@"
				SELECT drive, isnull(sum(convert(bigint, size)), 0) totalpages
				FROM ({allDbFilesQuery}) AllDrives
				GROUP BY drive");
		}

		protected long GetDiskFreeSpaceInMb(string driveLetter, DbConnection conn)
		{
			string sqlText = Invariant($@"
				CREATE TABLE #FreeSpace (Drive char(1), MB_Free int)

				INSERT #FreeSpace EXEC sys.xp_fixeddrives

				SELECT MB_Free FROM #FreeSpace
				WHERE Drive = '{driveLetter}'

				DROP TABLE #FreeSpace");
			long result = Utilities.ConvertToInt32(conn.ExecuteScalar(sqlText), -1);

			return result;
		}

		string GetDiskDbFiles(string driveLetter, IEnumerable<string> dbNames, DbConnection conn)
		{
			StringBuilder fileLines = new StringBuilder();
			string sqlText = GetDiskDbFileQuery(driveLetter, dbNames);
			DataTable diskDbFiles = Utilities.GetDataTableFromQuery(conn, sqlText);

			foreach (DataRow fileRow in diskDbFiles.Rows)
			{
				string dbName = fileRow[0].ToString();
				string filePath = fileRow[1].ToString();
				long filePages = Convert.ToInt64(fileRow[2], CultureInfo.InvariantCulture);
				long fileSizeInMb = filePages / 128; // 8 KB pages => 128 pages per MB

				string line = Invariant($"\r\nDB: {dbName} - File: {filePath} - Size: {fileSizeInMb:#,##0} MB");
				fileLines.AppendLine(line);
			}

			return fileLines.ToString();
		}

		string GetDiskDbFileQuery(string driveLetter, IEnumerable<string> dbNames)
		{
			StringBuilder dbFilesQuery = new StringBuilder();

			foreach (string dbName in dbNames)
			{
				if (dbFilesQuery.Length > 0)
				{
					dbFilesQuery.Append(" UNION ");
				}

				string dbFileQuery = Invariant(
					$"SELECT '{dbName}' dbName, physical_name collate database_default filename, size FROM [{dbName}].sys.database_files WHERE left(ltrim(physical_name), 1) = '{driveLetter}'");
				dbFilesQuery.Append(dbFileQuery);
			}

			if (dbFilesQuery.Length == 0)
			{
				return null;
			}

			return Invariant($@"
				SELECT dbName, filename, size
				FROM ({dbFilesQuery}) DbFiles
				ORDER BY dbName ASC, size DESC");
		}

		const int SqlExpressDataPagesThresholdConst = 1179648; // * 8KB page = 9.0 GB

		const int FreeSpaceUsedSpaceMinRatioConst = 1;

#if DEBUG
		protected virtual
#endif
		int FreeSpaceUsedSpaceMinRatio
		{
			get { return FreeSpaceUsedSpaceMinRatioConst; }
		}

#if DEBUG
		protected virtual
#endif
		int SqlExpressDataPagesThreshold
		{
			get { return SqlExpressDataPagesThresholdConst; }
		}

#if DEBUG
		protected virtual
#endif
		bool IsExpressSqlServerEdition(DbConnection.SqlServerEdition serverEdition)
		{
			return (serverEdition == DbConnection.SqlServerEdition.Express);
		}

		static IEnumerable<string> GetDatabases(DbConnection connection)
		{
			return (RefDbTableNameResolver.ShouldUseSharedAvailabilityGroupDatabases(connection))
				? connection.GetDatabases(DatabaseType.All & ~DatabaseType.EDW)
				: connection.GetDatabases(DatabaseType.Operational | DatabaseType.UserRepository | DatabaseType.Audit);
		}
	}
}
