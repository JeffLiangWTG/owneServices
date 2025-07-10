using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Balances;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Balances
{
	[TestedType(typeof(vw_AccOrgBalance))]
	class vw_AccOrgBalanceTest : DbCreateScriptTest
	{
	}
}
