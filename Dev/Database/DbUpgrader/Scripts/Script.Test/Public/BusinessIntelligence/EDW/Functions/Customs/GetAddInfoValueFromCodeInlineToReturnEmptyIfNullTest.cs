using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Customs;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Customs.Testing
{
	[TestedType(typeof(GetAddInfoValueFromCodeInlineToReturnEmptyIfNull))]
	class GetAddInfoValueFromCodeInlineToReturnEmptyIfNullTest : BiCreateScriptTest
	{
		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}
	}
}
