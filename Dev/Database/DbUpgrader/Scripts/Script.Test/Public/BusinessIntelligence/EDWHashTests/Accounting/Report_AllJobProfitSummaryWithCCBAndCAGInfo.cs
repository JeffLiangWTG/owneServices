using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests.Accounting
{
	[TestedType(typeof(Report_AllJobProfitSummaryWithCCBAndCAGInfo))]
	class Report_AllJobProfitSummaryWithCCBAndCAGInfoEDWHashTest : EdwHashTest
	{
		protected override string expectedMainDbFunctionHash => "8550891ECDBAE51D88FEACF4543765A476382305227153D6BF9C05923CBC0F09";
		protected override string expectedEdwDbFunctionHash => "E3AC3CAAEC0F9F935AC1A98B483815BEC17731838F510E09C86060FD783D36B0";

		protected override string edwScriptPath => "ReportFunctions/Accounting/Report_AllJobProfitSummaryWithCCBAndCAGInfo.sql";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new Report_AllJobProfitSummaryWithCCBAndCAGInfo();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.Report_AllJobProfitSummaryWithCCBAndCAGInfo();
		}
	}
}
