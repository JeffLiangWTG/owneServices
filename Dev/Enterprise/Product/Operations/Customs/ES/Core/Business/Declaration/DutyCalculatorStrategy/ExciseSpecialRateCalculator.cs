using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class ExciseSpecialRateCalculator : IExtraFeeCalculator
	{
		public ExciseSpecialRateCalculator(CusEntryLine entryLine)
		{
			this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
			invoiceLine = entryLine.RandomLine;
		}
		readonly CusEntryLine entryLine;
		readonly JobComInvoiceLine invoiceLine;

		public ZString RateCode => invoiceLine.SpecialExciseCode;

		public IEnumerable<IDutyCalculationIntermediateResult> CalculateExtraFees()
		{
			var fees = new List<IDutyCalculationIntermediateResult>();
			var specialExciseRate = invoiceLine.CurrentSpecialExciseRate;
			if (specialExciseRate != null)
			{
				var universalSpecialRateData = new ExciseUniversalRateCalcData(entryLine);
				var calculatedSpecialDuty = RateCalculationVisitor.CalculateParticipatingFees(universalSpecialRateData, specialExciseRate.ZZ2_RateFormula);
				fees.AddRange(calculatedSpecialDuty.IntermediateResults);
			}
			return fees;
		}
	}
}
