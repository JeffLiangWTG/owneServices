using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.MainDb;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.MainDb
{
	[TestedType(typeof(usp_GetCdcTableListFromDb))]
	internal class usp_GetCdcTableListFromDbTest : DbCreateScriptTest
	{
	}
}
