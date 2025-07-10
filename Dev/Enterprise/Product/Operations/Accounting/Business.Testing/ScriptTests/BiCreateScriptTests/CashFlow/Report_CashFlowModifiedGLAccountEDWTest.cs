using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.Accounting.Business.Testing.ScriptTests.BaseTests;

namespace Enterprise.Accounting.Business.Testing.ScriptTests.BiCreateScriptTests.CashFlow
{
	[UseSnapshotProtection(new[] { DatabaseType.Main })]
	class Report_CashFlowModifiedGLAccountEDWTest : Report_CashFlowModifiedGLAccountTest
	{
	}
}

