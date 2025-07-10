using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Accounting.Utilities;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.ReportFunctions.Accounting.Utilities.Testing
{
	[TestedType(typeof(JobRevRecognitionDates))]
	class JobRevRecognitionDatesTest : BiCreateScriptTest
	{
		protected override string ScriptDbName { get { return Db.EdwDatabaseName; } }
	}
}
