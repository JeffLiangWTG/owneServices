using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests.MasterFiles
{
	[TestedType(typeof(csfn_LocoReportingZones))]
	class csfn_LocoReportingZonesEDWHashTest : EdwHashTest
	{
		protected override string edwScriptPath => "ReportFunctions/MasterFiles/csfn_LocoReportingZones.sql";
		protected override string expectedMainDbFunctionHash => "BAD4595EF54667F7B86BCF3CFEB53F2018968A6B1E957DDA4DF917D4AB185925";
		protected override string expectedEdwDbFunctionHash => "BEB359600F2FE8B8C401FB20DA5336EB0B0CC825670AC302615E16B11990049A";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new csfn_LocoReportingZones();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.MasterFiles.csfn_LocoReportingZones();
		}
	}
}

