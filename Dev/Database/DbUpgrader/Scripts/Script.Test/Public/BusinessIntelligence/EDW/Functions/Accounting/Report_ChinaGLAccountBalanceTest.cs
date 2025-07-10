using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing
{
	[TestedType(typeof(Report_ChinaGLAccountBalance))]
	class Report_ChinaGLAccountBalanceTest : BiCreateScriptTest
	{
		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}
	}
}
