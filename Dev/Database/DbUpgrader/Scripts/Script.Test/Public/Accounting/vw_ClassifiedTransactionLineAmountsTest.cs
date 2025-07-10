using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(vw_ClassifiedTransactionLineAmounts))]
	class vw_ClassifiedTransactionLineAmountsTest : DbCreateScriptTest
	{
	}
}

