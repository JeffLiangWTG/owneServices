using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DbUpgrader.Startup.BeforeAndAfter.DotNetPrerequisiteCheck
{
	class DotNetPrerequisiteChecker
	{
		readonly IDotNetPrerequisiteViolatingPCsProvider violatingPCsProvider;
		readonly IMinimumDotNetVersionDefinition versionDefinition;
		readonly IDotNetPrerequisiteDataProvider dotNetPrerequisiteDataProvider;

		public DotNetPrerequisiteChecker(IDotNetPrerequisiteViolatingPCsProvider violatingPCsProvider, IMinimumDotNetVersionDefinition versionDefinition, IDotNetPrerequisiteDataProvider dotNetPrerequisiteDataProvider)
		{
			this.violatingPCsProvider = violatingPCsProvider;
			this.versionDefinition = versionDefinition;
			this.dotNetPrerequisiteDataProvider = dotNetPrerequisiteDataProvider;
		}

		public void CheckDotNetVersions(DbConnection connection)
		{
			var dbDotNetVersionRecords = dotNetPrerequisiteDataProvider.GetVersionsRecords(connection);
			PatchDataTableWithCurrentMachineValue(dbDotNetVersionRecords);

			string[] whitelist = DataRegistry.Instance.DotNetPreUpgradeCheckWhitelist.Split(',').Select(p => p.Trim()).ToArray();
			var pcsWithProblems = violatingPCsProvider.GetPCsViolatingDotNetPrerequisite(dbDotNetVersionRecords.Rows, whitelist);

			if (pcsWithProblems.Any())
			{
				var message = string.Format(CultureInfo.InvariantCulture, "The following PCs in your environment do not have the required version of the .NET Framework (v{0}):{1}{2}{1}Please update these computers and run the current version of {3} on each of these computers so that the system can record the new .NET framework version before starting this upgrade.",
					   versionDefinition.MinimumDotNetVersionRequired, System.Environment.NewLine, string.Join(System.Environment.NewLine, pcsWithProblems), Core.Constants.ProductName);
				throw new PlatformNotSupportedException(message);
			}

			dotNetPrerequisiteDataProvider.RemoveOldVersionsRecords();
		}

		void PatchDataTableWithCurrentMachineValue(DataTable dbDotNetVersionRecords)
		{
			var currentMachineValue = DotNetRecorder.GetVersionRecordForCurrentComputer();
			var needle = PcWithDotNetVersionRecord.RegistryPrefix + "|" + currentMachineValue.FullyQualifiedName;

			foreach (DataRow row in dbDotNetVersionRecords.Rows)
			{
				if (string.Equals((string)row["SD_Name"], needle, StringComparison.OrdinalIgnoreCase))
				{
					row["value"] = $"{currentMachineValue.DotNetVersion}|RecordingDate:{DateTime.UtcNow:s}";
				}
			}
		}
	}
}
