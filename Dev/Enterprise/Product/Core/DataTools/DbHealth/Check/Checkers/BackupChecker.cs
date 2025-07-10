using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DbHealth.Check
{
	using static FormattableString;

	class BackupChecker : IChecker
	{
		void IChecker.Check(DbConnection connection, DbHealthWarningList warningList, ILogger logger)
		{
			if (Env.Instance.IsProductionSystem)
			{
				var dbNames = (Env.Registry.BackupReferenceDatabases)
					? connection.GetDatabases(DatabaseType.AllExclusive & ~DatabaseType.BI)
					: connection.GetOperationalRepositoryAndFullRecoveryDbs();
				DoCheckBackupIsBeingPerformedPeriodically(dbNames, connection, warningList);
			}
		}

		string IChecker.Description
		{
			get { return "Check database is regularly backed up"; }
		}

		/// <summary>
		/// Checks if the operational databases (main + SDXXX DBs) are being backed up daily.
		/// </summary>
		/// <returns>Warning List</returns>
		protected virtual void DoCheckBackupIsBeingPerformedPeriodically(IEnumerable<string> dbNames, DbConnection connection, DbHealthWarningList warningList)
		{
			var skipBackupOfNonMainDatabasesWithoutActivity = SkipBackupOfNonMainDatabasesWithoutActivity;
			var backupPath = BackupDirectoryPath;

			var listOfBackupFiles = GetListOfBackupFiles(connection, backupPath);

			foreach (string dbName in dbNames)
			{
				var backupName = $"{dbName}.bak";
				var currentBackupName = $"{dbName}.bak.CURRENT";

				var backupFileNameToUse = listOfBackupFiles.FirstOrDefault(x => string.Compare(x, backupName, StringComparison.OrdinalIgnoreCase) == 0);

				if (string.IsNullOrEmpty(backupFileNameToUse))
				{
					backupFileNameToUse = listOfBackupFiles.FirstOrDefault(x => string.Compare(x, currentBackupName, StringComparison.OrdinalIgnoreCase) == 0);
				}

				if (!string.IsNullOrEmpty(backupFileNameToUse))
				{
					if (!skipBackupOfNonMainDatabasesWithoutActivity || Db.ShouldAlwaysBackup(dbName))
					{
						var fullBackupPath = Path.Combine(backupPath, backupFileNameToUse);
						try
						{
							var backupInfoTable = GetBackupInfo(connection, fullBackupPath);
							DataRow backupInfoHeader = null;
							if (backupInfoTable.Rows.Count > 0)
							{
								backupInfoHeader = backupInfoTable.Rows[0];
							}

							AddWarningIfBackupIsNotRecent(warningList, backupInfoHeader, dbName, fullBackupPath);
						}
						catch (SqlException e)
						{
							if (IsFileUsedByAnotherProcessException(e, backupPath, backupName))
							{
								// Try to check dbName.CURRENT backup
								warningList.Add(GetBackupFileIsUsedByAnotherProcessWarning(dbName, backupPath, backupFileNameToUse));
								var fullCurrentBackupPath = Path.Combine(backupPath, currentBackupName);
								try
								{
									var backupInfoTable = GetBackupInfo(connection, fullCurrentBackupPath);
									DataRow backupInfoHeader = null;
									if (backupInfoTable.Rows.Count > 0)
									{
										backupInfoHeader = backupInfoTable.Rows[0];
									}

									AddWarningIfBackupIsNotRecent(warningList, backupInfoHeader, dbName, fullCurrentBackupPath);
								}
								catch (SqlException sqlException2) when (IsFileNotExistsException(sqlException2, backupPath, currentBackupName))
								{
									warningList.Add(GetBackupFileIsAbsentWarning(dbName, fullCurrentBackupPath));
								}
							}
							else
							{
								warningList.Add(GetBackupCheckerFailureWarning(dbName, e.Message, fullBackupPath));
							}
						}
					}
				}
				else if (IsDbOlderThanTwoDays(connection, dbName))
				{
					string description = Invariant($"There is NO backup for database [{dbName}].");
					warningList.Add(GetBackupCheckerStandardWarning(dbName, description));
				}
			}
		}

		static bool IsFileUsedByAnotherProcessException(SqlException e, string backupPath, string backupName)
		{
			return IsExpectedException(e, backupPath, backupName, 32);
		}

		static bool IsFileNotExistsException(SqlException e, string backupPath, string backupName)
		{
			return IsExpectedException(e, backupPath, backupName, 2);
		}

		static bool IsExpectedException(SqlException e, string backupPath, string backupName, int errorCode)
		{
			var regexTml = Invariant($@"\D* '{Regex.Escape(string.Format(CultureInfo.InvariantCulture, "{0}\\{1}", backupPath, backupName))}'\. \D*{errorCode}\D*RESTORE HEADERONLY\D*");
			return e.Number == 3201 && new Regex(regexTml, RegexOptions.IgnoreCase).IsMatch(e.Message);
		}

		protected virtual bool SkipBackupOfNonMainDatabasesWithoutActivity => Env.Registry.SkipBackupOfNonMainDatabasesWithoutActivity;

		protected virtual string BackupDirectoryPath => Env.Registry.BackupDirectoryPath;

		protected virtual DataTable GetBackupInfo(DbConnection connection, string fullBackupPath)
		{
			var sqlText = string.Format(Culture.Invariant, @"restore headeronly from disk = N'{0}'", fullBackupPath);
			return Utilities.GetDataTableFromQuery(connection, sqlText);
		}

		internal virtual List<string> GetListOfBackupFiles(DbConnection connection, string backupPath)
		{
			var sqlText = string.Format(Culture.Invariant, @"
DECLARE @DirInfo TABLE (FileName varchar(max), Depth int, IsFile bit);
INSERT @DirInfo 
EXEC xp_dirtree '{0}', 1, 1
Select FileName
From @DirInfo
Where FileName like '{1}.bak'
   OR FileName like '{1}[_]SD[0-9][0-9][0-9].bak'
   OR FileName like '{1}[_]UserRepository.bak'
   OR FileName like '{1}.bak.CURRENT'
   OR FileName like '{1}[_]SD[0-9][0-9][0-9].bak.CURRENT'
   OR FileName like '{1}[_]UserRepository.bak.CURRENT'"
, backupPath
, Db.DatabaseName
);
			if (Env.Registry.BackupReferenceDatabases)
			{
				sqlText = string.Format(Culture.Invariant, @"{0}
   OR FileName like '{1}[_]RefDb%.bak'
   OR FileName like '{1}[_]RefDb%.bak.CURRENT'", sqlText, Db.DatabaseName);
			}
			var list = new List<string>();
			var dt = Utilities.GetDataTableFromQuery(connection, sqlText);
			for (var i = 0; i < dt.Rows.Count; i++)
			{
				list.Add(dt.Rows[i][0].ToString());
			}

			return list;
		}

#if DEBUG
		public
#endif
		void AddWarningIfBackupIsNotRecent(DbHealthWarningList warningList, DataRow backupRow, string dbName, string backupFilePath)
		{
			if (backupRow == null || backupRow["BackupFinishDate"] == DBNull.Value)
			{
				var description =
Invariant($@"Database [{dbName}] has a corrupted backup.
Last backup was taken to path [{backupFilePath}].");
				warningList.Add(GetBackupCheckerStandardWarning(dbName, description));
				ErrorReporter.ReportOnce(dbName, description);
			}
			else
			{
				var backupTime = Convert.ToDateTime(backupRow["BackupFinishDate"], CultureInfo.InvariantCulture);
				var hoursOld = GetBackupAgeInHours(backupTime);
				var formattedBackupTime = Env.Time.FormatDateTime(backupTime);
				var totalHoursOldThreshold = GetBackupThresholdConsideringWeekends(backupTime);

				if (hoursOld > totalHoursOldThreshold)
				{
					var descriptionRaw =
Invariant($@"Database [{dbName}] is not being backed up periodically.
Last backup was taken on {formattedBackupTime} to path [{backupFilePath}].");
					var description = string.Format(CultureInfo.InvariantCulture, descriptionRaw, dbName, formattedBackupTime, backupFilePath);
					warningList.Add(GetBackupCheckerStandardWarning(dbName, description));
				}
			}
		}

		TimeSpan GetBackupAgeInHours(DateTime backupTime)
		{
			var serverTimeNow = Db.Connection.ExecuteScalar<DateTime>("select getdate()");
			return serverTimeNow - backupTime;
		}

		DbHealthWarning GetBackupCheckerFailureWarning(string dbName, string errorMessage, string fullBackupPath)
		{
			string description = Invariant($"{Constants.ProductName} failed to check backup file for database [{dbName}], file location: '{fullBackupPath}'.\r\n\r\nERROR: {errorMessage}");

			string action = "Contact your system administrator.";
			DatabaseWarning warning = new DatabaseWarning(dbName, DatabaseWarning.BackupWarning, description, action);
			return warning;
		}

		DbHealthWarning GetBackupCheckerStandardWarning(string dbName, string description)
		{
			string action =
				Constants.ProductName + " databases should be backed up regularly (we recommend daily), usually to a separate physical disk "
									  + "and the backup files then transferred to tape before they are overwritten by the next backup (this is your IT manager responsibility)."
									  + "\r\n\r\nBackup tapes should have a planned cycle of reuse and replacement. "
									  + "Backup tapes MUST be held offsite to prevent loss in case of theft, fire or natural disasters. "
									  + "\r\n\r\nOften backup cycles use many tapes that allow various aging of backup data and "
									  + "a possible archive of monthly or quarterly information for retention purposes. "
									  + "\r\n\r\nWe recommend a thorough and comprehensive backup strategy be developed and checked regularly.";
			DatabaseWarning warning = new DatabaseWarning(dbName, DatabaseWarning.BackupWarning, description, action);
			return warning;
		}

		static DatabaseWarning GetBackupFileIsUsedByAnotherProcessWarning(string dbName, string backupPath, string backupName)
		{
			return new DatabaseWarning(dbName, DatabaseWarning.BackupWarning,
				string.Format(CultureInfo.InvariantCulture, "Backup file for the database [{0}] is used by another process. Checking .CURRENT backup instead. File location: {1}\\{2}", dbName, backupPath, backupName),
				string.Empty);
		}

		static DatabaseWarning GetBackupFileIsAbsentWarning(string dbName, string fullBackupPath)
		{
			return new DatabaseWarning(
				dbName,
				DatabaseWarning.BackupWarning,
				Invariant($"{Constants.ProductName} failed to check backup file for database [{dbName}], file is not found: '{fullBackupPath}'."),
				"Contact your system administrator.");
		}

		protected virtual bool IsDbOlderThanTwoDays(DbConnection connection, string dbName)
		{
			string sqlText = Invariant($@"
IF exists(SELECT null FROM sys.databases WHERE name = '{dbName}' AND datediff(day, create_date, getdate()) > 2) 
	SELECT 1 
ELSE 
	SELECT 0");
			return (Convert.ToInt32(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture) == 1);
		}

		protected static readonly TimeSpan backupThresholdForSelfHosting = TimeSpan.FromHours(30);
		protected static readonly TimeSpan backupThresholdForHosting = TimeSpan.FromDays(30);
		protected static readonly TimeSpan backupWeekendDaysGracePeriod = TimeSpan.FromDays(1);

#if DEBUG
		protected virtual
#endif
		TimeSpan GetBackupThresholdConsideringWeekends(DateTime lastBackupTime)
		{
			var result = EnvProxy.IsHostedWithCargowise ? backupThresholdForHosting : backupThresholdForSelfHosting;

			if (lastBackupTime.DayOfWeek == DayOfWeek.Friday
				|| (lastBackupTime.DayOfWeek == DayOfWeek.Saturday && lastBackupTime.Hour < 12))
			{
				result = result.Add(backupWeekendDaysGracePeriod.Add(TimeSpan.FromDays(1)));
			}
			else if ((lastBackupTime.DayOfWeek == DayOfWeek.Saturday && lastBackupTime.Hour >= 12)
				|| (lastBackupTime.DayOfWeek == DayOfWeek.Sunday && lastBackupTime.Hour < 12))
			{
				result = result.Add(backupWeekendDaysGracePeriod);
			}

			return result;
		}
	}
}
