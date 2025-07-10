using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.InternationalLogistics;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.ReportFunctions.InternationalLogistics.Testing
{
	[TestedType(typeof(ctfn_JobShipmentOrg))]
	class ctfn_JobShipmentOrgTest : BiCreateScriptTest
	{
		protected override string ScriptDbName { get { return Db.EdwDatabaseName; } }
	}
}
