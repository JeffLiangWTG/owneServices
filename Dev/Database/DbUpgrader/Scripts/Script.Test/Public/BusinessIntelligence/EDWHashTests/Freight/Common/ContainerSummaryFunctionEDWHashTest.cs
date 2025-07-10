using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Common;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests.Freight.Common
{
	[TestedType(typeof(ContainerSummaryFunction))]
	internal class ContainerSummaryFunctionEDWHashTest : EdwHashTest
	{
		protected override string edwScriptPath => "ReportFunctions/Freight/Common/ContainerSummaryFunction.sql";

		protected override string expectedMainDbFunctionHash => "0A2425E2A722EBEBD48A3CC36D61D4432D5A18F246BEFD81EEC174D3C3A778A9";
		protected override string expectedEdwDbFunctionHash => "D8F99A186D4C3096713389BB254A98D673D05B084FDD829DDC0CC174C8AED1A4";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new ContainerSummaryFunction();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Freight.Common.ContainerSummaryFunction();
		}
	}
}

