using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(DutyCalculatorStrategy))]
	class DutyCalculatorStrategyTest : Customs.Business.Testing.DutyCalculatorStrategyAbstractTest<DutyCalculatorStrategy>
	{
		protected override DutyCalculatorStrategy GetDutyCalculatorStrategy() => new DutyCalculatorStrategy((JobDeclaration)Declaration);

		protected override ZString UniversalTariffType => Constants.CusTariffCode.Schedule1Part1;

		protected override bool ExpectedShouldCalculateDuties => false;
	}
}
