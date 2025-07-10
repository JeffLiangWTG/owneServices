using CargoWise.DbUpgrader.Scripts.Definitions.LandedCosting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.LandedCosting
{
	[TestedType(typeof(sp_UpdateOrAddNewLandedLineCostItem))]
	class sp_UpdateOrAddNewLandedLineCostItemTest : DbCreateScriptTest
	{
	}
}
