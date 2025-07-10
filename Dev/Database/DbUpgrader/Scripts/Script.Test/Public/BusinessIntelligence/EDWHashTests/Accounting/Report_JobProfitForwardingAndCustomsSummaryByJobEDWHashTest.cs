using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests.Accounting
{
	[TestedType(typeof(Report_JobProfitForwardingAndCustomsSummaryByJob))]
	class Report_JobProfitForwardingAndCustomsSummaryByJobEDWHashTest : EdwHashTest
	{
		protected override string edwScriptPath => "ReportFunctions/Accounting/Report_JobProfitForwardingAndCustomsSummaryByJob.sql";
		protected override string expectedMainDbFunctionHash => "F4DA2C4530DA60BD68717C119A358F0DFD8BC19A86DB7BCAD0B9064967D83FEE";
		protected override string expectedEdwDbFunctionHash => "5886971BF3AC25AE69C4675B027931DE4A398DD64B066C77868435B985B4AE10";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new Report_JobProfitForwardingAndCustomsSummaryByJob();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Accounting.Report_JobProfitForwardingAndCustomsSummaryByJob();
		}
	}
}
