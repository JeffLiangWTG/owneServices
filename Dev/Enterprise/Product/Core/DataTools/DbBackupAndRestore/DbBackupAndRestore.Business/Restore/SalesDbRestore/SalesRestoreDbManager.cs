using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.Common;
using Enterprise.DataTools.DbBackupAndRestore.Business.Common;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	public class SalesRestoreDbManager : DbRestoreManager
	{
		public SalesRestoreDbManager() : base(new DbHeaderOnlyReader())
		{
		}

		public void RestoreDatabase(string backupFilePath)
		{
			try
			{
				FireOnTaskStarted("Sales Database restore process started\r\n");

				FireOnTaskStarted("Getting target database info\n");
				var restoreDbInfo = GetTargetDatabaseInfo();

				FireOnTaskStarted("Searching backup file");
				ValidateBackupFileOrGetNewIfNull(restoreDbInfo.ServerName, ref backupFilePath);
				FireOnSubtaskStarted(string.Format("Backup file was found:\r\t{0}\n", backupFilePath), 0);

				restoreDbInfo.DbFiles?.SetDbFileTypeVisible(DbFileInfo.DbTypeRefDB, false);

				var mainServerConfig = new DbServerConfiguration(restoreDbInfo.ServerName, backupFilePath);
				var auditServerConfig = AuditDbServerConfiguration.Empty;
				var edwServerConfig = EdwDbServerConfiguration.Empty;
				var dbRestoreSettings = new DbRestoreSettings(restoreDbInfo.DatabaseName, restoreDbInfo.DbFiles, DbRestoreOption.CopyProdToTest);

				RestoreDatabases(mainServerConfig, auditServerConfig, edwServerConfig, dbRestoreSettings);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				FireOnTaskFailed(ex.Message);
			}
		}

		#region Get Target Database Info

		SalesRestoreDbInfo GetTargetDatabaseInfo()
		{
			var restoreDbInfo =
				  File.Exists(@"..\" + LoadInfoFilePath)
				? SalesRestoreDbInfo.GetRestoreDbInfo(@"..\" + LoadInfoFilePath)
				: SalesRestoreDbInfo.GetRestoreDbInfo(LoadInfoFilePath);

			if (!restoreDbInfo.ServerExists)
			{
				var message = string.Format("Could not open a connection to SQL Server. Server [{0}] was not found or was not accessible.", restoreDbInfo.ServerName);
				throw new DbBackupAndRestoreException(message);
			}

			if (!restoreDbInfo.DatabaseExists)
			{
				var message = string.Format("Database [{0}] doesn't exist.", restoreDbInfo.DatabaseName);
				throw new DbBackupAndRestoreException(message);
			}

			return restoreDbInfo;
		}

		#endregion

		#region Get Backup File Path

		void ValidateBackupFileOrGetNewIfNull(string serverName, ref string backupFilePath)
		{
			if (string.IsNullOrEmpty(backupFilePath))
			{
				backupFilePath = GetBackupFilePath(serverName);
			}

			if (!File.Exists(backupFilePath))
			{
				var message = string.IsNullOrEmpty(backupFilePath) ?
					"No file selected."
					: string.Format("Backup file name passed as parameter [{0}] does not exist.", backupFilePath);
				throw new DbBackupAndRestoreException(message);
			}
		}

		string GetBackupFilePath(string serverName)
		{
			string backupFilePath = null;

			var desktopDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
			backupFilePath = SearchBackupFile(desktopDirectory);

			if (string.IsNullOrEmpty(backupFilePath))
			{
				backupFilePath = ShowAttachedOBrowseFileInterface(serverName, desktopDirectory);
			}

			return backupFilePath;
		}

		protected string SearchBackupFile(string searchDirectory)
		{
			string backupFilePath = null;

			var files = Directory.GetFiles(searchDirectory, "*" + Utilities.FullBackupFileExtension);
			var standartBackupFiles = new List<string>();

			if (files.Length > 0)
			{
				foreach (var fileName in files)
				{
					if (Utilities.DetermineFileValidity(fileName) != FileValidity.NotValid)
					{
						standartBackupFiles.Add(fileName);
					}
				}
			}

			if (standartBackupFiles.Count > 0)
			{
				backupFilePath = FindLastBackupFile(standartBackupFiles.ToArray());
			}

			return backupFilePath;
		}

		string FindLastBackupFile(string[] backupFiles)
		{
			string lastBackupFile = null;

			if (backupFiles.Length == 1)
			{
				lastBackupFile = backupFiles[0];
			}
			else
			{
				var lastTime = DateTime.MinValue;
				foreach (var fileName in backupFiles)
				{
					var info = new FileInfo(fileName);
					if (info.CreationTimeUtc > lastTime)
					{
						lastTime = info.CreationTimeUtc;
						lastBackupFile = fileName;
					}
				}
			}

			return lastBackupFile;
		}

		string ShowAttachedOBrowseFileInterface(string serverName, string defaultBackupFolderLocation)
		{
			if (OnShowOpenFileDialog == null || SyncInvoke == null)
			{
				return null;
			}

			var asyncResult = SyncInvoke.BeginInvoke(OnShowOpenFileDialog, new object[] { serverName, defaultBackupFolderLocation });
			return SyncInvoke.EndInvoke(asyncResult).ToString();
		}

		#endregion

		const string LoadInfoFilePath = "ediLoad.ini";
		public delegate string ShowOpenFileDialogDelegate(string serverName, string defaultBackupFolderLocation);
		public ShowOpenFileDialogDelegate OnShowOpenFileDialog;
	}
}
