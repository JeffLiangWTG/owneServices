using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Data;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DbUpgrader.Startup.BeforeAndAfter.DotNetPrerequisiteCheck
{
	class DotNetCorePrerequisiteChecker
	{
		readonly IDotNetCorePrerequisiteViolatingPCsProvider violatingPCsProvider;
		readonly IDotNetPrerequisiteDataProvider dataProvider;

		public DotNetCorePrerequisiteChecker(IDotNetCorePrerequisiteViolatingPCsProvider violatingPCsProvider, IDotNetPrerequisiteDataProvider dataProvider)
		{
			this.violatingPCsProvider = violatingPCsProvider;
			this.dataProvider = dataProvider;
		}

		public void CheckDotNetCoreRuntimes(DbConnection connection)
		{
			var dbDotNetCoreRuntimesRecords = dataProvider.GetVersionsRecords(connection);
			PatchDataTableWithCurrentMachineValue(dbDotNetCoreRuntimesRecords);

			var whitelist = DataRegistry.Instance.DotNetPreUpgradeCheckWhitelist.Split(',').Select(p => p.Trim()).ToArray();
			var pcsWithProblems = violatingPCsProvider.GetPCsViolatingDotNetCorePrerequisite(dbDotNetCoreRuntimesRecords.Rows, whitelist);

			if (pcsWithProblems.Any())
			{
				var exceptionMessage = new StringBuilder();

				exceptionMessage.AppendLine("The following PCs in your environment do not have the required versions of the .NET runtimes:");
				foreach (var pc in pcsWithProblems)
				{
#pragma warning disable CW1103  // No access to ZDateTime.ShortDateFormat at this stage.
					var formatedRecordedDate = pc.Value.recorded.ToString("dd-MMM-yy", CultureInfo.InvariantCulture);
#pragma warning restore CW1103
					exceptionMessage.AppendFormat(CultureInfo.InvariantCulture, "Computer [{0}] is missing the following runtimes (last refreshed {1}):\r\n", pc.Key, formatedRecordedDate);
					for (var index = 0; index < pc.Value.missingRuntime.Length; index++)
					{
						exceptionMessage.AppendFormat(CultureInfo.InvariantCulture, "\tMissing {0}{1}\r\n", pc.Value.missingRuntime[index], index < pc.Value.missingRuntime.Length - 1 ? "," : "");
					}
				}

				exceptionMessage.AppendLine();
				exceptionMessage.Append(
$@"Please update these computers and run the current version of {Core.Constants.ProductName} on each of these computers so that the system can record the new .NET runtimes before starting this upgrade.

For more details please see the update note:
	- https://wisetechacademy.com/search?quickstart=6edd4257-dd13-4c51-b677-8c8d195599e4

The runtimes can be downloaded directly from Microsoft from:
	- https://dotnet.microsoft.com/en-us/download/dotnet/8.0");

				throw new PlatformNotSupportedException(exceptionMessage.ToString());
			}

			dataProvider.RemoveOldVersionsRecords();
		}

		void PatchDataTableWithCurrentMachineValue(DataTable dbDotNetCoreRuntimesRecords)
		{
			var currentMachineValue = DotNetRecorder.GetRuntimeRecordForCurrentComputer();
			var needle = PcWithDotNetCoreRuntimesRecord.RegistryPrefix + "|" + currentMachineValue.FullyQualifiedName;

			foreach (DataRow row in dbDotNetCoreRuntimesRecords.Rows)
			{
				if (string.Equals((string)row["SD_Name"], needle, StringComparison.OrdinalIgnoreCase))
				{
#pragma warning disable CW1061
					row["value"] = $"{currentMachineValue.ToDbValue()}|RecordingDate:{DateTime.UtcNow:s}";
#pragma warning restore CW1061
				}
			}
		}
	}
}
