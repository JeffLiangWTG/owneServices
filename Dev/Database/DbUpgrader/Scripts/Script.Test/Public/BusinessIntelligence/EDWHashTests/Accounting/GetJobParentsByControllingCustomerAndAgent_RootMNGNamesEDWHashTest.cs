using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests.Accounting
{
	[TestedType(typeof(GetJobParentsByControllingCustomerAndAgent))]
	class GetJobParentsByControllingCustomerAndAgentEDWHashTest : EdwHashTest
	{
		protected override string expectedMainDbFunctionHash => "411C541D3D57DAF7641332F661B3B3FA65F2B0C8D05AC94C8991C887D4D08FB0";
		protected override string expectedEdwDbFunctionHash => "09C6C1F72B2DDC5DD52EC17128FA74FE95513E6A36E33398A407CE7A3D9AFCB5";

		protected override string edwScriptPath => "ReportFunctions/Accounting/GetJobParentsByControllingCustomerAndAgent.sql";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new GetJobParentsByControllingCustomerAndAgent();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Accounting.GetJobParentsByControllingCustomerAndAgent();
		}
	}

	[TestedType(typeof(GetRootMNGNames))]
	class GetRootMNGNamesEDWHashTest : EdwHashTest
	{
		protected override string expectedMainDbFunctionHash => "ACD33BD82A8C5729F4C330365985A1D39E93D2A8172B5AA096DF813F8A4A92E6";
		protected override string expectedEdwDbFunctionHash => "9B60B41710E82E494073BCAACA1A12F5916A4BA2D255C5B3BC77514AD6389DA4";

		protected override string edwScriptPath => "ReportFunctions/Accounting/GetRootMNGNames.sql";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new GetRootMNGNames();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Accounting.GetRootMNGNames();
		}
	}
}
