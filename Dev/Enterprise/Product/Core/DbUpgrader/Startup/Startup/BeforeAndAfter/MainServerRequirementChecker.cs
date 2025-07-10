using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Startup.BeforeAndAfter.DotNetPrerequisiteCheck;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DbUpgrader.Startup
{
	public class MainServerRequirementChecker : RequirementChecker
	{
		public MainServerRequirementChecker(IEnumerable<string> dbsBeingUpgraded, IEnumerable<string> allDatabases)
			: base(dbsBeingUpgraded, allDatabases)
		{
		}

		#region Mandatory Checks

		#region SQL Server Name

		public void CheckServerName(DbConnection connection)
		{
			var checker = new SqlServerNameChecker();
			checker.CheckServerName(connection);
		}

		#endregion

		#region Database Naming

		public void CheckDbNaming()
		{
			ValidateDbName(Db.DatabaseName);
		}

		protected void ValidateDbName(string dbName)
		{
			DataUtils.ValidateMainDatabaseName(dbName);
		}

		#endregion

		#region DotNet version

		public void CheckDotNet(DbConnection connection, IUpgradeTaskWorkflowLogger logger)
		{
			logger.StartTask("Checking .NET requirements");
			if (!DataRegistry.Instance.EnableDotNetPreUpgradeCheck)
			{
				logger.ShowInfoMessage(".NET requirements check skipped because it's disabled in the registry.");
				return;
			}

			var versionDefinition = new MinimumDotNetVersionDefinition();
			var pcsProvider = new DotNetPrerequisiteViolatingPCsProvider(versionDefinition);
			var dotNetDataProvider = new DotNetPrerequisiteDataProvider();
			var checker = new DotNetPrerequisiteChecker(pcsProvider, versionDefinition, dotNetDataProvider);
			checker.CheckDotNetVersions(connection);

			var registration = ObjectFactory.Get<IProductRegistration>();
			if (registration.Key.HostedLocation != CargoWise.Licensing.Constants.NotHostedWithCargoWise)
			{
				logger.ShowInfoMessage("Skipping .NET Core requirements check because this is not a self hosted client.");
				return;
			}

			var pcsProviderCore = new DotNetCorePrerequisiteViolatingPCsProvider();
			var dotNetCoreDataProvider = new DotNetCorePrerequisiteDataProvider();
			var checkerNetCore = new DotNetCorePrerequisiteChecker(pcsProviderCore, dotNetCoreDataProvider);
			checkerNetCore.CheckDotNetCoreRuntimes(connection);
			logger.ShowInfoMessage(".NET requirements check finished.");
		}

		#endregion

		#endregion

		#region Disk Space

		public string[] GetLowDiskSpaceList(AdminConnection connection)
		{
			List<string> lowDiskSpaceDriveList = new List<string>();

			if (dbsBeingUpgraded.Any())
			{
				string sqlText = String.Format(CultureInfo.InvariantCulture, @"
					DECLARE @Drives TABLE (
						Drive char(1) COLLATE database_default, FreeSpace int);
					INSERT INTO @Drives
						EXEC sys.xp_fixeddrives;
					SELECT Drive, FreeSpace
						FROM @Drives
						WHERE Drive IN (
							SELECT distinct left(ltrim(fil.physical_name), 1) COLLATE database_default
							FROM sys.master_files fil
							INNER JOIN sys.databases db ON db.database_id = fil.database_id
							WHERE db.name in ('{0}')
						);",
					String.Join("','", dbsBeingUpgraded.ToArray()));

				var dbFileDrives = DataUtils.GetDataTableFromQuery(connection, sqlText);

				foreach (DataRow driveRow in dbFileDrives.Rows)
				{
					string driveLetter = driveRow[0].ToString();
					int driveFreeSpaceMb = Utilities.ConvertToInt32(driveRow[1]);

					if (IsDiskSpaceLow(driveFreeSpaceMb))
					{
						string lowDiskSpaceLine = String.Format(CultureInfo.InvariantCulture,
							"Drive [{0}] in the Database Server has only {1} MB free. It should have at least 5 GB of free space.",
							driveLetter, driveFreeSpaceMb);
						lowDiskSpaceDriveList.Add(lowDiskSpaceLine);
					}
				}
			}

			return lowDiskSpaceDriveList.ToArray();
		}

		protected virtual bool IsDiskSpaceLow(int freeSpaceMb)
		{
			return (freeSpaceMb < 5000);
		}

		#endregion
	}
}
