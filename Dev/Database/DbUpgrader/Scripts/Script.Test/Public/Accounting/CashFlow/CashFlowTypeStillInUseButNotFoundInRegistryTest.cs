using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.CashFlow;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.CashFlow.Testing
{
	[TestedType(typeof(CashFlowTypeStillInUseButNotFoundInRegistry))]
	class CashFlowTypeStillInUseButNotFoundInRegistryTest : EdwHashTest
	{
		protected override string expectedMainDbFunctionHash => "D69F97963EB5D5B601392FFDFD8B876FBF6387A4B8212A22EE7EF1727417CC0C";
		protected override string expectedEdwDbFunctionHash => "F4155001921567E96A6E0EF5470AD6D9CBC12DFD62B75C3E7419F39259712885";

		protected override string edwScriptPath => "Function/Accounting/CashFlow/CashFlowTypeStillInUseButNotFoundInRegistry.sql";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new CashFlowTypeStillInUseButNotFoundInRegistry();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.CashFlow.CashFlowTypeStillInUseButNotFoundInRegistry();
		}
	}
}

