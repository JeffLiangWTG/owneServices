using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Balances;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Balances
{
	[TestedType(typeof(vw_AccOrgBalanceFromDetailedRecords))]
	class vw_AccOrgBalanceFromDetailedRecordsTest : DbCreateScriptTest
	{
		// This view is only used by the stored procedure CheckAndRepairAccOrgBalance.
		// The tests for this stored procedure (and this view) are located here:
		//
		// Path: \Dev\Enterprise\Product\Operations\Accounting\Business\StabilityCheck\AccountingStabilityChecker.cs
		// Method: TestStabilityChecker()
		// Method: TestStabilityChecker_OnlyClaimTotalDiff()
		// 
		// Path: \Dev\Enterprise\Product\Core\DbUpgrader\Transformation\DataModification\Public\Accounting\PopulateAccOrgBalanceFromDetailedRecords.cs
		// Method: TestRunAndAssertResultsTwice()
		// 
		// Path: DEV\Enterprise\Product\Core\Database\Script\Public\Accounting\Balances\CheckAndRepairAccOrgBalance.cs
		// Method: TestClaim()
	}
}

