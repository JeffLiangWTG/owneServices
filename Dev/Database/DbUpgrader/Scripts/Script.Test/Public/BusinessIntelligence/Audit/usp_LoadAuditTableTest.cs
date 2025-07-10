using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.Audit;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.Audit.Testing
{
	[TestedType(typeof(usp_LoadAuditTable))]
	internal class usp_LoadAuditTableTest : BiCreateScriptTest
	{
		protected override string ScriptDbName
		{
			get { return Db.AuditDatabaseName; }
		}
	}
}

