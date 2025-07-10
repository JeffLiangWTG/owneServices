using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(vw_NonReversedClassifiedTransactionLineAmounts))]
	class vw_NonReversedClassifiedTransactionLineAmountsTest : DbCreateScriptTest
	{
	}
}

