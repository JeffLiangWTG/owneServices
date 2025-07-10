using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles
{
	[TestedType(typeof(csfn_LocoReportingZones))]
	class csfn_LocoReportingZonesTest : DbCreateScriptTest
	{
		public void TestReport()
		{
			AssertEquals("AUSR", TestConnection.ExecuteScalar(@"SELECT ZoneCode FROM csfn_LocoReportingZones(NULL) WHERE LocoCode = 'AUSYD'"));
			AssertEquals("AUSR", TestConnection.ExecuteScalar(@"SELECT ZoneCode FROM csfn_LocoReportingZones('B97AEF8B-DBD4-4B20-B07F-13202F73436B')"));
		}
	}
}

