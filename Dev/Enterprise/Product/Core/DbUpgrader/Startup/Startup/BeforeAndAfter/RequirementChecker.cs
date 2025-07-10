using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Shared;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DbUpgrader.Startup
{
	/// <summary>
	/// Summary description for RequirementChecker.
	/// </summary>
	public abstract class RequirementChecker
	{
		protected RequirementChecker(IEnumerable<string> dbsBeingUpgraded, IEnumerable<string> allDatabases)
		{
			this.dbsBeingUpgraded = dbsBeingUpgraded;
			this.allDatabases = allDatabases;
		}

		#region Mandatory Checks

		#region SQL Server Version

		public void CheckServerVersion(DbConnection connection, IUpgradeManager upgradeManager)
		{
			CheckSqlServerEdition(connection);
			CheckSqlServerGeneration(connection);
			CheckServicePackVersions(connection, upgradeManager);
		}

		public static bool CheckServerVersion(SqlServerVersionNumber serverVersionNumber, string serverFullVersionText, out string failureMessage)
		{
			CheckSqlServerGeneration(serverVersionNumber);
			return CheckServicePackVersions(serverVersionNumber, serverFullVersionText, out failureMessage);
		}

		protected void CheckSqlServerEdition(DbConnection connection)
		{
			var sqlServerEdition = GetSqlServerEdition(connection);
			if (sqlServerEdition == DbConnection.SqlServerEdition.Express
				|| sqlServerEdition == DbConnection.SqlServerEdition.Other
				|| (sqlServerEdition != DbConnection.SqlServerEdition.EnterpriseDeveloper
					&& Enterprise.Environment.Env.Registry.OnlySupportsSqlServerEnterpriseEdition)
			)
			{
				var message = string.Format(
					 "{0}\r\n\r\nThe upgrade cannot proceed and will be rolled back.\r\n\r\n",
					 SqlCutOverHelper.UpgradeToSqlEnterpriseEditionAction);

				throw new ServerRequirementsNotMetException(message);
			}
		}

		protected void CheckSqlServerGeneration(DbConnection connection)
		{
			CheckSqlServerGeneration(connection.ServerVersionNumber);
		}

		static void CheckSqlServerGeneration(SqlServerVersionNumber sqlServerVersion)
		{
			if (!sqlServerVersion.IsMinimumRequiredVersionOrAbove)
			{
				string message = string.Format(
					 "{0} The upgrade cannot proceed and will be rolled back.\r\n\r\n{1}",
					 SqlCutOverHelper.VersionNoLongerSupportsOlderSqlVersionsDisplayText,
					 SqlCutOverHelper.UpgradeToSupportedSqlVersionAction);

				throw new ServerRequirementsNotMetException(message);
			}
		}

		internal virtual DbConnection.SqlServerEdition GetSqlServerEdition(DbConnection connection)
		{
			return connection.ServerEdition;
		}

		#region Service Pack Versions

		protected void CheckServicePackVersions(DbConnection connection, IUpgradeTaskWorkflowLogger logger)
		{
			CheckDatabaseVersionUpToDate(connection, logger);
			CheckWindowsServicePackUpToDate(connection.ServerFullVersionText);
		}

		static bool CheckServicePackVersions(SqlServerVersionNumber serverVersionNumber, string serverFullVersionText, out string failureMessage)
		{
			var result = serverVersionNumber.IsSupported(out failureMessage);
			CheckWindowsServicePackUpToDate(serverFullVersionText);
			return result;
		}

		protected void CheckDatabaseVersionUpToDate(DbConnection connection, IUpgradeTaskWorkflowLogger logger)
		{
			if (!connection.ServerVersionNumber.IsSupported(out var failureMessage))
			{
				throw new ServerRequirementsNotMetException(failureMessage);
			}

			if (!string.IsNullOrEmpty(failureMessage))
			{
				logger.ShowInfoMessage(failureMessage);
			}
		}

		protected static void CheckWindowsServicePackUpToDate(string fullVersion)
		{
			string windowsVersion;
			string windowsServicePack;
			int windowsServicePackNumber;

			try
			{
				windowsVersion = fullVersion.Substring(fullVersion.IndexOf("Windows"));
				windowsVersion = windowsVersion.Substring(0, windowsVersion.IndexOf("(")).Trim();
				var windowsServicePackIndex = fullVersion.IndexOf("Service Pack");
				windowsServicePack = windowsServicePackIndex >= 0
					? fullVersion.Substring(windowsServicePackIndex + 13, 1).Trim()
					: "0";
				windowsServicePackNumber = Utilities.ConvertToInt32(windowsServicePack);
			}
			catch (Exception e)
			{
				throw new Exception("Unable to check the Windows Service Pack version." + System.Environment.NewLine + e.Message);
			}

			const string spMessageMask = "At least Service Pack {0} is needed for Windows {1}. Current one is [{2}]\r\nPlease install the latest Windows Service Pack on the SQL Server machine.";

			// Windows NT4.0
			if (windowsVersion.IndexOf("4.0") > 0 && windowsServicePackNumber < 6)
			{
				throw new ServerRequirementsNotMetException(string.Format(spMessageMask, "6", "NT4.0", windowsServicePack));
			}

			// Windows 2000
			if (windowsVersion.IndexOf("5.0") > 0 && windowsServicePackNumber < 3)
			{
				throw new ServerRequirementsNotMetException(string.Format(spMessageMask, "3", "2000", windowsServicePack));
			}

			// Windows XP
			if (windowsVersion.IndexOf("5.1") > 0 && windowsServicePackNumber < 1)
			{
				throw new ServerRequirementsNotMetException(string.Format(spMessageMask, "1", "XP", windowsServicePack));
			}
		}

		#endregion

		#endregion

		#region Collation

		public void CheckCollation(DbConnection connection)
		{
			CheckServerCollationIsCaseInsensitive(connection);
			FixDbCollations(connection);
			CheckTempDbAndMaindDbCollationsAreDifferent_DebugOnly(connection);
		}

		protected void CheckServerCollationIsCaseInsensitive(DbConnection connection)
		{
			string sqlText = "SELECT serverproperty('collation')";
			string collation = connection.ExecuteScalar(sqlText).ToString().ToUpper(CultureInfo.InvariantCulture);
			if (collation.IndexOf("_CI", StringComparison.OrdinalIgnoreCase) < 0 && (collation.IndexOf("_CS", StringComparison.OrdinalIgnoreCase) >= 0 || collation.IndexOf("_BIN", StringComparison.OrdinalIgnoreCase) >= 0))
			{
				string message = string.Format(
					CultureInfo.InvariantCulture,
					"Your database server was installed as case-sensitive (Collation: {0}). " +
					"{1} requires a case-insensitive installation. Please contact your System Administrator.",
					collation,
					Core.Constants.ProductName);
				throw new ServerRequirementsNotMetException(message);
			}
		}

		[System.Diagnostics.Conditional("DEBUG")]
		protected void CheckTempDbAndMaindDbCollationsAreDifferent_DebugOnly(DbConnection connection)
		{
			if (!Globals.IsTest)
			{
				string sqlText = "SELECT DatabasePropertyEx('tempdb','Collation')";
				string collation = connection.ExecuteScalar(sqlText).ToString().ToUpper(CultureInfo.InvariantCulture);

				if (collation == Db.DatabaseCollation.ToUpper(CultureInfo.InvariantCulture))
				{
					string message = string.Format(
						CultureInfo.InvariantCulture,
						"Your tempdb database has the same collation as the main database. " +
						"This collation has to be different on developer machines as we can test collation conflict issues." +
						System.Environment.NewLine + "Please refer to the Wiki page below in order to fix this:" +
						System.Environment.NewLine + "https://wisetechglobal.sharepoint.com/Development/Development%20Team%20Workspace/Shared%20Documents/All%20Teams/Setup%20DB%20Collation.docx?d=w2a841f3d458143fe937f50b331b3acc8");
					throw new ServerRequirementsNotMetException(message);
				}
			}
		}

		protected void FixDbCollations(DbConnection connection)
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				DECLARE @SqlCmd varchar(max) = '';
				SELECT @SqlCmd = @SqlCmd + 'ALTER DATABASE [' + name + '] COLLATE {0};'
					FROM sys.databases
					WHERE collation_name != '{0}'
					AND name in ('{1}');
				IF (@SqlCmd != '') EXEC (@SqlCmd);
				",
				Db.DatabaseCollation,
				string.Join("','", allDatabases));

			connection.ExecuteNonQuery(sqlText);
		}

		#endregion

		#region Implementation

		protected readonly IEnumerable<string> dbsBeingUpgraded;
		protected readonly IEnumerable<string> allDatabases;

		#endregion

		#endregion
	}
}
