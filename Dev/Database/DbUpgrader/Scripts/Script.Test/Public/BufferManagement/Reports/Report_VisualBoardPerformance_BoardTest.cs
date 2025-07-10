using CargoWise.DbUpgrader.Scripts.Definitions.BufferManagement.Reports;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BufferManagement.Reports.Testing
{
	[TestedType(typeof(Report_VisualBoardPerformance_Board))]
	sealed class Report_VisualBoardPerformance_BoardTest : DbCreateScriptTest
	{
		// This function is tested at the business layer: Enterprise.BufferManagement.Business.Test.Report_VisualBoardPerformanceTest
	}
}

