using CargoWise.DbUpgrader.Scripts.Definitions.ToBeClassified;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.ToBeClassified
{
	[TestedType(typeof(vw_HotChequeTransactions))]
	class vw_HotChequeTransactionsTest : DbCreateScriptTest
	{
	}
}

