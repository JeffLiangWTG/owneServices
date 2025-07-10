using System;
using System.Collections.Generic;
using System.Data;

namespace Enterprise.DbUpgrader.Startup.BeforeAndAfter.DotNetPrerequisiteCheck
{
	interface IDotNetCorePrerequisiteViolatingPCsProvider
	{
		IDictionary<string, (DateTime recorded, bool hasIIS, string[] missingRuntime)> GetPCsViolatingDotNetCorePrerequisite(DataRowCollection rowsToCheck, string[] whitelist);
	}
}
