using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.CashFlow;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting.CashFlow.Testing
{
	[TestedType(typeof(Report_CashFlowModifiedGLAccount))]
	class Report_CashFlowModifiedGLAccountTest : BiCreateScriptTest
	{
		protected override string ScriptDbName => Db.EdwDatabaseName;
	}
}

