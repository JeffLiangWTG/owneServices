using System.IO;
using System.Text.RegularExpressions;
using CargoWise.Data;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	public class SalesRestoreDbInfo
	{
		#region Constructor

		SalesRestoreDbInfo(string serverName, string databaseName)
		{
			ServerName = serverName;
			DatabaseName = databaseName;
			Initialize();
		}

		void Initialize()
		{
			try
			{
				DbFiles = DbFileInfoCollection.GetDbFileInfoCollectionFromDb(ServerName, DatabaseName);
				ServerExists = true;
				DatabaseExists = DbFiles.Count > 0;
			}
			catch (SqlException ex)
			{
				DbErrorMatch errorMatch = new DbErrorMatch(ex);
				if (errorMatch.ExceptionType == DbErrorType.ServerDoesNotExist)
				{
					ServerExists = false;
					DatabaseExists = false;
				}
				else
				{
					throw;
				}
			}
		}

		#endregion

		public static SalesRestoreDbInfo GetRestoreDbInfo(string loadInfoFilePath)
		{
			string fileText = File.ReadAllText(loadInfoFilePath);
			string serverName = GetServerName(fileText);
			string databaseName = GetDatabaseName(fileText);
			SalesRestoreDbInfo dbInfo = new SalesRestoreDbInfo(serverName, databaseName);
			return dbInfo;
		}

		#region Implementation

		protected static string GetDatabaseName(string loadInfoText)
		{
			string dbName = Regex.Match(loadInfoText, "(DATABASE=)((\\w|-)+)\r\n", RegexOptions.IgnoreCase).Groups[2].Value;

			if (string.IsNullOrEmpty(dbName))
			{
				dbName = Db.DatabaseName;
			}

			return dbName;
		}

		protected static string GetServerName(string loadInfoText)
		{
			//Get server name
			string serverName = Regex.Match(loadInfoText, "(SERVER=)((\\w|-)+)\r\n", RegexOptions.IgnoreCase).Groups[2].Value;

			if (string.IsNullOrEmpty(serverName))
			{
				serverName = Db.ServerName;
			}

			//Get instance
			string instance = Regex.Match(loadInfoText, "(INSTANCE=)((\\w|-)*)\r\n", RegexOptions.IgnoreCase).Groups[2].Value;

			if (!string.IsNullOrEmpty(instance))
			{
				serverName += "\\" + instance;
			}

			return serverName;
		}

		#endregion

		public string ServerName { get; private set; }
		public string DatabaseName { get; private set; }
		public DbFileInfoCollection DbFiles { get; private set; }
		public bool ServerExists { get; private set; }
		public bool DatabaseExists { get; private set; }
	}
}
