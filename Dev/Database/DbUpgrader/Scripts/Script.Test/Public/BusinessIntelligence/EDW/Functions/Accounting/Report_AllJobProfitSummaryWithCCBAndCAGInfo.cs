using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing
{
	[TestedType(typeof(Report_AllJobProfitSummaryWithCCBAndCAGInfo))]
	class Report_AllJobProfitSummaryWithCCBAndCAGInfoTest : BiCreateScriptTest
	{
		protected override string ScriptDbName => Db.EdwDatabaseName;
	}
}
