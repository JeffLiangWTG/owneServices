using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Consol;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests.Freight.Consol
{
	[TestedType(typeof(ViewFirstLastConsolTransport))]
	class ViewFirstLastConsolTransportEDWHashTest : EdwHashTest
	{
		protected override string edwScriptPath => "ReportFunctions/Freight/Consol/ViewFirstLastConsolTransport.sql";
		protected override string expectedMainDbFunctionHash => "0F8EE1F2A52A105A4478E5D5E8097F4CD364EEF546F6F2FFCCC9A78302D00EA6";
		protected override string expectedEdwDbFunctionHash => "2C9C352E0D28FA9378553512C23A933D6E30CED83E7960415B4633EC48C10003";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new ViewFirstLastConsolTransport();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Freight.Consol.ViewFirstLastConsolTransport();
		}
	}
}

