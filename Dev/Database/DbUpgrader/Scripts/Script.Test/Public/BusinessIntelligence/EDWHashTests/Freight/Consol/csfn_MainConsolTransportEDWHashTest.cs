using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Consol;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests.Freight.Consol
{
	[TestedType(typeof(csfn_MainConsolTransport))]
	class csfn_MainConsolTransportEDWHashTest : EdwHashTest
	{
		protected override string edwScriptPath => "ReportFunctions/Freight/Consol/csfn_MainConsolTransport.sql";
		protected override string expectedMainDbFunctionHash => "78CE4D1B166FA27CE50A5952A248211C2FC0A52E51342F841A2866997AB2385E";
		protected override string expectedEdwDbFunctionHash => "92C42AA57131EB96FD46DBA732DDF225F875A94BF7AFB909BC02E998AF054E17";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new csfn_MainConsolTransport();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Freight.Consol.csfn_MainConsolTransport();
		}
	}
}

