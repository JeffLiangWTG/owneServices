using System;
using System.IO;
using CargoWise.Common;

namespace Enterprise.LogShipping.Setup
{
	public abstract class DatabaseInfo
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "https://social.msdn.microsoft.com/Forums/en-US/f5832d74-ff17-4ca0-b69c-d579e1f54506/code-analysis-in-visual-studio-2010-generates-a-ca2214-warning-caused-by-code-contracts?forum=codecontracts")]
		public DatabaseInfo(LogShippingInfo setupInfo, string databaseName)
		{
			Argument.NotNull(setupInfo, nameof(setupInfo));
			Argument.NotNull(databaseName, nameof(databaseName));

			BackupFileName = string.Empty;
			ShouldInitialise = false;
			this.SetupInfo = setupInfo;
			this.DatabaseName = databaseName;
		}

		public LogShippingInfo SetupInfo
		{
			get
			{
				return setupInfo;
			}
			private set
			{
				Argument.NotNull(value, nameof(value));
				setupInfo = value;
			}
		}
		LogShippingInfo setupInfo;

		public string DatabaseName { get; private set; }
		public abstract string SecondaryDatabaseName { get; }
		public string BackupFileName { get; set; }
		public string BackupFullFileName
		{
			get
			{
				var directory = SetupInfo.BackupSourceDirectory ?? string.Empty;

				return Path.Combine(directory, BackupFileName);
			}
		}

		public bool ShouldInitialise { get; set; }
		public bool IsNew { get; set; }

		public Guid CopyJobId { get; set; }
		public Guid RestoreJobId { get; set; }

		public virtual bool SecondaryDatabaseExists()
		{
			var fullInstanceName = SetupInfo.SecondaryServer != null ? SetupInfo.SecondaryServer.FullInstanceName : string.Empty;

			return dbManager.IsDatabaseInStandByMode(fullInstanceName, SecondaryDatabaseName);
		}

		public override string ToString()
		{
			return SecondaryDatabaseName;
		}

		#region Implementation

		readonly DbManager dbManager = new DbManager();

		#endregion
	}
}
