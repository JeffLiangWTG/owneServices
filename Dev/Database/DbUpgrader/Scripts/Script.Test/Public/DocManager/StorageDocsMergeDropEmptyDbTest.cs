using CargoWise.DbUpgrader.Scripts.Definitions.DocManager;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.DocManager
{
	[TestedType(typeof(StorageDocsMergeDropEmptyDb))]
	class StorageDocsMergeDropEmptyDbTest : DbCreateScriptTest
	{
		// Msg 574, Level 16, State 0, Line 1
		// DROP DATABASE statement cannot be used inside a user transaction.
	}
}

