using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.Audit;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.Audit.Testing
{
	[TestedType(typeof(usp_MasterAuditLoad))]
	internal class usp_MasterAuditLoadTest : BiCreateScriptTest
	{
		public void TestExplicitTransactionProperlyManaged()
		{
			AssertEquals("Script has 'SET XACT_ABORT ON'", true, ScriptToTest.Text.IndexOf("SET XACT_ABORT ON", StringComparison.OrdinalIgnoreCase) >= 0);
		}

		protected override string ScriptDbName
		{
			get { return Db.AuditDatabaseName; }
		}
	}
}

