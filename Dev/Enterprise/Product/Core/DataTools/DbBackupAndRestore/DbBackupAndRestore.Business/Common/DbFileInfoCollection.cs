using System;
using System.Collections;
using System.IO;

using CargoWise.Data;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	public class DbFileInfoCollection : CollectionBase
	{
		public static DbFileInfoCollection GetDbFileInfoCollectionFromBackup(string dbServer, string backupFilePath, string dbType = DbFileInfo.DbTypeMain, string restoreDbName = "")
		{
			string sqlText = String.Format("RESTORE FILELISTONLY FROM DISK = '{0}'", backupFilePath);
			DbFileInfoCollection result = GetDbFileInfoCollection(dbServer, sqlText, backupFilePath, dbType, restoreDbName);

			return result;
		}

		public static DbFileInfoCollection GetDbFileInfoCollectionFromDb(string dbServer, string dbName, string dbType = DbFileInfo.DbTypeMain, string restoreDbName = "")
		{
			string sqlText = String.Format(@"
				SELECT
					s.name AS [LogicalName],
					s.physical_name AS [PhysicalName],
					CASE s.type WHEN 1 THEN 'L' ELSE 'D' END AS [Type]
				FROM sys.master_files AS s
				WHERE s.database_id = db_id('{0}')
				", dbName);
			DbFileInfoCollection result = GetDbFileInfoCollection(dbServer, sqlText, "", dbType, restoreDbName);

			return result;
		}

		static DbFileInfoCollection GetDbFileInfoCollection(string dbServer, string sqlText, string filePath = "", string dbType = DbFileInfo.DbTypeMain, string restoreDbName = "")
		{
			DbFileInfoCollection result = new DbFileInfoCollection();

			using (Db.DisableSchemaVersionCheck())
			using (DbConnection conn = Db.NewAdminConnection(dbServer, Db.SqlMasterDb))
			using (DbCommand cmd = conn.Command(sqlText))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					string logicalName = reader["LogicalName"].ToString().Trim();
					string fileType = reader["Type"].ToString().Trim();

					string physicalName = reader["PhysicalName"].ToString().Trim();
					FileInfo backupFile = new FileInfo(physicalName);
					string folderPath = backupFile.DirectoryName;

					DbFileInfo dbFile = new DbFileInfo(logicalName, folderPath, fileType, filePath, dbType, restoreDbName);
					result.Add(dbFile);
				}
			}

			return result;
		}

		public DbFileInfo this[int index]
		{
			get { return (DbFileInfo)List[index]; }
		}

		public int Add(DbFileInfo element)
		{
			if (List.Contains(element))
			{
				throw new InvalidOperationException("Attempt to add an existing element to the collection.");
			}

			if (element.FileType == DbFileInfo.FileTypeData)
			{
				((ISetFileSequence)element).SetFileSequencePerType(++DataFileCount);
			}
			else
			{
				((ISetFileSequence)element).SetFileSequencePerType(++LogFileCount);
			}

			return List.Add(element);
		}

		public void SetDbFileTypeVisible(string dbType, bool visible)
		{
			foreach (DbFileInfo dbFile in List)
			{
				if (dbFile.DbType == dbType)
				{
					dbFile.Visible = visible;
				}
			}
		}

		public void SetFolderPath(string dbType, string fileType, string path)
		{
			foreach (DbFileInfo dbFile in List)
			{
				if (dbFile.FileType == fileType)
				{
					if ((dbType == DbFileInfo.DbTypeAuditDB && dbFile.DbType == DbFileInfo.DbTypeAuditDB) ||
						(dbType == DbFileInfo.DbTypeEdwDB && dbFile.DbType == DbFileInfo.DbTypeEdwDB) ||
						(dbType == DbFileInfo.DbTypeMain && dbFile.DbType != DbFileInfo.DbTypeAuditDB && dbFile.DbType != DbFileInfo.DbTypeEdwDB))
					{
						dbFile.FolderPath = path;
					}
				}
			}
		}

		int DataFileCount;
		int LogFileCount;
	}
}
