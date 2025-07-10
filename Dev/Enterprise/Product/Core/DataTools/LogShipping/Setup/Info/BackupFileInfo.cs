using System.Collections.Generic;
using System.IO;
using CargoWise.Common;

namespace Enterprise.LogShipping.Setup
{
	public class BackupFileInfo
	{
		BackupFileInfo() { }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public static BackupFileInfo[] GetBackupFileInfoArray(System.Data.Common.DbConnection connection, DatabaseInfo matchInfo)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNull(matchInfo, nameof(matchInfo));

			List<BackupFileInfo> result = new List<BackupFileInfo>();

			string sqlText = string.Format("RESTORE FILELISTONLY FROM DISK = '{0}'", matchInfo.BackupFullFileName);

			string logFilePath =
				(string.IsNullOrEmpty(matchInfo.SetupInfo.RestoreLogDirectoryOverride)) ?
				GetPhysicalPath(connection, matchInfo, LogFileType) :
				matchInfo.SetupInfo.RestoreLogDirectoryOverride;

			string dataFilePath =
				(string.IsNullOrEmpty(matchInfo.SetupInfo.RestoreDataDirectoryOverride)) ?
				GetPhysicalPath(connection, matchInfo, DataFileType) :
				matchInfo.SetupInfo.RestoreDataDirectoryOverride;

			int logFileCount = 0;
			int dataFileCount = 0;

			using (var cmd = DbManager.NewSqlCommand(sqlText, connection))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					BackupFileInfo bkpFileInfo = new BackupFileInfo();

					if (reader["LogicalName"] != null)
					{
						bkpFileInfo.LogicalName = reader["LogicalName"].ToString().Trim();
					}

					if (reader["Type"] != null)
					{
						bkpFileInfo.Type = reader["Type"].ToString().Trim();
					}

					switch (bkpFileInfo.Type)
					{
						case LogFileType:
							string fileNum = logFileCount > 0 ? logFileCount.ToString() : string.Empty;
							bkpFileInfo.PhysicalName = Path.Combine(logFilePath, string.Format("{0}_Log{1}.ldf", matchInfo.SecondaryDatabaseName, fileNum));
							logFileCount++;
							break;

						case DataFileType:
							fileNum = dataFileCount > 0 ? dataFileCount.ToString() : string.Empty;
							bkpFileInfo.PhysicalName = Path.Combine(dataFilePath, string.Format("{0}_Data{1}.mdf", matchInfo.SecondaryDatabaseName, fileNum));
							dataFileCount++;
							break;
					}

					result.Add(bkpFileInfo);
				}
			}

			return result.ToArray();
		}

		static string GetPhysicalPath(System.Data.Common.DbConnection connection, DatabaseInfo info, string fileType)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNull(info, nameof(info));

			return info.SecondaryDatabaseExists() ? Path.GetDirectoryName(GetPhysicalNameFromDatabase(connection, info.SecondaryDatabaseName, fileType)) : Path.GetDirectoryName(GetPhysicalNameFromDatabase(connection, DbManager.MasterDbName, fileType));
		}

		static string GetPhysicalNameFromDatabase(System.Data.Common.DbConnection connection, string databaseName, string fileType)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNull(databaseName, nameof(databaseName));

			string result = string.Empty;
			string sqlText = string.Format(@"
					SELECT TOP 1 physical_name 
					FROM sys.master_files
					WHERE database_id = db_id('{0}')
					AND type = {1}
					ORDER BY file_id", databaseName, fileType == DataFileType ? 0 : 1);

			using (var cmd = DbManager.NewSqlCommand(sqlText, connection))
			{
				var execute = cmd.ExecuteScalar();

				if (execute != null)
				{
					result = (string)execute;
				}
			}

			return result;
		}

		public string LogicalName { get; private set; }
		public string PhysicalName { get; private set; }
		public string Type { get; private set; }

		public const string DataFileType = "D";
		public const string LogFileType = "L";
	}
}
