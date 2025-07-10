using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.WorkingTime.Testing
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.WorkingTime.BranchHolidayRuleLastFriday))]
	class BranchHolidayRuleLastFridayTest : BiCreateScriptTest
	{
		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}
	}
}

