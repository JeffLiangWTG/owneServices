using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers.CheckProcedures;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Testing
{
	[TestedType(typeof(WhsCheckFinalisedJobsWithUnPickedPickLines))]
	class WhsCheckFinalisedJobsWithUnPickedPickLinesTest : DbCreateScriptTest
	{
		/* 
		Tested in:
			Enterprise.Warehouse.Transactions.Business.Testing.TestFinaliseDocket_AdjustOut_TriggerPreventsSavingInInconsistentState
			Enterprise.Warehouse.Transactions.Business.Testing.TestCanNotAddUnPickedPickLinesToFinalisedPick
			Enterprise.Warehouse.Transactions.Business.Testing.TestCanNotAddUnPickedPickLinesToFinalisedPick_WorkOrder
			Enterprise.Warehouse.Transactions.Business.Testing.TestLockIsWorkingWithPickLineChangeOnFinalisedPick
			Enterprise.Warehouse.Transactions.Business.Testing.TestCanNotAddUnPickedPickLinesToFinalisedDocketLine
		*/
	}
}

