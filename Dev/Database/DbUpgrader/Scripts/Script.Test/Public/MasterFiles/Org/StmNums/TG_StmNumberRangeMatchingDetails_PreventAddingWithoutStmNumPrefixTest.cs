using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.Org.StmNums;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Org.StmNums.Testing
{
	[TestedType(typeof(TG_StmNumberRangeMatchingDetails_PreventAddingWithoutStmNumPrefix))]
	class TG_StmNumberRangeMatchingDetails_PreventAddingWithoutStmNumPrefixTest : DbCreateScriptTest
	{
	}
}
