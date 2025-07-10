using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Enterprise.Upgrades;

namespace Enterprise.DbUpgrader.Startup.BeforeAndAfter.DotNetPrerequisiteCheck
{
	class DotNetCorePrerequisiteViolatingPCsProvider : IDotNetCorePrerequisiteViolatingPCsProvider
	{
		public IMinimumVersionDefinition[] ExpectedVersions =>
		[
			new MinimumDotNetRuntimeRequired(),
			new MinimumDotNetDesktopRuntimeRequired(),
			new MinimumAspNetCoreRuntimeRequired()
		];

		public virtual IDictionary<string, (DateTime recorded, bool hasIIS, string[] missingRuntime)> GetPCsViolatingDotNetCorePrerequisite(DataRowCollection rowsToCheck, string[] whitelist)
		{
			var pcsWithProblems = new Dictionary<string, (DateTime recorded, bool hasIIS, string[] missingRuntime)>();
			foreach (DataRow dotNetCoreRuntimesRow in rowsToCheck)
			{
				var pcName = dotNetCoreRuntimesRow["SD_Name"].ToString().Split('|')[1];
				var computer = new PcWithDotNetCoreRuntimesRecord(pcName, dotNetCoreRuntimesRow["value"].ToString());

				if (whitelist.Contains(computer.FullyQualifiedName, StringComparer.OrdinalIgnoreCase) || IsAnOldRecord(computer))
				{
					continue;
				}

				var missingVersions = ExpectedVersions.Where(minVersionCheck => !minVersionCheck.IsSupported(computer.DotNetCoreRuntimes)).ToList();
				string[] missingVersionsAsString = missingVersions.Select(x => x.DisplayName).ToArray();
				if (missingVersions.Count > 0)
				{
					pcsWithProblems.Add(computer.FullyQualifiedName, (computer.RecordingDate, computer.IsIISInstalled, missingVersionsAsString));
				}
			}
			return pcsWithProblems;
		}

		public bool IsAnOldRecord(PcWithDotNetCoreRuntimesRecord dotNetCoreRuntimesRecord)
		{
#pragma warning disable CW1061		// Close enough is fine, however I wonder if we should just delete first and then load...
			return dotNetCoreRuntimesRecord.RecordingDate.AddDays(15) < DateTime.UtcNow;
#pragma warning restore CW1061
		}
	}
}
