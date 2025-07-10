using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_PreventPickingReservedLinesOnUnAllocatedJobs))]
	class TG_PreventPickingReservedLinesOnUnAllocatedJobsTest : DBCreateTriggerScriptTest
	{
		// Tested in:
		//		Enterprise.Warehouse.Transactions.Business.Testing.PreventPickingReservedLinesOnUnAllocatedJobsTest
		//		Enterprise.Warehouse.Transactions.Business.Testing.PreventPickingReservedLinesOnUnAllocatedJobsConcurrencyTest
	}
}

