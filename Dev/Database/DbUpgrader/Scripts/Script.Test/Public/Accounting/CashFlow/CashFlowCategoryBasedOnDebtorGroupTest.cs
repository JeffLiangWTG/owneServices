using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.CashFlow;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.CashFlow.Testing
{
	[TestedType(typeof(CashFlowCategoryBasedOnDebtorGroup))]
	class CashFlowCategoryBasedOnDebtorGroupTest : EdwHashTest
	{
		protected override string expectedMainDbFunctionHash => "DAE01D56AD9CAEC7B769514907C36BEA74B9BEE68485DBD62E3F78BE4BA9EA9B";
		protected override string expectedEdwDbFunctionHash => "65F83AC5AEFB23CC74628670161AA3C820E00471FC1B16DDA352877B026AF45F";
		protected override string edwScriptPath => "Model/Finance/CashFlow/vw_GRP__CashFlowCategoryBasedOnDebtorGroup.sql";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new CashFlowCategoryBasedOnDebtorGroup();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Finance.vw_GRP__CashFlowCategoryBasedOnDebtorGroup();
		}
	}
}

