using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Netting.Matching;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Netting.Matching.Testing
{
	[TestedType(typeof(NettingPayableLineToReceivableLineReferenceLevelMatch))]
	class NettingPayableLineToReceivableLineReferenceLevelMatchTest : DbCreateScriptTest
	{
		//This is tested in Enterprise.Accounting.Business.Testing.ScriptTests.NettingMatchTransactionsTest
	}
}

