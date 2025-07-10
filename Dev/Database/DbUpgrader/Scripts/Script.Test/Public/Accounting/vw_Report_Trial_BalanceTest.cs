using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	[TestedType(typeof(vw_Report_Trial_Balance))]
	class vw_Report_Trial_BalanceTest : DbCreateScriptTest
	{
	}
}

