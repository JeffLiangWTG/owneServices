using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Consol;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests.Freight.Consol
{
	[TestedType(typeof(csfn_JobConsolWithDirection))]
	class csfn_JobConsolWithDirectionEDWHashTest : EdwHashTest
	{
		protected override string edwScriptPath => "ReportFunctions/Freight/Consol/csfn_JobConsolWithDirection.sql";
		protected override string expectedMainDbFunctionHash => "7D296EF25E91A65AD8D249FA42B87FB9A78947D52C441021540DA769E9957FFB";
		protected override string expectedEdwDbFunctionHash => "B6EA04CBD3AA14C9ECE6952949DAAC28F1D485875EF79AFD32FCED6710A5CD35";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new csfn_JobConsolWithDirection();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Freight.Consol.csfn_JobConsolWithDirection();
		}
	}
}

