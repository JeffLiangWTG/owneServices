using System;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IL.Business
{
	public class EntryLineDutyCalculator : UniversalDutyCalculator<CusEntryLine, EntryLineUniversalRateCalcData>
	{
		public EntryLineDutyCalculator(CusEntryLine entryLine, RateCalculationVisitorMode rateCalculationVisitorMode, Func<CusEntryLine, RateView, IUniversalRateCalcData> createCalcDataFactory)
			: base(entryLine, rateCalculationVisitorMode, createCalcDataFactory)
		{
		}

		public EntryLineDutyCalculator(CusEntryLine entryLine, RateCalculationVisitorMode rateCalculationVisitorMode)
			: this(entryLine, rateCalculationVisitorMode, CreateILRateCalcData)
		{
		}

		static IUniversalRateCalcData CreateILRateCalcData(CusEntryLine entryLine, RateView rateView)
			=> new EntryLineUniversalRateCalcData(entryLine, rateView);
	}
}
