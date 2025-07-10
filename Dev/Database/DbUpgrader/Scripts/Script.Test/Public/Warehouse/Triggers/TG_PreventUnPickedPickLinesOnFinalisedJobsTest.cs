using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_PreventUnPickedPickLinesOnFinalisedJobs))]
	class TG_PreventUnPickedPickLinesOnFinalisedJobsTest : DBCreateTriggerScriptTest
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

