using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.CashFlow;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.CashFlow.Testing
{
	[TestedType(typeof(Report_CashFlowStatement))]
	class Report_CashFlowStatementTest : EdwHashTest
	{
		protected override string expectedMainDbFunctionHash => "B70685A744E044F7F8BBAD754B9358C3ED1D66C9E4A684D6DF9430E64C16F477";
		protected override string expectedEdwDbFunctionHash => "660F80B9980DC3BB9AAC975E76E7977284327CD75030A267DB05BE7ABCD8E1A4";

		protected override string edwScriptPath => "Function/Accounting/CashFlow/Report_CashFlowStatement.sql";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new Report_CashFlowStatement();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.CashFlow.Report_CashFlowStatement();
		}
	}
}

