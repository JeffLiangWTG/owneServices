using System;
using System.Collections.Specialized;
using CargoWise.Common;
using CargoWise.Data;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	public class DbServerBrowser : DisposableObject
	{
		public DbServerBrowser(string dbServer, bool isFolderOnly)
		{
			this.disableSchemaVersionCheck = Db.DisableSchemaVersionCheck();
			this.BrowseConn = GetBrowseConnection(dbServer);
			this.IsFolderOnly = isFolderOnly;
		}

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				disableSchemaVersionCheck.Dispose();
			}
			base.Dispose(isDisposing);
		}

		internal static DbConnection GetBrowseConnection(string dbServer)
		{
			return Db.NewAdminConnection(dbServer, Db.SqlMasterDb);
		}

		readonly IDisposable disableSchemaVersionCheck;
		readonly DbConnection BrowseConn;
		readonly bool IsFolderOnly;

		public StringCollection FixedDrives
		{
			get
			{
				if (fFixedDrives == null)
				{
					fFixedDrives = GetServerFixedDrives();
				}

				return fFixedDrives;
			}
		}

		StringCollection fFixedDrives;

		StringCollection GetServerFixedDrives()
		{
			StringCollection result = new StringCollection();
			string sqlText = String.Format("EXEC sys.xp_fixeddrives");

			using (DbCommand cmd = BrowseConn.Command(sqlText))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					string driveLetter = reader["drive"].ToString().Trim();
					result.Add(driveLetter + ":");
				}
			}

			return result;
		}

		public DirectoryEntryCollection GetDirectoryList(string directoryPath)
		{
			DirectoryEntryCollection allEntries = new DirectoryEntryCollection();
			DirectoryEntryCollection files = new DirectoryEntryCollection();

			string sqlText = String.Format("EXEC sys.xp_dirtree '{0}', 1, 1", directoryPath);

			using (DbCommand cmd = BrowseConn.Command(sqlText))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					bool isEntryAFile = (Convert.ToInt32(reader["file"]) == 1);

					if (!(IsFolderOnly && isEntryAFile))
					{
						string entryName = reader["subdirectory"].ToString().Trim();

						DirectoryEntry entry = new DirectoryEntry(directoryPath, entryName, isEntryAFile);
						if (isEntryAFile)
						{
							files.Add(entry);
						}
						else
						{
							allEntries.Add(entry);
						}
					}
				}
			}
			allEntries.AddRange(files);

			return allEntries;
		}

		#region BrowseResult

		public class BrowseResult
		{
			public BrowseResult()
			{
			}

			public string FullPath
			{
				get { return fFullPath; }
				set { fFullPath = value; }
			}

			string fFullPath = "";
		}

		#endregion
	}
}
