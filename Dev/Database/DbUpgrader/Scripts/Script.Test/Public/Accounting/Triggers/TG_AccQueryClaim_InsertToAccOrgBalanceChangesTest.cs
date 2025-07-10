using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Triggers;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Triggers.Testing
{
	[TestedType(typeof(TG_AccQueryClaim_InsertToAccOrgBalanceChanges))]
	class TG_AccQueryClaim_InsertToAccOrgBalanceChangesTest : DbCreateScriptTest
	{
		// The unit tests for this trigger are located here:
		// C:\Dev\Enterprise\Product\Core\Database\Script\Public\Accounting\Balances\vw_AccOrgBalance.cs: TestClaimTotal()
	}
}

