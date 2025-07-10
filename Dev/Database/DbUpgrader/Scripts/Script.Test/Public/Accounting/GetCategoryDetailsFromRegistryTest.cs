using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	[TestedType(typeof(GetCategoryDetailsFromRegistry))]
	class GetCategoryDetailsFromRegistryTest : EdwHashTest
	{
		protected override string expectedMainDbFunctionHash => "9456051680DEB23F2D1747810349D1677C5920DEC5C8BE47A8A947472117E3D2";
		protected override string expectedEdwDbFunctionHash => "C76D1855B8159F10F5C5207866281E49C3DE2588CEB1E8BAB6ED5E4CB9749045";

		protected override string edwScriptPath => "Function/Accounting/GetCategoryDetailsFromRegistry.sql";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new GetCategoryDetailsFromRegistry();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.GetCategoryDetailsFromRegistry();
		}
	}
}

