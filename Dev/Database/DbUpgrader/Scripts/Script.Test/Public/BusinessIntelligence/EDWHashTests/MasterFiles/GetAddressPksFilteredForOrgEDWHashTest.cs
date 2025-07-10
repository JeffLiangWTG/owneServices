using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests.MasterFiles
{
	[TestedType(typeof(GetAddressPksFilteredForOrg))]
	class GetAddressPksFilteredForOrgEDWHashTest : EdwHashTest
	{
		protected override string edwScriptPath => "ReportFunctions/MasterFiles/GetAddressPksFilteredForOrg.sql";
		protected override string expectedMainDbFunctionHash => "A6B404454EF5723EA496BFE7FADF1CA61712FE03FDDFE3AAE3C2D90A5996F312";
		protected override string expectedEdwDbFunctionHash => "219B1BED29AE973199EC5674B6B4F275A22EE10DA09CE5D8E415963E1D80EE44";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new GetAddressPksFilteredForOrg();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.MasterFiles.GetAddressPksFilteredForOrg();
		}
	}
}

