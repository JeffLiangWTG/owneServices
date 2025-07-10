using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests.Accounting
{
	[TestedType(typeof(Report_AllJobProfitCharge))]
	class Report_AllJobProfitChargeEDWHashTest : EdwHashTest
	{
		protected override string edwScriptPath => "ReportFunctions/Accounting/Report_AllJobProfitCharge.sql";
		protected override string expectedMainDbFunctionHash => "0A52F3C38BFC6860CF477FFC8D376D554CD2F9559AF7AB2AA675E447EF33192B";
		protected override string expectedEdwDbFunctionHash => "B9C198D325DE223D5EA4B9BF98159CE4D041BBE3FFB871F42459A9DE7A203244";

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Accounting.Report_AllJobProfitCharge();
		}

		protected override DbCreateScript GetMainDbFunction()
		{
			return new Report_AllJobProfitCharge();
		}
	}
}

