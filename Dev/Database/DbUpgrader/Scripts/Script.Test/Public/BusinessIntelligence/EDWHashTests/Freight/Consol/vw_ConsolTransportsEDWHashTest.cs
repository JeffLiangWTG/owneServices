using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Consol;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests.Freight.Consol
{
	[TestedType(typeof(vw_ConsolTransports))]
	class vw_ConsolTransportsEDWHashTest : EdwHashTest
	{
		protected override string edwScriptPath => "ReportFunctions/Freight/Consol/vw_ConsolTransports.sql";
		protected override string expectedMainDbFunctionHash => "45AA3FA5F019F0E10CD86583353DABE82107802C044382D9D4F8FE227BA8F5AB";
		protected override string expectedEdwDbFunctionHash => "536BFDB1A8D2FE0B9A21F830CFAA473A9542307D3D70AE69A17169381157C5F4";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new vw_ConsolTransports();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Freight.Consol.vw_ConsolTransports();
		}
	}
}

