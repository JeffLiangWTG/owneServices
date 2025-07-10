using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Consol;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests.Freight.Consol
{
	[TestedType(typeof(getConsolsStringForShipment))]
	class getConsolsStringForShipmentEDWHashTest : EdwHashTest
	{
		protected override string edwScriptPath => "ReportFunctions/Freight/Consol/GetConsolsStringForShipment.sql";
		protected override string expectedMainDbFunctionHash => "451C7FBEC0216CFD855A969E37FADD2DF158B021717B58268A7905439F443570";
		protected override string expectedEdwDbFunctionHash => "F0D0456EDB038DC72A39E2A937F12B7F463BA203B927E89DCDB56B958851EFDD";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new getConsolsStringForShipment();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Freight.Consol.GetConsolsStringForShipment();
		}
	}
}

