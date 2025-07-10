using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.CashFlow;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.CashFlow.Testing
{
	[TestedType(typeof(Report_CashFlowModifiedGLAccount))]
	class Report_CashFlowModifiedGLAccountTest : EdwHashTest
	{
		protected override string expectedMainDbFunctionHash => "64F34D9B330173A0F8B512A61788F394F599E964634642AC541195B7D05B8311";
		protected override string expectedEdwDbFunctionHash => "09C0F033E65DBF269C7DF6EAEE71FCA1AA67C721BE7E1116CB8BE81881CB421D";

		protected override string edwScriptPath => "Function/Accounting/CashFlow/Report_CashFlowModifiedGLAccount.sql";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new Report_CashFlowModifiedGLAccount();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.CashFlow.Report_CashFlowModifiedGLAccount();
		}
	}
}

