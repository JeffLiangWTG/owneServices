using System.Collections.Generic;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	[TestedType(typeof(ExitHeaderValidationModesCalculator))]
	class ExitHeaderValidationModesCalculatorTest : ValidationModesCalculatorAbstractTest<ExitHeaderValidationModesCalculator, CusExitHeader>
	{
		protected override IEnumerable<(CusExitHeader support, ValidationModes expectedResult, string description)> GetRecalculateValidationModesTestList()
		{
			var exitHeader = Factory.New<CusExitHeader>();
			yield return (exitHeader, ValidationModes.None, "ValidationModes.None for Report");
		}
	}
}
