using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests.Accounting
{
	[TestedType(typeof(csfn_AllJobProfitDetailCoreWithCCBAndCAGInfoWithTaxExpense))]
	class csfn_AllJobProfitDetailCoreWithCCBAndCAGInfoWithTaxExpenseEDWHashTest : EdwHashTest
	{
		protected override string expectedMainDbFunctionHash => "9582570A488626E905509EF2E2E22D5CDAC3EE112E264B6F2BAF9A9D63F75D24";
		protected override string expectedEdwDbFunctionHash => "554C30BC8FF7C51D86A48E5A55F0F4729A5166C5D66538409ADF6B6E84B6AA88";

		protected override string edwScriptPath => "ReportFunctions/Accounting/csfn_AllJobProfitDetailCoreWithCCBAndCAGInfoWithTaxExpense.sql";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new csfn_AllJobProfitDetailCoreWithCCBAndCAGInfoWithTaxExpense();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.csfn_AllJobProfitDetailCoreWithCCBAndCAGInfoWithTaxExpense();
		}
	}
}
