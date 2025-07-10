using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Data;
using CargoWise.Database.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.DbUpgrader.Shared
{
	#region UpgUtils class

	/// <summary>
	/// Utilities library class for DbUpgrader project
	/// </summary>
	public sealed class UpgUtils
	{
		UpgUtils() { }
		public static readonly UpgUtils Instance = new UpgUtils();

		public const string UpgraderPrefix = "DBUPG_";

		IDbUpgraderUserInteraction UserInteraction => GlobalServiceProvider.Instance.GetRequiredService<IDbUpgraderUserInteraction>();

		public static string GetTemplateDbName(string databaseName = null)
		{
			if (string.IsNullOrEmpty(databaseName))
			{
				databaseName = Db.Connection.CurrentDatabase;
			}

			return UpgraderPrefix + "NewTemplateDB_" + databaseName;
		}

		#region GUI Methods

		public bool ShowConfirmDisconnectUsersMessageBox(KeyValuePair<string, string>[] loggedInUsers)
		{
			return UserInteraction.PromptConfirmDisconnectUsers(loggedInUsers);
		}

		public bool ShowConfirmationMessageBox(string title, string message, string[] detailLines)
		{
			return UserInteraction.PromptUserConfirmation(title, message, detailLines);
		}

		public bool ShowRetryConfirmationMessageBox(string title, string message)
		{
			return UserInteraction.PromptRetryAction(title, message);
		}

		#endregion

		public static void CleanupAuxDatabases(AdminConnection conn, string mainDbName)
		{
			try
			{
				string sqlText = String.Format(CultureInfo.InvariantCulture, @"
					SELECT name
					FROM sys.databases
					WHERE name like '{0}%[_]{1}'
					OR name like '{0}%[_]{1}[_]%'",
					UpgraderPrefix,
					DataUtils.ReplaceSqlLikeWildcard(mainDbName));

				var dbs = DataUtils.GetListOfValuesFromQuery(conn, sqlText);

				foreach (string dbName in dbs)
				{
					new DbRemover(dbName).Drop(conn);
				}
			}
			catch (SqlException)
			{
				// Cleanup only. Ignore errors.
			}
		}
	}

#endregion
}
