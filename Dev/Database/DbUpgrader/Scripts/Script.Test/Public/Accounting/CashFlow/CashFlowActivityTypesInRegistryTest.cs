using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.CashFlow;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.CashFlow.Testing
{
	[TestedType(typeof(CashFlowActivityTypesInRegistry))]
	class CashFlowActivityTypesInRegistryTest : EdwHashTest
	{
		protected override string expectedMainDbFunctionHash => "0249C13196C78B2307A89FEB8926ABA5C4169C368A4A447F7F59F0DDE61CDA66";
		protected override string expectedEdwDbFunctionHash => "A0CB0E425AA4C8E26B08EDEB39AD077A5CD3EE5BD425ED8B135FE27E16B01B07";

		protected override string edwScriptPath => "Function/Accounting/CashFlow/CashFlowActivityTypesInRegistry.sql";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new CashFlowActivityTypesInRegistry();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.CashFlow.CashFlowActivityTypesInRegistry();
		}
	}
}

