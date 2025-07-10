using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests.MasterFiles
{
	[TestedType(typeof(GetAddressPksForOrg))]
	class GetAddressPksForOrgEDWHashTest : EdwHashTest
	{
		protected override string edwScriptPath => "ReportFunctions/MasterFiles/GetAddressPksForOrg.sql";
		protected override string expectedMainDbFunctionHash => "4D9D50F0642E8ED20AC7B3318192E64221D789E095870B41A0739518262AFD26";
		protected override string expectedEdwDbFunctionHash => "5668941DB8ED4EAA2515447E568F32EFDB3ED6D6E76043CE2EE956A8E6DBA3B7";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new GetAddressPksForOrg();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.MasterFiles.GetAddressPksForOrg();
		}
	}
}

