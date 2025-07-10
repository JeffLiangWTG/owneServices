using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Data;
using CargoWise.DataProtection;
using CargoWise.DataProtection.Administration.SqlServer;
using Enterprise.Builder.Generator.RestoreDatabase;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.Builder.Generator
{
	abstract class Restorer
	{
		public virtual void Restore()
		{
		}

		protected void CreateAndRestore(string serverName, string databaseName, string backupPath)
		{
			CreateDatabaseIfNotExists(serverName, databaseName);
			RestoreDatabaseUsingNewConnection(databaseName, backupPath);
			CreateLoginsForDatabase(serverName, databaseName);
		}

		protected void CreateDatabaseIfNotExists(string serverName, string databaseName)
		{
			var contextManager = (SqlExecutionContextManager)Application.ServiceProvider.GetRequiredService<IProtectedDataAdministrationSqlExecutionContextManager>();

			try
			{
				var context = contextManager.GetSqlExecutionContext(serverName, databaseName);

				var result = context.ExecuteScalar("SELECT count(*) FROM sys.databases WHERE name = @DatabaseName", System.Data.CommandType.Text, cmd =>
				{
					cmd.AddParameter("@DatabaseName", System.Data.DbType.String, databaseName);
				});

				var dbCount = Convert.ToInt32(result);

				if (dbCount == 0)
				{
					var sql = $"CREATE DATABASE [{databaseName}]";
					context.ExecuteNonQuery(sql, System.Data.CommandType.Text);
				}
			}
			finally
			{
				contextManager.Close();
			}
		}

		void CreateLoginsForDatabase(string serverName, string databaseName)
		{
			var serviceProvider = Application.ServiceProvider;
			var initService = serviceProvider.GetRequiredService<ISqlServerSecurityInitializationService>();
			var stateService = serviceProvider.GetRequiredService<IProtectedDataStateService>();
			var pdsFactory = serviceProvider.GetRequiredService<IProtectedDataServiceFactory>();
			var pds = pdsFactory.CreateSystemService(serverName, databaseName);

			if (!initService.IsServerInitialized(serverName))
			{
				initService.InitializeServer(serverName, databaseName);
			}

			stateService.ResestProtectedDataStatesToDefault(pds, serverName, databaseName, "Credentials Reset To Default By Generator");
			using (var adminConnection = Db.NewAdminConnection(serverName, databaseName))
			{
				((IDbLoginRepair)adminConnection).EnsureDbLoginsCorrectlyMappedToAllDatabases(msg => { });
			}
		}

		protected void RestoreDatabaseUsingNewConnection(string databaseName, string dbBackupPath)
		{
			using (var restoreConnection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				DoKillConnectionsAndRestoreDatabase(restoreConnection, databaseName, dbBackupPath);
			}

			using (var restoreConnection = Db.NewAdminConnection(databaseName))
			{
				((IDbLoginRepair)restoreConnection).EnsureDbLoginsHaveRightsToCurrentDatabase();
			}
		}

		/// <summary>
		/// Call methods to kill DB connections and to perform the DB restore.
		/// </summary>
		/// <returns>
		///   False if the user do not confirm the restore.
		/// </returns>
		protected bool DoKillConnectionsAndRestoreDatabase(AdminConnection conn, string databaseName, string dbBackupPath)
		{
			var restoreCompleted = false;

			if (DbConnectionKiller.KillOtherConnections(conn, databaseName))
			{
				DoRestoreDatabase(conn, databaseName, dbBackupPath);
				restoreCompleted = true;
			}

			return restoreCompleted;
		}

		protected void DoRestoreDatabase(DbConnection conn, string databaseName, string databaseBackupPath)
		{
			try
			{
				var sqlText = $@"RESTORE DATABASE {databaseName} FROM DISK='{databaseBackupPath}'
WITH
	REPLACE {MoveStatement(conn, databaseName, databaseBackupPath)}
, KEEP_CDC";
				conn.ExecuteNonQuery(sqlText, 1200);
			}
			catch (SqlException e)
			{
				var stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("Error restoring the database.");
				stringBuilder.Append(e.Message);

				if (e.Number == 3201)
				{
					var regex = new Regex(@".+Operating system error (?<systemErrorNumber>\d+)", RegexOptions.IgnoreCase);
					var match = regex.Match(e.Message);
					var detailedErrorMessage = string.Empty;

					if (match.Success && int.TryParse(match.Groups["systemErrorNumber"].Value, out var systemErrorNumber))
					{
						switch (systemErrorNumber)
						{
							case 5:
								detailedErrorMessage = "Please check if the backup device is accessible from your machine/network. If the issue persists, please contact the system administrator.";
								break;
						}

						stringBuilder.AppendLine().AppendLine(detailedErrorMessage);
					}
				}

				throw new Exception(stringBuilder.ToString(), e);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		protected string MoveStatement(DbConnection conn, string databaseName, string databaseBackupPath)
		{
			var logicalNames = new List<string>();
			var backupPaths = new List<string>();
			var destinationPaths = new List<string>();
			var sqlText = string.Empty;

			using (var result = conn.Command($"SELECT physical_name FROM {databaseName}.sys.database_files;").ExecuteReader())
			{
				while (result.Read())
				{
					destinationPaths.Add((string)result["physical_name"]);
				}
			}

			using (var result = conn.Command($"RESTORE FILELISTONLY FROM DISK='{databaseBackupPath}';").ExecuteReader())
			{
				while (result.Read())
				{
					logicalNames.Add((string)result["LogicalName"]);
					backupPaths.Add((string)result["PhysicalName"]);
				}
			}

			for (int i = 0; i < logicalNames.Count; i++)
			{
				var backupPath = backupPaths[i];
				var newFilename = AppendDbNameToFileName(databaseName, Path.GetFileNameWithoutExtension(backupPath));
				var newPath = Path.Combine(Path.GetDirectoryName(destinationPaths[0]), newFilename) + Path.GetExtension(backupPath);

				if (!destinationPaths.Contains(newPath) || !Directory.Exists(Path.GetDirectoryName(backupPath)))
				{
					sqlText += $", MOVE '{logicalNames[i]}' TO '{newPath}'";
				}
			}

			return sqlText;
		}

		protected string AppendDbNameToFileName(string databaseName, string fileName)
		{
			var end = string.Empty;
			var parts = fileName.Split(new[] { '_' }, 2);

			if (parts.Length > 1)
			{
				end = "_" + parts[1];
			}

			return databaseName + end;
		}
	}
}
