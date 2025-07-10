using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Customs;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.ReportFunctions.Customs.Testing
{
	[TestedType(typeof(FCLDeclarationContainers))]
	class FCLDeclarationContainersTest : BiCreateScriptTest
	{
		protected override string ScriptDbName { get { return Db.EdwDatabaseName; } }
	}
}
