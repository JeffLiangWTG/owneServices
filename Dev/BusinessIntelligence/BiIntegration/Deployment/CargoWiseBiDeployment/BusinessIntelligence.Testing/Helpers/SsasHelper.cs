using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using CargoWise.Bi.Common;
using CargoWise.Bi.Deployment.AnalysisServices;
using CargoWise.BuildTools;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.Resource.Version;
using Microsoft.AnalysisServices.AdomdClient;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Bi.BusinessIntelligence.Testing
{
	public static class SsasHelper
	{
		#region Restore Model

		public static void RestoreModelBackup(SsasServer connection, string cubeName, bool checkUserHasReaderRole = false, string userName = "")
		{
			RestoreModelBackup(connection, cubeName);
		}

		public static DisposableCube RestoreModelBackupIfRequiredForSingleTest(SsasServer connection, string cubeName, bool checkUserHasReaderRole = false, string userName = "")
		{
			return new DisposableCube(connection, cubeName);
		}

		public static void RestoreModelBackup(SsasServer connection, string cubeName)
		{
			var cubeSuffix = cubeName.Replace($"{Db.DatabaseName}_", "");
			var backupPath = Path.Combine(BuildConstants.LocalEnterprisePath, CubeBackupFilePath, cubeSuffix + ".abf");
			connection.RestoreModel(backupPath, cubeName, true);
			connection.SetDatabaseVersion(cubeName, MainDbSchemaVersion);
		}

		public static string MainDbSchemaVersion
		{
			get
			{
				return mainDbSchemaVersion ?? (mainDbSchemaVersion = SchemaVersion.Application.ToString());
			}
		}
		[ThreadSafe]
		static string mainDbSchemaVersion;

		const string CubeBackupFilePath = @"BusinessIntelligence\BiIntegration\Deployment\CargoWiseBiDeployment\BusinessIntelligence.Testing\ModelBackup";

		#endregion

		#region Drop Model
		public static void DropModel(SsasServer connection, string cubeName)
		{
			connection.DropModel(cubeName);
		}

		#endregion

		public static DataTable ExecuteDaxQuery(string analysisServerName, string cubeName, string executeCommand)
		{
			var result = new DataTable();
			try
			{
				var connectionString = string.Format("Provider=MSOLAP;Data Source={0};Catalog={1}", analysisServerName, cubeName);
				using (var connection = new AdomdConnection())
				{
					connection.ConnectionString = connectionString;
					connection.Open();

					using (var command = new AdomdCommand(executeCommand, connection))
					using (var reader = command.ExecuteReader())
					{
						var columns = new List<string>();

						for (int i = 0; i < reader.FieldCount; i++)
						{
							var columnName = reader.GetName(i);
							var dataType = reader.GetFieldType(i);

							result.Columns.Add(columnName, dataType);
							columns.Add(columnName);
						}

						while (reader.Read())
						{
							DataRow row = result.NewRow();
							foreach (var column in columns)
							{
								if (reader[column] == null)
								{
									row[column] = DBNull.Value;
								}
								else
								{
									row[column] = reader[column];
								}
							}
							result.Rows.Add(row);
						}
					}
				}
				result = RemoveTableNameFromColumns(result);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{ }
			return result;
		}

		public static DataTable RemoveTableNameFromColumns(DataTable dataTable)
		{
			var regex = new Regex(@"^(?<tableName>.+?)(\[(?<columnName>.+?)\])", RegexOptions.IgnoreCase);
			var m = regex.Match(dataTable.Columns[0].ColumnName);
			var tableName = m.Groups["tableName"].ToString();

			if (!string.IsNullOrEmpty(tableName))
			{
				dataTable.TableName = tableName;
				foreach (DataColumn column in dataTable.Columns)
				{
					var match = regex.Match(column.ColumnName);
					column.ColumnName = match.Groups["columnName"].ToString();
				}
			}

			return dataTable;
		}

		public static string AnalysisServerName
		{
			get
			{
				var analysisServerName = BiServers.LoadAnalysisServerUsingCacheIfPossible(Db.Connection);
				if (string.IsNullOrWhiteSpace(analysisServerName))
				{
					analysisServerName = @"localhost";
				}
				return analysisServerName;
			}
		}
	}
}
