using System;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.DutyCalculator.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(AddOrCvdDepartureRateCalculationVisitor))]
	sealed class AddOrCvdDepartureRateCalculationVisitorTest : RateCalculationVisitorAbstractTest
	{
		protected override IDutyCalculationIntermediateResult[] GetApplicableExpectedResults(IDutyCalculationIntermediateResult[] allExpressionResults)
			=> allExpressionResults.Where(a => a.ParticipatingExpression).ToArray();

		protected override IRateCalculationVisitorCreator NewFormulaVisitorCreator()
		{
			return new AddOrCvdDepartureRateCalculationVisitor.Creator();
		}

		public void TestCalculateDuties_WithIfHasCertFormula_ShouldAssumeCertificatePresent()
		{
			testingData.ValueForDuty = 1000;

			var formulaForTesting = "IF(HAS(\"CERT\", \"D1234\"), VFD * 0.041, VFD * 0.042)";
			var formulaExpressionResults = new[]
			{
				new DutyCalculationIntermediateResult(41.000m, 0.041m, 1000m, "%"),
			};
			CalculateAndAssert(formulaForTesting, 41.000m, formulaExpressionResults);

			formulaForTesting = "IF(HAS(\"LICENCE\", \"L1234\"), VFD * 0.041, VFD * 0.042)";
			formulaExpressionResults = new[]
			{
				new DutyCalculationIntermediateResult(42.000m, 0.042m, 1000m, "%"),
			};
			CalculateAndAssert(formulaForTesting, 42.000m, formulaExpressionResults);

			testingData.AdditionalInformationList.Add(Tuple.Create("LICENCE", "L1234"));
			formulaForTesting = "IF(HAS(\"LICENCE\", \"L1234\"), VFD * 0.041, VFD * 0.042)";
			formulaExpressionResults = new[]
			{
				new DutyCalculationIntermediateResult(41.000m, 0.041m, 1000m, "%"),
			};
			CalculateAndAssert(formulaForTesting, 41.000m, formulaExpressionResults);
		}

		public void TestCalculateDuties_WithNestedIfHasCertFormula_ShouldTakeMaximumRate()
		{
			testingData.ValueForDuty = 1000;

			var formulaForTesting = "IF(HAS(\"CERT\", \"D1234\"), VFD * 0.041, IF(HAS(\"CERT\", \"D9876\"), VFD * 0.042, VFD * 0.043))";
			var formulaExpressionResults = new[]
			{
				new DutyCalculationIntermediateResult(42.000m, 0.042m, 1000m, "%"),
			};
			CalculateAndAssert(formulaForTesting, 42.000m, formulaExpressionResults);

			formulaForTesting = "IF(HAS(\"LICENCE\", \"L1234\"), VFD * 0.041, IF(HAS(\"LICENCE\", \"L9876\"), VFD * 0.042, VFD * 0.043))";
			formulaExpressionResults = new[]
			{
				new DutyCalculationIntermediateResult(43.000m, 0.043m, 1000m, "%"),
			};
			CalculateAndAssert(formulaForTesting, 43.000m, formulaExpressionResults);

			testingData.AdditionalInformationList.Add(Tuple.Create("LICENCE", "L9876"));
			formulaForTesting = "IF(HAS(\"LICENCE\", \"L1234\"), VFD * 0.041, IF(HAS(\"LICENCE\", \"L9876\"), VFD * 0.042, VFD * 0.043))";
			formulaExpressionResults = new[]
			{
				new DutyCalculationIntermediateResult(42.000m, 0.042m, 1000m, "%"),
			};
			CalculateAndAssert(formulaForTesting, 42.000m, formulaExpressionResults);
		}
	}
}
