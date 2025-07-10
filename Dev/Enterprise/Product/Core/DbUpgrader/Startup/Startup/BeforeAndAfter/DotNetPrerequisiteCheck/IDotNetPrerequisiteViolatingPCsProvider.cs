using System.Collections.Generic;
using System.Data;
using Enterprise.Upgrades;

namespace Enterprise.DbUpgrader.Startup.BeforeAndAfter.DotNetPrerequisiteCheck
{
	interface IDotNetPrerequisiteViolatingPCsProvider
	{
		IEnumerable<PcWithDotNetVersionRecord> GetPCsViolatingDotNetPrerequisite(DataRowCollection rowsToCheck, string[] whitelist);
	}
}
