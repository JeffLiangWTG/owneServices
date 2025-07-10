using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(ProfitAndLossReport))]
	class ProfitAndLossReportTest : EdwHashTest
	{
		protected override string expectedMainDbFunctionHash => "FC0127D7956986870E49D58186D924DA607DBCF6699B0BE6DEAD1FAC527F52BF";
		protected override string expectedEdwDbFunctionHash => "7E8CF54A8A54B4608698A66A8B8FDEF3BABD21F9A93519F0CCA83280A0B0FAEB";

		protected override string edwScriptPath => "Function/Accounting/ProfitAndLossReport.sql";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new ProfitAndLossReport();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.ProfitAndLossReport();
		}
	}
}

