using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(Report_ForwardingJobProfitAnalysis))]
	class Report_ForwardingJobProfitAnalysisTest : EdwHashTest
	{
		protected override string expectedMainDbFunctionHash => "D66BC9FE995EFB5EEFDB9E32E97E7FC8960000E41D644720572C132F057650E7";
		protected override string expectedEdwDbFunctionHash => "0B4A90DA49CD2457E937DEEDC855FDE39037DCCAB8EEB2EBE138484C6ADEBAF8";

		protected override string edwScriptPath => "ReportFunctions/Accounting/Report_ForwardingJobProfitAnalysis.sql";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new Report_ForwardingJobProfitAnalysis();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Accounting.Report_ForwardingJobProfitAnalysis();
		}
	}
}

