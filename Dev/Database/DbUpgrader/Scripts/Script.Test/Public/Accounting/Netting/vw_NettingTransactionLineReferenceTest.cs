using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Netting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Netting.Testing
{
	[TestedType(typeof(vw_NettingTransactionLineReference))]
	class vw_NettingTransactionLineReferenceTest : DbCreateScriptTest
	{
	}
}
