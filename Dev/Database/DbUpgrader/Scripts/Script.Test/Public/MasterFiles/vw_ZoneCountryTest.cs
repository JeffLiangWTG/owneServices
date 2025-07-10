using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Testing
{
	[TestedType(typeof(vw_ZoneCountry))]
	class vw_ZoneCountryTest : DbCreateIndexedViewScriptTest
	{
	}
}

