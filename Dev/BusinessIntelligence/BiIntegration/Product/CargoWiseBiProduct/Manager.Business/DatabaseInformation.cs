using System;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.Bi.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.Resource.Version;

namespace CargoWise.Bi.Product.Manager.Business
{
	public class DatabaseInformation
	{
		public DatabaseInformation(string serverName, string dbName, DbType dbType)
		{
			DbType = dbType;
			if (!string.IsNullOrEmpty(serverName))
			{
				try
				{
					using (var connection = GetConnection(serverName))
					{
						if (connection.DatabaseExists(dbName))
						{
							ServerName = serverName;
							ServerVersion = GetServerVersion(connection);

							DatabaseName = dbName;
							DatabaseVersion = GetDatabaseVersion(connection);
						}
					}
				}
				catch (SqlException ex)
				{
					ErrorMessage = ex.Message;
				}
			}
		}

		public string ErrorMessage { get; private set; }
		public string ServerName { get; private set; }
		public string ServerVersion { get; private set; }
		public string DatabaseName { get; private set; }
		public string DatabaseVersion { get; private set; }

		DbType DbType { get; set; }

		DbConnection GetConnection(string serverName)
		{
			if (!string.IsNullOrEmpty(serverName))
			{
				return (serverName == Db.ServerName) ?
					Db.Connection :
					Db.NewExtraConnectionWithMainDbCredentials(
						serverName,
						Db.SqlMasterDb);
			}
			else
			{
				return null;
			}
		}

		#region Server Version

		string GetServerVersion(DbConnection connection)
		{
			if (connection != null)
			{
				var regex = new Regex(@"Microsoft\s+SQL\s+Server\s+\d+\s+\(.+?\)", RegexOptions.IgnoreCase);
				var sqlServerVersionName = regex.Match(connection.ServerFullVersionText).Value;
				return sqlServerVersionName + " - " + connection.ServerVersionNumber;
			}
			else
			{
				return null;
			}
		}

		#endregion

		#region Database Version

		string GetDatabaseVersion(DbConnection connection)
		{
			string databaseVersion = null;
			if (DbType == DbType.MainDb)
			{
				var majorDbVersion = DbRegistry.DatabaseMajorSchemaVersion.LoadValue(Db.Connection);
				var minorDbVersion = DbRegistry.DatabaseMinorSchemaVersion.LoadValue(Db.Connection);
				databaseVersion = new VersionLabel(majorDbVersion, minorDbVersion).ToString();
			}
			else
			{
				if (connection != null && connection.DatabaseExists(DatabaseName))
				{
					databaseVersion = BiMasterState.GetBiDatabaseExtPty(connection, DatabaseName, BiConstants.MainDbSchemaVersionExtPtyName);
				}
			}
			return databaseVersion;
		}

		#endregion

		#region Database Size

		enum FileType
		{
			Data,
			Log
		}

		public string DatabaseSize
		{
			get
			{
				if (databaseSize == null)
				{
					databaseSize = GetDatabaseSize(FileType.Data);
				}
				return databaseSize;
			}
		}
		string databaseSize;

		public string DatabaseLogSize
		{
			get
			{
				if (databaseLogSize == null)
				{
					databaseLogSize = GetDatabaseSize(FileType.Log);
				}
				return databaseLogSize;
			}
		}
		string databaseLogSize;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "file size format")]
		string GetDatabaseSize(FileType fileType)
		{
			if (!string.IsNullOrEmpty(ServerName) && !string.IsNullOrEmpty(DatabaseName))
			{
				using (var adminConnection = Db.NewAdminConnection(ServerName, DatabaseName))
				{
					var totalSize = GetSize(adminConnection, fileType);
					var sizeUsed = GetSizeUsed(adminConnection, fileType);
					var percentageUsed = sizeUsed / totalSize;
					var unit = "MB";

					if (totalSize > 10000)
					{
						totalSize /= 1024;
						sizeUsed /= 1024;
						unit = "GB";
					}

					return string.Format(CultureInfo.InvariantCulture, "{0:n} {2} ({1:n} {2} used, {3:P0})", totalSize, sizeUsed, unit, percentageUsed);
				}
			}
			else
			{
				return "";
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		decimal GetSize(DbConnection connection, FileType fileType)
		{
			using (var cmd = connection.Command("dbo.usp_GetDbSizeWithFileType"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@FileTypeDesc", SqlDbType.NVarChar, 60, (fileType == FileType.Data) ? "ROWS" : "LOG");
				cmd.AddOutputParameter("@SizeMb", SqlDbType.Decimal, 32, 0, 2, null);
				cmd.ExecuteNonQuery();

				var totalSize = Convert.ToDecimal(cmd.GetParameterValue("@SizeMb"), CultureInfo.InvariantCulture);

				return totalSize;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		decimal GetSizeUsed(DbConnection connection, FileType fileType)
		{
			using (var cmd = connection.Command("dbo.usp_GetSizeUsedWithFileType"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@FileType", SqlDbType.TinyInt, 0, (fileType == FileType.Data) ? 0 : 1);
				cmd.AddOutputParameter("@SizeUsedMb", SqlDbType.Decimal, 32, 0, 2, null);
				cmd.ExecuteNonQuery();

				var sizeUsed = Convert.ToDecimal(cmd.GetParameterValue("@SizeUsedMb"), CultureInfo.InvariantCulture);

				return sizeUsed;
			}
		}

		#endregion
	}

	public enum DbType
	{
		MainDb,
		BiDb
	}
}
