using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests.Accounting
{
	[TestedType(typeof(Report_AllJobProfitSummary))]
	class Report_AllJobProfitSummaryEDWHashTest : EdwHashTest
	{
		protected override string edwScriptPath => "ReportFunctions/Accounting/Report_AllJobProfitSummary.sql";
		protected override string expectedMainDbFunctionHash => "5C6CF0720FAF9D06878EDB007393E9FBBECBABD865F6A9558E3FF25AF75795FD";
		protected override string expectedEdwDbFunctionHash => "11167407CD6DBFBE9B86E8CFB2937BB7CE7868ED6CA3BD64087FA91096D07ECD";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new Report_AllJobProfitSummary();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Accounting.Report_AllJobProfitSummary();
		}
	}
}

