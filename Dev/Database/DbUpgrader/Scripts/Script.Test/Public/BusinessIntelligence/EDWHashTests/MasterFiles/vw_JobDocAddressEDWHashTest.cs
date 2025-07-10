using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests.MasterFiles
{
	[TestedType(typeof(vw_JobDocAddress))]
	class vw_JobDocAddressEDWHashTest : EdwHashTest
	{
		protected override string edwScriptPath => "ReportFunctions/MasterFiles/vw_JobDocAddress.sql";
		protected override string expectedMainDbFunctionHash => "386E09BA9D9E0A39B4B71DDAF05FA8987F3AE16DFA261F0FEEB29B49CF3D5046";
		protected override string expectedEdwDbFunctionHash => "23EF10D9C3424CEF5B3B423CC8C89CA57AC223A51E7C2797396BEDF86AF6574B";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new vw_JobDocAddress();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.MasterFiles.vw_JobDocAddress();
		}
	}
}

