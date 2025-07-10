using System.Collections.Generic;

namespace Enterprise.LogShipping.Setup
{
	public class LogShippingInfo
	{
		#region Database Info List

		public MainDatabaseInfo MainDatabase { get; set; }

		public List<DatabaseInfo> GetDatabaseInfoListToProcess()
		{
			List<DatabaseInfo> result = new List<DatabaseInfo> { MainDatabase };

			if (ShouldLogShipEDocsDatabases && MainDatabase != null)
			{
				result.AddRange(MainDatabase.DependentDatabases.ToArray());
			}

			return result;
		}

		#endregion

		public SqlServerInfo PrimaryServer { get; set; }
		public string PrimaryServerFromLSMetadata { get; set; }
		public SqlServerInfo SecondaryServer { get; set; }

		public string BackupSourceDirectory { get; set; }
		public string BackupLocalCopyDirectory { get; set; }

		public bool ShouldLogShipEDocsDatabases { get; set; }
		public bool ShouldInitializeSecondaryDb { get; set; }
		public SetupAction SetupAction { get; set; }
		public bool ShouldDropDatabaseAfterLSRemoving { get; set; }
		public string OriginalBackupSourceDirectory { get; set; }
		public string OriginalBackupLocalCopyDirectory { get; set; }

		public string RestoreDataDirectoryOverride { get; set; }
		public string RestoreLogDirectoryOverride { get; set; }

		public bool ConfigurationChanged
		{
			get
			{
				return (OriginalBackupLocalCopyDirectory == null || OriginalBackupSourceDirectory == null)
					   || (BackupLocalCopyDirectory != null && BackupLocalCopyDirectory.ToUpper() != OriginalBackupLocalCopyDirectory.ToUpper())
					   || (BackupSourceDirectory != null && BackupSourceDirectory.ToUpper() != OriginalBackupSourceDirectory.ToUpper())
					   || GetDatabaseInfoListToProcess().Find(dbInfo => dbInfo.ShouldInitialise) != null;
			}
		}

		public bool SecondaryDatabasesExist()
		{
			var result = true;

			foreach (DatabaseInfo info in GetDatabaseInfoListToProcess())
			{
				if (info != null)
				{
					result = info.SecondaryDatabaseExists();

					if (!result)
					{
						break;
					}
				}
			}

			return result;
		}

		public override string ToString()
		{
			return MainDatabase != null ? MainDatabase.SecondaryDatabaseName : string.Empty;
		}
	}

	public enum SetupAction
	{
		Setup,
		Change,
		Remove
	}
}
