using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers.CheckProcedures;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Testing
{
	[TestedType(typeof(WhsCheckStockOnHandIsBalanced_ForInsert))]
	class WhsCheckStockOnHandIsBalanced_ForInsertTest : WhsCheckStockOnHandIsBalancedTestCase
	{
		protected override string ProcedureName => TestWhsDataSetupHelper.WhsCheckStockOnHandIsBalanced_ForInsert;
	}
}
