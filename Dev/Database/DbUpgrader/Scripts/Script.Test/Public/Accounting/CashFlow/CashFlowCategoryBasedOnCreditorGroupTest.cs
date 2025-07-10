using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.CashFlow;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.CashFlow.Testing
{
	[TestedType(typeof(CashFlowCategoryBasedOnCreditorGroup))]
	class CashFlowCategoryBasedOnCreditorGroupTest : EdwHashTest
	{
		protected override string expectedMainDbFunctionHash => "4591208FAE431F2EC0113677EF4D67F5C6EF5729005E2B85ED54B8561847F1F5";
		protected override string expectedEdwDbFunctionHash => "9E9FE87BAEA373CE8F039E2CEDC6F8D61B51130DFEA6CF2F0D4270B736299541";

		protected override string edwScriptPath => "Model/Finance/CashFlow/vw_GRP__CashFlowCategoryBasedOnCreditorGroup.sql";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new CashFlowCategoryBasedOnCreditorGroup();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Finance.vw_GRP__CashFlowCategoryBasedOnCreditorGroup();
		}
	}
}
