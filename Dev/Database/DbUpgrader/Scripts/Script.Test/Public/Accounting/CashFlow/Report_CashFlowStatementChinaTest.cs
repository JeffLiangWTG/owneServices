using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.CashFlow;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.CashFlow.Testing
{
	[TestedType(typeof(Report_CashFlowStatementChina))]
	class Report_CashFlowStatementChinaTest : EdwHashTest
	{
		protected override string expectedMainDbFunctionHash => "84F23AB097316D3AB59F50CE9E3237E352185A27EE02E996ABEEF96688157C1D";
		protected override string expectedEdwDbFunctionHash => "8ADBCBE2AE7C1A39222E2F2CF216E651358B5DF904A9D2CCB7B8533471648A87";

		protected override string edwScriptPath => "Function/Accounting/CashFlow/Report_CashFlowStatementChina.sql";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new Report_CashFlowStatementChina();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.CashFlow.Report_CashFlowStatementChina();
		}
	}
}

