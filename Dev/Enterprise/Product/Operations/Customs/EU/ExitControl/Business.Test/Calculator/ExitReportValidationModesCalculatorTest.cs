using System.Collections.Generic;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	[TestedType(typeof(ExitReportValidationModesCalculator))]
	class ExitReportValidationModesCalculatorTest : ValidationModesCalculatorAbstractTest<ExitReportValidationModesCalculator, CusExitReport>
	{
		protected override IEnumerable<(CusExitReport support, ValidationModes expectedResult, string description)> GetRecalculateValidationModesTestList()
		{
			var exitReport = Factory.New<CusExitReport>();
			yield return (exitReport, ValidationModes.None, "ValidationModes.None for Report");
		}
	}
}
