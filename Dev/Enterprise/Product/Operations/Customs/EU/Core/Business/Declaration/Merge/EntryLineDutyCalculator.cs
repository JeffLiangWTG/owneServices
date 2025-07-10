using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.Business
{
	public class EntryLineDutyCalculator : UniversalDutyCalculator<Declaration.CusEntryLine, EUUniversalRateCalcData>
	{
		public EntryLineDutyCalculator(Declaration.CusEntryLine entity, RateCalculationVisitorMode rateCalculationVisitorMode, Func<CusEntryLine, RateView, IUniversalRateCalcData> createCalcDataFactory)
			: base(entity, rateCalculationVisitorMode, createCalcDataFactory)
		{
		}

		public EntryLineDutyCalculator(Declaration.CusEntryLine entity, RateCalculationVisitorMode rateCalculationVisitorMode)
			: this(entity, rateCalculationVisitorMode, CreateEURateCalcData)
		{
		}

		static IUniversalRateCalcData CreateEURateCalcData(CusEntryLine entryLine, RateView rateView)
			=> new EUUniversalRateCalcData((Declaration.CusEntryLine)entryLine, rateView);
	}
}
