using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.DutyCalculator.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(EntryLineDutyCalculator))]
	sealed class EntryLineDutyCalculatorTest : UniversalDutyCalculatorAbstractTest<CusEntryLine, EntryLineUniversalRateCalcData>
	{
		protected override UniversalDutyCalculator<CusEntryLine, EntryLineUniversalRateCalcData> CreateDutyCalculator()
			=> new EntryLineDutyCalculator(Factory.New<CusEntryLine>(), RateCalculationVisitorMode.Default);
	}
}
