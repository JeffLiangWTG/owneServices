using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers.CheckProcedures;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Testing
{
	[TestedType(typeof(WhsCheckPickLinesAreNotOverCommitting))]
	class WhsCheckPickLinesAreNotOverCommittingsTest : DbCreateScriptTest
	{
		// Tested in:
		//		Enterprise.Warehouse.Transactions.Business.Testing.PreventOverCommitOfStockViaPickLineTest
		//		Enterprise.Warehouse.Transactions.Business.Testing.PreventOverCommitOfStockViaPickLineConcurrencyTest
	}
}

