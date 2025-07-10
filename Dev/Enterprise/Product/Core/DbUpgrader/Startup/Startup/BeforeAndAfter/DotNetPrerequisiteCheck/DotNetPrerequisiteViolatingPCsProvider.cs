using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Enterprise.Upgrades;

namespace Enterprise.DbUpgrader.Startup.BeforeAndAfter.DotNetPrerequisiteCheck
{
	class DotNetPrerequisiteViolatingPCsProvider : IDotNetPrerequisiteViolatingPCsProvider
	{
		readonly IMinimumDotNetVersionDefinition dotNetVersionCheck;

		public DotNetPrerequisiteViolatingPCsProvider(IMinimumDotNetVersionDefinition dotNetVersionCheck)
		{
			this.dotNetVersionCheck = dotNetVersionCheck;
		}

		public virtual IEnumerable<PcWithDotNetVersionRecord> GetPCsViolatingDotNetPrerequisite(DataRowCollection rowsToCheck, string[] whitelist)
		{
			var pcsWithProblems = new List<PcWithDotNetVersionRecord>();
			foreach (DataRow dotNetVersionRow in rowsToCheck)
			{
				var computer = new PcWithDotNetVersionRecord(new string[] { dotNetVersionRow["SD_Name"].ToString(), dotNetVersionRow["value"].ToString() });

				if (!dotNetVersionCheck.IsDotNetSupportedVersion(computer.DotNetVersion) && !dotNetVersionCheck.IsAnOldRecord(computer) && !whitelist.Contains(computer.FullyQualifiedName, StringComparer.OrdinalIgnoreCase))
				{
					pcsWithProblems.Add(computer);
				}
			}
			return pcsWithProblems;
		}
	}
}
