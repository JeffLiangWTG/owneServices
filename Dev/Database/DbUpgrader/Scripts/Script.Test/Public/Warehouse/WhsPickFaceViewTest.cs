using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse
{
	[TestedType(typeof(WhsPickFaceView))]
	class WhsPickFaceViewTest : DbCreateScriptTest
	{
		//This View is Tested in \Dev\Enterprise\Product\Operations\Warehouse\Transactions\Business\Testing\WhsSqlViewTestCases.cs
	}
}

